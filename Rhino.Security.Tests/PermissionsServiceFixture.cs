using Rhino.Security.Model;
using Xunit;
using Xunit.Abstractions;

namespace Rhino.Security.Tests
{
    using System.Collections.Generic;

    public class PermissionsServiceFixture : DatabaseFixture
    {
        public PermissionsServiceFixture(ITestOutputHelper outputHelper) : base(outputHelper)
        {}

        [Fact]
        public void CanCreatePermission()
        {
            Permission permission = permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();
            session.Evict(permission);
            ICollection<Permission> all = session.CreateCriteria<Permission>().List<Permission>();

            Assert.Equal(1, all.Count);
        }

        [Fact]
        public void CanGetPermissionByUser()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(user);
            Assert.Single(permissions);
        }

        [Fact]
        public void CanGetPermissionsByUser_WhenDefinedOnGroup()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(user);
            Assert.Single(permissions);
        }

        [Fact]
        public void CanGetPermissionsByEntity()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(account);
            Assert.Single(permissions);
        }

        [Fact]
        public void CanGetPermissionsByEntity_WhenDefinedOnEntityGroup()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(account);
            Assert.Single(permissions);
        }

        [Fact]
        public void CanGetPermissionsByUserAndEntity()
        {
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(user, account);
            Assert.Single(permissions);
        }

        [Fact]
        public void CanGetPermissionsByUserAndOperationName()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(user, account);
            Assert.Single(permissions);
        }

        [Fact]
        public void CanGetPermissionsByUserAndOperationName_WhenParentOperationWasGranted()
        {
            permissionsBuilderService
                .Allow("/Account")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(user, account);
            Assert.Single(permissions);
        }

        [Fact]
        public void CanGetPermissionsByUserAndOperationName_WhenPermissionOnEverything()
        {
            permissionsBuilderService
                .Allow("/Account")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            session.Flush(); 

            Permission[] permissions = permissionService.GetGlobalPermissionsFor(user, "/Account/Edit");
            Assert.Single(permissions);
        }        

        [Fact]
        public void CanGetPermissionsByUserAndMultipleOperationNames_WhenPermissionOnEverything()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            permissionsBuilderService
                .Allow("/Account/Disable")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetGlobalPermissionsFor(user, new string[] { "/Account/Edit", "/Account/Disable" });
            Assert.Equal(2, permissions.Length);
        }

        [Fact]
        public void CanGetPermissionsByUserAndMultipleOperationNames_WhenPermissionOnASingleOperation()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetGlobalPermissionsFor(user, new string[] { "/Account/Edit", "/Account/Disable" });
            Assert.Single(permissions);
        }

        [Fact]
        public void CanGetPermissionByUserEntityAndOperation()
        {
            permissionsBuilderService
                .Allow("/Account")
                .For(user)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(user, account, "/Account/Edit");
            Assert.Single(permissions);
        }

        [Fact]
        public void CanGetPermissionByUserEntityAndMultipleOperations()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(user, account, new string[] { "/Account/Edit", "/Account/Disable" });
            Assert.Single(permissions);
        }

        [Fact]
        public void CanGetPermissionsByUserEntityAndMultipleOperations()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            permissionsBuilderService
                .Allow("/Account/Disable")
                .For(user)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(user, account, new string[] { "/Account/Edit", "/Account/Disable" });
            Assert.Equal(2, permissions.Length);
        }

        [Fact]
        public void CanGetPermissionByOperation()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor("/Account/Edit");
            Assert.Single(permissions);
        }

        [Fact]
        public void CanGetPermissionByMultipleOperations()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(new string[] { "/Account/Edit", "/Account/Disable" });
            Assert.Single(permissions);
        }

        [Fact]
        public void CanGetPermissionsByMultipleOperations()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            permissionsBuilderService
                .Allow("/Account/Disable")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(new string[] { "/Account/Edit", "/Account/Disable" });
            Assert.Equal(2, permissions.Length);
        }

        [Fact]
        public void CanGetPermissionByOperation_WhenParentOperationWasGranted()
        {
            permissionsBuilderService
                .Allow("/Account")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor("/Account/Edit");
            Assert.Single(permissions);
        }

        [Fact]
        public void CanGetPermissionByMultipleOperations_WhenParentOperationWasGranted()
        {
            permissionsBuilderService
                .Allow("/Account")
                .For(user)
                .On(account)
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(new string[] { "/Account/Edit", "/Account/Disable" });
            Assert.Single(permissions);
        }

        [Fact]
        public void PermissionsAreOrderedByLevelAndThenByDenyOrAllow()
        {
            permissionsBuilderService
                .Allow("/Account")
                .For(user)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(user)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On("Important Accounts")
                .Level(20)
                .Save();
            permissionsBuilderService
                .Deny("/Account/Edit")
                .For(user)
                .On("Important Accounts")
                .Level(20)
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(user);
            Assert.Equal(4, permissions.Length);

            Assert.Equal(20, permissions[0].Level);
            Assert.False(permissions[0].Allow);

            Assert.Equal(20, permissions[1].Level);
            Assert.True(permissions[1].Allow);

            Assert.Equal(1, permissions[2].Level);
            Assert.False(permissions[2].Allow);

            Assert.Equal(1, permissions[3].Level);
            Assert.True(permissions[3].Allow);
        }

        [Fact]
        public void CanSetPermissionOnEverythingAndGetItOnEntity()
        {
            permissionsBuilderService
                .Allow("/Account")
                .For(user)
                .OnEverything()
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(user, account, "/Account/Edit");
            Assert.Single(permissions);
        }

        [Fact]
        public void CanGetPermissionsSetOnarentGroupUserIsAssociatedWith()
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
            

            Permission[] permissions = permissionService.GetPermissionsFor(user, account, "/Account/Edit");
            Assert.Single(permissions);
        }

        [Fact]
        public void CanRemovePermission()
        {
            Permission permission = permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            session.Flush();

            Permission[] permissions = permissionService.GetPermissionsFor(user, account, "/Account/Edit");
            Assert.Single(permissions);

            authorizationRepository.RemovePermission(permission);
            

            permissions = permissionService.GetPermissionsFor(user, account, "/Account/Edit");
            Assert.Empty(permissions);
        }
    }
}
