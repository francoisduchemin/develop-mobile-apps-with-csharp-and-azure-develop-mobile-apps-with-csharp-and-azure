using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Abstractions;
using TodoList.Helpers;
using TodoList.Models;
using Xamarin.Forms;

namespace TodoList.ViewModels
{
	public class TaskListViewModel : BaseViewModel
	{
        ICloudService cloudService;

		public TaskListViewModel()
		{
            cloudService = ServiceLocator.Instance.Resolve<ICloudService>();
            Table = cloudService.GetTable<TodoItem>();

			Title = "Task List";
            items.CollectionChanged += this.OnCollectionChanged;

			RefreshCommand = new Command(async () => await ExecuteRefreshCommand());
			AddNewItemCommand = new Command(async () => await ExecuteAddNewItemCommand());
			LogoutCommand = new Command(async () => await ExecuteLogoutCommand());
			LoadMoreCommand = new Command<TodoItem>(async (TodoItem item) => await LoadMore(item));

			// Subscribe to events from the Task Detail Page
			MessagingCenter.Subscribe<TaskDetailViewModel>(this, "ItemsChanged", async (sender) =>
			{
				await ExecuteRefreshCommand();
			});

			// Execute the refresh command
			RefreshCommand.Execute(null);
		}

        public ICloudTable<TodoItem> Table { get; set; }
        public Command RefreshCommand { get; }
        public Command AddNewItemCommand { get; }
		public Command LogoutCommand { get; }
        public Command LoadMoreCommand { get; }


		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Debug.WriteLine("[TaskList] OnCollectionChanged: Items have changed");
		}

		ObservableCollection<TodoItem> items = new ObservableCollection<TodoItem>();
		public ObservableCollection<TodoItem> Items
		{
			get { return items; }
			set { SetProperty(ref items, value, "Items"); }
		}

		TodoItem selectedItem;
		public TodoItem SelectedItem
		{
			get { return selectedItem; }
			set
			{
				SetProperty(ref selectedItem, value, "SelectedItem");
				if (selectedItem != null)
				{
					Application.Current.MainPage.Navigation.PushAsync(new Pages.TaskDetail(selectedItem));
					SelectedItem = null;
				}
			}
		}

		async Task ExecuteRefreshCommand()
		{
			if (IsBusy)
				return;
			IsBusy = true;

			try
			{
                var identity = await cloudService.GetIdentityAsync();
				if (identity != null)
				{
					var name = identity.UserClaims.FirstOrDefault(c => c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")).Value;
					Title = $"Tasks for {name}";
				}
				var list = await Table.ReadItemsAsync(0, 20);
				Items.Clear();
                foreach (var item in list)
                    Items.Add(item);
                hasMoreItems = true;
			}
			catch (Exception ex)
			{
				await Application.Current.MainPage.DisplayAlert("Items Not Loaded", ex.Message, "OK");
			}
			finally
			{
				IsBusy = false;
			}
		}

		async Task ExecuteAddNewItemCommand()
		{
			if (IsBusy)
				return;
			IsBusy = true;

			try
			{
				await Application.Current.MainPage.Navigation.PushAsync(new Pages.TaskDetail());
			}
			catch (Exception ex)
			{
				await Application.Current.MainPage.DisplayAlert("Item Not Added", ex.Message, "OK");
			}
			finally
			{
				IsBusy = false;
			}
		}

		async Task RefreshList()
		{
			await ExecuteRefreshCommand();
			MessagingCenter.Subscribe<TaskDetailViewModel>(this, "ItemsChanged", async (sender) =>
			{
				await ExecuteRefreshCommand();
			});
		}

		async Task ExecuteLogoutCommand()
		{
			if (IsBusy)
				return;
			IsBusy = true;

			try
			{
				await cloudService.LogoutAsync();
				Application.Current.MainPage = new NavigationPage(new Pages.EntryPage());
			}
			catch (Exception ex)
			{
				await Application.Current.MainPage.DisplayAlert("Logout Failed", ex.Message, "OK");
			}
			finally
			{
				IsBusy = false;
			}
		}

		bool hasMoreItems = true;

		async Task LoadMore(TodoItem item)
		{
			if (IsBusy)
			{
				Debug.WriteLine($"LoadMore: bailing because IsBusy = true");
				return;
			}

			// If we are not displaying the last one in the list, then return.
			if (!Items.Last().Id.Equals(item.Id))
			{
				Debug.WriteLine($"LoadMore: bailing because this id is not the last id in the list");
				return;
			}

			// If we don't have more items, return
			if (!hasMoreItems)
			{
				Debug.WriteLine($"LoadMore: bailing because we don't have any more items");
				return;
			}

			IsBusy = true;
			try
			{
				var list = await Table.ReadItemsAsync(Items.Count, 20);
				if (list.Count > 0)
				{
					Debug.WriteLine($"LoadMore: got {list.Count} more items");

					//Items.AddRange(list);

					foreach (var listItem in list)
						Items.Add(listItem);
				}
				else
				{
					Debug.WriteLine($"LoadMore: no more items: setting hasMoreItems= false");
					hasMoreItems = false;
				}
			}
			catch (Exception ex)
			{
				await Application.Current.MainPage.DisplayAlert("LoadMore Failed", ex.Message, "OK");
			}
			finally
			{
				IsBusy = false;
			}
		}
	}
}
