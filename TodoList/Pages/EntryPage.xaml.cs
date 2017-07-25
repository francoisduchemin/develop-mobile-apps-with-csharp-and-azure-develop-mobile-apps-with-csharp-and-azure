using System;
using System.Collections.Generic;
using TodoList.ViewModels;
using Xamarin.Forms;

namespace TodoList.Pages
{
    public partial class EntryPage : ContentPage
    {
        public EntryPage()
        {
            InitializeComponent();
            BindingContext = new EntryPageViewModel();
        }
    }
}
