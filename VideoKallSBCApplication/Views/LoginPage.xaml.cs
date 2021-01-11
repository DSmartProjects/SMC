
using SBCDBModule;
using SBCDBModule.DB;
using VideoKallSMC.Views;
using VideoKallSMC.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VideoKallSBCApplication;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VideoKallSMC
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        internal LoginPageViewModel _loginVM = null;
  
        // LoginPageViewModel dbcontext = new LoginPageViewModel();
        public LoginPage()
        {
            this.InitializeComponent();
            _loginVM= new LoginPageViewModel();
            this.DataContext = _loginVM;
        }

        private void TitleBarLeftLogo_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Home));
        }

        private void TitleBarFrameLogo_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Home));
        }
    }
}
