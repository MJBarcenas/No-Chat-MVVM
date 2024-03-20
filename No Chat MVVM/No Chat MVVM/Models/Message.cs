using System;
using Xamarin.Forms;

namespace No_Chat_MVVM.Models
{
    public class Message
     {
        public int id { get; set; } 
        public string alias { get; set; }           
        public string message { get; set; }        
        public Uri image { get; set; }       
        public Color color { get; set; }        
        public LayoutOptions layoutOption { get; set; }       
        public int column { get; set; }
        public bool isVisible { get; set; }
    }
}
