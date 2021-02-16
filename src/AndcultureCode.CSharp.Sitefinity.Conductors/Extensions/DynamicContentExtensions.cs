using AndcultureCode.CSharp.Sitefinity.Core.Attributes;
using AndcultureCode.CSharp.Sitefinity.Core.Models.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.RelatedData;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace AndcultureCode.CSharp.Sitefinity.Conductors.Extensions
{
    public static class DynamicContentExtensions
    {
        public static DynamicContent SetDataItemProperties<T>(this T item, PropertyInfo[] properties, string providerName) where T : SitefinityContent
        {
            SitefinityMetadataAttribute metadataAttribute = (SitefinityMetadataAttribute)item.GetType().GetCustomAttribute(typeof(SitefinityMetadataAttribute));

            if (metadataAttribute == null)
            {
                throw new Exception($"Could not cast attribute to {nameof(SitefinityMetadataAttribute)}");
            }

            var dynamicContentType = metadataAttribute.DynamicContentType;

            if (dynamicContentType == null)
            {
                return null;
            }

            var resolvedDynamicContentType = TypeResolutionService.ResolveType(dynamicContentType);

            // TODO: should this be done in the constructor?
            var dynamicModuleManager = DynamicModuleManager.GetManager(providerName);
            var dataItem = dynamicModuleManager.CreateDataItem(resolvedDynamicContentType);

            properties.ToList().ForEach(property => SetPropertyInDataItem(
                item,
                metadataAttribute,
                ref dataItem,
                property,
                providerName
            ));

            return dataItem;
        }

        private static void SetPropertyInDataItem<T>(
           T item,
           SitefinityMetadataAttribute metadataAttribute,
           ref DynamicContent dataItem,
           PropertyInfo property,
           string providerName
       )
        {
            if (!property.PropertyType.IsSubclassOf(typeof(SitefinityContent)) &&
                !property.PropertyType.IsSubclassOf(typeof(Content)))
            {
                dataItem.SetValue(property.Name, item.GetType().GetProperty(property.Name)?.GetValue(item));
            }
            else if (property.PropertyType.IsSubclassOf(typeof(SitefinityContent)))
            {
                dataItem = SetPropertyInDataItemAsRelationship(
                    item,
                    metadataAttribute,
                    property,
                    dataItem,
                    providerName
                );
            }
        }

        private static DynamicContent SetPropertyInDataItemAsRelationship<T>(
            T item,
            SitefinityMetadataAttribute metadataAttribute,
            PropertyInfo property,
            DynamicContent dataItem,
            string providerName
        )
        {
            var parentDynamicContentType = metadataAttribute.ParentDynamicContentType;
            var sitefinityContentItem = (SitefinityContent)item.GetType().GetProperty(property.Name)?.GetValue(item);

            SitefinityMetadataAttribute sitefinityType = (SitefinityMetadataAttribute)sitefinityContentItem?.GetType()
                .GetCustomAttribute(typeof(SitefinityMetadataAttribute));

            if (sitefinityType != null)
            {
                var relatedDynamicContentType = sitefinityType.DynamicContentType;

                var contentItemType = TypeResolutionService.ResolveType(relatedDynamicContentType.ToString());

                // TODO: should this be done in the constructor?
                var dynamicModuleManager = DynamicModuleManager.GetManager(providerName);
                var dynamicContentItem = dynamicModuleManager.GetDataItem(contentItemType, sitefinityContentItem.Id);

                // If this property happens to be the Parent Relation, set the parent + SystemParentId. Otherwise, create the relation
                if (parentDynamicContentType != null && relatedDynamicContentType.Equals(parentDynamicContentType))
                {
                    dataItem.SetParent(dynamicContentItem);
                    dataItem.SystemParentId = dynamicContentItem.Id;

                    return dataItem;
                }

                dataItem.CreateRelation(dynamicContentItem, property.Name);
            }

            return dataItem;
        }

    }
}
