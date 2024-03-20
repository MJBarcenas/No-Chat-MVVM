using No_Chat_MVVM.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace No_Chat_MVVM
{ 
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            if (Preferences.ContainsKey("ROOMID") && Preferences.ContainsKey("ALIAS"))
            {
                Module.room = Preferences.Get("ROOMID", "0");
                Module.alias = Preferences.Get("ALIAS", "");

                MainPage = new NavigationPage(new Views.Room())
                {
                    BarBackgroundColor = Color.FromHex("#32515f")
                };

            } else
            {
                MainPage = new NavigationPage(new Views.Login())
                {
                    BarBackgroundColor = Color.FromHex("#32515f")
                };
            }
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
