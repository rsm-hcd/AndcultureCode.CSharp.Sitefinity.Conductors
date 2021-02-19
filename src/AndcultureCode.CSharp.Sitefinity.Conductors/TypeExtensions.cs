using System;

namespace AndcultureCode.CSharp.Sitefinity.Conductors
{
    public static class TypeExtensions
    {
        public static bool IsBoolean(this Type type) => type.FullName.Contains("System.Boolean");
        public static bool IsNullableBoolean(this Type type) => type.FullName.Contains("System.Nullable") && type.IsBoolean();
        public static bool IsDateTime(this Type type) => type.FullName.Contains("System.DateTime");
        public static bool IsNullableDateTime(this Type type) => type.FullName.Contains("System.Nullable") && type.IsDateTime();
        public static bool IsDecimal(this Type type) => type.FullName.Contains("System.Decimal");
        public static bool IsNullableDecimal(this Type type) => type.FullName.Contains("System.Nullable") && type.IsDecimal();
        public static bool IsString(this Type type) => type.FullName.Contains("System.String");
    }

}
