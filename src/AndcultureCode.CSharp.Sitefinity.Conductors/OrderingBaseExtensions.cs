using AndcultureCode.CSharp.Sitefinity.Core.Models.Content;
using System;
using System.Linq;
using System.Linq.Expressions;
using Telerik.Sitefinity.DynamicModules.Model;

namespace AndcultureCode.CSharp.Sitefinity.Conductors
{
    public static class OrderingBaseExtensions
    {
        public static IOrderedQueryable<DynamicContent> GetOrderedQueryable<TEntity>(
            this OrderingBase<TEntity> orderingBase,
            IQueryable<DynamicContent> queryable
        )
            where TEntity : SitefinityContent, new()
        {
            if (orderingBase.OrderByParameter == null)
            {
                throw new Exception("You must set the `OrderByParameter` property before attempting to get the ordered queryable");
            }
            var orderByFunc = orderingBase.OrderByParameter.GetOrderByFunc();
            var orderedQuery = orderByFunc(queryable);
            orderingBase.ThenByParameters.ForEach(thenByParameter =>
            {
                var thenByFunc = thenByParameter.GetThenByFunc();
                orderedQuery = thenByFunc(orderedQuery);
            });
            return orderedQuery;
        }

        public static OrderingBase<TEntity> OrderBy<TEntity, TProperty>(
            this OrderingBase<TEntity> orderingBase,
            Expression<Func<TEntity, TProperty>> expression
        )
            where TEntity : SitefinityContent, new()
        {
            return orderingBase.OrderByBase(OrderingDirection.Ascending, expression);
        }

        public static OrderingBase<TEntity> OrderByDescending<TEntity, TProperty>(
            this OrderingBase<TEntity> orderingBase,
            Expression<Func<TEntity, TProperty>> expression
        )
            where TEntity : SitefinityContent, new()
        {
            return orderingBase.OrderByBase(OrderingDirection.Descending, expression);
        }

        public static OrderingBase<TEntity> ThenBy<TEntity, TProperty>(
            this OrderingBase<TEntity> orderingBase,
            Expression<Func<TEntity, TProperty>> expression
        )
            where TEntity : SitefinityContent, new()
        {
            return orderingBase.ThenByBase(OrderingDirection.Ascending, expression);
        }

        public static OrderingBase<TEntity> ThenByDescending<TEntity, TProperty>(
            this OrderingBase<TEntity> orderingBase,
            Expression<Func<TEntity, TProperty>> expression
        )
            where TEntity : SitefinityContent, new()
        {
            return orderingBase.ThenByBase(OrderingDirection.Descending, expression);
        }

        private static Type GetPropertyType<TEntity>(
            this OrderingBase<TEntity> orderingBase,
            string propertyName
        )
            where TEntity : SitefinityContent, new()
        {
            Type type = orderingBase.Entity.GetType();
            PropertyInfo propertyInfo = type.GetProperty(propertyName);
            return propertyInfo.PropertyType;
        }

        private static OrderingBase<TEntity> OrderByBase<TEntity, TProperty>(
            this OrderingBase<TEntity> orderingBase,
            OrderingDirection orderDirection,
            Expression<Func<TEntity, TProperty>> expression
        )
            where TEntity : SitefinityContent, new()
        {
            if (orderingBase.ThenByParameters.Count > 0)
            {
                throw new Exception("You cannot set the `OrderByParameter` property after using `ThenBy()` or `ThenByDescending()`");
            }
            if (orderingBase.OrderByParameter != null)
            {
                throw new Exception("You cannot set the `OrderByParameter` property after it's already been set!");
            }
            var propertyName = OrderingUtils.ParseProperties(expression.Body.ToString());
            var orderingParameter = new OrderingParameter(orderDirection, propertyName, orderingBase.GetPropertyType(propertyName));
            orderingBase.OrderByParameter = orderingParameter;
            return orderingBase;
        }

        private static OrderingBase<TEntity> ThenByBase<TEntity, TProperty>(
            this OrderingBase<TEntity> orderingBase,
            OrderingDirection orderDirection,
            Expression<Func<TEntity, TProperty>> expression
        )
            where TEntity : SitefinityContent, new()
        {
            if (orderingBase.OrderByParameter == null)
            {
                throw new Exception("You must set the `OrderByParameter` property first by using `OrderBy()` or `OrderByDescending()` before using `ThenBy()` or `ThenByDescending()`");
            }
            var propertyName = OrderingUtils.ParseProperties(expression.Body.ToString());
            var orderingParameter = new OrderingParameter(orderDirection, propertyName, orderingBase.GetPropertyType(propertyName));
            orderingBase.ThenByParameters.Add(orderingParameter);
            return orderingBase;
        }
    }

}
