using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VideoKallSMC;
using VideoKallSMC.ViewModel;
using VideoKallSMC.Views;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using VideoKallSBCApplication;
using Windows.UI.Core;
using VideoKallSMC.Communication;
using VideoKallSBCApplication.SerialPort;
using System.Threading.Tasks;
using VideoKallSBCApplication.TestResults;
using System.IO.MemoryMappedFiles;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Pipes;
 
using System.Text;
using System.Threading;
using VideoKallSBCApplication.Communication;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using VideoKallSBCApplication.ViewModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VideoKallSBCApplication
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static Windows.Media.MediaExtensionManager mediaExtensionMgr;
        public void EnsureMediaExtensionManager()
        {
            if (mediaExtensionMgr == null)
            {
                mediaExtensionMgr = new Windows.Media.MediaExtensionManager();
                mediaExtensionMgr.RegisterSchemeHandler("Microsoft.Samples.SimpleCommunication.StspSchemeHandler", "stsp:");
            }
        }
        public static TestPanelViewModel TestPanelVM = null;
        public   static TestResultsModel _testResult = new TestResultsModel();
        public static TestResultsModel TestresultModel { get { return _testResult; } }
        public static MainPage mainPage = null;
        public MainPageViewModel mainpagecontext = new MainPageViewModel();
        DispatcherTimer   Watchdog = null;
        public delegate void StethoscopeEvents(bool islungs);
        StethoscopeEvents StartStethoscope;
        bool isMCCConnectedFirstTime = true;
        public MainPage()
        {
            this.InitializeComponent();
            mainPage = this;
            TestPanelVM = new TestPanelViewModel();
            this.DataContext = mainpagecontext;
            RightPanelHolder.Navigate(typeof(LoginPage));
             pagePlaceHolder.Navigate(typeof(LogoPage));
            TestresultModel.NotifyStatusMessage = UpdateNotification;
            TestresultModel.StethoscopeTx.TXevents += Tx_TXevents;
            StartStethoscope += StartST;
        }


        async void UpdateNotification(string s, int code)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // StatusTxt.Text = s;
            });
        }
        

        public DataacquistionappComm DataacqAppComm { get; set; }
      public SerialPortComm SerialPortCommchannel{ get; set; }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            commChannel = new CommunicationChannel();
            commChannel.MessageReceived += CommChannel_MessageReceived;
            commChannel.ErrorMessage += CommChannel_ErrorMessage;
            commChannel.Initialize();
            commChannel.Listen();
            mainpagecontext.UpdateIPaddress(commChannel.IPAddress, commChannel.PortNo);

            SerialPortCommchannel = new SerialPortComm();

            Watchdog = new DispatcherTimer();
            Watchdog.Tick += Watchdog_Tick;
            Watchdog.Interval = new TimeSpan(0, 0, 0,0,500);
            DataacqAppComm = new DataacquistionappComm();
              DataacqAppComm.MessageReceived += CommChannel_MessageReceived;
            //  DataacqAppComm.Initialize();
            // await DataacqAppComm.Connect();
            await DataacqAppComm.ResetCommFiles();
            DataacqAppComm.SendMessageToDataacquistionapp(CommunicationCommands.DATAACQUISTIONAPPCONNECTION);
            Watchdog.Start();

          //   scokcomm.Initialize();
         //   scokcomm.Connect();
        }
        socketComm scokcomm = new socketComm();
        bool IsDatacquistionappconnected { get; set; } = false;
        int watchdogcount = 0;
        private    void Watchdog_Tick(object sender, object e)
        {
            Watchdog.Stop();
          
            watchdogcount++;
            //if (!IsDatacquistionappconnected)
            //    DataacqAppComm.SendMessageToDataacquistionapp(CommunicationCommands.DATAACQUISTIONAPPCONNECTION);
            //   Task<string> loadAsyncTask = SerialPortCommchannel.ReadData();
            //  string bytesRead = await loadAsyncTask;
            if (watchdogcount > 2)
                 DataacqAppComm.ReadCommFile();

            if (watchdogcount >3)
            {
                watchdogcount = 0;
                // IsDatacquistionappconnected = false;
                // DataacqAppComm.SendMessageToDataacquistionapp(CommunicationCommands.DATAACQUISTIONAPPCONNECTION);
            }

            Watchdog.Start();
        }

        void StopTimer()
        {
            Watchdog.Stop();
        }

        private void CommChannel_ErrorMessage(object sender, CommunicationMsg e)
        {
             
        }
        public GlucoUtility GlucoCMDUtitlity { get; } = new GlucoUtility();
        private void CommChannel_MessageReceived(object sender, CommunicationMsg msg)
        {
            switch(msg.Msg.ToLower())
            {
                case "<mccs>":
                case "<smcc>":
                    if(isMCCConnectedFirstTime)
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

            if(msg.Msg.ToLower().Contains("stpic>") || 
                msg.Msg.ToLower().Contains("drpic>") ||
                msg.Msg.ToLower().Contains("mrpic>") ||
                 msg.Msg.ToLower().Contains("mrexp>") ||
                 msg.Msg.ToLower().Contains("imagesaved>"))
            {
                commChannel.SendMessageToMCC(msg.Msg);
            }

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
                if(!isDermascope)
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
            catch(Exception ex)
            {
                throw ex;
            }
           
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

      async  void StartOtosope()
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

        private async void Tx_TXevents(object sender, EventArgs e)
        {
           if(((string)sender).ToLower().Contains(("Ready for streaming at").ToLower()))
            {
                commChannel.SendMessageToMCC(string.Format(CommunicationCommands.STCHESTRESPONSE,(string)sender));
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

        public async void LogExceptions(string exception)
        {

            try
            {
                string msg = exception + Environment.NewLine;
                // msg = Environment.NewLine + msg + Environment.NewLine;
                string filename = "Exceptionlogs.txt";
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile pinfofile = await localFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
                //  await Windows.Storage.FileIO.AppendTextAsync(pinfofile, msg, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                await Windows.Storage.FileIO.WriteTextAsync(pinfofile, msg, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            }
            catch (Exception)
            { }
        }
        public CommunicationChannel commChannel { get; set; }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            commChannel.SendMessageToMCC(CommunicationCommands.SBCShutdown);
        }
         
    } 
}
