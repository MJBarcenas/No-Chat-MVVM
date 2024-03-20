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
    public class OptionViewModel : BindableObject
    {
        public OptionViewModel()
        {
            updateProfileCommand = new Command(updateProfileExecute);
            updateBackgroundCommand = new Command(updateBackgroundExecute);
            logoutCommand = new Command(logoutExecute);
        }

        string profileName = Module.alias;
        public string ProfileName
        {
            get => profileName;
            set
            {
                if (value == ProfileName) { return; }
                profileName = value;
                OnPropertyChanged(nameof(ProfileName));
            }
        }

        Uri profileAvatar = Module.userImage.image;
        public Uri ProfileAvatar
        {
            get => profileAvatar;
            set
            {
                if (value == ProfileAvatar) { return; }
                profileAvatar = value;
                OnPropertyChanged(nameof(ProfileAvatar));
            }
        }

        public ICommand updateProfileCommand { get; set; }
        async void updateProfileExecute()
        {
            FileResult file = await MediaPicker.PickPhotoAsync();

            if (file == null) { return; }

            try
            {
                MultipartFormDataContent content = new MultipartFormDataContent();
                content.Add(new StreamContent(await file.OpenReadAsync()), "file", file.FileName);
                HttpResponseMessage response = await Module.httpClient.PostAsync($"https://event-venue.website/No_Chat/functions.php?functionname=uploadImage&type=user&alias={Module.alias}", content);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    if (result != "File too large" || result != "Unsupported image type")
                    {
                        Random number = new Random();
                        ProfileAvatar = new Uri($"https://event-venue.website/No_Chat/images/user_images/{result}?{number.Next(0, 100)}");
                        
                        await Task.Run(() => { Module.showMessage("Upload succes!", Color.FromHex("#1eb980"), Application.Current.MainPage); });
                    }
                    else
                    {
                        await Task.Run(() => { Module.showMessage(result, Color.FromHex("#CC0000"), Application.Current.MainPage); });
                    }
                }
                else
                {
                    await Task.Run(() => { Module.showMessage("Something went wrong", Color.FromHex("#CC0000"), Application.Current.MainPage); });
                }
            }
            catch (Exception)
            {

                await Task.Run(() => { Module.showMessage("No Internet Connection", Color.FromHex("#CC0000"), Application.Current.MainPage); });
            }
        }

        public ICommand updateBackgroundCommand { get; set; }
        async void updateBackgroundExecute()
        {
            FileResult background = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Select background image"
            });

            if (background != null)
            {
                Preferences.Set("BACKGROUND_IMAGE_PATH", background.FullPath);
            }
        }

        public ICommand logoutCommand { get; set; }
        async void logoutExecute()
        {
            Preferences.Clear();

            Page loginPage = new Views.Login();
            Page rootPage = Application.Current.MainPage.Navigation.NavigationStack[0];
            Application.Current.MainPage.Navigation.InsertPageBefore(loginPage, rootPage);
            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }
    }
}
