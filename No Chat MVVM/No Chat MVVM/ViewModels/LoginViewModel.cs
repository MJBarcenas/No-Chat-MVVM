using No_Chat_MVVM.Views;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace No_Chat_MVVM.ViewModels
{
    public class LoginViewModel : BindableObject
    {
        public LoginViewModel()
        {
            loginCommand = new Command(loginExecute);
            //loginExecute();
        }

        string alias = "van_";
        public string Alias
        {
            get => alias;
            set
            {
                if (value == Alias) { return; }
                alias = value;
                OnPropertyChanged(nameof(Alias));
            }
        }

        string room = "3001";
        public string Room
        {
            get => room;
            set
            {
                if (value == Room) { return; }
                room = value;
                OnPropertyChanged(nameof(Room));
            }
        }

        public ICommand loginCommand { get; set; }
        private async void loginExecute()
        {
            try
            {
                //Check internet connection
                if (!CrossConnectivity.Current.IsConnected)
                {
                    await Task.Run(() => { Module.showMessage("Check your internet connection", Color.FromHex("#ed2721"), Application.Current.MainPage); });
                    return;
                }

                //Check credentials
                if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(room))
                {
                    await Task.Run(() => { Module.showMessage("Please fill up all fields", Color.FromHex("#ed2721"), Application.Current.MainPage); });
                    return;
                }
                else
                {
                    Preferences.Set("ROOMID", room);
                    Preferences.Set("ALIAS", alias);

                    HttpResponseMessage response = await Module.httpClient.GetAsync($"https://event-venue.website/No_Chat/functions.php?functionname=createNew&alias={alias}&roomID={room}");
                    if (response.IsSuccessStatusCode)
                    {
                        Module.room = room;
                        Module.alias = alias;
                        await Application.Current.MainPage.Navigation.PushAsync(new Views.Room());
                    }
                }
            }
            catch (Exception)
            {
                await Task.Run(() => { Module.showMessage("Something went wrong", Color.FromHex("#ed2721"), Application.Current.MainPage); });
            }
        }
    }
}
