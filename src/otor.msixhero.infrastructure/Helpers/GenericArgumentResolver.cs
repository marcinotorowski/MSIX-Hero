using System;
using System.Collections.Generic;
using System.Linq;

namespace Otor.MsixHero.Infrastructure.Helpers
{
    public static class GenericArgumentResolver
    {
        public static Type GetResultType(Type type, Type genericType, int argumentPosition = 0)
        {
            if (type == null)
            {
                return null;
            }

            if (genericType.IsClass)
            {
                return GetResultTypeFromClass(type, genericType, argumentPosition);
            }
            
            if (genericType.IsInterface)
            {
                return GetResultTypeFromInterface(type, genericType, argumentPosition);
            }

            return null;
        }

        private static Type GetResultTypeFromInterface(Type type, Type genericType, int argumentPosition)
        {
            if (!genericType.IsInterface)
            {
                throw new ArgumentException($"The type must be an interface, but is {type.Name}.");
            }

            var toCheck = new HashSet<Type>();
            foreach (var item in type.GetInterfaces())
            {
                toCheck.Add(item);
            }

            while (toCheck.Any())
            {
                var item = toCheck.Last();
                if (item.IsGenericType)
                {
                    if (item.GetGenericTypeDefinition() == genericType)
                    {
                        return item.GetGenericArguments()[argumentPosition];
                    }
                }

                foreach (var inter in item.GetInterfaces())
                {
                    toCheck.Add(inter);
                }

                toCheck.Remove(item);
            }

            return null;
        }

        private static Type GetResultTypeFromClass(Type type, Type genericType, int argumentPosition)
        {
            if (!genericType.IsClass)
            {
                throw new ArgumentException($"The type must be a class but is {type.Name}.");
            }

            if (type == typeof(object))
            {
                return null;
            }

            while (true)
            {
                if (type.IsGenericType)
                {
                    if (type.GetGenericTypeDefinition() == genericType)
                    {
                        return type.GetGenericArguments()[argumentPosition];
                    }
                }

                type = type.BaseType;
                if (type == typeof(object) || type == null)
                {
                    break;
                }
            }

            return null;
        }
    }
}
