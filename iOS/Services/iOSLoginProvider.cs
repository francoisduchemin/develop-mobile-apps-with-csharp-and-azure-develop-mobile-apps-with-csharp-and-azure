using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using TodoList.Abstractions;
using TodoList.iOS.Services;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(iOSLoginProvider))]
namespace TodoList.iOS.Services
{
	public class iOSLoginProvider : ILoginProvider
	{
		public async Task LoginAsync(MobileServiceClient client)
		{
			await client.LoginAsync(RootView, "aad");
		}

		public UIViewController RootView => UIApplication.SharedApplication.KeyWindow.RootViewController;
	}

}
