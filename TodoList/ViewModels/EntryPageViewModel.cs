﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TodoList.Abstractions;
using TodoList.Helpers;
using Xamarin.Forms;

namespace TodoList.ViewModels
{
	public class EntryPageViewModel : BaseViewModel
	{
		public EntryPageViewModel()
		{
			Title = "Task List";
		}

		Command loginCmd;
		public Command LoginCommand => loginCmd ?? (loginCmd = new Command(async () => await ExecuteLoginCommand()));

		async Task ExecuteLoginCommand()
		{
			if (IsBusy)
				return;
			IsBusy = true;

			try
			{
				var cloudService = ServiceLocator.Instance.Resolve<ICloudService>();
				await cloudService.LoginAsync();
				Application.Current.MainPage = new NavigationPage(new Pages.TaskList());
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"[Login] Error = {ex.Message}");
			}
			finally
			{
				IsBusy = false;
			}
		}
	}
}
