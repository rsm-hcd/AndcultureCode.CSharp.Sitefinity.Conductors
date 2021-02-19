using AndcultureCode.CSharp.Sitefinity.Core.Models.Content;
using System.Collections.Generic;

namespace AndcultureCode.CSharp.Sitefinity.Conductors
{
    /// <summary>
    /// Provides support for typed ordering using the Dynamic
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class OrderingBase<TEntity> where TEntity : SitefinityContent, new()
    {
        #region Properties

        public TEntity Entity { get; }
        public OrderingParameter OrderByParameter { get; set; }
        public List<OrderingParameter> ThenByParameters { get; set; }

        #endregion Properties

        public OrderingBase()
        {
            OrderByParameter = null;
            ThenByParameters = new List<OrderingParameter>();

            Entity = new TEntity();
        }
    }
}
