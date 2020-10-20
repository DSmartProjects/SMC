using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VideoKallSBCApplication.Model;
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

namespace VideoKallSBCApplication.TestResults
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OxymeterResults : Page  
    {
        public OxymeterResults()
        {
            this.InitializeComponent();
  
            MainPage.TestresultModel.oxymeterDataReceiveCallback += OnDataReceive;
            MainPage.TestresultModel.DeviceConnectionTimeCallback += DeviceConnectionStatus;
        }

        private async void DeviceConnectionStatus(DeviceTypesenums type, string parm2, bool status)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (type == DeviceTypesenums.OXIMETER)
                {
                    TxtConnectionTime.Text = parm2;
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
         async void OnDataReceive(PulseOxyResult data)
        {
             await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
             {
                 TextSPO2.Text = data.spo2;
                 PRData.Text = data.PR;
                 TestDatetime.Text = data.DateTimeOfTest.ToString();
            });
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            MainPage.TestresultModel.oxymeter.Connect();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
 
        }
    }
}
