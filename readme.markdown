Rhino Security
==============

Rhino Security is part of the [Rhino Tools](http://hibernatingrhinos.com/) collection by [Ayende Rahien](http://ayende.com/blog).

What is this?
-------------

Rhino Security is a security framework to provide row level security for NHibernate. Rhino Security is perfect for people who want to set up user and group security in their NHibernate domain models. It supports ACL and role based security using a model similar to this:

![yUML Rhino Security](http://yuml.me/diagram/scruffy/class/%5BUser%5D%3C1-*++%5BPermission%5D%2C%20%5BPermission%5D++-1%3E%5BOperation%5D%2C%20%5BOperation%5D++-%3E%5BOperation%5D%2C%20%5BUser%5D%3C*-%5BUserGroup%5D)
                                             
*Based on [this blog post](http://weblogs.asp.net/arturtrosin/archive/2009/04/02/rhino-tools-rhino-security-guide.aspx)*


Getting Started
---------------

Registering into NHibernate

BEFORE creating the session factory, call the following:

	Security.Configure(cfg, SecurityTableStructure.Schema);

### Container Configuration


Rhino Security make use of Common Service Locator (http://www.codeplex.com/CommonServiceLocator), you need to set the ServiceLocator.SetLocatorProvider() to provide the following services:
 * IAuthorizationService
 * IAuthorizationRepository
 * IPermissionsBuilderService
 * IPermissionsService

ALL services must be TRANSIENT, and the container needs to provide access to the current ISession. 

The following is an example of configuring Rhino Security using Windsor:

	WindsorServiceLocator windsorServiceLocator = new WindsorServiceLocator(container);
	ServiceLocator.SetLocatorProvider(() => windsorServiceLocator);

	container.Register(
		Component.For<IAuthorizationService>()
			.ImplementedBy<AuthorizationService>()
			.Lifestyle.Is(Lifestyle.Transient),
		Component.For<IAuthorizationRepository>()
			.ImplementedBy<AuthorizationRepository>()
			.Lifestyle.Is(Lifestyle.Transient),
		Component.For<IPermissionsBuilderService>()
			.ImplementedBy<PermissionsBuilderService>()
			.Lifestyle.Is(Lifestyle.Transient),
		Component.For<IPermissionsService>()
			.ImplementedBy<PermissionsService>()
			.Lifestyle.Is(Lifestyle.Transient)
		);
	);

                 
More Information
----------------

More information about the library can be found [in the Rhino Security category on Ayende's blog](http://ayende.com/blog/tags/rhino-security).

Try [this Google search](http://www.google.com/search?q=rhino+security+nhibernate) to find a ton of other information about it.    


-----

Intro by [Tobin Harris](http://tobinharris.com), he asks that people contribute to make it better :)
