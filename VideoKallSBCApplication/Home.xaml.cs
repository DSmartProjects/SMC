using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VideoKallSBCApplication.ViewModel;
using VideoKallSMC;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VideoKallSBCApplication
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Home : Page
    {
        public static Home HomePage;
        public HomeViewModel HomeVM = null;
        public Home()
        {
            HomePage = this;
            this.InitializeComponent();
            HomeVM = new HomeViewModel();
            this.DataContext = HomeVM;
      
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
