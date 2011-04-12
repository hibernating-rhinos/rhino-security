namespace Rhino.Security.Tests
{
    using System;
    using NHibernate;
    using NHibernate.Criterion;
    using Rhino.Security.Model;
    using Xunit;

    public class DeleteEntityEventListenerFixture_SqlServerProblem : DatabaseFixtureSqlServer
    {
        [Fact]
        public void DoesDeletingEntityRemoveEntityReferences()
        {
            var user = new User() { Name = "Alberto" };
            session.Save(user);
            session.Flush();

            var group = new UsersGroup() { Name = "Guests" };
            session.Save(group);
            session.Flush();

            authorizationRepository.AssociateUserWith(user, group);
            session.Flush();

            var deleteAccountOperation = authorizationRepository.CreateOperation("/Account/Delete");
            session.Flush();

            var californiaGroup = authorizationRepository.CreateEntitiesGroup("California");
            session.Flush();

            var account = new Account() { Name = "Bob" };
            session.Save(account);
            session.Flush();

            authorizationRepository.AssociateEntityWith<Account>(account, californiaGroup);
            session.Flush();

            permissionsBuilderService
                .Allow(deleteAccountOperation)
                .For(group)
                .On(californiaGroup)
                .Level(10)
                .Save();

            session.Flush();

            bool isAllowed = authorizationService.IsAllowed<Account>(user, account, deleteAccountOperation.Name);

            session.Delete(account);
            session.Flush();

            var accountFetch = session.CreateCriteria<Account>()
                .Add(Restrictions.Eq("Name", "Bob"))
                .UniqueResult<Account>();

            Assert.Null(accountFetch);
        }

    }
}
