using GalaSoft.MvvmLight.Command;
using SBCDBModule;
using SBCDBModule.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VideoKallSBCApplication;
using VideoKallSBCApplication.Helpers;
using VideoKallSMC.Views;
using Windows.UI.Xaml;

namespace VideoKallSMC.ViewModel
{
    class LoginPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private bool _isAdmin = false;
        public bool IsAdmin
        {
            get
            {
                return _isAdmin;
            }
            set
            {
                _isAdmin = value;
                OnPropertyChanged("IsAdmin");
            }
        }
        private ICommand _submitCommand = null;
        public ICommand SubmitCommand
        {
            get
            {
                if (_submitCommand == null)
                    _submitCommand = new RelayCommand(ExecuteSubmitCommand);
                return _submitCommand;
            }
        }

        string _userid =string.Empty;
        public string Userid
        {
            get { return _userid; }
            set
            {
                _userid = value;
                OnPropertyChanged("EnableSubmitButton");
            }
        }

        string _password =string.Empty;
        public string PasswordTxt
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged("EnableSubmitButton");
            }
        }

        public bool EnableSubmitButton
        {
            get
            {
                if (string.IsNullOrEmpty(PasswordTxt) || string.IsNullOrEmpty(Userid))
                {
                    return false;
                }
                return true;
            }
        }
        public bool LoginFailedMsg1Visible { get; set; }
        public bool LoginFailedMsg2Visible { get; set; }
        public bool LoginFailedMsg3Visible { get; set; }
        public string LoginErrorMessage { get; set; }
        public string LoginErrorMessage2 { get; set; }
        public void ExecuteSubmitCommand()
        {
            IsAdmin = !string.IsNullOrEmpty(Userid) && !string.IsNullOrEmpty(Constants.Admin_PWD) && Userid.Equals(Constants.Admin_UNAME, StringComparison.InvariantCultureIgnoreCase) && PasswordTxt.Equals(Constants.Admin_PWD, StringComparison.InvariantCultureIgnoreCase) ? true : false;
            if (IsAdmin)
            {                 
                Toast.ShowToast("", Constants.Login_Success_MSG);
                testPanel = null;
                testPanel = new TestPanel();
                MainPage.mainPage.mainpagecontext.TitleBarVisibility = Visibility.Visible;
                MainPage.mainPage.mainpagecontext.TitleBarLeftMenuVisibility = Visibility.Collapsed;
                MainPage.mainPage.mainpagecontext.TitleBarRightMenuVisibility = Visibility.Visible;
                MainPage.mainPage.pagePlaceHolder.Navigate(typeof(TestPanel), testPanel);
            }
            else
            {
                Toast.ShowToast("",Constants.InValid_UNAME_PWD);
                return;
            }

            //if (videocallPage == null)
            //    videocallPage = new Videocallpage();
            // MainPage.mainPage.pagePlaceHolder.Navigate(typeof(Settings), videocallPage);

            //    SBCDB dbmodule = new SBCDB();
            //    User loggedinUser = dbmodule.GetLoggedinUser(Userid.Trim().ToLower());
            //    if (loggedinUser == null)
            //    {
            //        //LoginFailedMsg1Visible = true;
            //        //LoginFailedMsg2Visible = true;
            //        //LoginFailedMsg3Visible = true;

            //        //LoginErrorMessage = "User name: " + Userid + " not found.";
            //        //LoginErrorMessage2 = "Please enter valid user id or contact admin.";
            //        //OnPropertyChanged("LoginFailedMsg1Visible");
            //        //OnPropertyChanged("LoginFailedMsg2Visible");
            //        //OnPropertyChanged("LoginFailedMsg3Visible");
            //        //OnPropertyChanged("LoginErrorMessage");
            //        //OnPropertyChanged("LoginErrorMessage2");
            //        return;
            //    }

            //    if (string.Compare(PasswordTxt, loggedinUser.Password) == 0)
            //    {
            //        if (testPanel == null)
            //            testPanel = new TestPanel();               
            //        //MainPage.mainPage.RightPanelHolder.Navigate(typeof(TestPanel), testPanel);
            //        if (videocallPage == null)
            //            videocallPage = new Videocallpage();
            //        // MainPage.mainPage.pagePlaceHolder.Navigate(typeof(Settings), videocallPage);
            //        MainPage.mainPage.pagePlaceHolder.Navigate(typeof(TestPanel), testPanel);
            //    }
            //    else
            //    {
            //        //LoginFailedMsg1Visible = true;
            //        //LoginFailedMsg2Visible = true;
            //        //LoginFailedMsg3Visible = true;
            //        //LoginErrorMessage = "Password not matched.";
            //        //LoginErrorMessage2 = "Please enter valid password or contact admin.";

            //        //OnPropertyChanged("LoginFailedMsg1Visible");
            //        //OnPropertyChanged("LoginFailedMsg2Visible");
            //        //OnPropertyChanged("LoginFailedMsg3Visible");
            //        //OnPropertyChanged("LoginErrorMessage");
            //        //OnPropertyChanged("LoginErrorMessage2");
            //    }

            //}
        }
        Videocallpage videocallPage = null;
        TestPanel testPanel = null;
    } //class
}
