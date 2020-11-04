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
            TestTypes.Add("Blood Pressure");
            TestTypes.Add("Thermometer");
            TestTypes.Add("Pulse Oximeter");
            TestTypes.Add("Spirometry");
            TestTypes.Add("Gluco Monitor");
            TestTypes.Add("Stethoscope(Chest)");
            TestTypes.Add("Stethoscope (Back)");
            TestTypes.Add("Otoscope");
            TestTypes.Add("Dermascope");
            TestTypes.Add("EKG");
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
                case "blood pressure":
                    _testPanelVM.IsMsgConnected = Visibility.Collapsed;
                    // BPCuffPage bp = new BPCuffPage();
                    TestResultDisplay.Navigate(typeof(BPCuffPage));
                    break;
                case "thermometer":
                    _testPanelVM.IsMsgConnected = Visibility.Collapsed;
                    TestResultDisplay.Navigate(typeof(ThermoMeter),_testPanelVM);
                    break;
                case "pulse oximeter":
                    _testPanelVM.IsMsgConnected = Visibility.Collapsed;
                    TestResultDisplay.Navigate(typeof(OxymeterResults),_testPanelVM);
                    break;
                case "gluco monitor":
                    _testPanelVM.IsMsgConnected = Visibility.Collapsed;
                    TestResultDisplay.Navigate(typeof(Glucometer), _testPanelVM);
                    break;
                case "stethoscope(chest)":
                    _testPanelVM.IsMsgConnected = Visibility.Collapsed;
                    TestResultDisplay.Navigate(typeof(StethoscopeChest));
                    break;
                case "dermascope":
                    _testPanelVM.IsMsgConnected = Visibility.Collapsed;
                    TestResultDisplay.Navigate(typeof(DefaultTest));
                    break;

                default:
                    TestResultDisplay.Navigate(typeof(DefaultTest));
                    break;
            }
        }
    }
}
