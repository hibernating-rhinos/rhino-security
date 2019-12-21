using System.Linq;
using Rhino.Security.Linq;
using Rhino.Security.Model;
using Xunit;
using Xunit.Abstractions;

namespace Rhino.Security.Tests
{
    public class AuthorizationService_Queries_Linq_Fixture : DatabaseFixture
    {
        private IQueryable<Account> query;

        public AuthorizationService_Queries_Linq_Fixture(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            query = session.Query<Account>();
        }

        [Fact]
        public void WillReturnNothingIfNoPermissionHasBeenDefined()
        {
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(user, "/Account/Edit", query);
            Assert.Empty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnNothingForUsersGroupIfNoPermissionHasBeenDefined()
        {
            UsersGroup[] usersgroups = authorizationRepository.GetAssociatedUsersGroupFor(user);
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(usersgroups[0], "/Account/Edit", query);
            Assert.Empty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnNothingIfOperationNotDefined()
        {
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(user, "/Account/Delete", query);
            Assert.Empty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnNothingForUsersGroupIfOperationNotDefined()
        {
            UsersGroup[] usersgroups = authorizationRepository.GetAssociatedUsersGroupFor(user);
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(usersgroups[0], "/Account/Delete", query);
            Assert.Empty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnResultIfAllowPermissionWasDefined()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();

            session.Flush(); 
            
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(user, "/Account/Edit", query);
            Assert.NotEmpty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnResultForUsersGroupIfAllowPermissionWasDefined()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(usersgroup)
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", query);
            Assert.NotEmpty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnResultIfAllowPermissionWasDefinedOnEverything()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            session.Flush();
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(user, "/Account/Edit", query);
            Assert.NotEmpty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnResultForUsersGroupIfAllowPermissionWasDefinedOnEverything()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(usersgroup)
                .OnEverything()
                .DefaultLevel()
                .Save();
            session.Flush();
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", query);
            Assert.NotEmpty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnNothingIfAllowPermissionWasDefinedOnGroupAndDenyPermissionOnUser()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For("Administrators")
                .OnEverything()
                .DefaultLevel()
                .Save();
            session.Flush();
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(user, "/Account/Edit", query);
            Assert.Empty(queryWithPermissions.ToList());

        }

        [Fact]
        public void WillReturnResultForUsersGroupIfAllowPermissionWasDefinedOnGroupAndDenyPermissionOnUser()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .OnEverything()
                .DefaultLevel()
                .Save();
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            session.Flush();
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", query);
            Assert.NotEmpty(queryWithPermissions.ToList());

        }


        [Fact]
        public void WillReturnNothingIfAllowedPermissionWasDefinedWithDenyPermissionWithHigherLevel()
        {            
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For("Administrators")
                .OnEverything()
                .Level(5)
                .Save();
            session.Flush();
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(user, "/Account/Edit", query);
            Assert.Empty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnNothingForUsersGroupIfAllowedPermissionWasDefinedWithDenyPermissionWithHigherLevel()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(usersgroup)
                .OnEverything()
                .DefaultLevel()
                .Save();
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(usersgroup)
                .OnEverything()
                .Level(5)
                .Save();
            session.Flush();
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", query);
            Assert.Empty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnResultIfAllowedPermissionWasDefinedWithDenyPermissionWithLowerLevel()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .OnEverything()
                .Level(10)
                .Save();
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For("Administrators")
                .OnEverything()
                .Level(5)
                .Save();
            session.Flush();
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(user, "/Account/Edit", query);
            Assert.NotEmpty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnResultForUsersGroupIfAllowedPermissionWasDefinedWithDenyPermissionWithLowerLevel()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(usersgroup)
                .OnEverything()
                .Level(10)
                .Save();
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(usersgroup)
                .OnEverything()
                .Level(5)
                .Save();
            session.Flush();
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", query);
            Assert.NotEmpty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnResultOnAccountIfPermissionWasGrantedOnAnything()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();

            session.Flush();
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(user, "/Account/Edit", query);
            Assert.NotEmpty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnResultForUsersGroupOnAccountIfPermissionWasGrantedOnAnything()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(usersgroup)
                .OnEverything()
                .DefaultLevel()
                .Save();

            session.Flush();
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", query);
            Assert.NotEmpty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturNothingOnAccountIfPermissionWasDeniedOnAnything()
        {
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            session.Flush();
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(user, "/Account/Edit", query);
            Assert.Empty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturNothingForUsersGroupOnAccountIfPermissionWasDeniedOnAnything()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(usersgroup)
                .OnEverything()
                .DefaultLevel()
                .Save();

            session.Flush();
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", query);
            Assert.Empty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnResultOnAccountIfPermissionWasGrantedOnGroupAssociatedWithUser()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .On(account)
                .DefaultLevel()
                .Save();

            session.Flush();
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(user, "/Account/Edit", query);
            Assert.NotEmpty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnResultForUsersGroupOnAccountIfPermissionWasGrantedOnGroup()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(usersgroup)
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            var queryWithPermissions = authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", query);
            Assert.NotEmpty(queryWithPermissions.ToList());
        }


        [Fact]
        public void WillReturnNothingOnAccountIfPermissionWasDeniedOnGroupAssociatedWithUser()
        {
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For("Administrators")
                .On(account)
                .DefaultLevel()
                .Save();

            session.Flush();
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(user, "/Account/Edit", query);
            Assert.Empty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnNothingForUsersGroupoOnAccountIfPermissionWasDeniedOnGroup()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(usersgroup)
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            var queryWithPermissions = authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", query);
            Assert.Empty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnResultOnAccountIfPermissionWasGrantedToUser()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            var queryWithPermissions = authorizationService.AddPermissionsToQuery(user, "/Account/Edit", query);
            Assert.NotEmpty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnNothingOnAccountIfPermissionWasDeniedToUser()
        {
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            var queryWithPermissions = authorizationService.AddPermissionsToQuery(user, "/Account/Edit", query);
            Assert.Empty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnResultOnEntityGroupIfPermissionWasGrantedToUsersGroupAssociatedWithUser()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            session.Flush();

            var queryWithPermissions = authorizationService.AddPermissionsToQuery(user, "/Account/Edit", query);
            Assert.NotEmpty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnResultForUsersGroupOnEntityGroupIfPermissionWasGrantedToUsersGroup()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(usersgroup)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            session.Flush();

            var queryWithPermissions = authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", query);
            Assert.NotEmpty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnNothingOnAccountIfPermissionWasDeniedToUserOnTheGroupTheEntityIsAssociatedWith()
        {
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(user)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            session.Flush();

            var queryWithPermissions = authorizationService.AddPermissionsToQuery(user, "/Account/Edit", query);
            Assert.Empty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnNothingForUsersGroupOnAccountIfPermissionWasDeniedToUserOnTheGroupTheEntityIsAssociatedWith()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(usersgroup)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();

            session.Flush();
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", query);
            Assert.Empty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnResultOnAccountIfPermissionWasAllowedToUserOnTheGroupTheEntityIsAssociatedWith()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();

            session.Flush();
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(user, "/Account/Edit", query);
            Assert.NotEmpty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnResultForUsersGroupOnAccountIfPermissionWasAllowedToUserOnTheGroupTheEntityIsAssociatedWith()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(usersgroup)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();

            session.Flush();
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", query);
            Assert.NotEmpty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnNothingIfPermissionWasAllowedToChildGroupUserIsAssociatedWith()
        {
            authorizationRepository.CreateChildUserGroupOf("Administrators", "Helpdesk");
            

            permissionsBuilderService
               .Allow("/Account/Edit")
               .For("Helpdesk")
               .On("Important Accounts")
               .DefaultLevel()
               .Save();

            session.Flush();
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(user, "/Account/Edit", query);
            Assert.Empty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnNothingForUsersGroupIfPermissionWasAllowedToChildGroupOfVerifiedUsersGroup()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");
            authorizationRepository.CreateChildUserGroupOf("Administrators", "Helpdesk");
            

            permissionsBuilderService
               .Allow("/Account/Edit")
               .For("Helpdesk")
               .On("Important Accounts")
               .DefaultLevel()
               .Save();

            session.Flush();
            var queryWithPermissions = authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", query);
            Assert.Empty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnResultIfPermissionWasAllowedToParentGroupUserIsAssociatedWith()
        {
            authorizationRepository.CreateChildUserGroupOf("Administrators", "Helpdesk");
            

            authorizationRepository.DetachUserFromGroup(user, "Administrators");
            authorizationRepository.AssociateUserWith(user, "Helpdesk");
            

            permissionsBuilderService
               .Allow("/Account/Edit")
               .For("Administrators")
               .On("Important Accounts")
               .DefaultLevel()
               .Save();
            session.Flush();

            var queryWithPermissions = authorizationService.AddPermissionsToQuery(user, "/Account/Edit", query);
            Assert.NotEmpty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnNothingForUsersGroupIfPermissionWasAllowedToParentGroupOfVerifiedUsersGroup()
        {            
            authorizationRepository.CreateChildUserGroupOf("Administrators", "Helpdesk");
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Helpdesk");
            
            permissionsBuilderService
               .Allow("/Account/Edit")
               .For("Administrators")
               .On("Important Accounts")
               .DefaultLevel()
               .Save();
            session.Flush();

            var queryWithPermissions = authorizationService.AddPermissionsToQuery(usersgroup, "/Account/Edit", query);
            Assert.Empty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnNothingOnUserWhenNoPermissionsAreDefinedUsingExtensionMethod()
        {
            var queryWithPermissions = query
                .AddPermissions(authorizationService, user, "/Account/Edit");

            Assert.Empty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnNothingOnUserGroupWhenNoPermissionsAreDefinedUsingExtensionMethod()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");

            var queryWithPermissions = query
                .AddPermissions(authorizationService, usersgroup, "/Account/Edit");

            Assert.Empty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnResultForUserWhenPermissionsAreDefinedUsingExtensionMethod()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();

            session.Flush();

            var queryWithPermissions = query
                .AddPermissions(authorizationService, user, "/Account/Edit");

            Assert.NotEmpty(queryWithPermissions.ToList());
        }

        [Fact]
        public void WillReturnResultForUserGroupWhenPermissionsAreDefinedUsingExtensionMethod()
        {
            UsersGroup usersgroup = authorizationRepository.GetUsersGroupByName("Administrators");

            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(usersgroup)
                .On(account)
                .DefaultLevel()
                .Save();

            session.Flush();

            var queryWithPermissions = query
                .AddPermissions(authorizationService, usersgroup, "/Account/Edit");

            Assert.NotEmpty(queryWithPermissions.ToList());
        }
    }
}
