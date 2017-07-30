using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Plugin.Connectivity;
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

		#region Offline Sync Initialization
		async Task InitializeAsync()
		{
			// Short circuit - local database is already initialized
			if (client.SyncContext.IsInitialized)
				return;

			// Create a reference to the local sqlite store
			var store = new MobileServiceSQLiteStore("offlinecache.db");

			// Define the database schema
			store.DefineTable<TodoItem>();

			// Actually create the store and update the schema
			await client.SyncContext.InitializeAsync(store);
		}
		#endregion

		//public ICloudTable<T> GetTable<T>() where T : TableData
		//{
		//	return new AzureCloudTable<T>(client);
		//}

		/// <summary>
		/// Returns a link to the specific table.
		/// </summary>
		/// <typeparam name="T">The model</typeparam>
		/// <returns>The table reference</returns>
		public async Task<ICloudTable<T>> GetTableAsync<T>() where T : TableData
		{
			await InitializeAsync();
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

		public async Task SyncOfflineCacheAsync()
		{
			await InitializeAsync();

			if (!(await CrossConnectivity.Current.IsRemoteReachable(client.MobileAppUri.Host, 443)))
			{
				Debug.WriteLine($"Cannot connect to {client.MobileAppUri} right now - offline");
				return;
			}

			// Push the Operations Queue to the mobile backend
			await client.SyncContext.PushAsync();

			// Pull each sync table
			var taskTable = await GetTableAsync<TodoItem>(); 
            await taskTable.PullAsync();
		}

		public async Task LogoutAsync()
		{
			if (client.CurrentUser == null || client.CurrentUser.MobileServiceAuthenticationToken == null)
				return;

			// Log out of the identity provider (if required)

			// Invalidate the token on the mobile backend
			var authUri = new Uri($"{client.MobileAppUri}/.auth/logout");
			using (var httpClient = new HttpClient())
			{
				httpClient.DefaultRequestHeaders.Add("X-ZUMO-AUTH", client.CurrentUser.MobileServiceAuthenticationToken);
				await httpClient.GetAsync(authUri);
			}

			// Remove the token from the cache
			DependencyService.Get<ILoginProvider>().RemoveTokenFromSecureStore();

			// Remove the token from the MobileServiceClient
			await client.LogoutAsync();
		}
	}
}
