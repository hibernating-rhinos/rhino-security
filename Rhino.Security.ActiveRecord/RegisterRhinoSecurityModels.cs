using System;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Internal;

namespace Rhino.Security.ActiveRecord
{
	public class RegisterRhinoSecurityModels
	{
		private readonly ActiveRecordModelBuilder modelBuilder = new ActiveRecordModelBuilder();

		public void BeforeNHibernateInitialization()
		{
			ActiveRecordStarter.ModelsValidated+=delegate
			{
				foreach (Type type in RhinoSecurity.Entities)
				{
					modelBuilder.CreateDummyModelFor(type);
				}
			};
		}
	}
}