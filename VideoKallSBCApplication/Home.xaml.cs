using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VideoKallSBCApplication.Communication;
using VideoKallSBCApplication.SerialPort;
using VideoKallSBCApplication.TestResults;
using VideoKallSBCApplication.ViewModel;
using VideoKallSMC;
using VideoKallSMC.Communication;
using VideoKallSMC.Views;
using Windows.ApplicationModel.Core;
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
using static VideoKallSBCApplication.MainPage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VideoKallSBCApplication
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Home : Page
    {

        #region properties 
        public CommunicationChannel commChannel { get; set; }
        public DataacquistionappComm DataacqAppComm { get; set; }
        public SerialPortComm SerialPortCommchannel { get; set; }
        public GlucoUtility GlucoCMDUtitlity { get; } = new GlucoUtility();
        public static TestResultsModel _testResult = new TestResultsModel();
        public static TestResultsModel TestresultModel { get { return _testResult; } }
        #endregion
        bool isMCCConnectedFirstTime = true;
        bool IsDatacquistionappconnected { get; set; } = false;
        DispatcherTimer Watchdog = null;
        StethoscopeEvents StartStethoscope;



        public static Home HomePage;
        public HomeViewModel HomeVM = null;
 



        public   Home()
        {
            HomePage = this;
            this.InitializeComponent();
            HomeVM = new HomeViewModel();
            this.DataContext = HomeVM;
            TestresultModel.NotifyStatusMessage = UpdateNotification;
            TestresultModel.StethoscopeTx.TXevents += Tx_TXevents;
            StartStethoscope += StartST;
            commChannel = new CommunicationChannel();
            commChannel.MessageReceived += CommChannel_MessageReceived;
            commChannel.ErrorMessage += CommChannel_ErrorMessage;
            commChannel.Initialize();
            commChannel.Listen();
            HomeVM.UpdateIPaddress(commChannel.IPAddress, commChannel.PortNo);
      
        }

        async void UpdateNotification(string s, int code)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // StatusTxt.Text = s;
            });
        }
        async void StartST(bool islungs)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TestresultModel.StethoscopeTx.Initialize(islungs);
            });
        }
        async void StartDermascope()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                string msg = string.Format(CommunicationCommands.DERMASACOPE,
                1, MainPage.mainPage.RightPanelHolder.ActualHeight, MainPage.mainPage.RightPanelHolder.ActualWidth,
              MainPage.mainPage.ActualWidth, MainPage.mainPage.ActualHeight);

                MainPage.mainPage.DataacqAppComm.SendMessageToDataacquistionapp(msg);
            });

        }
        void StopDermascope()
        {
            string msg = string.Format(CommunicationCommands.STOPDERMO);
            MainPage.mainPage.DataacqAppComm.SendMessageToDataacquistionapp(msg);
        }

        async void StartOtosope()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                string msg = string.Format(CommunicationCommands.OTOSCOPE,
               1, MainPage.mainPage.RightPanelHolder.ActualHeight, MainPage.mainPage.RightPanelHolder.ActualWidth,
             MainPage.mainPage.ActualWidth, MainPage.mainPage.ActualHeight);

                MainPage.mainPage.DataacqAppComm.SendMessageToDataacquistionapp(msg);
            });

        }

        void StopOtoscope()
        {
            string msg = string.Format(CommunicationCommands.STOPOTOSCOPE);
            MainPage.mainPage.DataacqAppComm.SendMessageToDataacquistionapp(msg);
        }
        public void TakePic()
        {
            string msg = string.Format(CommunicationCommands.TAKEPIC);
            MainPage.mainPage.DataacqAppComm.SendMessageToDataacquistionapp(msg);
        }
        void StartDeviceCommand(String cmd)
        {
            SerialPortCommchannel?.WriteData(cmd);
        }
        int PatientID = 1;
        void SaveImage(bool isDermascope = false)
        {
            // StorageFolder storageFolder = KnownFolders.PicturesLibrary;
            //StorageFolder SavedImageFolder = null;
            //try
            //{
            //    SavedImageFolder = await storageFolder.CreateFolderAsync("VideoKall\\SavedImage", CreationCollisionOption.FailIfExists);
            //}
            //catch(Exception ex)
            //{
            //    SavedImageFolder = await StorageFolder.GetFolderFromPathAsync(storageFolder.Path + "\\VideoKall\\SavedImage");
            //}

            try
            {
                if (!isDermascope)
                    MainPage.mainPage.DataacqAppComm.SendMessageToDataacquistionapp("otosaveimage>" + PatientID.ToString());
                else
                    MainPage.mainPage.DataacqAppComm.SendMessageToDataacquistionapp("dersaveimage>" + PatientID.ToString());




                //   string p = SavedImageFolder.Path + "\\" + "capturedImage.png";
                //StorageFile f = await StorageFile.GetFileFromPathAsync(p);
                //if (f != null)
                //{
                //    await f.RenameAsync(fname);
                //    await f.MoveAsync(SavedImageFolder);
                //}
                //    commChannel.SendMessageToMCC(CommunicationCommands.NotifySAVEDIMAGE);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private async void Tx_TXevents(object sender, EventArgs e)
        {
            if (((string)sender).ToLower().Contains(("Ready for streaming at").ToLower()))
            {
                commChannel.SendMessageToMCC(string.Format(CommunicationCommands.STCHESTRESPONSE, (string)sender));
            }
            else
            {
                commChannel.SendMessageToMCC(string.Format(CommunicationCommands.STMSG, (string)sender));
            }

            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // StatusTxt.Text = (string)sender;

            });

        }

        private async void Page_Loaded(object sender, RoutedEventArgs e) {
            //   scokcomm.Initialize();
            //   scokcomm.Connect();
            SerialPortCommchannel = new SerialPortComm();
            Watchdog = new DispatcherTimer();
            Watchdog.Tick += Watchdog_Tick;
            Watchdog.Interval = new TimeSpan(0, 0, 0, 0, 500);

            Watchdog.Start();
            DataacqAppComm = new DataacquistionappComm();
            DataacqAppComm.MessageReceived += CommChannel_MessageReceived;
            //  DataacqAppComm.Initialize();
            // await DataacqAppComm.Connect();    



            DataacqAppComm.SendMessageToDataacquistionapp(CommunicationCommands.DATAACQUISTIONAPPCONNECTION);
            await DataacqAppComm?.ResetCommFiles();
        }
        int watchdogcount = 0;
        private void Watchdog_Tick(object sender, object e)
        {
            Watchdog.Stop();

            watchdogcount++;
            if (watchdogcount > 2)
                DataacqAppComm.ReadCommFile();

            if (watchdogcount > 3)
            {
                watchdogcount = 0;
            }

            Watchdog.Start();
        }

        private void CommChannel_ErrorMessage(object sender, CommunicationMsg e)
        {
            
        }

        private void CommChannel_MessageReceived(object sender, CommunicationMsg msg)
        {
            switch (msg.Msg.ToLower())
            {
                case "<mccs>":
                case "<smcc>":
                    if (isMCCConnectedFirstTime)
                    {
                        commChannel.SendMessageToMCC(CommunicationCommands.SBCStart);
                        isMCCConnectedFirstTime = false;
                    }
                    else
                        commChannel.SendMessageToMCC(CommunicationCommands.SBCConnectionResponseCmd);
                    break;
                case "<p1d>":
                    //    StartDeviceCommand("<P1D>");
                    break;
                case "<p1ds>":
                    //   StopTimer();
                    break;
                case "<pulsestart>":
                    MainPage.TestresultModel.oxymeter.Connect();
                    break;
                case "<glucmd>":
                    //   TestresultModel.GlucoCMDUtitlity.LatestTestResult();
                    GlucoCMDUtitlity.LatestTestResult();
                    break;
                case "<thermocmd>": //< THERMOCMD >
                    MainPage.TestresultModel.Thermo.Connect();
                    break;
                case "<bpcmd>": //BPCMD
                    TestresultModel.bpcuff?.Connect();

                    break;
                case "<bpconcmd>": //<BPCONCMD>
                    string res = string.Format(CommunicationCommands.BPCONNECTIONTIME, TestresultModel.IsBpConnected.ToString(), TestresultModel.BpCuffConnectionTime);
                    commChannel.SendMessageToMCC(res);
                    break;
                case "<appc>":
                    // DataacqAppComm.SendMessageToDataacquistionapp(CommunicationCommands.DATAACQSTATUS);
                    break;

                case "<apps>":
                    IsDatacquistionappconnected = true;
                    // DataacqAppComm.SendMessageToDataacquistionapp(CommunicationCommands.DATAACQSTATUS);
                    break;
                case "<startdermo>":
                    //  if( IsDatacquistionappconnected)
                    {
                        StartDermascope();
                    }
                    break;
                case "<stopdermo>":
                    //   if (IsDatacquistionappconnected)
                    {
                        StopDermascope();
                    }
                    break;
                case "<startoto>":
                    //    if (IsDatacquistionappconnected)
                    {
                        StartOtosope();
                    }
                    break;
                case "<stopoto>":
                    //     if (IsDatacquistionappconnected)
                    {
                        StopOtoscope();
                    }
                    break;

                case "<pic>":
                    //    if (IsDatacquistionappconnected)
                    {
                        TakePic();
                    }
                    break;
                case "<startstchecst>":

                    StartStethoscope?.Invoke(false);
                    break;
                case "<startstlungs>":
                    StartStethoscope?.Invoke(true);
                    break;
                case "<otosaveimage>":
                    {
                        SaveImage();
                    }
                    break;
                case "<dersaveimage>":
                    {
                        SaveImage(true);
                    }
                    break;
                case "<startspirofvc>":
                case "<startspirovc>":
                case "<stopspiro>":
                    {
                        MainPage.mainPage.DataacqAppComm.SendMessageToDataacquistionapp(msg.Msg);
                    }
                    break;

            }

            if (msg.Msg.ToLower().Contains("stpic>") ||
                msg.Msg.ToLower().Contains("drpic>") ||
                msg.Msg.ToLower().Contains("mrpic>") ||
                 msg.Msg.ToLower().Contains("mrexp>") ||
                 msg.Msg.ToLower().Contains("imagesaved>"))
            {
                commChannel.SendMessageToMCC(msg.Msg);
            }
        }

        private void BtnPatient_Click(object sender, RoutedEventArgs e)
        {
            // MainPage.mainPage.pagePlaceHolder.Navigate(typeof(Videocallpage));
            this.Frame.Navigate(typeof(Videocallpage));
           // MainPage.mainPage.pagePlaceHolder.Navigate(typeof(Videocallpage));
        }

        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoginPage));
        }
    }
}
