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
using VideoKallSBCApplication.Stethosope;
using Windows.Storage;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VideoKallSMC.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        StConfig Dfaultsettings;
        StConfig StSettings;
        string StConfigFile = "ST_sbcConfig.txt";
        public Settings()
        {
            this.InitializeComponent();
            DeviceListDisplay();
          
            Dfaultsettings.IP = MainPage.mainPage.commChannel.IPAddress;
            Dfaultsettings.PORT = "8445";
            Dfaultsettings.USERNAME = "rnk";
            Dfaultsettings.PASSWORD = "test";
            Dfaultsettings.CUTOFFFILTER = "0";
            Dfaultsettings.CUTOFFFILTERLUNGS = "-259";
            Dfaultsettings.FREQUENCYHEART = "8000";
            Dfaultsettings.FREQUENCYLUNGS = "8000";
            Dfaultsettings.GAIN = "0";
            Dfaultsettings.CODEC = "";
            Dfaultsettings.RECORDING_DEVICE_ID = "7";
            //RECORDING-DEVICE-ID:11

            StSettings.IP = MainPage.mainPage.commChannel.IPAddress;
            StSettings.PORT = "8445";
            StSettings.USERNAME = "rnk";
            StSettings.PASSWORD = "test";
            StSettings.CUTOFFFILTER = "0";
            StSettings.CUTOFFFILTERLUNGS = "-259";
            StSettings.FREQUENCYHEART = "8000";
            StSettings.FREQUENCYLUNGS = "8000";
            StSettings.GAIN = "0";
            StSettings.CODEC = "";
            StSettings.RECORDING_DEVICE_ID = "7";
            ReadRecordingDevices();
            // IP: 192.168.0.33
            //PORT: 8445
            //USERNAME: rnk
            // PASSWORD:test
            // CUTOFFFILTER:0
            //CUTOFFFILTERLUNGS: -259
            //FREQUENCYHEART: 8000
            //FREQUENCYLUNGS: 8000
            //GAIN: 0
            //CODEC: ""
            //RECORDING - DEVICE - ID:11
        }

        async void ReadSTConfigFile()
        {
            try
            {
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile IpAddressFile = await localFolder.GetFileAsync(StConfigFile);
                var alltext = await Windows.Storage.FileIO.ReadLinesAsync(IpAddressFile);
                foreach (var line in alltext)
                {
                    string[] data = line.Split(':');
                    switch (data[0])
                    {
                        case "IP":
                            StSettings.IP = data[1];
                            break;
                        case "PORT":// 8445
                            StSettings.PORT = data[1];
                            break;
                        case "USERNAME":
                            StSettings.USERNAME = data[1];
                            break;
                        case "PASSWORD":
                            StSettings.PASSWORD = data[1];
                            break;
                        case "CUTOFFFILTER":
                            StSettings.CUTOFFFILTER = data[1];
                            break;
                        case "CUTOFFFILTERLUNGS":
                            StSettings.CUTOFFFILTERLUNGS = data[1];
                            break;
                        case "FREQUENCYHEART":
                            StSettings.FREQUENCYHEART = data[1];
                            break;
                        case "FREQUENCYLUNGS":
                            StSettings.FREQUENCYLUNGS = data[1];
                            break;
                        case "GAIN":
                            StSettings.GAIN = data[1];
                            break;
                        case "CODEC":
                            StSettings.CODEC = data[1];
                            break;
                        case "RECORDING-DEVICE-ID":
                            StSettings.RECORDING_DEVICE_ID = data[1];
                            break;

                    }
                }
            }
            catch (Exception) { }
            TxtFilterHeart.Text = StSettings.CUTOFFFILTER;
            TxtFilterlungs.Text = StSettings.CUTOFFFILTERLUNGS;
            TxtFrequencyHeart.Text = StSettings.FREQUENCYHEART;
            TxtFrequencylungs.Text = StSettings.FREQUENCYLUNGS;
            RecdevID.Text = StSettings.RECORDING_DEVICE_ID;
            for (int i = 0; i < CmbRecordingDevices.Items.Count; i++)
            {
                string val = (string)CmbRecordingDevices.Items[i];
                if (val.Contains(StSettings.RECORDING_DEVICE_ID))
                {
                    CmbRecordingDevices.SelectedIndex = i;
                    break;
                }
            }
        }

        async void WriteConfigFile()
        {
            try
            {
               string RecDevice = (string)CmbRecordingDevices.SelectedValue;
                StSettings.RECORDING_DEVICE_ID = RecDevice.Split(':')[0];
                StSettings.CUTOFFFILTER = TxtFilterHeart.Text;
                StSettings.CUTOFFFILTERLUNGS = TxtFilterlungs.Text;
                StSettings.FREQUENCYHEART = TxtFrequencyHeart.Text;
                StSettings.FREQUENCYLUNGS = TxtFrequencylungs.Text;
                RecdevID.Text = StSettings.RECORDING_DEVICE_ID;
                string msg =  "IP" + ":" + StSettings.IP + Environment.NewLine +
                             "PORT" + ":" + StSettings.PORT + Environment.NewLine +
                             "USERNAME" + ":" + StSettings.USERNAME + Environment.NewLine +
                             "PASSWORD" + ":" + StSettings.PASSWORD + Environment.NewLine +
                             "CUTOFFFILTER" + ":" + StSettings.CUTOFFFILTER + Environment.NewLine +
                             "CUTOFFFILTERLUNGS" + ":" + StSettings.CUTOFFFILTERLUNGS + Environment.NewLine +
                             "FREQUENCYHEART" + ":" + StSettings.FREQUENCYHEART + Environment.NewLine +
                             "FREQUENCYLUNGS" + ":" + StSettings.FREQUENCYLUNGS + Environment.NewLine +
                             "GAIN" + ":" + StSettings.GAIN + Environment.NewLine +
                             "CODEC" + ":" + StSettings.CODEC + Environment.NewLine +
                             "RECORDING-DEVICE-ID" + ":" + StSettings.RECORDING_DEVICE_ID + Environment.NewLine;

                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile pinfofile = await localFolder.CreateFileAsync(StConfigFile, CreationCollisionOption.OpenIfExists);
                await Windows.Storage.FileIO.WriteTextAsync(pinfofile, msg, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            }
            catch (Exception)
            { }
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

        private void BtnRec_Click(object sender, RoutedEventArgs e)
        {
             Stethoscope tx = new Stethoscope();
             tx.GenerateRecordingDevices();
            ReadRecordingDevices();
        }

       async void ReadRecordingDevices()
        {
            try {
                CmbRecordingDevices.Items.Clear();
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile sampleFile = await localFolder.GetFileAsync("RecordingDevices.txt");
                var alltext = await Windows.Storage.FileIO.ReadLinesAsync(sampleFile);
                //  ID: DEVICE NAME : DEVICE TYPE
                //------------------------------------------------------------
                foreach (var line in alltext)
                {
                    if (line.ToLower().Contains(("ID: DEVICE NAME : DEVICE TYPE").ToLower()) ||
                        line.Contains("--------------"))
                    {
                        continue;
                    }
                    CmbRecordingDevices.Items.Add(line);
                }
                if (CmbRecordingDevices.Items.Count > 0)
                    CmbRecordingDevices.SelectedIndex = 0;
            }
            catch(Exception) {  }
            
        }

        private void TxtFrequencyHeart_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!TxtFrequencyHeart.Text.All(char.IsDigit))
                TxtFrequencyHeart.Text = "";
        }

        private void TxtFrequencylungs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!TxtFrequencylungs.Text.All(char.IsDigit))
                TxtFrequencylungs.Text = "";
        }

        private void TxtFilterHeart_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!TxtFilterHeart.Text.All(char.IsDigit))
                TxtFilterHeart.Text = "";
        }

        private void TxtFilterlungs_TextChanged(object sender, TextChangedEventArgs e)
        {
            string val = TxtFilterlungs.Text;
            if (val.Contains("-"))
            {
                string txt = val.Substring(1);
                if (!txt.All(char.IsDigit))
                    TxtFilterlungs.Text = "";
            }
            else
            {
                if (!TxtFilterlungs.Text.All(char.IsDigit))
                    TxtFilterlungs.Text = "";
            }
            
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            WriteConfigFile();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            
            ReadSTConfigFile();
        }

       
    }
    struct StConfig
    {
      public  string IP;
      public  string PORT;//:8445
      public  string USERNAME;//:rnk
      public  string PASSWORD;//:test
      public  string CUTOFFFILTER;//:0
      public  string CUTOFFFILTERLUNGS;//:-259
      public  string FREQUENCYHEART;//:8000
      public  string FREQUENCYLUNGS;//:8000
      public  string GAIN;//:0
      public  string CODEC;//:""
      public  string RECORDING_DEVICE_ID;//:11
    }
}
