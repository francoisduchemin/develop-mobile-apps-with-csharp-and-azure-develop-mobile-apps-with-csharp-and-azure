using System;
using System.Collections.Generic;
using TodoList.Models;
using TodoList.ViewModels;
using Xamarin.Forms;

namespace TodoList.Pages
{
    public partial class TaskDetail : ContentPage
    {
        public TaskDetail(TodoItem item = null)
        {
            InitializeComponent();
			BindingContext = new TaskDetailViewModel(item);
		}
    }
}
