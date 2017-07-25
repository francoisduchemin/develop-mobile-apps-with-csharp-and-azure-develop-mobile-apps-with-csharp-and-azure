using System;
using System.Collections.Generic;
using TodoList.ViewModels;
using Xamarin.Forms;

namespace TodoList.Pages
{
    public partial class TaskList : ContentPage
    {
        public TaskList()
        {
            InitializeComponent();
			BindingContext = new TaskListViewModel();
		}
    }
}
