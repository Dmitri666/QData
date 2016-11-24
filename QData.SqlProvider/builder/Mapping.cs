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
        }

        private TypeMap CurrentMap { get; set; }

        public bool EnableMapping { get; set; }

        public void SetCurrentMap(Type sourceType)
        {
            if (this.EnableMapping)
            {
                this.CurrentMap =
                    this.mapperConfiguration.GetAllTypeMaps().FirstOrDefault(x => x.SourceType == sourceType);
            }
        }

        public string GetMapNameForMember(string member)
        {
            if (!this.EnableMapping)
            {
                return member;
            }

            var propertyMap =
                this.CurrentMap.GetPropertyMaps()
                    .FirstOrDefault(
                        x => x.DestinationProperty.Name.Equals(member, StringComparison.CurrentCultureIgnoreCase));

            if (propertyMap.CustomExpression != null)
            {
                var c = new ExpressionConverter();
                var node = c.Convert(propertyMap.CustomExpression);
            }

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