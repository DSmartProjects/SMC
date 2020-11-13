﻿using GalaSoft.MvvmLight.Command;
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

        string _userid = "admin";
        public string Userid
        {
            get { return _userid; }
            set
            {
                _userid = value;
                OnPropertyChanged("EnableSubmitButton");
            }
        }

        string _password = "admin";
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
            testPanel = new TestPanel();
            MainPage.mainPage.RightPanelHolder.Navigate(typeof(TestPanel), testPanel);
            if (videocallPage == null)
                videocallPage = new Videocallpage();
            MainPage.mainPage.pagePlaceHolder.Navigate(typeof(Settings), videocallPage);

            SBCDB dbmodule = new SBCDB();
            User loggedinUser = dbmodule.GetLoggedinUser(Userid.Trim().ToLower());
            if (loggedinUser == null)
            {
                LoginFailedMsg1Visible = true;
                LoginFailedMsg2Visible = true;
                LoginFailedMsg3Visible = true;

                LoginErrorMessage = "User name: " + Userid + " not found.";
                LoginErrorMessage2 = "Please enter valid user id or contact admin.";
                OnPropertyChanged("LoginFailedMsg1Visible");
                OnPropertyChanged("LoginFailedMsg2Visible");
                OnPropertyChanged("LoginFailedMsg3Visible");
                OnPropertyChanged("LoginErrorMessage");
                OnPropertyChanged("LoginErrorMessage2");
                return;
            }

            if (string.Compare(PasswordTxt, loggedinUser.Password) == 0)
            {
                if (testPanel == null)
                    testPanel = new TestPanel();
                MainPage.mainPage.RightPanelHolder.Navigate(typeof(TestPanel), testPanel);
                if (videocallPage == null)
                    videocallPage = new Videocallpage();
                MainPage.mainPage.pagePlaceHolder.Navigate(typeof(Settings), videocallPage);
            }
            else
            {
                LoginFailedMsg1Visible = true;
                LoginFailedMsg2Visible = true;
                LoginFailedMsg3Visible = true;
                LoginErrorMessage = "Password not matched.";
                LoginErrorMessage2 = "Please enter valid password or contact admin.";

                OnPropertyChanged("LoginFailedMsg1Visible");
                OnPropertyChanged("LoginFailedMsg2Visible");
                OnPropertyChanged("LoginFailedMsg3Visible");
                OnPropertyChanged("LoginErrorMessage");
                OnPropertyChanged("LoginErrorMessage2");
            }

        }
        Videocallpage videocallPage = null;
        TestPanel testPanel = null;
    } //class
}
