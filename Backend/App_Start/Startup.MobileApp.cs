using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Config;
using Backend.DataObjects;
using Backend.Models;
using Owin;

namespace Backend
{
	public partial class Startup
	{
		public static void ConfigureMobileApp(IAppBuilder app)
		{
			var config = new HttpConfiguration();
			var mobileConfig = new MobileAppConfiguration();

			mobileConfig
				.AddTablesWithEntityFramework()
				.ApplyTo(config);

			// To display errors in the browser during development, uncomment the following
			// line. Comment it out again when you deploy your service for production use.
			config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always; 

			Database.SetInitializer(new MobileServiceInitializer());

			app.UseWebApi(config);
		}
	}

	//public class MobileServiceInitializer : CreateDatabaseIfNotExists<MobileServiceContext>
    public class MobileServiceInitializer : DropCreateDatabaseIfModelChanges<MobileServiceContext>
	{
		protected override void Seed(MobileServiceContext context)
		{
			List<TodoItem> todoItems = new List<TodoItem>
			{
				new TodoItem { Id = Guid.NewGuid().ToString(), Text = "First item", Complete = false },
				new TodoItem { Id = Guid.NewGuid().ToString(), Text = "Second item", Complete = false }
			};

			foreach (TodoItem todoItem in todoItems)
			{
				context.Set<TodoItem>().Add(todoItem);
			}

			base.Seed(context);
		}
	}
}