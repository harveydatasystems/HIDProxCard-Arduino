using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            reader = new ProxCardReader();
            reader.CardReceived += Reader_CardReceived;
            reader.InitWatcher();
        }
        
        private ProxCardReader reader;

        private void Reader_CardReceived(object sender, CardDataEventArgs e)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"Facility: {e.FacilityId}");
            sb.AppendLine($"Card:     {e.CardId}");
            sb.AppendLine($"{DateTime.Now}");

            output.Text = sb.ToString();
        }

        





    }
}
