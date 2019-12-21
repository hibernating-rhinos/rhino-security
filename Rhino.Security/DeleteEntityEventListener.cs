using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Event;
using Rhino.Security.Model;

namespace Rhino.Security
{
	/// <summary>
	/// Litenens for when a secured entity is deleted from the system and deletes 
	/// associated security data.
	/// </summary>
	[Serializable]
	public class DeleteEntityEventListener : IPreDeleteEventListener
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<bool> OnPreDeleteAsync(PreDeleteEvent @event, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
		/// Handles PreDelete event to delete an entity's associated security data.
		/// </summary>
		/// <param name="deleteEvent">Event object containing the delete operation information.</param>
		/// <returns>False, indicating the delete operation should not be vetoed.</returns>
		public bool OnPreDelete(PreDeleteEvent deleteEvent)
		{
			Guid securityKey = Security.ExtractKey(deleteEvent.Entity);

			if (!Guid.Empty.Equals(securityKey))
			{
				var entityReference = deleteEvent.Session.CreateCriteria<EntityReference>()
					.Add(Restrictions.Eq("EntitySecurityKey", securityKey))
					.SetCacheable(true)
					.UniqueResult<EntityReference>();

				if (entityReference != null)
                {
                    using (ISession childSession = deleteEvent.Session.SessionWithOptions()
                        .Connection()
                        .OpenSession())
                    {
                        // because default flush mode is auto, a read after a scheduled delete will invoke
                        // the auto-flush behaviour, causing a constraint violation exception in the 
                        // underlying database, because there still are EntityGroup entities that need
                        // the deleted EntityReference/SecurityKey.
                        childSession.FlushMode = FlushMode.Commit;

                        childSession.Delete(entityReference);

                        //Also remove EntityReferencesToEntitiesGroups and Permissions that reference this entity

                        //Get list of EntitiesGroups that have the entity as a member
                        IEnumerable<EntitiesGroup> entitiesGroups = childSession.CreateCriteria<EntitiesGroup>()
                            .CreateCriteria("Entities")
                            .Add(Restrictions.Eq("EntitySecurityKey", securityKey))
                            .SetCacheable(true)
                            .List<EntitiesGroup>();

                        foreach (EntitiesGroup group in entitiesGroups)
                        {
                            group.Entities.Remove(entityReference);
                        }

                        ////Get list of Permissions that references the entity
                        IEnumerable<Permission> permissions = childSession.CreateCriteria<Permission>()
                            .Add(Restrictions.Eq("EntitySecurityKey", securityKey))
                            .SetCacheable(true)
                            .List<Permission>();

                        foreach (Permission permission in permissions)
                        {
                            childSession.Delete(permission);
                        }

                        childSession.Flush();
                    }
                }
			}

			return false;
		}
	}
}