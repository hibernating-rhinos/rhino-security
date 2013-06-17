using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Rhino.Security.Interfaces;
using Rhino.Security.Services;

namespace Rhino.Security.Windsor
{
	/// <summary>
	/// Installs the services needed for Rhino Security to work properly with Windsor.
	/// </summary>
	public class RhinoSecurityInstaller : IWindsorInstaller
	{
		/// <summary>
		/// Install the services needed for Rhino Security to work properly with Windsor.
		/// </summary>
		/// <param name="container">The container.</param>
		/// <param name="store">The configuration store.</param>
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(
				Component.For<IAuthorizationService>()
					.ImplementedBy<AuthorizationService>()
					.LifeStyle.Transient,
				Component.For<IAuthorizationRepository>()
					.ImplementedBy<AuthorizationRepository>()
					.LifeStyle.Transient,
				Component.For<IPermissionsBuilderService>()
					.ImplementedBy<PermissionsBuilderService>()
					.LifeStyle.Transient,
				Component.For<IPermissionsService>()
					.ImplementedBy<PermissionsService>()
					.LifeStyle.Transient
				);
		}
	}
}
