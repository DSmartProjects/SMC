using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VideoKallSBCApplication;
using VideoKallSMC.Views;
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
    public sealed partial class BPCuffPage : Page,  INotifyPropertyChanged
    {
        public BPCuffPage()
        {
            this.InitializeComponent();
            Resultgrid.DataContext = this;
            MainPage.TestresultModel.BpEvent += BPResultCallback;
            MainPage.TestresultModel.DeviceConnectionTimeCallback += DeviceConnectionStatus;
        }


        async void DeviceConnectionStatus(DeviceTypesenums type, string datetime, bool status)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (type == DeviceTypesenums.BPMONITOR && status)
                {
                    isConnected = status;
                    ConnectionTime = datetime;
                    OnPropertyChanged("ConnectionTime");
                    OnPropertyChanged("isConnected");
                }
            });
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        async void  BPResultCallback(BPResult res)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TestDateTime = res.DateTimeOfTest.ToString();
                ResultSys = res.SYS.ToString();
                ResultDia = res.DIA.ToString();
                ResultPulse = res.Pulse.ToString();

                OnPropertyChanged("TestDateTime");
                OnPropertyChanged("ResultSys");
                OnPropertyChanged("ResultDia");
                OnPropertyChanged("ResultPulse");
            });
        }

        public string TestDateTime { get; set; }
        public string ResultSys { get; set; }
        public string ResultDia { get; set; } 
        public string ResultPulse { get; set; }
        public string ConnectionTime { get; set; }
        public bool isConnected { get; set; } = false;
        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            MainPage.TestresultModel.bpcuff?.Connect( );
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            isConnected = MainPage.TestresultModel.IsBpConnected;
            ConnectionTime = MainPage.TestresultModel.BpCuffConnectionTime;
            OnPropertyChanged("isConnected");
            OnPropertyChanged("ConnectionTime");
        }
    }
}
