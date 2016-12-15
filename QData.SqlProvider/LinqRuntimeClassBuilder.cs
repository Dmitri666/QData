// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LinqRuntimeClassBuilder.cs" company="">
//   
// </copyright>
// <summary>
//   The dynamic class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace QData.SqlProvider
{
    /// <summary>
    /// The dynamic class.
    /// </summary>
    public abstract class DynamicClass
    {
        #region Public Methods and Operators

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            PropertyInfo[] props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            for (int i = 0; i < props.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(props[i].Name);
                sb.Append("=");
                sb.Append(props[i].GetValue(this, null));
            }

            sb.Append("}");
            return sb.ToString();
        }

        #endregion
    }

    /// <summary>
    /// The dynamic property.
    /// </summary>
    public class DynamicProperty
    {
        #region Fields

        /// <summary>
        /// The name.
        /// </summary>
        private readonly string name;

        /// <summary>
        /// The type.
        /// </summary>
        private readonly Type type;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicProperty"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public DynamicProperty(string name, Type type)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            this.name = name;
            this.type = type;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        public Type Type
        {
            get
            {
                return this.type;
            }
        }

        #endregion
    }

    /// <summary>
    /// The signature.
    /// </summary>
    internal class Signature : IEquatable<Signature>
    {
        #region Fields

        /// <summary>
        /// The hash code.
        /// </summary>
        public int hashCode;

        /// <summary>
        /// The properties.
        /// </summary>
        public DynamicProperty[] properties;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Signature"/> class.
        /// </summary>
        /// <param name="properties">
        /// The properties.
        /// </param>
        public Signature(IEnumerable<DynamicProperty> properties)
        {
            this.properties = properties.ToArray();
            this.hashCode = 0;
            foreach (DynamicProperty p in properties)
            {
                this.hashCode ^= p.Name.GetHashCode() ^ p.Type.GetHashCode();
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is Signature ? this.Equals((Signature)obj) : false;
        }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Equals(Signature other)
        {
            if (this.properties.Length != other.properties.Length)
            {
                return false;
            }

            for (int i = 0; i < this.properties.Length; i++)
            {
                if (this.properties[i].Name != other.properties[i].Name
                    || this.properties[i].Type != other.properties[i].Type)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The get hash code.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return this.hashCode;
        }

        #endregion
    }

    /// <summary>
    /// The class factory.
    /// </summary>
    internal class ClassFactory
    {
        #region Static Fields

        /// <summary>
        /// The instance.
        /// </summary>
        public static readonly ClassFactory Instance = new ClassFactory();

        #endregion

        #region Fields

        /// <summary>
        /// The classes.
        /// </summary>
        private readonly Dictionary<Signature, Type> classes;

        private readonly Dictionary<Type, KeyValuePair<Type,Dictionary<string,string>>> proxies;

        /// <summary>
        /// The module.
        /// </summary>
        private readonly ModuleBuilder module;

        /// <summary>
        /// The rw lock.
        /// </summary>
        private readonly ReaderWriterLock rwLock;

        /// <summary>
        /// The class count.
        /// </summary>
        private int classCount;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes static members of the <see cref="ClassFactory"/> class.
        /// </summary>
        static ClassFactory()
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="ClassFactory"/> class from being created.
        /// </summary>
        private ClassFactory()
        {
            AssemblyName name = new AssemblyName("DynamicClasses");
            AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
#if ENABLE_LINQ_PARTIAL_TRUST
            new ReflectionPermission(PermissionState.Unrestricted).Assert();
#endif
            try
            {
                this.module = assembly.DefineDynamicModule("Module");
            }
            finally
            {
#if ENABLE_LINQ_PARTIAL_TRUST
                PermissionSet.RevertAssert();
#endif
            }

            this.classes = new Dictionary<Signature, Type>();
            this.proxies = new Dictionary<Type, KeyValuePair<Type, Dictionary<string, string>>>();
            this.rwLock = new ReaderWriterLock();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get dynamic class.
        /// </summary>
        /// <param name="properties">
        /// The properties.
        /// </param>
        /// <returns>
        /// The <see cref="Type"/>.
        /// </returns>
        public Type GetDynamicClass(IEnumerable<DynamicProperty> properties)
        {
            this.rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                var signature = new Signature(properties);
                Type type;
                if (!this.classes.TryGetValue(signature, out type))
                {
                    type = this.CreateDynamicClass(signature.properties);
                    this.classes.Add(signature, type);
                }

                return type;
            }
            finally
            {
                this.rwLock.ReleaseReaderLock();
            }
        }

        

       

        #endregion

            #region Methods

        /// <summary>
        /// The create dynamic class.
        /// </summary>
        /// <param name="properties">
        /// The properties.
        /// </param>
        /// <returns>
        /// The <see cref="Type"/>.
        /// </returns>
        private Type CreateDynamicClass(DynamicProperty[] properties)
        {
            LockCookie cookie = this.rwLock.UpgradeToWriterLock(Timeout.Infinite);
            try
            {
                string typeName = string.Format("DynamicClass{0}", this.classCount);
#if ENABLE_LINQ_PARTIAL_TRUST
                new ReflectionPermission(PermissionState.Unrestricted).Assert();
#endif
                try
                {
                    TypeBuilder tb = this.module.DefineType(
                        typeName, 
                        TypeAttributes.Class | TypeAttributes.Public, 
                        typeof(DynamicClass));
                    FieldInfo[] fields = this.GenerateProperties(tb, properties);
                    this.GenerateEquals(tb, fields);
                    this.GenerateGetHashCode(tb, fields);
                    Type result = tb.CreateType();
                    this.classCount++;
                    return result;
                }
                finally
                {
#if ENABLE_LINQ_PARTIAL_TRUST
                    PermissionSet.RevertAssert();
#endif
                }
            }
            finally
            {
                this.rwLock.DowngradeFromWriterLock(ref cookie);
            }
        }


        /// <summary>
        /// The generate equals.
        /// </summary>
        /// <param name="tb">
        /// The tb.
        /// </param>
        /// <param name="fields">
        /// The fields.
        /// </param>
        private void GenerateEquals(TypeBuilder tb, FieldInfo[] fields)
        {
            MethodBuilder mb = tb.DefineMethod(
                "Equals", 
                MethodAttributes.Public | MethodAttributes.ReuseSlot | MethodAttributes.Virtual | MethodAttributes.HideBySig, 
                typeof(bool), 
                new[] { typeof(object) });
            ILGenerator gen = mb.GetILGenerator();
            LocalBuilder other = gen.DeclareLocal(tb);
            Label next = gen.DefineLabel();
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Isinst, tb);
            gen.Emit(OpCodes.Stloc, other);
            gen.Emit(OpCodes.Ldloc, other);
            gen.Emit(OpCodes.Brtrue_S, next);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Ret);
            gen.MarkLabel(next);
            foreach (FieldInfo field in fields)
            {
                Type ft = field.FieldType;
                Type ct = typeof(EqualityComparer<>).MakeGenericType(ft);
                next = gen.DefineLabel();
                gen.EmitCall(OpCodes.Call, ct.GetMethod("get_Default"), null);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, field);
                gen.Emit(OpCodes.Ldloc, other);
                gen.Emit(OpCodes.Ldfld, field);
                gen.EmitCall(OpCodes.Callvirt, ct.GetMethod("Equals", new[] { ft, ft }), null);
                gen.Emit(OpCodes.Brtrue_S, next);
                gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Ret);
                gen.MarkLabel(next);
            }

            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// The generate get hash code.
        /// </summary>
        /// <param name="tb">
        /// The tb.
        /// </param>
        /// <param name="fields">
        /// The fields.
        /// </param>
        private void GenerateGetHashCode(TypeBuilder tb, FieldInfo[] fields)
        {
            MethodBuilder mb = tb.DefineMethod(
                "GetHashCode", 
                MethodAttributes.Public | MethodAttributes.ReuseSlot | MethodAttributes.Virtual | MethodAttributes.HideBySig, 
                typeof(int), 
                Type.EmptyTypes);
            ILGenerator gen = mb.GetILGenerator();
            gen.Emit(OpCodes.Ldc_I4_0);
            foreach (FieldInfo field in fields)
            {
                Type ft = field.FieldType;
                Type ct = typeof(EqualityComparer<>).MakeGenericType(ft);
                gen.EmitCall(OpCodes.Call, ct.GetMethod("get_Default"), null);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, field);
                gen.EmitCall(OpCodes.Callvirt, ct.GetMethod("GetHashCode", new[] { ft }), null);
                gen.Emit(OpCodes.Xor);
            }

            gen.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// The generate properties.
        /// </summary>
        /// <param name="tb">
        /// The tb.
        /// </param>
        /// <param name="properties">
        /// The properties.
        /// </param>
        /// <returns>
        /// The <see cref="FieldInfo[]"/>.
        /// </returns>
        private FieldInfo[] GenerateProperties(TypeBuilder tb, DynamicProperty[] properties)
        {
            FieldInfo[] fields = new FieldBuilder[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                DynamicProperty dp = properties[i];
                FieldBuilder fb = tb.DefineField("_" + dp.Name, dp.Type, FieldAttributes.Private);
                PropertyBuilder pb = tb.DefineProperty(dp.Name, PropertyAttributes.HasDefault, dp.Type, null);
                MethodBuilder mbGet = tb.DefineMethod(
                    "get_" + dp.Name, 
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, 
                    dp.Type, 
                    Type.EmptyTypes);
                ILGenerator genGet = mbGet.GetILGenerator();
                genGet.Emit(OpCodes.Ldarg_0);
                genGet.Emit(OpCodes.Ldfld, fb);
                genGet.Emit(OpCodes.Ret);
                MethodBuilder mbSet = tb.DefineMethod(
                    "set_" + dp.Name, 
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, 
                    null, 
                    new[] { dp.Type });
                ILGenerator genSet = mbSet.GetILGenerator();
                genSet.Emit(OpCodes.Ldarg_0);
                genSet.Emit(OpCodes.Ldarg_1);
                genSet.Emit(OpCodes.Stfld, fb);
                genSet.Emit(OpCodes.Ret);
                pb.SetGetMethod(mbGet);
                pb.SetSetMethod(mbSet);
                fields[i] = fb;
            }

            return fields;
        }

       
        #endregion
    }
}