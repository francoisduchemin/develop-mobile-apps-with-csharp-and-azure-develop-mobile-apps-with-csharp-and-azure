using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using TodoList.Models;

namespace TodoList.Abstractions
{
	public interface ICloudService
	{
        //ICloudTable<T> GetTable<T>() where T : TableData;
        Task<ICloudTable<T>> GetTableAsync<T>() where T : TableData;
		Task LoginAsync();
	       Task<AppServiceIdentity> GetIdentityAsync();
	       Task LogoutAsync();
        Task SyncOfflineCacheAsync();
	}

	//public interface ICloudService
	//{
	//	Task<ICloudTable<T>> GetTableAsync<T>() where T : TableData;

	//	Task<MobileServiceUser> LoginAsync();

	//	Task LogoutAsync();

	//	Task<AppServiceIdentity> GetIdentityAsync();

	//	Task SyncOfflineCacheAsync();
	//}
}
