using Microsoft.Samples.SimpleCommunication;
using System;
using System.Threading;
using System.Threading.Tasks;
using VideoKallSBCApplication;
using VideoKallSBCApplication.Communication;
using VideoKallSMC.ViewModel;
using Windows.Media.MediaProperties;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using System.Runtime.InteropServices;
using VideoKallSBCApplication.Views;
using Windows.ApplicationModel.Core;
using System.ComponentModel;

namespace VideoKallSMC.Views
{
    public sealed partial class Videocallpage : Page
    {
        // public VideoViewModel VideoVM { get; set; }
        public MainPageViewModel MainPageVM { get; set; }
        MainPage rootPage = MainPage.mainPage;
        CaptureDevice device = null;
        bool? roleIsActive = null;
        int isTerminator = 0;
        bool activated = false;
        string ipAddress = string.Empty;

        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int WM_APPCOMMAND = 0x319;
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(int Msg, IntPtr lParam);

        public Videocallpage()
        {
            this.InitializeComponent();
            rootPage.EnsureMediaExtensionManager();
            DefaultVisibilities();
        }
        public void DefaultVisibilities()
        {
            //HostNameTextbox.Visibility = Visibility.Visible;
            btnInitConsult.Visibility = Visibility.Visible;
            PreviewVideo.Visibility = Visibility.Collapsed;
            RemoteVideo.Visibility = Visibility.Visible;
            CallingScreen.Visibility = Visibility.Collapsed;
            //OutgoingCall.Visibility = Visibility.Collapsed;
            btnEndConsult.Visibility = Visibility.Collapsed;
           // HostNameTextbox.Visibility = Visibility.Visible;
        }

        private async void CallDevice(CaptureDevice device)
        {
            try
            {
                if (device != null)
                {
                    PreviewVideo.Source = device.CaptureSource;
                    await device.CaptureSource.StartPreviewAsync();
                }
                if (device == null)
                {
                    //Debug.WriteLine("No camera device found!");
                    return;
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //ipAddress = (string)e.Parameter;
            ipAddress = MainPage.mainPage.mainpagecontext.NPT_IPAddress;
            InitializePreviewVideo();
        }

        private async void InitiateConsultation()
        {
            try
            {
                var data = await MainPage.mainPage.mainpagecontext.ReadNPTConfig();
                if (!string.IsNullOrEmpty(MainPage.mainPage.mainpagecontext.NPT_IPAddress))
                {
                    var address = MainPage.mainPage.mainpagecontext.NPT_IPAddress; //VideoVM!=null&&!string.IsNullOrEmpty(VideoVM.IpAddress)?VideoVM.IpAddress:string.Empty;
                    roleIsActive = true;
                    RemoteVideo.Source = new Uri("stsp://" + address);
                    PreviewVideo.Visibility = Visibility.Visible;
                  //  HostNameTextbox.IsEnabled = btnInitConsult.IsEnabled = false;
                    RemoteVideo.Visibility = Visibility.Collapsed;
                    CallingScreen.Play();                  
                    CallingScreen.Visibility = Visibility.Visible;
                    //OutgoingCall.Visibility = Visibility.Visible;
                    //OutgoingCall.Play();
                    btnEndConsult.Visibility = Visibility.Visible;
                    btnInitConsult.Visibility = Visibility.Collapsed;
                   // HostNameTextbox.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
               
        public async void InitializePreviewVideo()
        {
            try
            {
                var cameraFound = await CaptureDevice.CheckForRecordingDeviceAsync();

                if (cameraFound)
                {
                    device = new CaptureDevice();
                    await InitializeAsync();
                    CallDevice(device);
                    device.IncomingConnectionArrived += Device_IncomingConnectionArrived;
                    device.CaptureFailed += Device_CaptureFailed;
                    RemoteVideo.MediaFailed += RemoteVideo_MediaFailed;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if (activated)
            {
                RemoteVideo.Stop();

                RemoteVideo.Source = null;
            }

            if (device != null)
            {
                await device.CleanUpPreviewAsync();
                device = null;
            }
        }

        const int ERROR_FILE_NOT_FOUND = 2;
        const int ERROR_ACCESS_DENIED = 5;
        const int ERROR_NO_APP_ASSOCIATED = 1155;
        private async Task InitializeAsync(CancellationToken cancel = default(CancellationToken))
        {
            try
            {
                if (device != null)
                {
                    await device.InitializeAsync();
                    await StartRecordToCustomSink();

                    //HostNameTextbox.IsEnabled = btnInitConsult.IsEnabled = true;
                    btnEndConsult.IsEnabled = true;
                    RemoteVideo.Source = null;

                    // Each client starts out as passive
                    roleIsActive = false;
                    Interlocked.Exchange(ref isTerminator, 0);
                }
            }
            catch (Win32Exception e)
            {
                if (e.NativeErrorCode == ERROR_FILE_NOT_FOUND ||
                    e.NativeErrorCode == ERROR_ACCESS_DENIED ||
                    e.NativeErrorCode == ERROR_NO_APP_ASSOCIATED)
                {
                    MainPage.mainPage.pagePlaceHolder.Navigate(typeof(Videocallpage));
                    // MainPage.mainPage.pagePlaceHolder.Navigate(typeof(LogoPage));
                    //this.Frame.Navigate(typeof(CallSummary));
                }
            }
            catch (Exception)
            {
                //rootPage.NotifyUser("Initialization error. Restart the sample to try again.", NotifyType.ErrorMessage);
            }

        }

        async void RemoteVideo_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (Interlocked.CompareExchange(ref isTerminator, 1, 0) == 0)
            {
                ContentDialog errorMessage = new ContentDialog
                {
                    Content = "Nurse currently unavailable!",                   
                    ContentTemplate = (DataTemplate)this.Resources["ContentTemplateStyle"],
                    PrimaryButtonText = "Call Again",
                    CloseButtonText = "Cancel",
                };
                errorMessage.Opacity = 1;
                errorMessage.CornerRadius = new CornerRadius(5,5,5,5);
                errorMessage.PrimaryButtonStyle = (Style)this.Resources["PurpleButtonStyle"];
                errorMessage.CloseButtonStyle = (Style)this.Resources["CancelButtonStyle"];                
                errorMessage.PrimaryButtonClick -= CallAgainCommandCompletedAsync;
                errorMessage.PrimaryButtonClick += CallAgainCommandCompletedAsync;
                errorMessage.CloseButtonClick -= CancelCommandCompletedAsync;
                errorMessage.CloseButtonClick += CancelCommandCompletedAsync;
                var nurseShowMSG = errorMessage.ShowAsync();

                //MessageDialog errorMessage = new MessageDialog("Nurse currently unavailable!");
                //// Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
                //errorMessage.Commands.Add(new UICommand(
                //    "Call Again",
                //    new UICommandInvokedHandler(this.ProceedCommandInvokedHandlerAsync)));
                //errorMessage.Commands.Add(new UICommand(
                //    "Cancel",
                //    new UICommandInvokedHandler(this.CancelCommandInvokedHandlerAsync)));
                //// Setup Content

                //// Set the command that will be invoked by default
                //errorMessage.DefaultCommandIndex = 0;

                //// Set the command to be invoked when escape is pressed
                //errorMessage.CancelCommandIndex = 1;
                //await errorMessage.ShowAsync();

            }
        }

        private async void CancelCommandCompletedAsync(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await EndCallAsync();
        }

        private async void CallAgainCommandCompletedAsync(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await device.CaptureSource.StopPreviewAsync();
            await device.CleanUpPreviewAsync();
            //await device.CleanUpAsync();
            // end the CallButton. session
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, (() =>
            {
                RemoteVideo.Stop();
                RemoteVideo.Source = null;
                PreviewVideo.Source = null;
                PreviewVideo.Visibility = Visibility.Collapsed;
                // device.mediaSink.Dispose();
            }));
            // Start waiting for a new CallButton.
            await InitializeAsync();
            ////await device.CleanUpAsync();
            if (device!=null)
                CallDevice(device);

            InitiateConsultation();
        }

        private async void CancelCommandInvokedHandlerAsync(IUICommand command)
        {
            await EndCallAsync();
        }

       

        async void Device_IncomingConnectionArrived(object sender, IncomingConnectionEventArgs e)
        {
            e.Accept();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, (() =>
            {
                CallingScreen.Stop();
                CallingScreen.Visibility = Visibility.Collapsed;
                //OutgoingCall.Visibility = Visibility.Collapsed;
                //OutgoingCall.Stop();
                RemoteVideo.Visibility = Visibility.Visible;
            }));


            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, (() =>
            {
                activated = true;
                var remoteAddress = e.RemoteUrl;

                btnEndConsult.IsEnabled = true;
                Interlocked.Exchange(ref isTerminator, 0);

                if (!((bool)roleIsActive))
                {
                    // Passive client
                    RemoteVideo.Source = new Uri(remoteAddress);
                    device = new CaptureDevice();
                   // HostNameTextbox.IsEnabled = btnInitConsult.IsEnabled = false;
                }

                remoteAddress = remoteAddress.Replace("stsp://", "");
            }));
        }

        async void Device_CaptureFailed(object sender, Windows.Media.Capture.MediaCaptureFailedEventArgs e)
        {
            //try
            //{
            //    if (Interlocked.CompareExchange(ref isTerminator, 1, 0) == 0)
            //    {
            //        await EndCallAsync();
            //      //  this.Frame.Navigate(typeof(LogoPage));
            //    }
            //}
            //catch (Exception ex)
            //{
            //    await EndCallAsync();
            //}
        }

        private async Task StartRecordToCustomSink()
        {
            MediaEncodingProfile mep = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Qvga);

            mep.Video.FrameRate.Numerator = 15;
            mep.Video.FrameRate.Denominator = 1;
            mep.Container = null;

            await device.StartRecordingAsync(mep);
        }

        /// <summary>
        /// This is the click handler for the 'Default' button.  You would replace this with your own handler
        /// if you have a button or buttons on this page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CallButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {
                InitiateConsultation();
            }

        }

        private async void EndCallButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b != null && Interlocked.CompareExchange(ref isTerminator, 1, 0) == 0)
            {
                await EndCallAsync();
            }
        }

        private async Task EndCallAsync()
        {

           // await device.CaptureSource.StopPreviewAsync();
            await device.CleanUpPreviewAsync();
            //await device.CleanUpAsync();
            // end the CallButton. session
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, (() =>
            {
                RemoteVideo.Stop();
                RemoteVideo.Source = null;
                PreviewVideo.Source = null;
                PreviewVideo.Visibility = Visibility.Collapsed;
                // device.mediaSink.Dispose();
            }));
            // Start waiting for a new CallButton.
            await InitializeAsync();
            MainPage.mainPage.pagePlaceHolder.Navigate(typeof(Videocallpage));
        }

    }
}
