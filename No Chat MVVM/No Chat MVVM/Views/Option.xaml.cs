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
    public partial class Option : ContentPage
    {
        public Option()
        { 
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var navigationPage = Application.Current.MainPage as NavigationPage;
            navigationPage.BarBackgroundColor = Color.Black;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            var navigationPage = Application.Current.MainPage as NavigationPage;
            navigationPage.BarBackgroundColor = Color.FromHex("#32515f");
        }
    }
}