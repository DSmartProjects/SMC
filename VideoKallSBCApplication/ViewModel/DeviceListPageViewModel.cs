using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.ServiceModel.Dispatcher;
using System.Diagnostics;
using Windows.UI.Core;
using VideoKallSBCApplication.Views;
using VideoKallSBCApplication.BLEDevices;
using VideoKallSMC.Views;
using VideoKallSBCApplication;
using SBCDBModule;
using SBCDBModule.DB;
/// <summary>
/// 
/// 
/// </summary>
namespace VideoKallSBCApplication 
{

    public class DeviceListPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public List<DeviceInformation> BLEUnknownDevices = new List<DeviceInformation>();
      //  ObservableCollection<string> DeviceTypes = new ObservableCollection<string>();
        public delegate void Notify(string msg);
        public Notify NotifyStatusMessage;

        public ObservableCollection<BLEDeviceInfo> BLEDevices { get { return MainPage.TestresultModel.BLEDevices; } }
        public DeviceListPageViewModel()
        {
            //DeviceTypeList
            MainPage.TestresultModel.bpcuff.NotifyStatusMessage += NotifyDeviceConnection;
            DeviceTypeList = MainPage.TestresultModel.DeviceTypelist;
        }

        void NotifyDeviceConnection(string msg, int errorOrStatus = 0)
        {
            NotifyStatusMessage?.Invoke(msg);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        ICommand _DeviceDiscovery = null;
        public ICommand DeviceDiscovery
        {
            get
            {
                if (_DeviceDiscovery == null)
                    _DeviceDiscovery = new RelayCommand(ExecuteDeviceDiscoveryCommand);
                return _DeviceDiscovery;
            }
        }
        public void ExecuteDeviceDiscoveryCommand()
        {
            try
            {
                if (DiscoveryBtnTxt.Equals("Device Search"))
                {
                    StartDeviceSearch();
                    DiscoveryBtnTxt = "Stop Device Search";
                    OnPropertyChanged("DiscoveryBtnTxt");
                }
                else
                {
                    StopDeviceSearch();
                    DiscoveryBtnTxt = "Device Search";
                    OnPropertyChanged("DiscoveryBtnTxt");
                }
            }
            catch (Exception ex)
            {
                NotifyStatusMessage?.Invoke("Device Search failed. Error: " + ex.Message);
            }


        }

        ICommand _DevicePairCommand;
        public ICommand DevicePairCommand
        {
            get
            {
                if (_DevicePairCommand == null)
                    _DevicePairCommand = new RelayCommand(PairDevice);
                return _DevicePairCommand;
            }
        }

        BLEDeviceInfo selectedBLEdevice = null;
        public BLEDeviceInfo SelectedDevice
        {
            get { return selectedBLEdevice; } 
            set { selectedBLEdevice = value;
                 DeviceTypeSelected = "";
                 OnPropertyChanged("DeviceTypeSelected");
                if (selectedBLEdevice == null || selectedBLEdevice.DeviceTypeInfo == null)
                    return;
                for (int i = 0; i< DeviceTypeList.Count(); i++)
                {
                    if(DeviceTypeList[i].Equals(selectedBLEdevice.DeviceTypeInfo.DeviceTypeName))
                    {
                        DeviceTypeSelected = selectedBLEdevice.DeviceTypeInfo.DeviceTypeName;
                        OnPropertyChanged("DeviceTypeSelected");
                        break;
                    }
                }
            } 
        }

        public string DiscoveryBtnTxt { get; set; } = "Device Search";
        public bool IsDeviceDiscoveryIsEnabled { get; set; } = true;

        public ICommand _ConnectCommand;
        public ICommand ConnectCommand
        {
            get
            {
                if (_ConnectCommand == null)
                    _ConnectCommand = new RelayCommand(ConnectSelectedDevice);
                return _ConnectCommand;
            }
        }

        ICommand _DoneCommand;
        public ICommand DoneCommand
        {
            get
            {
                if (_DoneCommand == null)
                    _DoneCommand = new RelayCommand(Done);
                return _DoneCommand;
            }
        }

        void Done()
        {
            StopDeviceSearch();
            MainPage.mainPage.pagePlaceHolder.GoBack();
        }

        public string DeviceTypeSelected { get; set; } ="";
        public List<string> DeviceTypeList { get; set; }
        void ConnectSelectedDevice()
        {
            //MainPage.TestresultModel.bpcuff?.Connect(SelectedDevice as BLEDeviceInfo);
        }

        private async void PairDevice()
        {
            try
            {
                var bleDeviceDisplay = SelectedDevice as BLEDeviceInfo;
                if (bleDeviceDisplay == null)
                {
                    NotifyStatusMessage?.Invoke("Please select a device.");
                    return;
                }
                DevicePairingResult result = await bleDeviceDisplay.DeviceInfo.Pairing.PairAsync();
                if (result.Status == DevicePairingResultStatus.AlreadyPaired)
                {
                    NotifyStatusMessage?.Invoke("Device already paired.");
                }
                else if (result.Status == DevicePairingResultStatus.Paired)
                {
                    NotifyStatusMessage?.Invoke("Device already paired.");
                }
                else
                {
                    NotifyStatusMessage?.Invoke("Paired Result: " + result.ToString());
                }
            }catch(Exception ex)
            {
                NotifyStatusMessage?.Invoke("Exception: " + ex.Message);
            }

        }
       
        DeviceWatcher deviceWatcher = null;
        public void StartDeviceSearch()
        {
            StopDeviceSearch();

           string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };
          // BT_Code: Example showing paired and non-paired in a single query.
          string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";
          deviceWatcher = DeviceInformation.CreateWatcher( aqsAllBluetoothLEDevices,
                                                            requestedProperties,
                                                            DeviceInformationKind.AssociationEndpoint);
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;
            BLEDevices.Clear();
            deviceWatcher.Start();
            NotifyStatusMessage?.Invoke("Device Search Initiated.");
        }

        public void StopDeviceSearch()
        {
            if (deviceWatcher != null)
            {
                // Unregister the event handlers.
                deviceWatcher.Added -= DeviceWatcher_Added;
                deviceWatcher.Updated -= DeviceWatcher_Updated;
                deviceWatcher.Removed -= DeviceWatcher_Removed;
                deviceWatcher.EnumerationCompleted -= DeviceWatcher_EnumerationCompleted;
                deviceWatcher.Stopped -= DeviceWatcher_Stopped;

                // Stop the watcher.
                deviceWatcher.Stop();
                deviceWatcher = null;
                NotifyStatusMessage?.Invoke("Device Search Stopped.");
            }
        }

        private async  void DeviceWatcher_Stopped(DeviceWatcher sender, object args)
        {
            await DeviceListPage.dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == deviceWatcher)
                {
                    DiscoveryBtnTxt = "Device Search";
                    OnPropertyChanged("DiscoveryBtnTxt");
                    NotifyStatusMessage?.Invoke("Device Search Stopped.");
                }
            });

        }

        private BLEDeviceInfo FindBluetoothLEDeviceDisplay(string id)
        {
            foreach (BLEDeviceInfo bleDeviceDisplay in  BLEDevices)
            {
                if (bleDeviceDisplay.Id == id)
                {
                    return bleDeviceDisplay;
                }
            }
            return null;
        }

        private DeviceInformation FindUnknownDevices(string id)
        {
            foreach (DeviceInformation bleDeviceInfo in BLEUnknownDevices)
            {
                if (bleDeviceInfo.Id == id)
                {
                    return bleDeviceInfo;
                }
            }
            return null;
        }

        private async void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            await DeviceListPage.dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                DiscoveryBtnTxt = "Device Search";
                OnPropertyChanged("DiscoveryBtnTxt");
                NotifyStatusMessage?.Invoke("Device Search Completed.");
            }); 
        }

        private async void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            await DeviceListPage.dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    // Debug.WriteLine(String.Format("Updated {0}{1}", deviceInfoUpdate.Id, ""));

                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher)
                    {
                        BLEDeviceInfo bleDeviceDisplay = FindBluetoothLEDeviceDisplay(deviceInfoUpdate.Id);
                        if (bleDeviceDisplay != null)
                        {
                            // Device is already being displayed - update UX.
                            BLEDevices.Remove(bleDeviceDisplay);
                            NotifyStatusMessage?.Invoke(bleDeviceDisplay.Name + " Removed.");
                            return;
                        }

                        DeviceInformation deviceInfo = FindUnknownDevices(deviceInfoUpdate.Id);
                        if (deviceInfo != null)
                        {
                            BLEUnknownDevices.Remove(deviceInfo);
                            NotifyStatusMessage?.Invoke(deviceInfo.Id + " Removed.");
                        }
                    }
                }
            });
        }

        private  async void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            await DeviceListPage.dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                   // Debug.WriteLine(String.Format("Updated {0}{1}", deviceInfoUpdate.Id, ""));

                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher)
                    {
                        BLEDeviceInfo bleDeviceDisplay = FindBluetoothLEDeviceDisplay(deviceInfoUpdate.Id);
                        if (bleDeviceDisplay != null)
                        {
                            // Device is already being displayed - update UX.
                            bleDeviceDisplay.Update(deviceInfoUpdate);
                            return;
                        }

                        DeviceInformation deviceInfo = FindUnknownDevices(deviceInfoUpdate.Id);
                        if (deviceInfo != null)
                        {

                            // If device has been updated with a friendly name it's no longer unknown.
                            if (deviceInfo.Name != String.Empty)
                            {
                                NotifyStatusMessage?.Invoke("device found:" + deviceInfo.Name);
                                deviceInfo.Update(deviceInfoUpdate);
                                BLEDevices.Add(new BLEDeviceInfo(deviceInfo));
                                BLEUnknownDevices.Remove(deviceInfo);
                            }
                        }
                    }
                }
            });
        }

        private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation devinfo)
        {
            await DeviceListPage.dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    if (!string.IsNullOrEmpty(devinfo.Name))
                    {
                        NotifyStatusMessage?.Invoke("device found:" + devinfo.Name);
                        BLEDevices.Add(new BLEDeviceInfo(devinfo));
                       // Debug.WriteLine("Name: {0} Id {1}", devinfo.Name, devinfo.Id);
                    }

                    else
                    {
                        // Add it to a list in case the name gets updated later. 
                        BLEUnknownDevices.Add(devinfo);
                    }
                }
            });
        }
    }

    public class BLEDeviceInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string DeviceConnectiontime { get; set; }
        public DeviceInformation DeviceInfo { get; private set; }
        public string Id => DeviceInfo.Id;
        public string Name => DeviceInfo.Name;
        public bool IsPaired => DeviceInfo.Pairing.IsPaired;
        public bool canPair => DeviceInfo.Pairing.CanPair;
        bool _isconnected = false;
        public bool IsConnected {
            get { return _isconnected; }
            set { _isconnected = value;
                 OnPropertyChanged("IsConnected");
                DeviceConnectiontime = DateTime.Now.ToString();
            }
          
    }
        public DeviceTypesenums DeviceType { get; set; }
        public bool IsConnectable => (bool?)DeviceInfo.Properties["System.Devices.Aep.Bluetooth.Le.IsConnectable"] == true;
        public IReadOnlyDictionary<string, object> Properties => DeviceInfo.Properties;
        public BitmapImage GlyphBitmapImage { get; private set; }

        public string DevicecmdStatus { get; set; }
        public BLEDeviceInfo(DeviceInformation device)
        {
            DeviceInfo = device;
            IsConnected = (bool?) DeviceInfo.Properties["System.Devices.Aep.IsConnected"] == true;
            UpdateGlyphBitmapImage();
            if(DeviceType == DeviceTypesenums.UNKNOWN )
                 updateDeviceTypes(DeviceInfo.Name);
        }

        public DeviceType DeviceTypeInfo { get; set; }

        void updateDeviceTypes(string devicename)
        {
            DeviceType dv = MainPage.TestresultModel.GetDeviceType(1);
            if(devicename.ToLower().Contains(dv.DeviceName.ToLower()) ||
                devicename.ToLower().Contains("blesmart_"))
            {
                DeviceType = DeviceTypesenums.BPMONITOR;
                DeviceTypeInfo = dv;
                return;
            }

            dv = MainPage.TestresultModel.GetDeviceType(2);
            if (devicename.ToLower().Contains(dv.DeviceName.ToLower()))  
            {
                DeviceType = DeviceTypesenums.OXIMETER;
                DeviceTypeInfo = dv;
                return;
            }

            dv = MainPage.TestresultModel.GetDeviceType(3);
            if (devicename.ToLower().Contains(dv.DeviceName.ToLower()))
            {
                DeviceType = DeviceTypesenums.THERMOMETER;
                DeviceTypeInfo = dv;
                return;
            }

            dv = MainPage.TestresultModel.GetDeviceType(4);
            if (devicename.ToLower().Contains(dv.DeviceName.ToLower()))
            {
                DeviceType = DeviceTypesenums.GLUCOMONITOR;
                DeviceTypeInfo = dv;
                return;
            }

        }
        public void Update(DeviceInformationUpdate devupdate)
        {
            DeviceInfo.Update(devupdate);
            IsConnected = (bool?)DeviceInfo.Properties["System.Devices.Aep.IsConnected"] == true;
            OnPropertyChanged("Id");
            OnPropertyChanged("Name");
            OnPropertyChanged("DeviceInformation");
            OnPropertyChanged("IsPaired");
            OnPropertyChanged("IsConnected");
            OnPropertyChanged("Properties");
            OnPropertyChanged("IsConnectable");

            UpdateGlyphBitmapImage();
            updateDeviceTypes(DeviceInfo.Name);
        }
        private async void UpdateGlyphBitmapImage()
        {
            DeviceThumbnail deviceThumbnail = await DeviceInfo.GetGlyphThumbnailAsync();
            var glyphBitmapImage = new BitmapImage();
            await glyphBitmapImage.SetSourceAsync(deviceThumbnail);
            GlyphBitmapImage = glyphBitmapImage;
            OnPropertyChanged("GlyphBitmapImage");
        }
    }
}
