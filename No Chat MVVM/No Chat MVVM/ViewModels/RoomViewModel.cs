using Newtonsoft.Json;
using No_Chat_MVVM.Models;
using No_Chat_MVVM.Views;
using Plugin.Connectivity;
using PusherClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace No_Chat_MVVM.ViewModels
{
    public class RoomViewModel : BindableObject
    {
        public RoomViewModel()
        {
            sendCommand = new Command(sendExecute);
            optionCommand = new Command(optionExecute);
            loadMoreDataCommand = new Command(loadMoreDataExecute);
            loadData();
        }

        public ObservableCollection<Models.Message> messagesObservable { get; set; } = new ObservableCollection<Models.Message>();

        ImageSource backgroundImageSource;
        public ImageSource BackgroundImageSource
        {
            get => backgroundImageSource;
            set
            {
                if (value == BackgroundImageSource) { return; }
                backgroundImageSource = value;
                OnPropertyChanged(nameof(BackgroundImageSource));
            }
        }

        string room;
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

        string userMessage;
        public string UserMessage
        {
            get => userMessage;
            set
            {
                if (value == UserMessage) { return; }
                userMessage = value;
                OnPropertyChanged(nameof(UserMessage));
            }
        }

        Uri roomImage;
        public Uri RoomImage
        {
            get => roomImage;
            set
            {
                if (value == RoomImage) { return; }
                roomImage = value;
                OnPropertyChanged(nameof(RoomImage));
            }
        }

        bool sendEnabled = true;
        public bool SendEnabled
        {
            get => sendEnabled;
            set
            {
                sendEnabled = value;
                OnPropertyChanged(nameof(SendEnabled));
            }
        }

        bool refreshViewIsRefreshing = false;
        public bool RefreshViewIsRefreshing
        {
            get => refreshViewIsRefreshing;
            set
            {
                refreshViewIsRefreshing = value;
                OnPropertyChanged(nameof(RefreshViewIsRefreshing));
            }
        }

        ItemsUpdatingScrollMode messageScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView;
        public ItemsUpdatingScrollMode MessageScrollMode
        {
            get => messageScrollMode;
            set
            {
                messageScrollMode = value;
                OnPropertyChanged(nameof(MessageScrollMode));
            }
        }

        public ICommand sendCommand { get; set; }
        private async void sendExecute()
        {
            SendEnabled = false;
            if (!string.IsNullOrWhiteSpace(UserMessage))
            {
                await Module.httpClient.GetAsync($"https://event-venue.website/No_Chat/functions.php?functionname=notifySingleMessage&id=93&message={userMessage}&alias={Module.alias}&userImage={Module.userImage.image}");

                string message = userMessage.Replace("'", "\\'");

                await Module.httpClient.GetAsync($"https://event-venue.website/No_Chat/functions.php?functionname=insertMessage&userMessage={message}&alias={Module.alias}&roomID={Module.room}");

                UserMessage = string.Empty;

            }
            SendEnabled = true;
        }

        public ICommand optionCommand { get; set; }
        private async void optionExecute()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new Views.Option());
        }

        private Channel channel;
        private async Task ListenPublicChannelVerifiedAsync()
        {
            await Module.pusher.ConnectAsync();
            await Module.pusher.SubscribeAsync("my-channel");
            channel = Module.pusher.GetChannel("my-channel");
            channel.Bind("my-event", (PusherEvent eventData) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Message message = JsonConvert.DeserializeObject<Message>(eventData.Data);
                    message.isVisible = true;

                    if (messagesObservable.Count > 0)
                    {
                        Message lastMessage = messagesObservable.Last();

                        if (lastMessage.alias == message.alias)
                        {
                            lastMessage.isVisible = false;
                            lastMessage.image = null;

                            messagesObservable[messagesObservable.Count - 1] = lastMessage;
                        }
                    }
                    
                    if (message.alias == Module.alias)
                    {
                        message.isVisible = false;
                        message.color = Color.FromHex("#ebebeb");
                        message.layoutOption = LayoutOptions.EndAndExpand;
                    } else
                    {
                        message.isVisible = true;
                        message.color = Color.FromHex("#e0d5ff");
                        message.layoutOption = LayoutOptions.StartAndExpand;
                    }

                    messagesObservable.Add(message);
                });
            });
        }

        async void loadData()
        {
            //Check internet connection
            if (!CrossConnectivity.Current.IsConnected)
            {
                await Task.Run(() => { Module.showMessage("Check your internet connection", Color.FromHex("#ed2721"), Application.Current.MainPage); });
                return;
            }

            messagesObservable.Clear();

            Room = $"Room ID: {Module.room}";

            //Connect to pusher
            await ListenPublicChannelVerifiedAsync();
            
            //Get the image for the room
            HttpResponseMessage getRoomResponse = await Module.httpClient.GetAsync($"https://event-venue.website/No_Chat/functions.php?functionname=getImage&type=room&identifier={Module.room}");

            if (getRoomResponse.IsSuccessStatusCode)
            {
                string getRoomResponseString = await getRoomResponse.Content.ReadAsStringAsync();

                Module.roomImage.image = new Uri($"https://event-venue.website/No_Chat/images/room_images/{getRoomResponseString}");
                RoomImage = Module.roomImage.image;

            }


            //Get all the users in the database and store their respective images
            //in an observable collection
            HttpResponseMessage getUsersResponse = await Module.httpClient.GetAsync($"https://event-venue.website/No_Chat/functions.php?functionname=getUsers&roomID={Module.room}");

            if (getUsersResponse.IsSuccessStatusCode)
            {
                string getUsersResponseString = await getUsersResponse.Content.ReadAsStringAsync();

                if (getUsersResponseString == "NONE")
                {
                    return;
                }

                NoChatImage[] getUsersParsedResponseString = JsonConvert.DeserializeObject<NoChatImage[]>(getUsersResponseString);

                for (int i = 0; i < getUsersParsedResponseString.Length; i++)
                {
                    HttpResponseMessage imageResponse = await Module.httpClient.GetAsync($"https://event-venue.website/No_Chat/functions.php?functionname=getImage&type=user&identifier={getUsersParsedResponseString[i].alias}");

                    if (imageResponse.IsSuccessStatusCode)
                    {
                        string imageResponseString = await imageResponse.Content.ReadAsStringAsync();

                        getUsersParsedResponseString[i].image = new Uri($"https://event-venue.website/No_Chat/images/user_images/{imageResponseString}");

                        if (getUsersParsedResponseString[i].alias == Module.alias)
                        {
                            Module.userImage.image = getUsersParsedResponseString[i].image;
                        }
                    }

                    Module.roomUsersImages.Add(getUsersParsedResponseString[i]);
                }

            }

            //Get the messages in the room
            HttpResponseMessage response = await Module.httpClient.GetAsync($"https://event-venue.website/No_Chat/functions.php?functionname=loadPreviousChats&roomID={Module.room}");

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                if (responseString == "NONE")
                {
                    return;
                }
                Message[] parsedResponseString = JsonConvert.DeserializeObject<Message[]>(responseString);

                for (int i = 0; i < parsedResponseString.Length; i++)
                {
                    if ((i + 1 == parsedResponseString.Length || parsedResponseString[i + 1].alias != parsedResponseString[i].alias) && parsedResponseString[i].alias != Module.alias)
                    {
                        parsedResponseString[i].isVisible = true;
                        parsedResponseString[i].image = Module.roomUsersImages.Find((x) => x.alias == parsedResponseString[i].alias).image;
                    }
                    else
                    {
                        parsedResponseString[i].isVisible = false;
                    }

                    if (parsedResponseString[i].alias == Module.alias)
                    {
                        parsedResponseString[i].color = Color.FromHex("#ebebeb");
                        parsedResponseString[i].layoutOption = LayoutOptions.EndAndExpand;
                        parsedResponseString[i].column = 2;
                    }
                    else
                    {
                        parsedResponseString[i].color = Color.FromHex("#e0d5ff");
                        parsedResponseString[i].layoutOption = LayoutOptions.StartAndExpand;
                        parsedResponseString[i].column = 0;
                    }
                    messagesObservable.Add(parsedResponseString[i]);
                }
            }
        }

        public ICommand loadMoreDataCommand { get; set; }
        async void loadMoreDataExecute()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await Task.Run(() => { Module.showMessage("Check your internet connection", Color.FromHex("#ed2721"), Application.Current.MainPage); });
                return;
            }

            RefreshViewIsRefreshing = true;
            MessageScrollMode = ItemsUpdatingScrollMode.KeepItemsInView;

            int lastMessageID = messagesObservable.First().id;
            HttpResponseMessage response = await Module.httpClient.GetAsync($"https://event-venue.website/No_Chat/functions.php?functionname=loadPreviousChatsFrom&id={lastMessageID}&roomID={Module.room}");

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                if (responseString == "NONE")
                {
                    RefreshViewIsRefreshing = false;
                    MessageScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView;
                    return;
                }
                Message[] parsedResponseString = JsonConvert.DeserializeObject<Message[]>(responseString);

                for (int i = 0; i < parsedResponseString.Length; i++)
                {
                    if ((i + 1 == parsedResponseString.Length || parsedResponseString[i + 1].alias != parsedResponseString[i].alias) && parsedResponseString[i].alias != Module.alias)
                    {
                        parsedResponseString[i].isVisible = true;
                        parsedResponseString[i].image = Module.roomUsersImages.Find((x) => x.alias == parsedResponseString[i].alias).image;
                    }
                    else
                    {
                        parsedResponseString[i].isVisible = false;
                    }

                    if (parsedResponseString[i].alias == Module.alias)
                    {
                        parsedResponseString[i].color = Color.FromHex("#ebebeb");
                        parsedResponseString[i].layoutOption = LayoutOptions.EndAndExpand;
                        parsedResponseString[i].column = 2;
                    }
                    else
                    {
                        parsedResponseString[i].color = Color.FromHex("#e0d5ff");
                        parsedResponseString[i].layoutOption = LayoutOptions.StartAndExpand;
                        parsedResponseString[i].column = 0;
                    }
                    messagesObservable.Insert(0, parsedResponseString[i]);
                }
            }

            RefreshViewIsRefreshing = false;
            MessageScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView;
        }
    }
}
