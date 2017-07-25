using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using TodoList.Abstractions;
using TodoList.Models;
using Xamarin.Forms;

namespace TodoList.Services
{
	public class AzureCloudService : ICloudService
	{
		MobileServiceClient client;
        List<AppServiceIdentity> identities = null;

		public AzureCloudService()
		{
			client = new MobileServiceClient("https://todoappfrancois.azurewebsites.net");
		}

		public ICloudTable<T> GetTable<T>() where T : TableData
		{
			return new AzureCloudTable<T>(client);
		}

		public Task LoginAsync()
		{
			var loginProvider = DependencyService.Get<ILoginProvider>();
			return loginProvider.LoginAsync(client);
		}

		public async Task<AppServiceIdentity> GetIdentityAsync()
		{
			if (client.CurrentUser == null || client.CurrentUser?.MobileServiceAuthenticationToken == null)
			{
				throw new InvalidOperationException("Not Authenticated");
			}

			if (identities == null)
			{
				identities = await client.InvokeApiAsync<List<AppServiceIdentity>>("/.auth/me");
			}

			if (identities.Count > 0)
				return identities[0];
			return null;
		}
	}
}
