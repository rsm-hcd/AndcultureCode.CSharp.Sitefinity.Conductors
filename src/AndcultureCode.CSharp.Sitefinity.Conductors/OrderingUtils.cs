using System;

namespace AndcultureCode.CSharp.Sitefinity.Conductors
{
    public static class OrderingUtils
    {
        /// <summary>
        /// Parses properties from an expression body that has been converted to a string.
        ///
        /// Ex: "login.Users.Roles" gets parsed to "Users.Roles"
        /// 
        /// NOTE: The ordering capabilities of this are currently limited in that you cannot order by related object properties.  See
        /// the sitefinity URL https://knowledgebase.progress.com/articles/Article/How-to-query-dynamic-content-ordering-by-a-custom-field
        /// for more information on what is supported
        /// </summary>
        /// <param name="expressionBody">The body of the expression, as a string, indicating the property we want to order by.</param>
        /// <returns>The property we want to include, as a string.</returns>
        public static string ParseProperties(string expressionBody)
        {
            var firstDot = expressionBody.IndexOf(".") + 1;

            var orderingProperty = expressionBody.Substring(firstDot, expressionBody.Length - firstDot);

            if (orderingProperty.Contains("."))
            {
                throw new Exception("The ordering capabilities of this are currently limited in that you cannot order by related object properties.");
            }

            return orderingProperty;
        }
    }

}
