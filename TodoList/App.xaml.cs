using TodoList.Abstractions;
using TodoList.Services;
using Xamarin.Forms;

namespace TodoList
{
    public partial class App : Application
    {
        public static ICloudService CloudService { get; set; }

        public App()
        {
            InitializeComponent();

			CloudService = new AzureCloudService();
			MainPage = new NavigationPage(new Pages.EntryPage());

		}

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
