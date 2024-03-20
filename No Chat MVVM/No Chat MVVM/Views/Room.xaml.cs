using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;
using Xamarin.Forms.Xaml;

namespace No_Chat_MVVM.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Room : ContentPage
    {
        public Room()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            setBackGroundImage();
        }
        public void setBackGroundImage()
        {
            if (Preferences.ContainsKey("BACKGROUND_IMAGE_PATH"))
            {
                string backgroundImagePath = Preferences.Get("BACKGROUND_IMAGE_PATH", "");
                this.BackgroundImageSource = ImageSource.FromFile(backgroundImagePath);
                MainStackLayout.BackgroundColor = Color.FromRgba(0, 0, 0, .5);
            }
        }
    }
}