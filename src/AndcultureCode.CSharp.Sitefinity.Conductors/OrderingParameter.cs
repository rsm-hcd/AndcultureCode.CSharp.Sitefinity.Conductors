using System;
using System.Linq;
using System.Linq.Expressions;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Model;

namespace AndcultureCode.CSharp.Sitefinity.Conductors
{
    public class OrderingParameter
    {
        public OrderingDirection Direction { get; }
        public string PropertyName { get; }
        public Type PropertyType { get; }

        public OrderingParameter(OrderingDirection direction, string name, Type type)
        {
            Direction = direction;
            PropertyName = name;
            PropertyType = type;
        }

        private Expression<Func<DynamicContent, bool>> GetBooleanExpression => item => item.GetValue<bool>(PropertyName);
        private Expression<Func<DynamicContent, bool?>> GetBooleanNullExpression => item => item.GetValue<bool?>(PropertyName);
        private Expression<Func<DynamicContent, DateTime>> GetDateTimeExpression => item => item.GetValue<DateTime>(PropertyName);
        private Expression<Func<DynamicContent, DateTime?>> GetDateTimeNullExpression => item => item.GetValue<DateTime?>(PropertyName);
        private Expression<Func<DynamicContent, decimal>> GetDecimalExpression => item => item.GetValue<decimal>(PropertyName);
        private Expression<Func<DynamicContent, decimal?>> GetDecimalNullExpression => item => item.GetValue<decimal?>(PropertyName);
        private Expression<Func<DynamicContent, string>> GetStringExpression => item => item.GetValue<string>(PropertyName);

        private Func<IQueryable<DynamicContent>, IOrderedQueryable<DynamicContent>> GetOrderByDirection<TKey>(Expression<Func<DynamicContent, TKey>> expression)
        {
            if (Direction == OrderingDirection.Ascending)
            {
                return queryable => queryable.OrderBy(expression);
            }

            return queryable => queryable.OrderByDescending(expression);
        }

        public Func<IQueryable<DynamicContent>, IOrderedQueryable<DynamicContent>> GetOrderByFunc()
        {
            if (PropertyType.IsNullableBoolean())
            {
                return GetOrderByDirection(GetBooleanNullExpression);
            }
            if (PropertyType.IsBoolean())
            {
                return GetOrderByDirection(GetBooleanExpression);
            }
            if (PropertyType.IsNullableDateTime())
            {
                return GetOrderByDirection(GetDateTimeNullExpression);
            }
            if (PropertyType.IsDateTime())
            {
                return GetOrderByDirection(GetDateTimeExpression);
            }
            if (PropertyType.IsNullableDecimal())
            {
                return GetOrderByDirection(GetDecimalNullExpression);
            }
            if (PropertyType.IsDecimal())
            {
                return GetOrderByDirection(GetDecimalExpression);
            }
            if (PropertyType.IsString())
            {
                return GetOrderByDirection(GetStringExpression);
            }
            throw new Exception($"The property type for '{PropertyName}' of '{PropertyType.FullName}' is not supported by the OrderingBase class");
        }

        private Func<IOrderedQueryable<DynamicContent>, IOrderedQueryable<DynamicContent>> GetThenByDirection<TKey>(Expression<Func<DynamicContent, TKey>> expression)
        {
            if (Direction == OrderingDirection.Ascending)
            {
                return queryable => queryable.ThenBy(expression);
            }

            return queryable => queryable.ThenByDescending(expression);
        }

        public Func<IOrderedQueryable<DynamicContent>, IOrderedQueryable<DynamicContent>> GetThenByFunc()
        {
            if (PropertyType.IsNullableBoolean())
            {
                return GetThenByDirection(GetBooleanNullExpression);
            }
            if (PropertyType.IsBoolean())
            {
                return GetThenByDirection(GetBooleanExpression);
            }
            if (PropertyType.IsNullableDateTime())
            {
                return GetThenByDirection(GetDateTimeNullExpression);
            }
            if (PropertyType.IsDateTime())
            {
                return GetThenByDirection(GetDateTimeExpression);
            }
            if (PropertyType.IsNullableDecimal())
            {
                return GetThenByDirection(GetDecimalNullExpression);
            }
            if (PropertyType.IsDecimal())
            {
                return GetThenByDirection(GetDecimalExpression);
            }
            if (PropertyType.IsString())
            {
                return GetThenByDirection(GetStringExpression);
            }
            throw new Exception($"The property type for '{PropertyName}' of '{PropertyType.FullName}' is not supported by the OrderingBase class");
        }

    }

}
