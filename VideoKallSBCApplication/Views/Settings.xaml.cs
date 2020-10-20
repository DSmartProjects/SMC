using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VideoKallSBCApplication;
using VideoKallSBCApplication.SerialPort;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VideoKallSMC.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
            DeviceListDisplay();
        }

          
       void DeviceListDisplay()
        {
            serialPortCmb.ItemsSource = MainPage.mainPage.SerialPortCommchannel.DeviceList.Keys.ToArray();
            serialPortCmb.SelectedIndex = 0;
         
        }

        private void BtnRefreshcomport_Click(object sender, RoutedEventArgs e)
        {
            DeviceListDisplay();
            MainPage.mainPage.mainpagecontext.UpdateIPaddress(MainPage.mainPage.commChannel.IPAddress, MainPage.mainPage.commChannel.PortNo);
        }

        private void SerialPortCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainPage.mainPage.SerialPortCommchannel.SelectDevice = MainPage.mainPage.SerialPortCommchannel.DeviceList.ElementAt(serialPortCmb.SelectedIndex).Value;
         }

        private void Chkdiagnostic_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox c = (CheckBox)e.OriginalSource;
         

            MainPage.mainPage.RightPanelHolder.Visibility = (bool)c.IsChecked ? Visibility : Visibility.Collapsed;
        }
    }
}
