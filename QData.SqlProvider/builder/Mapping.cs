using System.Collections.Generic;

namespace QData.SqlProvider.builder
{
    using System;
    using System.Linq;

    using AutoMapper;
    using AutoMapper.QueryableExtensions.Impl;

    using QData.Common;
    using QData.LinqConverter;

    public class Mapping
    {
        private readonly MapperConfiguration mapperConfiguration;

        public Mapping(MapperConfiguration mapperConfiguration)
        {
            this.mapperConfiguration = mapperConfiguration;
            this.MapContext = new Stack<TypeMap>();
        }
        private Stack<TypeMap> MapContext { get; set; }

        private TypeMap CurrentMap { get; set; }


        public bool EnableMapping { get; set; }

        public void EnterMapContext(Type sourceType)
        {
            if (this.EnableMapping)
            {
                var currentMap = this.mapperConfiguration.GetAllTypeMaps().FirstOrDefault(x => x.SourceType == sourceType);
                this.MapContext.Push(currentMap);
                this.CurrentMap = currentMap;
            }
        }

        public void LeaveMapContext()
        {
            if (this.EnableMapping)
            {
                this.MapContext.Pop();
                this.CurrentMap = this.MapContext.Count > 0 ? this.MapContext.Peek() : null;
            }
        }

        public void Reset()
        {
            if (this.EnableMapping)
            {
                this.CurrentMap = this.MapContext.Peek();
            }
        }

        public string Map(string member)
        {
            if (!this.EnableMapping)
            {
                return member;
            }

            var propertyMap = this.CurrentMap.GetPropertyMaps().FirstOrDefault(
                        x => x.DestinationProperty.Name.Equals(member, StringComparison.CurrentCultureIgnoreCase));

            //if (propertyMap.CustomExpression != null)
            //{
            //    var c = new ExpressionConverter();
            //    var node = c.Convert(propertyMap.CustomExpression);
            //}

            if (propertyMap.DestinationPropertyType.IsGenericType
                && typeof(IModelEntity).IsAssignableFrom(propertyMap.DestinationPropertyType.GenericTypeArguments[0]))
            {
                var sourceType = propertyMap.CustomExpression != null
                                     ? propertyMap.CustomExpression.Body.Type.GenericTypeArguments[0]
                                     : propertyMap.SourceType.GenericTypeArguments[0];

                this.CurrentMap =
                    this.mapperConfiguration.GetAllTypeMaps().FirstOrDefault(x => x.SourceType == sourceType);
                
            }
            else if (typeof(IModelEntity).IsAssignableFrom(propertyMap.DestinationPropertyType))
            {
                var sourceType = propertyMap.CustomExpression != null
                                     ? propertyMap.CustomExpression.Body.Type
                                     : propertyMap.SourceType;

                this.CurrentMap =
                    this.mapperConfiguration.GetAllTypeMaps().FirstOrDefault(x => x.SourceType == sourceType);
                
            }

            return propertyMap.SourceMember.Name;
        }
    }
}