using AndcultureCode.CSharp.Sitefinity.Core.Attributes;
using AndcultureCode.CSharp.Sitefinity.Core.Models.Content;
using System;
using System.Linq;
using System.Reflection;
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
        /// <summary>
        /// Adapts item T into a DynamicContent item containing fields/values found in item T. This will also create
        /// any relational data found in the item.
        /// </summary>
        /// <typeparam name="T">A subclass of SitefinityContent.</typeparam>
        /// <param name="item">The item being adapted into a DynamicContent data item.</param>
        /// <param name="properties">The property info of item T.</param>
        /// <param name="providerName">The provider name used when retrieving the DynamicModuleManager.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Sets an individual item in the dataItem property using the respective value found in item T.
        /// If the individual property being processed is another Sitefinity dynamic content object, it will
        /// instead create a relationship between the dataItem and property in question.
        /// </summary>
        /// <typeparam name="T">A subclass of SitefinityContent.</typeparam>
        /// <param name="item">The item being adapted into the dataItem object.</param>
        /// <param name="metadataAttribute">The SitefinityMetadataAttribute found on item T.</param>
        /// <param name="dataItem">The DynamicContent data item being adapted from item T.</param>
        /// <param name="property">The property on item T being adapted.</param>
        /// <param name="providerName">The provider name used when retrieving the DynamicModuleManager.</param>
        private static void SetPropertyInDataItem<T>(
           T item,
           SitefinityMetadataAttribute metadataAttribute,
           ref DynamicContent dataItem,
           PropertyInfo property,
           string providerName
        ) where T : SitefinityContent
        {
            var sitefinityContentSubclass = property.PropertyType.IsSubclassOf(typeof(SitefinityContent));
            var contentSubclass = property.PropertyType.IsSubclassOf(typeof(Content));

            // Enums must be adapted to a string value since Sitefinity stores them as a string
            if (property.PropertyType.IsEnum)
            {
                dataItem.SetValue(property.Name, item.GetType().GetProperty(property.Name)?.GetValue(item)?.ToString());
                return;
            }

            if (!sitefinityContentSubclass && !contentSubclass)
            {
                dataItem.SetValue(property.Name, item.GetType().GetProperty(property.Name)?.GetValue(item));
                return;
            }

            if (sitefinityContentSubclass)
            {
                dataItem = SetPropertyInDataItemAsRelationship(
                    item,
                    metadataAttribute,
                    property,
                    dataItem,
                    providerName
                );
                return;
            }
        }

        /// <summary>
        /// Sets relational data in the DynamicContent data item using the property value found in item T.
        /// </summary>
        /// <typeparam name="T">A subclass of SitefinityContent.</typeparam>
        /// <param name="item">The item being adapted into the dataItem object.</param>
        /// <param name="metadataAttribute">The SitefinityMetadataAttribute found on item T.</param>
        /// <param name="property">The property on item T being adapted.</param>
        /// <param name="dataItem">The DynamicContent data item being adapted from item T.</param>
        /// <param name="providerName">The provider name used when retrieving the DynamicModuleManager.</param>
        /// <returns></returns>
        private static DynamicContent SetPropertyInDataItemAsRelationship<T>(
            T item,
            SitefinityMetadataAttribute metadataAttribute,
            PropertyInfo property,
            DynamicContent dataItem,
            string providerName
        ) where T : SitefinityContent
        {
            var parentDynamicContentType = metadataAttribute.ParentDynamicContentType;
            var sitefinityContentItem = (SitefinityContent)item.GetType().GetProperty(property.Name)?.GetValue(item);

            SitefinityMetadataAttribute sitefinityType = (SitefinityMetadataAttribute)sitefinityContentItem?.GetType()
                .GetCustomAttribute(typeof(SitefinityMetadataAttribute));

            if (sitefinityType == null)
            {
                return dataItem;
            }

            var relatedDynamicContentType = sitefinityType.DynamicContentType;

            var contentItemType = TypeResolutionService.ResolveType(relatedDynamicContentType.ToString());

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

            return dataItem;
        }

    }
}
