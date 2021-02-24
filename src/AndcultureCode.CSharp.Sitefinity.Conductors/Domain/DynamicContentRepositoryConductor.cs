using AndcultureCode.CSharp.Core;
using AndcultureCode.CSharp.Core.Extensions;
using AndcultureCode.CSharp.Core.Interfaces;
using AndcultureCode.CSharp.Sitefinity.Conductors.Extensions;
using AndcultureCode.CSharp.Sitefinity.Core.Constants;
using AndcultureCode.CSharp.Sitefinity.Core.Interfaces;
using AndcultureCode.CSharp.Sitefinity.Core.Models.Content;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace AndcultureCode.CSharp.Sitefinity.Conductors.Domain
{
    public class DynamicContentRepositoryConductor : IDynamicContentRepositoryConductor
    {
        #region Constants

        public const string ERROR_CONTENT_TYPE_NULL_OR_EMPTY = "ERROR_CONTENT_TYPE_NULL_OR_EMPTY";
        public const string ERROR_COULD_NOT_RESOLVE_TYPE = "ERROR_COULD_NOT_RESOLVE_TYPE";
        public const string ERROR_FAILED_TO_GET_DYNAMIC_MODULE_MANAGER = "ERROR_FAILED_TO_GET_DYNAMIC_MODULE_MANAGER";

        #endregion Constants

        #region Public Properties

        /// <summary>
        /// The provider name used by default to get dynamic content.
        /// Defaults to "OpenAccessProvider".
        /// </summary>
        public string DefaultProviderName { get; set; } = "OpenAccessProvider";

        #endregion Public Properties

        #region Private Properties

        private ILogger<IDynamicContentRepositoryConductor> _logger { get; }

        #endregion Private Properties

        #region Public Methods

        public DynamicContentRepositoryConductor(
            ILogger<IDynamicContentRepositoryConductor> logger
        )
        {
            _logger = logger;
        }

        /// <summary>
        /// Creates a dynamic content item of the specified type T.
        /// </summary>
        /// <typeparam name="T">A subclass of SitefinityContent.</typeparam>
        /// <param name="item"></param>
        /// <param name="publish">Flag denoting whether or not to publish the item after creating it. Defaults to false</param>
        /// <returns></returns>
        public IResult<DynamicContent> Create<T>(T item, bool publish) where T : SitefinityContent => Do<DynamicContent>.Try((r) =>
        {
            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            var dataItem = item.SetDataItemProperties(properties, DefaultProviderName);

            var dynamicModuleManager = DynamicModuleManager.GetManager(DefaultProviderName);
            dynamicModuleManager.SaveChanges();

            if (publish)
            {
                dataItem.ApprovalWorkflowState = ApprovalWorkflowStates.PUBLISHED;
                dataItem = (DynamicContent) dynamicModuleManager.Lifecycle.Publish(dataItem);

                dynamicModuleManager.SaveChanges();
            }

            return dataItem;
        })
        .Result;

        /// <summary>
        /// Gets live content from the dynamic module manager by type and optionally applies a filter.
        /// </summary>
        /// <param name="contentType">String representation of the content type.</param>
        /// <param name="filter">Optional filter to apply to result set.</param>
        /// <returns></returns>
        public IResult<IQueryable<DynamicContent>> GetLiveContentByType(
            string contentType,
            Expression<Func<DynamicContent, bool>> filter = null
        ) => GetContentByType(contentType, ContentLifecycleStatus.Live, filter);

        /// <summary>
        /// Gets content from the dynamic module manager by type and optionally applies a filter.
        /// </summary>
        /// <param name="contentType">String representation of the content type.</param>
        /// <param name="status">Status of the content you want returned. Null returns content in all statuses.</param>
        /// <param name="filter">Optional filter to apply to result set.</param>
        /// <returns></returns>
        public IResult<IQueryable<DynamicContent>> GetContentByType(
            string contentType,
            ContentLifecycleStatus? status = null,
            Expression<Func<DynamicContent, bool>> filter = null
        ) => Do<IQueryable<DynamicContent>>.Try((r) =>
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                r.AddErrorAndLog(
                    _logger,
                    ERROR_CONTENT_TYPE_NULL_OR_EMPTY,
                    $"Content type passed into method {nameof(GetContentByType)} is null or empty."
                );
                return null;
            }

            var providerName = DefaultProviderName;

            DynamicModuleManager dynamicModuleManager = DynamicModuleManager.GetManager(providerName);

            if (dynamicModuleManager == null)
            {
                r.AddErrorAndLog(
                    _logger,
                    ERROR_FAILED_TO_GET_DYNAMIC_MODULE_MANAGER,
                    $"Attempt to get dynamic module manager with provider name {providerName} in method {nameof(GetContentByType)} returned null."
                );
                return null;
            }

            Type dynamicContentType = TypeResolutionService.ResolveType(contentType);

            if (dynamicContentType == null)
            {
                r.AddErrorAndLog(
                    _logger,
                    ERROR_COULD_NOT_RESOLVE_TYPE,
                    $"Attempt to resolve dynamic type with name of {contentType} in method {nameof(GetContentByType)} returned null."
                );
                return null;
            }

            var dynamicContent = dynamicModuleManager
                .GetDataItems(dynamicContentType)
                .Where(e => status.HasValue ? e.Status == status : true);

            if (filter != null)
            {
                dynamicContent = dynamicContent.Where(filter);
            }

            return dynamicContent;
        })
        .Result;

        #endregion Public Methods

    }
}
