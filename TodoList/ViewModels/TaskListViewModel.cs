﻿using System;
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

			// Execute the refresh command
			RefreshCommand.Execute(null);
		}

        public ICloudTable<TodoItem> Table { get; set; }
        public Command RefreshCommand { get; }
        public Command AddNewItemCommand { get; }

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
					var name = identity.UserClaims.FirstOrDefault(c => c.Type.Equals("name")).Value;
					Title = $"Tasks for {name}";
				}
				var list = await Table.ReadAllItemsAsync();
				Items.Clear();
                foreach (var item in list)
                    Items.Add(item);
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
	}
}
