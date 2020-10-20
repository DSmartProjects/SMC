using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VideoKallSBCApplication;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VideoKallSBCApplication.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DeviceListPage : Page
    {

        public static CoreDispatcher dispatcher;
       public DeviceListPageViewModel vmdevicelist = new DeviceListPageViewModel();
     
        public DeviceListPage( )
        {
            this.InitializeComponent();
            this.DataContext =  vmdevicelist;
             vmdevicelist.NotifyStatusMessage += UpdateStatus;
            dispatcher = Window.Current.Dispatcher;
            BleList.ItemsSource = vmdevicelist.BLEDevices; 
        }
        private async void UpdateStatus(string msg)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MainPage.mainPage.StatusTxt.Text = msg;
            });
        }

        private void BleList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var bleDeviceDisplay = e;
        }
    }
}
