using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VideoKallSBCApplication;
using VideoKallSBCApplication.BLEDevices;
 
using VideoKallSBCApplication.TestResults;
using VideoKallSBCApplication.ViewModel;
using VideoKallSBCApplication.Views;
using VideoKallSMC.ViewModel;
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
    public sealed partial class TestPanel : Page
    {     
        public TestPanelViewModel _testPanelVM = null;
        List<string> TestTypes = new List<string>();
        public TestPanel()
        {
            this.InitializeComponent();
            _testPanelVM = MainPage.TestPanelVM;
            this.DataContext = _testPanelVM;
            msgConnect.Visibility = Visibility.Collapsed;
            //bpcuff.NotifyStatusMessage += NotifyMessage;
            TestTypes.Add("Select a Test types");
            TestTypes.Add("Blood Pressure Cuff");
            TestTypes.Add("Pulse Oximeter");
            TestTypes.Add("Thermometer");
            TestTypes.Add("Dermatoscope");
            TestTypes.Add("Otoscope");
            TestTypes.Add("Spirometer");
            TestTypes.Add("Glucose Monitor");
            TestTypes.Add("Chest Stethoscope");
            TestTypes.Add("Seat Back Stethoscope");  
           // TestTypes.Add("EKG");
            TestTypesList.ItemsSource = TestTypes;
        }

        void NotifyMessage(string msg, int errorOrStatus)
        {
            // MainPage.mainPage.StatusTxt.Text = msg;
        }

      //  OxymeterResults ox = new OxymeterResults();
        private void TestTypesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var colorName = e.AddedItems[0] ;
            switch (e.AddedItems[0].ToString().ToLower())
            {
                case "blood pressure cuff":
                    _testPanelVM.IsMsgConnected = Visibility.Collapsed;
                    // BPCuffPage bp = new BPCuffPage();
                    TestResultDisplay.Navigate(typeof(BPCuffPage));
                    break;
                case "pulse oximeter":
                    _testPanelVM.IsMsgConnected = Visibility.Collapsed;
                    TestResultDisplay.Navigate(typeof(OxymeterResults), _testPanelVM);
                    break;
                case "thermometer":
                    _testPanelVM.IsMsgConnected = Visibility.Collapsed;
                    TestResultDisplay.Navigate(typeof(ThermoMeter),_testPanelVM);
                    break;
                case "dermatoscope":
                    _testPanelVM.IsMsgConnected = Visibility.Collapsed;
                    TestResultDisplay.Navigate(typeof(DefaultTest));
                    break;
                case "otoscope":
                    _testPanelVM.IsMsgConnected = Visibility.Collapsed;
                    TestResultDisplay.Navigate(typeof(DefaultTest));
                    break;
                case "spirometer":
                    _testPanelVM.IsMsgConnected = Visibility.Collapsed;
                    TestResultDisplay.Navigate(typeof(DefaultTest));
                    break;
                case "glucose monitor":
                    _testPanelVM.IsMsgConnected = Visibility.Collapsed;
                    TestResultDisplay.Navigate(typeof(Glucometer), _testPanelVM);
                    break;
                case "chest stethoscope":
                    _testPanelVM.IsMsgConnected = Visibility.Collapsed;
                    TestResultDisplay.Navigate(typeof(StethoscopeChest));
                    break;
                case "seat back stethoscope":
                    _testPanelVM.IsMsgConnected = Visibility.Collapsed;
                    TestResultDisplay.Navigate(typeof(StethoscopeChest));
                    break;                
            }
        }
    }
}
