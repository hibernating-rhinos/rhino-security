using System;
using CommonServiceLocator;
using NHibernate;
using Rhino.Security.Interfaces;
using Xunit;
using Rhino.Security.Model;
using Xunit.Abstractions;


namespace Rhino.Security.Tests
{
    public class UsersGroupsNotReturningAllAssociatedUsers: DatabaseFixture
    {
        private readonly long idMarcus;
        private readonly long idAyende;


        public UsersGroupsNotReturningAllAssociatedUsers(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            authorizationService = ServiceLocator.Current.GetInstance<IAuthorizationService>();
            permissionService = ServiceLocator.Current.GetInstance<IPermissionsService>();
            permissionsBuilderService = ServiceLocator.Current.GetInstance<IPermissionsBuilderService>();
            authorizationRepository = ServiceLocator.Current.GetInstance<IAuthorizationRepository>();

            User ayende = new User { Name = "ayende" };
            session.Save(ayende);
            session.Flush();
            session.Evict(ayende);
            User marcus = new User { Name = "marcus" };
            session.Save(marcus);
            session.Flush();
            session.Evict(marcus);

            idAyende = ayende.Id;
            idMarcus = marcus.Id;

            User fromDb = session.Get<User>(idAyende);
            Assert.NotNull(fromDb);
            Assert.Equal(ayende.Name, fromDb.Name);
            fromDb = session.Get<User>(idMarcus);
            Assert.NotNull(fromDb);
            Assert.Equal(marcus.Name, fromDb.Name);

            UsersGroup group = authorizationRepository.CreateUsersGroup("Admin");
            authorizationRepository.AssociateUserWith(ayende, "Admin");
            authorizationRepository.AssociateUserWith(marcus, "Admin");
            session.Flush();
            session.Evict(group);

        }

        [Fact]
        public void GetUsersByUsersGroup()
        {
            authorizationService = ServiceLocator.Current.GetInstance<IAuthorizationService>();
            permissionService = ServiceLocator.Current.GetInstance<IPermissionsService>();
            permissionsBuilderService = ServiceLocator.Current.GetInstance<IPermissionsBuilderService>();
            authorizationRepository = ServiceLocator.Current.GetInstance<IAuthorizationRepository>();
            
            User marcus = session.Get<User>(Convert.ToInt64(idMarcus));
            UsersGroup[] marcusGroups = authorizationRepository.GetAssociatedUsersGroupFor(marcus);
            Assert.Single(marcusGroups);
            Assert.Equal(2, marcusGroups[0].Users.Count);

            User ayende = session.Get<User>(Convert.ToInt64(idAyende));
            UsersGroup[] ayendeGroups = authorizationRepository.GetAssociatedUsersGroupFor(ayende);
            Assert.Single(ayendeGroups);
            Assert.Equal(2, ayendeGroups[0].Users.Count);
        }
    }
}
