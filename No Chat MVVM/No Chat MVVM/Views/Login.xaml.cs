using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace No_Chat_MVVM.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Login : ContentPage
    {
        public Login()
        {
            InitializeComponent();
            checkInternetAcces();
        }

        protected async override void OnAppearing()
        {
            base.OnDisappearing();
            await Module.pusher.DisconnectAsync();
        }

        void checkInternetAcces()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                DisplayAlert("Warning", "No Internet Connection", "OK");
            }
        }
    }
}