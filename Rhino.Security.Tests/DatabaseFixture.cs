using System;
using System.Data.SQLite;
using CommonServiceLocator;
using NHibernate;
using NHibernate.Cache;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.SqlCommand;
using NHibernate.Tool.hbm2ddl;
using Rhino.Security.Interfaces;
using Xunit;
using Xunit.Abstractions;
using Environment = NHibernate.Cfg.Environment;

namespace Rhino.Security.Tests
{
    [Collection("DatabaseFixture tests")]
    public abstract class DatabaseFixture : IDisposable
    {
        protected readonly ISessionFactory factory;
        protected Account account;
        protected IAuthorizationRepository authorizationRepository;
        protected IAuthorizationService authorizationService;
        protected IPermissionsService permissionService;
        protected IPermissionsBuilderService permissionsBuilderService;

        protected ISession session;
        protected User user;

        protected readonly ITestOutputHelper output;
        public bool UseSqlDatabase;

        protected DatabaseFixture(ITestOutputHelper output, bool useSqlDatabase = false)
        {
            this.output = output;
            UseSqlDatabase = useSqlDatabase;

            BeforeSetup();

            SillyContainer.SessionProvider = (() => session);
            var sillyContainer = new SillyContainer();
            ServiceLocator.SetLocatorProvider(() => sillyContainer);

            Assert.NotNull(typeof(SQLiteConnection));

            var driverName = typeof(SQLite20Driver).AssemblyQualifiedName;
            var dialectName = typeof(SQLiteDialect).AssemblyQualifiedName;
            var releaseConnections = "on_close";

            if (useSqlDatabase)
            {
                driverName = typeof(Sql2008ClientDriver).AssemblyQualifiedName;
                dialectName = typeof(MsSql2012Dialect).AssemblyQualifiedName;
                releaseConnections = "auto";
            }

            Configuration cfg = new Configuration()
                .SetProperty(Environment.ConnectionDriver, driverName)
                .SetProperty(Environment.Dialect, dialectName)
                .SetProperty(Environment.ConnectionString, ConnectionString)
                .SetProperty(Environment.ReleaseConnections, releaseConnections)
                .SetProperty(Environment.UseSecondLevelCache, "true")
                .SetProperty(Environment.UseQueryCache, "true")
                .SetProperty(Environment.ShowSql, "true")
                .SetProperty(Environment.FormatSql, "true")
                .SetProperty(Environment.CacheProvider, typeof(HashtableCacheProvider).AssemblyQualifiedName)
                .AddAssembly(typeof(User).Assembly);

            Security.Configure<User>(cfg, SecurityTableStructure.Prefix);

            factory = cfg.BuildSessionFactory();

            session = factory.WithOptions().Interceptor(new XUnitSqlCaptureInterceptor(this.output)).OpenSession();

            new SchemaExport(cfg).Execute(false, true, false, session.Connection, null);

            session.BeginTransaction();

            SetupEntities();

            session.Flush();
        }

        public virtual string ConnectionString
        {
            get { return UseSqlDatabase ? "Server=(local);Database=tests;Trusted_Connection=True;" : "Data Source=:memory:"; }
        }

        #region IDisposable Members

        public virtual void Dispose()
        {
            if (session.Transaction.IsActive)
                session.Transaction.Rollback();
            session.Dispose();
        }

        #endregion

        protected virtual void BeforeSetup()
        {
        }

        private void SetupEntities()
        {
            user = new User { Name = "Ayende" };
            account = new Account { Name = "south sand" };

            session.Save(user);
            session.Save(account);

            authorizationService = ServiceLocator.Current.GetInstance<IAuthorizationService>();
            permissionService = ServiceLocator.Current.GetInstance<IPermissionsService>();
            permissionsBuilderService = ServiceLocator.Current.GetInstance<IPermissionsBuilderService>();
            authorizationRepository = ServiceLocator.Current.GetInstance<IAuthorizationRepository>();

            authorizationRepository.CreateUsersGroup("Administrators");
            authorizationRepository.CreateEntitiesGroup("Important Accounts");
            authorizationRepository.CreateOperation("/Account/Edit");
            authorizationRepository.CreateOperation("/Account/Disable");

            authorizationRepository.AssociateUserWith(user, "Administrators");
            authorizationRepository.AssociateEntityWith(account, "Important Accounts");
        }
    }

    public class XUnitSqlCaptureInterceptor : EmptyInterceptor
    {
        public XUnitSqlCaptureInterceptor(ITestOutputHelper output)
        {
            this.Output = output;
        }

        public ITestOutputHelper Output { get; set; }

        public override SqlString OnPrepareStatement(SqlString sql)
        {
            this.Output.WriteLine(sql.ToString());

            return sql;
        }
    }
}