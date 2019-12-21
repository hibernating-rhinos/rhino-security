using System.Collections.Generic;
using System.Linq;
using System.Security;
using NHibernate.Criterion;
using Rhino.Security.Model;
using Rhino.Security.Services;
using Xunit;
using Xunit.Abstractions;

namespace Rhino.Security.Tests
{
    public class UserGroupQueriesTests : DatabaseFixture
    {
        public UserGroupQueriesTests(ITestOutputHelper output, bool useSqlDatabase = false) : base(output, useSqlDatabase)
        {
            authorizationRepository.CreateUsersGroup("SuperUsers");
            authorizationRepository.AssociateUserWith(user, "SuperUsers");

            authorizationRepository.CreateChildUserGroupOf("Administrators", "HelpDesk");
            authorizationRepository.AssociateUserWith(user, "HelpDesk");

            authorizationRepository.CreateUsersGroup("ParentGroup");
            authorizationRepository.CreateChildUserGroupOf("ParentGroup", "ChildGroup");
            authorizationRepository.AssociateUserWith(user, "ChildGroup");

            session.Flush();
        }

        [Fact]
        public void GetDirectGroups()
        {
            var directGroupsForUser = session.Query<UsersGroup>().Where(g => g.Users.Contains(user));

            var usersGroups = directGroupsForUser.ToList();

            foreach (var usersGroup in usersGroups)
            {
                output.WriteLine($"Groups for user {user.Name} {usersGroup.Name}");
            }

            AssertDirectGroups(usersGroups);
        }

        private static void AssertDirectGroups(IList<UsersGroup> usersGroups)
        {
            Assert.Collection(usersGroups.OrderBy(g => g.Name),
                group => Assert.Equal("Administrators", @group.Name),
                group => Assert.Equal("ChildGroup", @group.Name),
                group => Assert.Equal("HelpDesk", @group.Name),
                group => Assert.Equal("SuperUsers", @group.Name)
            );
        }

        [Fact]
        public void GetAllGroups()
        {
            var allGroupdForUser =
                from g in session.Query<UsersGroup>()
                    .Where(g => g.AllChildren.Any(g => g.Users.Contains(user)) || g.Users.Contains(user))
                select g;

            var usersGroups = allGroupdForUser.ToList();

            foreach (var usersGroup in usersGroups)
            {
                output.WriteLine($"Groups for user {user.Name} {usersGroup.Name}");
            }

            AssertAllGroups(usersGroups);
        }

        private static void AssertAllGroups(IList<UsersGroup> usersGroups)
        {
            Assert.Collection(usersGroups.OrderBy(g => g.Name),
                group => Assert.Equal("Administrators", @group.Name),
                group => Assert.Equal("ChildGroup", @group.Name),
                group => Assert.Equal("HelpDesk", @group.Name),
                group => Assert.Equal("ParentGroup", @group.Name),
                group => Assert.Equal("SuperUsers", @group.Name)
            );
        }

        [Fact]
        public void GetDirectGroupsCriterna()
        {
            var criteria = session.CreateCriteria(typeof (Account), "account");

            var directUsersGroups = SecurityCriterions.DirectUsersGroups(user);

            var usersGroups = directUsersGroups.GetExecutableCriteria(session).AddOrder(Order.Asc("Name")).List<UsersGroup>();

            foreach (var usersGroup in usersGroups)
            {
                output.WriteLine($"Groups for user {user.Name} {usersGroup.Name}");
            }

            AssertDirectGroups(usersGroups);
        }

        [Fact]
        public void GetAllGroupsCriterna()
        {
            var criteria = session.CreateCriteria(typeof (Account), "account");

            var allGroups = SecurityCriterions.AllGroups((IUser)user);

            var usersGroups = allGroups.GetExecutableCriteria(session).AddOrder(Order.Asc("Name")).List<UsersGroup>();

            foreach (var usersGroup in usersGroups)
            {
                output.WriteLine($"Groups for user: {user.Name} -> {usersGroup.Name}");
            }

            AssertAllGroups(usersGroups);
        }
    }
}