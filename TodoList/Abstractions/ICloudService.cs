using System;
using System.Threading.Tasks;
using TodoList.Models;

namespace TodoList.Abstractions
{
	public interface ICloudService
	{
		ICloudTable<T> GetTable<T>() where T : TableData;
		Task LoginAsync();
        Task<AppServiceIdentity> GetIdentityAsync();
	}
}
