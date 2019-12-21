using System.Linq;
using Rhino.Security.Interfaces;
using Rhino.Security.Model;

namespace Rhino.Security.Linq
{
    /// <summary>
    /// Extension methods to add permissions to Linq queries
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Add permissions to the linq query
        /// </summary>
        /// <param name="query">The query</param>
        /// <param name="authorizationService">Service to use for authorization</param>
        /// <param name="user">User to add permissions for</param>
        /// <param name="operation">The operation as string</param>
        /// <typeparam name="T">The entity type</typeparam>
        /// <returns>A new query with permissions added</returns>
        public static IQueryable<T> AddPermissions<T>(
            this IQueryable<T> query,
            IAuthorizationService authorizationService, 
            IUser user,
            string operation)
        {
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(user, operation, query);
            return queryWithPermissions;
        }

        /// <summary>
        /// Add permissions to the linq query
        /// </summary>
        /// <param name="query">The query</param>
        /// <param name="authorizationService">Service to use for authorization</param>
        /// <param name="usersGroup">Usergroup to add permissions for</param>
        /// <param name="operation">The operation as string</param>
        /// <typeparam name="T">The entity type</typeparam>
        /// <returns>A new query with permissions added</returns>
        public static IQueryable<T> AddPermissions<T>(
            this IQueryable<T> query,
            IAuthorizationService authorizationService, 
            UsersGroup usersGroup,
            string operation)
        {
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(usersGroup, operation, query);
            return queryWithPermissions;
        }
    }
}