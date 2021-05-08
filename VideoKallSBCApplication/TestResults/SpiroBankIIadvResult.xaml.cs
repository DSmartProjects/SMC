using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class SpiroBankIIadvResult : Page
    {
        public SpiroBankIIadvResult()
        {
            this.InitializeComponent();
            MainPage.mainPage.Spirobanadv.LivedataDisplayEvent += DisplayResult;
        }

       async void DisplayResult(string msg)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TxtResult.Text = msg;
            });
        }

        private void BtnVC_Click(object sender, RoutedEventArgs e)
        {
            MainPage.mainPage.Spirobanadv.StartVCTestcmd();
           
        }

        private void BtnFVC_Click(object sender, RoutedEventArgs e)
        {
            MainPage.mainPage.Spirobanadv.StartFVCTestcmd();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            MainPage.mainPage.Spirobanadv.StopTestAndCalculateResultcmd();
        }

        

             private void Btnresult_Click(object sender, RoutedEventArgs e)
        {
            MainPage.mainPage.Spirobanadv.GetResultCmd();
        }

    }
}
