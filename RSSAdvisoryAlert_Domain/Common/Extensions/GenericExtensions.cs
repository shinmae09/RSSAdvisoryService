namespace RSSAdvisoryAlert.Domain.Common.Extensions
{
    public static class GenericExtensions
    {
        public static T ThrowIfNull<T>(this T obj, string? paramName = null) where T : class
        {
            if (obj == null)
            {
                throw new ArgumentNullException(paramName ?? nameof(obj), "Object cannot be null");
            }

            return obj;
        }
    }
}
