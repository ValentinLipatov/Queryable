using System;

namespace Queryable
{
    public static class Throw
    {
        public static void IfNull(object value, string name)
        {
            if (value == null)
                throw new ArgumentNullException(name);
        }

        public static void IfNotAssignable(Type type, Type assignableType, string name)
        {
            if (!type.IsAssignableFrom(assignableType))
                throw new ArgumentOutOfRangeException(name);
        }
    }
}