using System;
using System.Linq.Expressions;

namespace Rhino.Security.Services
{
    internal class ExpressionHelper
    {
        public static Expression<Func<T, bool>> GetSecurityKeyExpression<T>(Guid entityGuid)
        {
            var objectType = typeof(T);

            var item = Expression.Parameter(typeof(T), "item");

            // Where(item => item)
            var property = Expression.Property(item, Security.GetSecurityKeyProperty(objectType));

            // Where(item => item.SomeProperty.Equals)
            var containsMethod = typeof(Guid).GetMethod("Equals", new[] {typeof(Guid)});

            // What we're searching for (e.g. SomeProperty.Contains("foo"))
            var searchExpression = Expression.Constant(entityGuid, typeof(Guid));

            // Call the "Contains" method for the "SomeProperty" with 
            // searchExpression as the constant to compare with
            var methodExpression = Expression.Call(property, containsMethod, searchExpression);

            // Create a lambda to use inside the where call
            var lambda = Expression.Lambda<Func<T, bool>>(methodExpression, item);

            return lambda;
        }

        public static Expression<Func<T, Guid>> GetSecurityKeyGetterExpression<T>()
        {
            var objectType = typeof(T);

            var item = Expression.Parameter(typeof(T), "item");

            // Where(item => item)
            var property = Expression.Property(item, Security.GetSecurityKeyProperty(objectType));

            var lambda = Expression.Lambda<Func<T, Guid>>(property, item);

            return lambda;
        }

    }
}