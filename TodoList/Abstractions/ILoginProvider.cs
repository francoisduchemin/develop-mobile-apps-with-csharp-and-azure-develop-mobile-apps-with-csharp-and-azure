using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;

namespace TodoList.Abstractions
{
	public interface ILoginProvider
	{
		Task LoginAsync(MobileServiceClient client);

		void RemoveTokenFromSecureStore();
	}
}
