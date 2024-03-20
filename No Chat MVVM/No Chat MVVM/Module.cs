using No_Chat_MVVM.Models;
using PusherClient;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.UI.Views.Options;
using Xamarin.Forms;

namespace No_Chat_MVVM
{
    public class Module
    {   
        //For internet access
        public static readonly HttpClient httpClient = new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = ServerCertificateValidation });
        private static bool ServerCertificateValidation(HttpRequestMessage requestMessage, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslErrors)
        {
            return true;
        }

        //Constants string
        public static string alias = string.Empty;
        public static string room = string.Empty;

        //Constants images
        public static NoChatImage userImage = new NoChatImage();
        public static NoChatImage roomImage = new NoChatImage();
        public static List<NoChatImage> roomUsersImages { get; set; } = new List<NoChatImage>();
        
        public static ImageSource backgroundImageSource;

        //Pusher settings
        public static Pusher pusher = new Pusher("efe71225fd32050a8ab8", new PusherOptions
        {
            Cluster = "ap1",
            Encrypted = true,
        });

        //Functions
        public async static void showMessage(string message, Color color, Page page)
        {
            MessageOptions messageOptions = new MessageOptions
            {
                Message = message,
                Foreground = Color.White,
                Font = Font.SystemFontOfSize(16),
                Padding = new Thickness(20)
            };

            ToastOptions options = new ToastOptions
            {
                MessageOptions = messageOptions,
                CornerRadius = new Thickness(4, 4, 4, 4),
                BackgroundColor = color
            };

            await page.DisplayToastAsync(options);
        }
    }
}
