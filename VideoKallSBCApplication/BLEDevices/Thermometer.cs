using SBCDBModule.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoKallSBCApplication.Helpers;
using VideoKallSBCApplication.Model;
using VideoKallSBCApplication.ViewModel;
using VideoKallSMC.Communication;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace VideoKallSBCApplication.BLEDevices
{
  public  class Thermometer
    {
        private BluetoothLEDevice bluetoothLeDevice = null;
        // Only one registered characteristic at a time.
        private GattCharacteristic ThermometerCharacteristic;
        private GattPresentationFormat presentationFormat;

        private Dictionary<string, GattDeviceService> GTTServicelist = new Dictionary<string, GattDeviceService>();
        IReadOnlyList<GattCharacteristic> characteristics = null;

        public async void Connect(TestPanelViewModel _testPanelVM)
        {
            DeviceType dv = MainPage.TestresultModel.GetDeviceType(3);
            BLEDeviceInfo device = null;
            foreach (var dev in MainPage.TestresultModel.BLEDevices)
            {
               if( dev.DeviceType == DeviceTypesenums.THERMOMETER  && dev.IsPaired)
                {
                    device = dev;
                    break;
                }
               else if(dev.DeviceType == DeviceTypesenums.THERMOMETER && !dev.IsPaired)
                {
                    MainPage.mainPage.commChannel.SendMessageToMCC(
                   string.Format(CommunicationCommands.THERMORCONNECTIONSTATUS, "Device is not paried."));
                   MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Device is not paried. ", 1);
                    return;
                }
            }

            GTTServicelist.Clear();
            if (device == null)
            {                
                MainPage.mainPage.commChannel.SendMessageToMCC(
                   string.Format(CommunicationCommands.THERMORCONNECTIONSTATUS, "Device not found with id:"));

                MainPage.TestresultModel.NotifyStatusMessage?.Invoke(" Device not found with id: "+ MainPage.TestresultModel.ThermoMeterId, 1);
                if (_testPanelVM.IsFromSMC_THRM)
                {
                    _testPanelVM.IsConnected_THRM = true;
                    _testPanelVM.Instruction_Note = Constants.GUIDE_NOTE;

                }
                return;
            }
             bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(device.DeviceInfo.Id);
            if (bluetoothLeDevice == null)
            {
                MainPage.mainPage.commChannel.SendMessageToMCC(
                     string.Format(CommunicationCommands.THERMORCONNECTIONSTATUS, "Device Unreachable."));
                MainPage.TestresultModel.NotifyStatusMessage?.Invoke(" Device Unreachable.", 1);
                return;
            }

            GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
            if (result.Status == GattCommunicationStatus.Success)
            {
                _testPanelVM.Instruction_Note = Constants.GUIDE_NOTE;
                _testPanelVM.IsConnected_THRM = false;
                var services = result.Services;
                foreach (var svc in services)
                {
                    GTTServicelist.Add(svc.Uuid.ToString().ToLower(), svc);
                }
                 SelectService(device); //MainPage.TestresultModel.ThermoServiceID
            }
            else
            {
                _testPanelVM.Instruction_Note = Constants.GUIDE_NOTE;
                _testPanelVM.IsConnected_THRM = true;
                MainPage.mainPage.commChannel.SendMessageToMCC(
                     string.Format(CommunicationCommands.THERMORCONNECTIONSTATUS, "Failed to connect. " + result.Status.ToString()));

                MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Failed to connect. " + result.Status.ToString(), 1);
            }

        }

        async void SelectService(BLEDeviceInfo dev)
        {
            try
            {
                var service = GTTServicelist[dev.DeviceTypeInfo.ServiceID.ToLower()];
                // Ensure we have access to the device.
                var accessStatus = await service.RequestAccessAsync();
                if (accessStatus == DeviceAccessStatus.Allowed)
                {
                    var result = await service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                    if (result.Status == GattCommunicationStatus.Success)
                    {
                        characteristics = result.Characteristics;
                    }
                    else
                    {
                        MainPage.mainPage.commChannel.SendMessageToMCC(
                     string.Format(CommunicationCommands.THERMORCONNECTIONSTATUS, "Error accessing service " + result.Status.ToString()));
                        MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Error accessing service " + result.Status.ToString(), 1);
                        return;
                    }
                }
                else
                {
                    MainPage.mainPage.commChannel.SendMessageToMCC(
                    string.Format(CommunicationCommands.THERMORCONNECTIONSTATUS, "Error accessing service " + accessStatus.ToString()));
                    MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Error accessing service " + accessStatus.ToString(), 1);
                    return;
                }
            }catch(Exception ex)
            {
                MainPage.mainPage.commChannel.SendMessageToMCC(
                    string.Format(CommunicationCommands.THERMORCONNECTIONSTATUS, "Exception " + ex.Message));
                MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Exception " + ex.Message, 2);
                return;
            }
            foreach (GattCharacteristic c in characteristics)
            {
                string characteristicname = DisplayHelpers.GetCharacteristicName(c);
                if (c.AttributeHandle.ToString().Equals( dev.DeviceTypeInfo.CharacteristicAttributeid) ) //MainPage.TestresultModel.ThermometerHandleid
                {
                    ThermometerCharacteristic = c;
                    break;
                }
            }
            if (ThermometerCharacteristic == null)
            {
                MainPage.mainPage.commChannel.SendMessageToMCC(
                    string.Format(CommunicationCommands.THERMORCONNECTIONSTATUS, "Thermometer Characteristic not found."));

                MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Thermometer Characteristic not found.", 1);
                return;
            }
           
            var resultch = await ThermometerCharacteristic.GetDescriptorsAsync(BluetoothCacheMode.Uncached);
            if (resultch.Status != GattCommunicationStatus.Success)
            {
                MainPage.mainPage.commChannel.SendMessageToMCC(
                   string.Format(CommunicationCommands.THERMORCONNECTIONSTATUS, "Descriptor read failure: " + resultch.Status.ToString()));
                MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Descriptor read failure: " + resultch.Status.ToString(), 1);
                return;
            }
            presentationFormat = null;
            if (ThermometerCharacteristic.PresentationFormats.Count > 0)
            {
                if (ThermometerCharacteristic.PresentationFormats.Count.Equals(1))
                {
                    // Get the presentation format since there's only one way of presenting it
                    presentationFormat = ThermometerCharacteristic.PresentationFormats[0];
                }
                else
                {
                    // It's difficult to figure out how to split up a characteristic and encode its different parts properly.
                    // In this case, we'll just encode the whole thing to a string to make it easy to print out.
                }
            }
             SubscribeDeviceReading();


        }
       async void SubscribeDeviceReading()
        {
            // initialize status
            GattCommunicationStatus status = GattCommunicationStatus.Unreachable;
            var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.None;
            if (ThermometerCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
            {
                cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
            }
            else if (ThermometerCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
            {
                cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
            }

            // BT_Code: Must write the CCCD in order for server to send indications.
            // We receive them in the ValueChanged event handler.
            status = await ThermometerCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);
            if (status == GattCommunicationStatus.Success)
            {
                MainPage.mainPage.commChannel.SendMessageToMCC(
                  string.Format(CommunicationCommands.THERMORCONNECTIONSTATUS, "Successfully Subscribed."));

                //AddValueBPMeasureChangedHandler();
                MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Successfully Subscribed.");
            }
            else
            {
                MainPage.mainPage.commChannel.SendMessageToMCC(
                 string.Format(CommunicationCommands.THERMORCONNECTIONSTATUS, $"Error registering for value changes: {status}"));
                MainPage.TestresultModel.NotifyStatusMessage?.Invoke($"Error registering for value changes: {status}", 1);
            }

            ThermometerCharacteristic.ValueChanged += Characteristic_ValueChanged;
            MainPage.TestresultModel.DeviceConnectedTime(DeviceTypesenums.THERMOMETER, DateTime.Now.ToString(), true);

        }

        private void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            try
            {
                byte[] data;
                IBuffer buffer = args.CharacteristicValue;
                ThermometerResult Result = new ThermometerResult();

                CryptographicBuffer.CopyToByteArray(buffer, out data);
                string rawdata = "";

                //  float temperature =(float) (((data[4] & 255) * 256 + (data[3] & 255)) * 0.01);
                // decimal temperature = (decimal) ((data[4] & 255) * 256 + (data[3] & 255)) ;
                int a = data[4] & 255;
                int b = data[3] & 255;
                int c = a * 256;
                int d = b + c;
                  decimal f = decimal.Round(((decimal)d / (decimal)100), 1);
                //temperature = decimal.Round((temperature / (decimal)100), 1);
                for (int i = 0; i < data.Length; i++)
                {
                    rawdata += data[i].ToString() + "-";
                }

                MainPage.TestresultModel.ThermometerData(f, data.Length>6? data[6]:0, data[2] == 193, rawdata);                
            }
            catch(Exception ex)
            {
                MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Exception: " +ex.Message, 2);

            }
            //  double temperature = (((0x0e & 0xff) * 256 + (0x78 & 0xff)) * 0.01);
            // double t = temperature;
            // double t1 = temperature1;

        }
    }///
}
