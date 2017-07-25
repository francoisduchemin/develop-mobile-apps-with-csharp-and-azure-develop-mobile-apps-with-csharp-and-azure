using System.Threading.Tasks;
using Android.Content;
using Microsoft.WindowsAzure.MobileServices;
using TodoList.Abstractions;
using TodoList.Droid.Services;

[assembly: Xamarin.Forms.Dependency(typeof(DroidLoginProvider))]
namespace TodoList.Droid.Services
{
	public class DroidLoginProvider : ILoginProvider
	{
		Context context;

		public void Init(Context context)
		{
			this.context = context;
		}

		public async Task LoginAsync(MobileServiceClient client)
		{
			await client.LoginAsync(context, "aad");
		}
	}

}
