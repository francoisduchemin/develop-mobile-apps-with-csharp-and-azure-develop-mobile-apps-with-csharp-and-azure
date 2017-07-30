using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Webkit;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using TodoList.Abstractions;
using TodoList.Droid.Services;
using TodoList.Helpers;
using Xamarin.Auth;

[assembly: Xamarin.Forms.Dependency(typeof(DroidLoginProvider))]
namespace TodoList.Droid.Services
{
	public class DroidLoginProvider : ILoginProvider
	{
		//Context context;
		public Context RootView { get; private set; }
		public AccountStore AccountStore { get; private set; }

        public void Init(Context context)
        {
            RootView = context;
            AccountStore = AccountStore.Create(context);
        }

		public async Task LoginAsync(MobileServiceClient client)
		{
			//await client.LoginAsync(context, "facebook");

			// Check if the token is available within the key store
			var accounts = AccountStore.FindAccountsForService("tasklist");
			if (accounts != null)
			{
				foreach (var acct in accounts)
				{
					string token;

					if (acct.Properties.TryGetValue("token", out token))
					{
						if (!IsTokenExpired(token))
						{
							client.CurrentUser = new MobileServiceUser(acct.Username);
							client.CurrentUser.MobileServiceAuthenticationToken = token;
							return;
						}
					}
				}
			}

			// Server Flow
			await client.LoginAsync(RootView, "facebook");

			// Store the new token within the store
			var account = new Account(client.CurrentUser.UserId);
			account.Properties.Add("token", client.CurrentUser.MobileServiceAuthenticationToken);
			AccountStore.Save(account, "tasklist");
		}

		bool IsTokenExpired(string token)
		{
			// Get just the JWT part of the token (without the signature).
			var jwt = token.Split(new Char[] { '.' })[1];

			// Undo the URL encoding.
			jwt = jwt.Replace('-', '+').Replace('_', '/');
			switch (jwt.Length % 4)
			{
				case 0: break;
				case 2: jwt += "=="; break;
				case 3: jwt += "="; break;
				default:
					throw new ArgumentException("The token is not a valid Base64 string.");
			}

			// Convert to a JSON String
			var bytes = Convert.FromBase64String(jwt);
			string jsonString = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

			// Parse as JSON object and get the exp field value,
			// which is the expiration date as a JavaScript primative date.
			JObject jsonObj = JObject.Parse(jsonString);
			var exp = Convert.ToDouble(jsonObj["exp"].ToString());

			// Calculate the expiration by adding the exp value (in seconds) to the
			// base date of 1/1/1970.
			DateTime minTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var expire = minTime.AddSeconds(exp);
			return (expire < DateTime.UtcNow);
		}

		public void RemoveTokenFromSecureStore()
		{
			// See https://developer.xamarin.com/guides/xamarin-forms/cloud-services/authentication/azure/
			CookieManager.Instance.RemoveAllCookie();

			var accounts = AccountStore.FindAccountsForService("tasklist");
			if (accounts != null)
			{
				foreach (var acct in accounts)
				{
					AccountStore.Delete(acct, "tasklist");
				}
			}
		}
	}

}
