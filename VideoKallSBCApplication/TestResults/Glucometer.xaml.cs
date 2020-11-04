using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VideoKallSBCApplication.Model;
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
using VideoKallSBCApplication.BLEDevices;
using System.ServiceModel.Channels;
using Windows.UI.Popups;
using Windows.UI.Core;
using VideoKallSBCApplication.ViewModel;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VideoKallSBCApplication.TestResults
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
   
    public sealed partial class Glucometer : Page
    {

        public GlucoUtility GlucoCMDUtitlity { get; } = new GlucoUtility();
        public Glucometer()
        {
            this.InitializeComponent();
            this.DataContext = this;
         //   MainPage.TestresultModel.glucoMonitor.Execute += Execute;
            MainPage.TestresultModel.GlucometerDataReceiveCallback = GlucometerResultEvent;
            
        }

        private TestPanelViewModel _testPanelVM = null;
       
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _testPanelVM = (TestPanelViewModel)e.Parameter;
            _testPanelVM.IsConnected_THRM = true;
        }


        private async void GlucometerResultEvent(GlucoResult glucoResult )
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (!string.IsNullOrEmpty(glucoResult.TestResult)  )
                {
                    TxtDate.Text = glucoResult.TestDay +" "+ glucoResult.TestTime;
                    TxtResult.Text = glucoResult.TestValue;
                    Txttype.Text = glucoResult.TestType;
                    //TxtMode.Text = glucoResult.Mode;
                    if (glucoResult.Mode != null)
                    {
                        TxtMode.Text = glucoResult.Mode;
                    }
                    else
                    {
                        TxtMode.Text = "";
                    }
                }
                else
                {
                    TxtTestDataByIndex.Text = glucoResult.RecordCount.ToString();
                }
                BtnTestData.IsEnabled = false;
                //_testPanelVM.IsMsgConnected = Visibility.Visible;
            });
               
        }

        private    void BtnTestData_Click(object sender, RoutedEventArgs e)
        {
            GlucoCMDUtitlity.LatestTestResult();
            //try
            //{
            //    commandType = 0;
            //    MainPage.TestresultModel.glucoMonitor.Connect();
            //}catch(Exception ex)
            //{
            //    MainPage.TestresultModel.NotifyStatusMessage("Exception: " + ex.Message);
            //}

        }
        private void Execute()
        {
             GlucoCMDUtitlity.Execute();
            //try
            //{
            //    switch (commandType)
            //    {
            //        case 0:
            //            MainPage.TestresultModel.glucoMonitor.LastestTestResultTime();
            //            break;
            //        case 2:
            //            MainPage.TestresultModel.glucoMonitor.DeleteAll();
            //            break;
            //        case 3:
            //            MainPage.TestresultModel.glucoMonitor.RecordCount();
            //            break;
            //        case 4:
            //            MainPage.TestresultModel.glucoMonitor.RecordByIndex(Convert.ToInt32(TxtTestDataByIndex.Text));
            //            TxtTestDataByIndex.IsEnabled = true;
            //            break;
            //    }
            //    commandType = 0;
            //}
            //catch(Exception ex)
            //{
            //    MainPage.TestresultModel.NotifyStatusMessage("Exception: " + ex.Message);
            //}

        }

        private async void BtnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            var messageDialog = new MessageDialog("Do you want to delete all records from device?");
            messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes") { Id = 0 });
            messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("No") { Id = 1 });
          var res = await messageDialog.ShowAsync();
            try
            {
                if (res.Label.Equals("Yes"))
                {
                     GlucoCMDUtitlity.DeleteAll();
                    //commandType = 2;
                    //MainPage.TestresultModel.glucoMonitor.Connect();
                }
            }catch(Exception ex)
            {
                MainPage.TestresultModel.NotifyStatusMessage("Exception: " + ex.Message);
            }
            
        }

        private void BtnTestNumberOfRecords_Click(object sender, RoutedEventArgs e)
        {
            //commandType = 3;
            //MainPage.TestresultModel.glucoMonitor.Connect();
           GlucoCMDUtitlity.NumberOfRecords();
        }

        private void BtnTestDataByIndex_Click(object sender, RoutedEventArgs e)
        {
            if ( TxtTestDataByIndex.Text.All(Char.IsDigit ))
            {
             //   commandType = 4;
                 TxtTestDataByIndex.IsEnabled = false;
                //MainPage.TestresultModel.glucoMonitor.Connect();
                GlucoCMDUtitlity.ResultbyIndex();
            }

        }
    }
}
