﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VideoKallSBCApplication.Helpers;
using VideoKallSBCApplication.Model;
using VideoKallSBCApplication.ViewModel;
using VideoKallSMC.ViewModel;
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
    public sealed partial class ThermoMeter : Page
    {

        private TestPanelViewModel _testPanelVM = null;
        public ThermoMeter()
        {
            this.InitializeComponent();
            MainPage.TestresultModel.ThermoResultcallback += ThermoResultCallback;
            MainPage.TestresultModel.DeviceConnectionTimeCallback += DeviceConnectionStatus;
            MainPage.TestPanelVM.InstuctionNoteCallBackCompleted += InstuctionNoteCallBackCompleted;
        }

        private async void InstuctionNoteCallBackCompleted(DeviceTypesenums type, string parm2)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (type == DeviceTypesenums.THERMOMETER)
                {
                    _testPanelVM.Instruction_Note = Constants.GUIDE_NOTE;
                    _testPanelVM.IsConnected_THRM = false;
                }
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _testPanelVM = (TestPanelViewModel)e.Parameter;
            _testPanelVM.IsConnected_THRM = true;
            
        }
      
        private async void DeviceConnectionStatus(DeviceTypesenums type, string parm2, bool status)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (type == DeviceTypesenums.THERMOMETER)
                {
                    //BtnTempConnect.IsEnabled = true;
                    // TxtConnectionTime.Text = parm2;
                    TxtConnectionTime.Text= Convert.ToDateTime(parm2).ToString( CultureInfo.CreateSpecificCulture(Constants.US_DATE_FORMATE));
                }
            });
        }        


        string tempformat = "{0}°{1}";
        private async  void ThermoResultCallback(ThermometerResult res)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
            decimal Conversion = res.Temp;
                if (TxtTmpUnitbtn.IsOn)
                {
                    Conversion =  decimal.Round((Conversion * (decimal)1.8) ,1) + 32;
                }
                else
                {
                    Conversion = decimal.Round(res.Temp, 1);
                }
                if (res.Status)
                    TxtTemprature.Text = string.Format(tempformat, Conversion.ToString(), TxtTmpUnitbtn.IsOn ? TxtTmpUnitbtn.OnContent : TxtTmpUnitbtn.OffContent);
                else
                    TxtTemprature.Text = "Error: Lo";

                TxtTestMode.Text = res.Mode;
                // TxtTestTime.Text = res.DateTimeOfTest.ToString();
                TxtTestTime.Text= Convert.ToDateTime(res.DateTimeOfTest).ToString(CultureInfo.CreateSpecificCulture(Constants.US_DATE_FORMATE));

            });
        }

        private void BtnTempConnect_Click(object sender, RoutedEventArgs e)
        {
            _testPanelVM.IsConnected_THRM = false;
            _testPanelVM.Instruction_Note = Constants.RE_CONNECT;
            //BtnTempConnect.IsEnabled = false;
            _testPanelVM.IsMsgConnected = Visibility.Visible;
            _testPanelVM.IsFromSMC_THRM = true;
            MainPage.TestresultModel.Thermo.Connect();
        }
         

        
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            
            if (MainPage.TestresultModel != null && !string.IsNullOrEmpty(MainPage.TestresultModel.ThermoResult.ConnectionTime))
            {
                string bpCuffConnTime = Convert.ToDateTime(MainPage.TestresultModel.ThermoResult.ConnectionTime).ToString( CultureInfo.CreateSpecificCulture(Constants.US_DATE_FORMATE)); 
                TxtConnectionTime.Text = Convert.ToDateTime(MainPage.TestresultModel.ThermoResult.ConnectionTime).ToString( CultureInfo.CreateSpecificCulture(Constants.US_DATE_FORMATE));
            }
            //TxtConnectionTime.Text= MainPage.TestresultModel.ThermoResult.ConnectionTime;
        }

        private void TxtTmpUnitbtn_Toggled(object sender, RoutedEventArgs e)
        {
            decimal Conversion = MainPage.TestresultModel.ThermoResult.Temp;
            if (TxtTmpUnitbtn.IsOn)
            {
                Conversion = decimal.Round((Conversion * (decimal)1.8), 1) + 32;
            }
            else
            {
                Conversion = decimal.Round(Conversion, 1);
            }
            TxtTemprature.Text = string.Format(tempformat, Conversion.ToString(), TxtTmpUnitbtn.IsOn ? TxtTmpUnitbtn.OnContent : TxtTmpUnitbtn.OffContent);

        }

        private void TxtTmpUnitbtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            
        }
                
    }
}
