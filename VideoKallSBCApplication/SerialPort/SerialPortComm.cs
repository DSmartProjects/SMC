using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.IO;
using Windows.Devices.SerialCommunication;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace VideoKallSBCApplication.SerialPort
{
    public struct SerialDeviceInfo
    {
      public  DeviceInformation devInfo;
      public  SerialDevice serialDevice;

        public static implicit operator DeviceInformation(SerialDeviceInfo v)
        {
            throw new NotImplementedException();
        }
    }
   public class SerialPortComm
    {
        DeviceWatcher deviceWatcher = null;
    
        public delegate void UpdateSerialDeviceStatus(string msg);
        public UpdateSerialDeviceStatus NotifySerialDeviceStatus;
        public  SerialPortComm ()
        {
            //Advanced Query String for all serial devices
            var deviceSelector = SerialDevice.GetDeviceSelector();
            // Create a device watcher to look for instances of the Serial Device  
            deviceWatcher = DeviceInformation.CreateWatcher(deviceSelector);
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
           // deviceWatcher.Start();
        }

        private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
 
        }

        private   void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate dev)
        {
       
        }


        private   void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate dev)
        {
             foreach(var v in DeviceList)
            {
                if (v.Value.devInfo.Id.Equals(dev.Id))

                {
                    DeviceList.Remove(dev.Id);
                    NotifySerialDeviceStatus?.Invoke("Serial Device Removed from Port: " + v.Key);

                    break;
                }
            }
        }

   
        private  async  void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation dev)
        {
             SerialDevice serialDevice = await  SerialDevice.FromIdAsync(dev.Id);
            var deviceAccessStatus = DeviceAccessInformation.CreateFromId(dev.Id).CurrentStatus;
            if (serialDevice != null)
            {
                SerialDeviceInfo devinfo;
                devinfo.devInfo = dev;
                devinfo.serialDevice = serialDevice;
                DeviceList.Add(serialDevice.PortName, devinfo);
                NotifySerialDeviceStatus?.Invoke("Serial Device added."); 
            }
           
        }

        public void Configure()
        {
           if( SelectDevice.serialDevice != null)
            {
                SelectDevice.serialDevice.BaudRate = BaudRate;
            }
        }

        public async Task<string>   ReadData()
        {
            DataReader _dataReaderObject = null;

            _dataReaderObject = new DataReader(SelectDevice.serialDevice.InputStream);
            _dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;
            string receivedStrings = "";
            try
            {
                Task<UInt32> loadAsyncTask;
                uint ReadBufferLength = 1024;
                if (_dataReaderObject != null)
                {
                    loadAsyncTask = _dataReaderObject.LoadAsync(ReadBufferLength).AsTask();
                    UInt32 bytesRead = await loadAsyncTask; 
                    if (bytesRead > 0)
                    {
                        receivedStrings = _dataReaderObject.ReadString(bytesRead);
                        //if (_presenter.IsCabinSW)
                        //    _presenter.SendMessage(receivedStrings);
                        //_presenter.ModelObject.MessageReceived(null, new CommunicationMsg(receivedStrings));

                        //_presenter.ModelObject.LogSerialPortMessage(PortName + "-" + "RX: " + receivedStrings);
                    }
                }

            }
            catch (Exception)
            {

            }
            return receivedStrings;
        }

       public async void WriteData(string cmd)
        {
          DataWriter  _dataWriteObject = new DataWriter(SelectDevice.serialDevice.OutputStream);


            try
            {
                //char[] buffer = new char[4];
                //cmd.CopyTo(0, buffer, 0, cmd.Length);
                //String InputString = new string(buffer);

                UInt32 bytesWritten = 0;
                if (_dataWriteObject != null)
                {
                    Task<UInt32> storeAsyncTask;
                    _dataWriteObject.WriteString(cmd);
                    // Transfer data to the serial device now
                    storeAsyncTask = _dataWriteObject.StoreAsync().AsTask();
                    bytesWritten = await storeAsyncTask;
                }

                if (bytesWritten > 0)
                {
                   // _presenter.ModelObject.LogSerialPortMessage(PortName + "-" + "TX:" + InputString);
                }
            }
            catch (Exception)
            {

            }

        }
         
     

       public SerialDeviceInfo SelectDevice { get; set; }
        public Dictionary<string, SerialDeviceInfo> DeviceList { get; } = new Dictionary<string, SerialDeviceInfo>();
        public uint BaudRate { get; set; } = 115200;

    }
}
