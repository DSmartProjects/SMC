using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
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

namespace VideoKallSBCApplication.Views
{

    public sealed partial class CallSummary : Page
    {
        int delay = 2000;

        public CallSummary()
        {
            this.InitializeComponent();
            //setTimeout(GoHome, delay);
        }

        //private void GoHome()
        //{
        //    this.Frame.Navigate(typeof(LogoPage));
        //}

        private void GoHome_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Videocallpage));
        }

        //public void setTimeout(Action TheAction, int Timeout)
        //{
        //    Thread t = new Thread(
        //        () =>
        //        {
        //            Thread.Sleep(Timeout);
        //            TheAction.Invoke();
        //        }
        //    );
        //    t.Start();
        //}
    }
}
