using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VideoKallSMC.Communication;
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

namespace VideoKallSBCApplication.TestResults
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DefaultTest : Page
    {
        public DefaultTest()
        {
            this.InitializeComponent();
        }

        bool toggle = false;


        private void BtnDermascope_Click(object sender, RoutedEventArgs e)
        {
            // "DER>SH:{0}>H:{1}>W{2}>";
            toggle = !toggle;

            string msg = string.Format(CommunicationCommands.DERMASACOPE,
                toggle?1:0, MainPage.mainPage.RightPanelHolder.ActualHeight, MainPage.mainPage.RightPanelHolder.ActualWidth, 
                MainPage.mainPage.ActualWidth, MainPage.mainPage.ActualHeight);
            
            if(!toggle)
            {
                TakePic();
              //  MainPage.mainPage.DataacqAppComm.SendMessageToDataacquistionapp(msg);
            }
            else
            MainPage.mainPage.DataacqAppComm.SendMessageToDataacquistionapp(msg);

           
        }

        void StartDermascope()
        {
            string msg = string.Format(CommunicationCommands.DERMASACOPE,
                1  , MainPage.mainPage.RightPanelHolder.ActualHeight, MainPage.mainPage.RightPanelHolder.ActualWidth,
              MainPage.mainPage.ActualWidth, MainPage.mainPage.ActualHeight);

            MainPage.mainPage.DataacqAppComm.SendMessageToDataacquistionapp(msg);
        }

        void StopDermascope()
        {
            string msg = string.Format(CommunicationCommands.STOPOTOSCOPE);
            MainPage.mainPage.DataacqAppComm.SendMessageToDataacquistionapp(msg);
        }

        void StartOtosope()
        {
            string msg = string.Format(CommunicationCommands.OTOSCOPE,
                1, MainPage.mainPage.RightPanelHolder.ActualHeight, MainPage.mainPage.RightPanelHolder.ActualWidth,
              MainPage.mainPage.ActualWidth, MainPage.mainPage.ActualHeight);

            MainPage.mainPage.DataacqAppComm.SendMessageToDataacquistionapp(msg);
        }

        void StopOtoscope()
        {
            string msg = string.Format(CommunicationCommands.OTOSCOPE);
            MainPage.mainPage.DataacqAppComm.SendMessageToDataacquistionapp(msg);
        }
        public void TakePic()
        {
            string msg = string.Format(CommunicationCommands.TAKEPIC);
            MainPage.mainPage.DataacqAppComm.SendMessageToDataacquistionapp(msg);
        }
    }
  
}
