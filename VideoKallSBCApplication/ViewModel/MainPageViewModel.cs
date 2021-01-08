using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VideoKallSBCApplication;
using VideoKallSBCApplication.Views;
using VideoKallSMC.Views;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;

namespace VideoKallSMC.ViewModel
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public delegate void GlucoCmd(); 
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private ICommand _accountCommand = null;
        public ICommand AccountCommand 
        {
            get
            {
                if (_accountCommand == null)
                    _accountCommand = new RelayCommand(ExecuteAccountCommand);
                return _accountCommand;
            }
        }
        void ExecuteAccountCommand()
        {
            MainPage.mainPage.pagePlaceHolder.Navigate(typeof(Accounts) );
        }

        private ICommand _logoffCommand = null;
        public ICommand LogOffCommand
        {
            get
            {
                if (_logoffCommand == null)
                    _logoffCommand = new RelayCommand(ExecuteLogOffCommand);
                return _logoffCommand;
            }
        }
        public void ExecuteLogOffCommand()
        {
             MainPage.mainPage.pagePlaceHolder.Navigate(typeof(Videocallpage));
             MainPage.mainPage.RightPanelHolder.Navigate(typeof(LoginPage));
        }
        //Settings
        private ICommand _settingsCommand = null;
        public ICommand SettingsCommand
        {
            get
            {
                if (_settingsCommand == null)
                    _settingsCommand = new RelayCommand(ExecuteSettingsCommand);
                return _settingsCommand;
            }
        }
        public void ExecuteSettingsCommand()
        {
            MainPage.mainPage.pagePlaceHolder.Navigate(typeof(Settings));
        }

        private ICommand _browserCommand = null;
        public ICommand BrowserCommand
        {
            get
            {
                if (_browserCommand == null)
                    _browserCommand = new RelayCommand(ExecuteBrowserCommand);
                return _browserCommand;
            }
        }
        public async void ExecuteBrowserCommand()
        {
            StorageFolder appFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            await Launcher.LaunchFolderAsync(appFolder);
        }
        ///
        private ICommand _Done = null;
        public ICommand Done
        {
            get
            {
                if (_Done == null)
                    _Done = new RelayCommand(ExecuteDoneCommand);
                return _Done;
            }
        }
        public  void ExecuteDoneCommand()
        {
             MainPage.mainPage.pagePlaceHolder.Navigate(typeof(Videocallpage));
        }

        private ICommand _DeviceListCommand = null;
        public ICommand DeviceListCommand
        {
            get
            {
                if (_DeviceListCommand == null)
                    _DeviceListCommand = new RelayCommand(ExecuteDeviceListCommand);
                return _DeviceListCommand;
            }
        }

        DeviceListPage _deviceListPage;// = new DeviceListPage();
        public void ExecuteDeviceListCommand()
        {
            //DeviceListPage
            if (_deviceListPage == null)
                _deviceListPage = new DeviceListPage( );
            MainPage.mainPage.pagePlaceHolder.Navigate(typeof(DeviceListPage), _deviceListPage);
        }
        
        public string StatusText { get; set; }

        public string TxtPortNumber { get; set; }
        public string TxtIPAddress { get; set; }

        private string _npt_IPAddress = null;
        public string NPT_IPAddress { get { return _npt_IPAddress; } set { _npt_IPAddress = value; OnPropertyChanged("NPT_IPAddress"); } }
        string NPT_ConfigFile = "NPTConfig.txt";

        private Visibility _titleBarVisibility = Visibility.Collapsed;
        public Visibility TitleBarVisibility { get { return _titleBarVisibility; } set { _titleBarVisibility = value; OnPropertyChanged("TitleBarVisibility"); } }
        
        public async Task<bool> ReadNPTConfig()
        {
            try
            {
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile IpAddressFile = await localFolder.GetFileAsync(NPT_ConfigFile);
                var alltext = await Windows.Storage.FileIO.ReadLinesAsync(IpAddressFile);
                foreach (var line in alltext)
                {
                    string[] data = line.Split(':');
                    switch (data[0])
                    {                       
                        case "NPT-IP-Address":
                            MainPage.mainPage.mainpagecontext.NPT_IPAddress = data[1];
                            break;
                    }
                }
            }
            catch (Exception)
            {
                try
                {
                    var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                    Windows.Storage.StorageFile pinfofile = await localFolder.CreateFileAsync(NPT_ConfigFile);
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }               
            }
            return true;
        }


        public void UpdateIPaddress (string ip,string port)
        {
            TxtPortNumber = port;
            TxtIPAddress = ip;
            OnPropertyChanged(TxtPortNumber);
            OnPropertyChanged(TxtIPAddress);
        }

    }//class
}
