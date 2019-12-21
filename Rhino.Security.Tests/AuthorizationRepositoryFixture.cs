using System;
using NHibernate.Exceptions;
using Rhino.Security.Model;
using Xunit;
using Xunit.Abstractions;

namespace Rhino.Security.Tests
{
	public class AuthorizationRepositoryFixture : DatabaseFixture
	{
        public AuthorizationRepositoryFixture(ITestOutputHelper outputHelper) : base(outputHelper)
        {}

		[Fact]
		public void CanSaveUser()
		{
			var ayende = new User {Name = "ayende"};
			session.Save(ayende);
			session.Flush();
			session.Evict(ayende);

			var fromDb = session.Get<User>(ayende.Id);
			Assert.NotNull(fromDb);
			Assert.Equal(ayende.Name, fromDb.Name);
		}

		[Fact]
		public void CanSaveAccount()
		{
			var ayende = new Account {Name = "ayende"};
			Assert.NotEqual(Guid.Empty, ayende.SecurityKey);
			session.Save(ayende);
			session.Flush();
			session.Evict(ayende);

			var fromDb = session.Get<Account>(ayende.Id);
			Assert.NotNull(fromDb);
			Assert.Equal(ayende.Name, fromDb.Name);
			Assert.Equal(fromDb.SecurityKey, ayende.SecurityKey);
		}

		[Fact]
		public void CanCreateUsersGroup()
		{
			var group = authorizationRepository.CreateUsersGroup("Admininstrators");


			session.Flush();
			session.Evict(group);

			var groupFromDb = session.Get<UsersGroup>(group.Id);
			Assert.NotNull(groupFromDb);
			Assert.Equal(group.Name, groupFromDb.Name);
		}

		[Fact]
		public void CanCreateEntitesGroup()
		{
			var group = authorizationRepository.CreateEntitiesGroup("Accounts");


			session.Flush();
			session.Evict(group);

			var groupFromDb = session.Get<EntitiesGroup>(group.Id);
			Assert.NotNull(groupFromDb);
			Assert.Equal(group.Name, groupFromDb.Name);
		}

		[Fact]
		public void CannotCreateEntitiesGroupWithSameName()
		{
			authorizationRepository.CreateEntitiesGroup("Admininstrators");
			session.Flush();

			var exception = Assert.Throws<GenericADOException>(() =>
				{
					authorizationRepository.CreateEntitiesGroup("Admininstrators");
					session.Flush();
				}).InnerException;
			Assert.Contains("unique", exception.Message, StringComparison.InvariantCultureIgnoreCase);
		}

		[Fact]
		public void CannotCreateUsersGroupsWithSameName()
		{
			authorizationRepository.CreateUsersGroup("Admininstrators");
			session.Flush();

			var exception = Assert.Throws<GenericADOException>(() =>
				{
					authorizationRepository.CreateUsersGroup("Admininstrators");
					session.Flush();
				}).InnerException;

			Assert.Contains("unique", exception.Message, StringComparison.InvariantCultureIgnoreCase);
		}

		[Fact]
		public void CanGetUsersGroupByName()
		{
			var group = authorizationRepository.CreateUsersGroup("Admininstrators");

			session.Flush();
			session.Evict(group);

			group = authorizationRepository.GetUsersGroupByName("Admininstrators");
			Assert.NotNull(group);
		}

		[Fact]
		public void CanGetEntitiesGroupByName()
		{
			var group = authorizationRepository.CreateEntitiesGroup("Accounts");


			session.Flush();
			session.Evict(group);

			group = authorizationRepository.GetEntitiesGroupByName("Accounts");
			Assert.NotNull(group);
		}

		[Fact]
		public void CanChangeUsersGroupName()
		{
			var group = authorizationRepository.CreateUsersGroup("Admininstrators");

			session.Flush();
			session.Evict(group);

			authorizationRepository.RenameUsersGroup("Admininstrators", "2");


			session.Flush();
			session.Evict(group);

			group = authorizationRepository.GetUsersGroupByName("2");
			Assert.NotNull(group);
			group = authorizationRepository.GetUsersGroupByName("Admininstrators");
			Assert.Null(group);
		}

		[Fact]
		public void CannotRenameUsersGroupToAnAlreadyExistingUsersGroup()
		{
			var group = authorizationRepository.CreateUsersGroup("Admininstrators");
			var group2 = authorizationRepository.CreateUsersGroup("ExistingGroup");

			session.Flush();

			session.Evict(group);
			session.Evict(group2);

			var exception = Assert.Throws<GenericADOException>(
				() =>
					{
						authorizationRepository.RenameUsersGroup("Admininstrators", "ExistingGroup");
						session.Flush();
					}).InnerException;
			Assert.Contains("unique", exception.Message, StringComparison.InvariantCultureIgnoreCase);
		}

		[Fact]
		public void CanChangeEntitiesGroupName()
		{
			var group = authorizationRepository.CreateEntitiesGroup("Accounts");

			session.Flush();
			session.Evict(group);

			authorizationRepository.RenameEntitiesGroup("Accounts", "2");


			session.Evict(group);

			group = authorizationRepository.GetEntitiesGroupByName("2");
			Assert.NotNull(group);
			group = authorizationRepository.GetEntitiesGroupByName("Accounts");
			Assert.Null(group);
		}

		[Fact]
		public void CannotRenameEntitiesGroupToAnAlreadyExistingEntitiesGroup()
		{
			var group = authorizationRepository.CreateEntitiesGroup("Accounts");
			var group2 = authorizationRepository.CreateEntitiesGroup("ExistingGroup");


			session.Flush();
			session.Evict(group);
			session.Evict(group2);

			var exception = Assert.Throws<GenericADOException>(
				() =>
					{
						authorizationRepository.RenameEntitiesGroup("Accounts", "ExistingGroup");
						session.Flush();
					}).InnerException;

			Assert.Contains("unique", exception.Message, StringComparison.InvariantCultureIgnoreCase);
		}

		[Fact]
		public void CannotRenameUsersGroupThatDoesNotExist()
		{
			Assert.Throws<InvalidOperationException>(() =>
			                                         authorizationRepository.RenameUsersGroup("NonExistingGroup",
			                                                                                  "Administrators"));
		}

		[Fact]
		public void CannotRenameEntitiesGroupThatDoesNotExist()
		{
			Assert.Throws<InvalidOperationException>(() =>
			                                         authorizationRepository.RenameEntitiesGroup("NonExistingGroup",
			                                                                                     "Accounts"));
		}


		[Fact]
		public void CanAssociateUserWithGroup()
		{
			var ayende = new User {Name = "ayende"};

			session.Save(ayende);
			var group = authorizationRepository.CreateUsersGroup("Admins");


			authorizationRepository.AssociateUserWith(ayende, "Admins");

			session.Flush();
			session.Evict(ayende);
			session.Evict(group);

			var groups = authorizationRepository.GetAssociatedUsersGroupFor(ayende);
			Assert.Single(groups);
			Assert.Equal("Admins", groups[0].Name);
		}

		[Fact]
		public void CanAssociateAccountWithMultipleGroups()
		{
			var ayende = new Account();
			ayende.Name = "ayende";

			session.Save(ayende);
			var group = authorizationRepository.CreateEntitiesGroup("Accounts");
			var group2 = authorizationRepository.CreateEntitiesGroup("Accounts of second group");


			authorizationRepository.AssociateEntityWith(ayende, "Accounts");

			authorizationRepository.AssociateEntityWith(ayende, "Accounts of second group");

			session.Flush();

			session.Evict(ayende);
			session.Evict(group);
			session.Evict(group2);

			var groups = authorizationRepository.GetAssociatedEntitiesGroupsFor(ayende);
			Assert.Equal(2, groups.Length);
			Assert.Equal("Accounts", groups[0].Name);
			Assert.Equal("Accounts of second group", groups[1].Name);
		}

		[Fact]
		public void CanAssociateUserWithNestedGroup()
		{
			var ayende = new User();
			ayende.Name = "ayende";

			session.Save(ayende);
			authorizationRepository.CreateUsersGroup("Admins");

			var group = authorizationRepository.CreateChildUserGroupOf("Admins", "DBA");


			authorizationRepository.AssociateUserWith(ayende, "DBA");

			session.Flush();
			session.Evict(ayende);
			session.Evict(group);

			var groups = authorizationRepository.GetAssociatedUsersGroupFor(ayende);
			Assert.Equal(2, groups.Length);
			Assert.Equal("Admins", groups[0].Name);
			Assert.Equal("DBA", groups[1].Name);
		}

		[Fact]
		public void CanAssociateAccountWithNestedGroup()
		{
			var beto = new Account();
			beto.Name = "beto account";

			session.Save(beto);
			authorizationRepository.CreateEntitiesGroup("Executive Accounts");

			var group = authorizationRepository.CreateChildEntityGroupOf("Executive Accounts", "Manager Accounts");

			authorizationRepository.AssociateEntityWith(beto, "Manager Accounts");

			session.Flush();
			session.Evict(beto);
			session.Evict(group);

			var groups = authorizationRepository.GetAssociatedEntitiesGroupsFor(beto);
			Assert.Equal(2, groups.Length);
			Assert.Equal("Executive Accounts", groups[0].Name);
			Assert.Equal("Manager Accounts", groups[1].Name);
		}


		[Fact]
		public void CanGetAncestryAssociationOfUserWithGroupWithNested()
		{
			var ayende = new User();
			ayende.Name = "ayende";

			session.Save(ayende);
			authorizationRepository.CreateUsersGroup("Admins");

			authorizationRepository.CreateChildUserGroupOf("Admins", "DBA");


			authorizationRepository.AssociateUserWith(ayende, "DBA");


			var groups = authorizationRepository.GetAncestryAssociation(ayende, "Admins");
			Assert.Equal(2, groups.Length);
			Assert.Equal("DBA", groups[0].Name);
			Assert.Equal("Admins", groups[1].Name);
		}

		[Fact]
		public void CanGetAncestryAssociationOfAccountWithGroupWithNested()
		{
			var beto = new Account();
			beto.Name = "beto account";

			session.Save(beto);
			authorizationRepository.CreateEntitiesGroup("Executive Accounts");

			authorizationRepository.CreateChildEntityGroupOf("Executive Accounts", "Manager Accounts");

			authorizationRepository.AssociateEntityWith(beto, "Manager Accounts");

			var groups = authorizationRepository.GetAncestryAssociationOfEntity(beto,
			                                                                    "Executive Accounts");
			Assert.Equal(2, groups.Length);
			Assert.Equal("Manager Accounts", groups[0].Name);
			Assert.Equal("Executive Accounts", groups[1].Name);
		}

		[Fact]
		public void CanGetAncestryAssociationOfUserWithGroupDirect()
		{
			var ayende = new User();
			ayende.Name = "ayende";

			session.Save(ayende);
			authorizationRepository.CreateUsersGroup("Admins");


			authorizationRepository.AssociateUserWith(ayende, "Admins");


			var groups = authorizationRepository.GetAncestryAssociation(ayende, "Admins");
			Assert.Single(groups);
			Assert.Equal("Admins", groups[0].Name);
		}

		[Fact]
		public void CanGetAncestryAssociationOfAccountWithGroupDirect()
		{
			var beto = new Account();
			beto.Name = "beto account";

			session.Save(beto);
			authorizationRepository.CreateEntitiesGroup("Executive Accounts");

			authorizationRepository.AssociateEntityWith(beto, "Executive Accounts");

			var groups = authorizationRepository.GetAncestryAssociationOfEntity(beto, "Executive Accounts");
			Assert.Single(groups);
			Assert.Equal("Executive Accounts", groups[0].Name);
		}

		[Fact]
		public void CanGetAncestryAssociationOfUserWithGroupWhereNonExists()
		{
			var ayende = new User();
			ayende.Name = "ayende";

			session.Save(ayende);
			authorizationRepository.CreateUsersGroup("Admins");


			var groups = authorizationRepository.GetAncestryAssociation(ayende, "Admins");
			Assert.Empty(groups);
		}

		[Fact]
		public void CanGetAncestryAssociationOfEntityWithGroupWhereNonExists()
		{
			var beto = new Account();
			beto.Name = "beto account";

			session.Save(beto);
			authorizationRepository.CreateEntitiesGroup("Executive Accounts");

			var groups = authorizationRepository.GetAncestryAssociationOfEntity(beto, "Executive Accounts");
			Assert.Empty(groups);
		}

		[Fact]
		public void CanGetAncestryAssociationOfUserWithGroupWhereThereIsDirectPathShouldSelectThat()
		{
			var ayende = new User();
			ayende.Name = "ayende";

			session.Save(ayende);
			authorizationRepository.CreateUsersGroup("Admins");


			authorizationRepository.CreateChildUserGroupOf("Admins", "DBA");

			authorizationRepository.AssociateUserWith(ayende, "Admins");
			authorizationRepository.AssociateUserWith(ayende, "DBA");


			var groups = authorizationRepository.GetAncestryAssociation(ayende, "Admins");
			Assert.Single(groups);
			Assert.Equal("Admins", groups[0].Name);
		}

		[Fact]
		public void CanGetAncestryAssociationOfAccountWithGroupWhereThereIsDirectPathShouldSelectThat()
		{
			var beto = new Account();
			beto.Name = "beto account";

			session.Save(beto);
			authorizationRepository.CreateEntitiesGroup("Executive Accounts");

			authorizationRepository.CreateChildEntityGroupOf("Executive Accounts", "Manager Accounts");

			authorizationRepository.AssociateEntityWith(beto, "Executive Accounts");
			authorizationRepository.AssociateEntityWith(beto, "Manager Accounts");

			var groups = authorizationRepository.GetAncestryAssociationOfEntity(beto, "Executive Accounts");
			Assert.Single(groups);
			Assert.Equal("Executive Accounts", groups[0].Name);
		}

		[Fact]
		public void CanGetAncestryAssociationOfUserWithGroupWhereThereIsTwoLevelNesting()
		{
			var ayende = new User();
			ayende.Name = "ayende";

			session.Save(ayende);
			authorizationRepository.CreateUsersGroup("Admins");


			authorizationRepository.CreateChildUserGroupOf("Admins", "DBA");

			authorizationRepository.CreateChildUserGroupOf("DBA", "SQLite DBA");

			authorizationRepository.AssociateUserWith(ayende, "SQLite DBA");


			var groups = authorizationRepository.GetAncestryAssociation(ayende, "Admins");
			Assert.Equal(3, groups.Length);
			Assert.Equal("SQLite DBA", groups[0].Name);
			Assert.Equal("DBA", groups[1].Name);
			Assert.Equal("Admins", groups[2].Name);
		}

		[Fact]
		public void CanGetAncestryAssociationOfAccountWithGroupWhereThereIsTwoLevelNesting()
		{
			var beto = new Account();
			beto.Name = "beto account";

			session.Save(beto);
			authorizationRepository.CreateEntitiesGroup("Executive Accounts");

			authorizationRepository.CreateChildEntityGroupOf("Executive Accounts", "Manager Accounts");

			authorizationRepository.CreateChildEntityGroupOf("Manager Accounts", "Employee Accounts");

			authorizationRepository.AssociateEntityWith(beto, "Employee Accounts");

			var groups = authorizationRepository.GetAncestryAssociationOfEntity(beto, "Executive Accounts");
			Assert.Equal(3, groups.Length);
			Assert.Equal("Employee Accounts", groups[0].Name);
			Assert.Equal("Manager Accounts", groups[1].Name);
			Assert.Equal("Executive Accounts", groups[2].Name);
		}

		[Fact]
		public void CanGetAncestryAssociationOfUserWithGroupWhereThereIsMoreThanOneIndirectPathShouldSelectShortest()
		{
			var ayende = new User();
			ayende.Name = "ayende";

			session.Save(ayende);
			authorizationRepository.CreateUsersGroup("Admins");


			authorizationRepository.CreateChildUserGroupOf("Admins", "DBA");

			authorizationRepository.CreateChildUserGroupOf("DBA", "SQLite DBA");

			authorizationRepository.AssociateUserWith(ayende, "DBA");
			authorizationRepository.AssociateUserWith(ayende, "SQLite DBA");


			var groups = authorizationRepository.GetAncestryAssociation(ayende, "Admins");
			Assert.Equal(2, groups.Length);
			Assert.Equal("DBA", groups[0].Name);
			Assert.Equal("Admins", groups[1].Name);
		}

		[Fact]
		public void CanGetAncestryAssociationOfAccountWithGroupWhereThereIsMoreThanOneIndirectPathShouldSelectShortest()
		{
			var beto = new Account();
			beto.Name = "beto account";

			session.Save(beto);
			authorizationRepository.CreateEntitiesGroup("Executive Accounts");

			authorizationRepository.CreateChildEntityGroupOf("Executive Accounts", "Manager Accounts");

			authorizationRepository.CreateChildEntityGroupOf("Manager Accounts", "Employee Accounts");

			authorizationRepository.AssociateEntityWith(account, "Manager Accounts");
			authorizationRepository.AssociateEntityWith(account, "Employee Accounts");

			var groups = authorizationRepository.GetAncestryAssociationOfEntity(account,
			                                                                    "Executive Accounts");
			Assert.Equal(2, groups.Length);
			Assert.Equal("Manager Accounts", groups[0].Name);
			Assert.Equal("Executive Accounts", groups[1].Name);
		}

		[Fact]
		public void CanAssociateAccountWithGroup()
		{
			var ayende = new Account();
			ayende.Name = "ayende";

			session.Save(ayende);
			var group = authorizationRepository.CreateEntitiesGroup("Accounts");


			authorizationRepository.AssociateEntityWith(ayende, "Accounts");


			session.Flush();
			session.Evict(ayende);
			session.Evict(group);

			var groups = authorizationRepository.GetAssociatedEntitiesGroupsFor(ayende);
			Assert.Single(groups);
			Assert.Equal("Accounts", groups[0].Name);
		}

		[Fact]
		public void CanCreateOperation()
		{
			authorizationRepository.CreateOperation("/Account/Delete");

			var operation = authorizationRepository.GetOperationByName("/Account/Delete");
			Assert.NotNull(operation);
		}

		[Fact]
		public void WhenCreatingNestedOperation_WillCreateParentOperation_IfDoesNotExists()
		{
			var operation = authorizationRepository.CreateOperation("/Account/Delete");

			var parentOperation = authorizationRepository.GetOperationByName("/Account");
			Assert.NotNull(parentOperation);
			Assert.Equal(operation.Parent, parentOperation);
		}

		[Fact]
		public void WhenCreatingNestedOperation_WillLinkToParentOperation()
		{
			authorizationRepository.CreateOperation("/Account/Delete");

			var parentOperation = authorizationRepository.GetOperationByName("/Account");
			Assert.NotNull(parentOperation); // was created in setup
			Assert.Equal(3, parentOperation.Children.Count); // /Edit, /Disable, /Delete
		}

		[Fact]
		public void CanRemoveUserGroup()
		{
			authorizationRepository.RemoveUsersGroup("Administrators");


			Assert.Null(authorizationRepository.GetUsersGroupByName("Administrators"));
		}

		[Fact]
		public void RemovingParentUserGroupWillFail()
		{
			authorizationRepository.CreateChildUserGroupOf("Administrators", "DBA");

			Assert.Throws<InvalidOperationException>(() => authorizationRepository.RemoveUsersGroup("Administrators"));
		}

		[Fact]
		public void RemovingParentEntityGroupWillFail()
		{
			authorizationRepository.CreateChildEntityGroupOf("Important Accounts", "Regular Accounts");

			Assert.Throws<InvalidOperationException>(() => authorizationRepository.RemoveEntitiesGroup("Important Accounts"));
		}


		[Fact]
		public void WhenRemovingUsersGroupThatHasAssociatedPermissionsThoseShouldBeRemoved()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For("Administrators")
				.OnEverything()
				.DefaultLevel()
				.Save();


			var permissions = permissionService.GetPermissionsFor(user);
			Assert.NotEmpty(permissions);

			authorizationRepository.RemoveUsersGroup("Administrators");


			permissions = permissionService.GetPermissionsFor(user);
			Assert.Empty(permissions);
		}

		[Fact]
		public void CanRemoveNestedUserGroup()
		{
			var dbaGroup = authorizationRepository.CreateChildUserGroupOf("Administrators", "DBA");


			authorizationRepository.RemoveUsersGroup("DBA");


			Assert.Null(authorizationRepository.GetUsersGroupByName("DBA"));

			var administratorsGroup =
				authorizationRepository.GetUsersGroupByName("Administrators");
			Assert.Equal(0,
			             administratorsGroup.DirectChildren.Count
				);
			Assert.Equal(0,
			             administratorsGroup.AllChildren.Count
				);

			Assert.Equal(0, dbaGroup.AllParents.Count);
		}

		[Fact]
		public void CanRemoveNestedEntityGroup()
		{
			var regularAccounts = authorizationRepository.CreateChildEntityGroupOf("Important Accounts",
			                                                                       "Regular Accounts");
			authorizationRepository.RemoveEntitiesGroup("Regular Accounts");

			Assert.Null(authorizationRepository.GetEntitiesGroupByName("Regular Accounts"));

			var importantAccounts = authorizationRepository.GetEntitiesGroupByName("Important Accounts");

			Assert.Equal(0, importantAccounts.DirectChildren.Count);
			Assert.Equal(0, importantAccounts.AllChildren.Count);
			Assert.Equal(0, regularAccounts.AllParents.Count);
		}

		[Fact]
		public void UsersAreNotAssociatedWithRemovedGroups()
		{
			authorizationRepository.CreateChildUserGroupOf("Administrators", "DBA");


			authorizationRepository.AssociateUserWith(user, "DBA");

			session.Flush();

			var associedGroups = authorizationRepository.GetAssociatedUsersGroupFor(user);
			Assert.Equal(2, associedGroups.Length);

			authorizationRepository.RemoveUsersGroup("DBA");

			session.Flush();

			associedGroups = authorizationRepository.GetAssociatedUsersGroupFor(user);
			Assert.Single(associedGroups);
		}

		[Fact]
		public void AccountsAreNotAssociatedWithRemovedGroups()
		{
			authorizationRepository.CreateChildEntityGroupOf("Important Accounts", "Regular Accounts");

			authorizationRepository.AssociateEntityWith(account, "Regular Accounts");

			session.Flush();

			var associatedGroups = authorizationRepository.GetAssociatedEntitiesGroupsFor(account);
			Assert.Equal(2, associatedGroups.Length);

			authorizationRepository.RemoveEntitiesGroup("Regular Accounts");
			session.Flush();

			associatedGroups = authorizationRepository.GetAssociatedEntitiesGroupsFor(account);
			Assert.Single(associatedGroups);
		}

		[Fact]
		public void CanRemoveEntitiesGroup()
		{
			authorizationRepository.RemoveEntitiesGroup("Important Accounts");

			Assert.Null(authorizationRepository.GetEntitiesGroupByName("Important Accounts"));
			;
		}


		[Fact]
		public void WhenRemovingEntitiesGroupAllPermissionsOnItWillBeDeleted()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For(user)
				.On("Important Accounts")
				.DefaultLevel()
				.Save();


			var permissions = permissionService.GetPermissionsFor(user);
			Assert.NotEmpty(permissions);

			authorizationRepository.RemoveEntitiesGroup("Important Accounts");


			permissions = permissionService.GetPermissionsFor(user);
			Assert.Empty(permissions);
		}

		[Fact]
		public void CanRemoveOperation()
		{
			authorizationRepository.RemoveOperation("/Account/Edit");

			Assert.Null(authorizationRepository.GetOperationByName("/Account/Edit"));
		}

		[Fact]
		public void CannotRemoveParentOperatio()
		{
			Assert.Throws<InvalidOperationException>(() => authorizationRepository.RemoveOperation("/Account"));
		}

		[Fact]
		public void CanRemoveNestedOperation()
		{
			authorizationRepository.RemoveOperation("/Account/Edit");

			var parent = authorizationRepository.GetOperationByName("/Account");

			Assert.Equal(1, parent.Children.Count); // /Disable
		}

		[Fact]
		public void CanRemoveUser()
		{
			authorizationRepository.RemoveUser(user);
			session.Delete(user);
		}

		[Fact]
		public void RemovingUserWillAlsoRemoveAssociatedPermissions()
		{
			permissionsBuilderService
				.Allow("/Account/Edit")
				.For(user)
				.OnEverything()
				.DefaultLevel()
				.Save();

			authorizationRepository.RemoveUser(user);
			session.Delete(user);
		}
	}
}