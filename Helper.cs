using System.Reflection;

namespace Queryable
{
    public static class Helper
    {
        public static Type GetElementType(Type type)
        {
            Type enumType = GetIEnumerableType(type);

            if (enumType == null)
                return type;

            return enumType.GetGenericArguments()[0];
        }

        private static Type GetIEnumerableType(Type type)
        {
            if (type == null || type == typeof(string))
                return null;

            if (type.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(type.GetElementType());

            if (type.IsGenericType)
            {
                foreach (Type arg in type.GetGenericArguments())
                {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);

                    if (ienum.IsAssignableFrom(type))
                        return ienum;
                }
            }

            Type[] ifaces = type.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    Type ienum = GetIEnumerableType(iface);

                    if (ienum != null)
                        return ienum;
                }
            }

            if (type.BaseType != null && type.BaseType != typeof(object))
                return GetIEnumerableType(type.BaseType);

            return null;
        }

        public static Type GetType(string typeName)
        {
            var result = Type.GetType(typeName);

            if (result != null)
                return result;

            return GetType(t => t.Name == typeName || t.FullName == typeName || t.AssemblyQualifiedName == typeName);
        }

        public static Type GetType(Func<Type, bool> predicate)
        {
            if (predicate == null)
                return null;

            var result = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => predicate.Invoke(t));

            if (result != null)
                return result;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                result = assembly.GetTypes().FirstOrDefault(t => predicate.Invoke(t));

                if (result != null)
                    return result;
            }

            return result;
        }
    }
}