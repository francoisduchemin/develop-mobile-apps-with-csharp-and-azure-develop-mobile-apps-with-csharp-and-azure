using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TodoList.Abstractions;
using TodoList.Helpers;
using TodoList.Models;
using Xamarin.Forms;

namespace TodoList.ViewModels
{
	public class TaskDetailViewModel : BaseViewModel
    {
		public TaskDetailViewModel(TodoItem item = null)
		{
			if (item != null)
			{
				Item = item;
				Title = item.Text;
			}
			else
			{
				Item = new TodoItem { Text = "New Item", Complete = false };
				Title = "New Item";
			}

			SaveCommand = new Command(async () => await ExecuteSaveCommand());
			DeleteCommand = new Command(async () => await ExecuteDeleteCommand());
		}

        public ICloudService CloudService => ServiceLocator.Instance.Resolve<ICloudService>();
		public TodoItem Item { get; set; }
		public Command SaveCommand { get; }
		public Command DeleteCommand { get; }

		async Task ExecuteSaveCommand()
		{
			if (IsBusy)
				return;
			IsBusy = true;

			try
			{
                var table = await CloudService.GetTableAsync<TodoItem>();
                await table.UpsertItemAsync(Item);
                await CloudService.SyncOfflineCacheAsync();

				MessagingCenter.Send<TaskDetailViewModel>(this, "ItemsChanged");
				await Application.Current.MainPage.Navigation.PopAsync();
			}
			catch (Exception ex)
			{
				await Application.Current.MainPage.DisplayAlert("Save Item Failed", ex.Message, "OK");
			}
			finally
			{
				IsBusy = false;
			}
		}

		//Command cmdDelete;

		async Task ExecuteDeleteCommand()
		{
			if (IsBusy)
				return;
			IsBusy = true;

			try
			{
				if (Item.Id != null)
				{
                    var table = await CloudService.GetTableAsync<TodoItem>();
                    await table.DeleteItemAsync(Item);
                    await CloudService.SyncOfflineCacheAsync();
                    MessagingCenter.Send<TaskDetailViewModel>(this, "ItemsChanged");
				}
				
				await Application.Current.MainPage.Navigation.PopAsync();
			}
			catch (Exception ex)
			{
				await Application.Current.MainPage.DisplayAlert("Delete Item Failed", ex.Message, "OK");
			}
			finally
			{
				IsBusy = false;
			}
		}
	}
}
