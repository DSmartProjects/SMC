using SBCDBModule.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using VideoKallSBCApplication;
using VideoKallSMC.Communication;
using VideoKallSMC.Views;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace VideoKallSBCApplication.BLEDevices
{
   public class BPCuff
    {
        private BluetoothLEDevice bluetoothLeDevice = null;
        // Only one registered characteristic at a time.
        private GattCharacteristic BloodPressureMeasurementCharacteristic;
        private GattCharacteristic BateryLevelCharacteristic;
        private GattPresentationFormat presentationFormat;
        private Dictionary<string, GattDeviceService> GTTServicelist = new Dictionary<string, GattDeviceService>();
        IReadOnlyList<GattCharacteristic> characteristics = null;
        public delegate void Notify(string msg, int errorOrStatus = 0);
        public Notify NotifyStatusMessage = null;

        void SendConnectionStatusmessageToMCC(string msg)
        {
            MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.BPCONSTATUS,
            msg));
        }
        public async void Connect()
        {
            BLEDeviceInfo device = null;

            foreach (var dev in MainPage.TestresultModel.BLEDevices)
            {
                 if( dev.DeviceType == DeviceTypesenums.BPMONITOR && dev.IsPaired)
                {
                    device = dev;
                    break;
                }else if(dev.DeviceType == DeviceTypesenums.BPMONITOR && !dev.IsPaired)
                {
                    SendConnectionStatusmessageToMCC("BP Monitor is not Paired.");
                    NotifyStatusMessage?.Invoke("BP Monitor is not Paired.", 3);
                    return;
                }

            }

             
            GTTServicelist.Clear();
            if(device == null)
            {
                SendConnectionStatusmessageToMCC("BP Monitor is not foud.");
                NotifyStatusMessage?.Invoke("BP Monitor is not foud.", 3);
                return;
            }

            bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(device.DeviceInfo.Id);
            if(bluetoothLeDevice == null)
            {
                SendConnectionStatusmessageToMCC("Device Unreachable.");
                NotifyStatusMessage?.Invoke("Device Unreachable.",1);
                return;
            }

            // Note: BluetoothLEDevice.GattServices property will return an empty list for unpaired devices. For all uses we recommend using the GetGattServicesAsync method.
            // BT_Code: GetGattServicesAsync returns a list of all the supported services of the device (even if it's not paired to the system).
            // If the services supported by the device are expected to change during BT usage, subscribe to the GattServicesChanged event.
            GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
            if (result.Status == GattCommunicationStatus.Success)
            {
                var services = result.Services;
                foreach(var svc in services)
                {
                    GTTServicelist.Add(svc.Uuid.ToString().ToLower(), svc);
                }
                SelectBPMeasureServicesAsync(device);//MainPage.TestresultModel.BPServiceID); 
            }
            else
            {
                SendConnectionStatusmessageToMCC("Failed to connect. " + result.Status.ToString());
                NotifyStatusMessage?.Invoke("Failed to connect. "+ result.Status.ToString(),1);
            }

        }

        public async  void SelectBPMeasureServicesAsync(BLEDeviceInfo dv)
        {
            try
            {
                var service = GTTServicelist[dv.DeviceTypeInfo.ServiceID.ToLower()];
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
                        SendConnectionStatusmessageToMCC("Error accessing service " + result.Status.ToString());
                        NotifyStatusMessage?.Invoke("Error accessing service "+ result.Status.ToString(),1);
                        return;
                    }
                }
                else
                {
                    SendConnectionStatusmessageToMCC("Error accessing service " + accessStatus.ToString());
                    NotifyStatusMessage?.Invoke("Error accessing service "+ accessStatus.ToString(),1);
                    return;
                }
            }
            catch(Exception ex)
            {
                SendConnectionStatusmessageToMCC("Exception " + ex.Message);
                NotifyStatusMessage?.Invoke("Exception " + ex.Message,2);
                return;
            }
            //BloodPressureMeasurement
            
            foreach (GattCharacteristic c in characteristics)
            {
                 string characteristicname = DisplayHelpers.GetCharacteristicName(c);
                if((string.Compare(characteristicname, MainPage.TestresultModel.BPCharacteristicName, true) == 0))
                {
                    BloodPressureMeasurementCharacteristic = c;
                    break;
                }  
            }

            if(BloodPressureMeasurementCharacteristic == null)
            {
                SendConnectionStatusmessageToMCC("BloodPressureMeasurement Characteristic not found.");
                NotifyStatusMessage?.Invoke("BloodPressureMeasurement Characteristic not found.",1);
                return;
            }
           // BloodPressureMeasurementCharacteristic = selectedCharacteristic;
            var resultch = await BloodPressureMeasurementCharacteristic.GetDescriptorsAsync(BluetoothCacheMode.Uncached);
            if (resultch.Status != GattCommunicationStatus.Success)
            {
                SendConnectionStatusmessageToMCC("Descriptor read failure: " + resultch.Status.ToString());
                NotifyStatusMessage?.Invoke("Descriptor read failure: " + resultch.Status.ToString(),1);
                return;
            }

            presentationFormat = null;
            if (BloodPressureMeasurementCharacteristic.PresentationFormats.Count > 0)
            {
                if (BloodPressureMeasurementCharacteristic.PresentationFormats.Count.Equals(1))
                {
                    // Get the presentation format since there's only one way of presenting it
                    presentationFormat = BloodPressureMeasurementCharacteristic.PresentationFormats[0];
                }
                else
                {
                    // It's difficult to figure out how to split up a characteristic and encode its different parts properly.
                    // In this case, we'll just encode the whole thing to a string to make it easy to print out.
                }
            }
            SubscribeBPMeasureService();
        }

        async void SubscribeBPMeasureService()
        {
            // initialize status
            GattCommunicationStatus status = GattCommunicationStatus.Unreachable;
            var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.None;
            if (BloodPressureMeasurementCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
            {
                cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
            }
            else if (BloodPressureMeasurementCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
            {
                cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
            }

            // BT_Code: Must write the CCCD in order for server to send indications.
            // We receive them in the ValueChanged event handler.
            status = await BloodPressureMeasurementCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue); 
            if (status == GattCommunicationStatus.Success)
            {
                AddValueBPMeasureChangedHandler();
                SendConnectionStatusmessageToMCC("Successfully subscribed.");
                NotifyStatusMessage?.Invoke("Successfully subscribed for value changes");
            }
            else
            {
                SendConnectionStatusmessageToMCC($"Error registering for value changes: {status}");
                NotifyStatusMessage?.Invoke($"Error registering for value changes: {status}",1);
            }
            //BateryLevel
           // SelectBateryLeveServicesAsync(ServiceID);
        }

        void AddValueBPMeasureChangedHandler()
        {
            BloodPressureMeasurementCharacteristic.ValueChanged += Characteristic_ValueChanged;
            MainPage.TestresultModel.DeviceConnectedTime(DeviceTypesenums.BPMONITOR, DateTime.Now.ToString(), true);


            //if (subscribedForNotifications)
            //{
            //    BloodPressureMeasurementCharacteristic.ValueChanged -= Characteristic_ValueChanged;
            //    BloodPressureMeasurementCharacteristic = null;
            //    subscribedForNotifications = false;
            //}
            //else
            //{
            //    if (!subscribedForNotifications)
            //    {

            //        BloodPressureMeasurementCharacteristic.ValueChanged += Characteristic_ValueChanged;
            //        subscribedForNotifications = true;
            //    }
            //}
        }

        private void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            byte[] data;
           IBuffer buffer=  args.CharacteristicValue;
              BPResult bpResult = new BPResult();
            
            try
            {
                CryptographicBuffer.CopyToByteArray(buffer, out data);
                bpResult.SYS = data[1].ToString();
                bpResult.DIA = data[3].ToString();
                bpResult.Pulse = data[14].ToString();
                bpResult.DateTimeOfTest = DateTime.Now;
                MainPage.TestresultModel.BPCuff(bpResult);
            }
            catch(Exception ex)
            {
                SendConnectionStatusmessageToMCC($"Error : {ex.Message}");
                NotifyStatusMessage?.Invoke($"Error : {ex.Message}",2);
            }
        }
        //
        public async void SelectBateryLeveServicesAsync(Guid gd)
        {
           
            foreach (GattCharacteristic c in characteristics)
            {
              //  string characteristicname = DisplayHelpers.GetCharacteristicName(c);
                if (c.AttributeHandle == 1296 )
                {
                        BateryLevelCharacteristic = c;
                    break;
                }
            }

            if (BateryLevelCharacteristic == null)
            {
                SendConnectionStatusmessageToMCC("BloodPressureMeasurement Characteristic not found.");
                NotifyStatusMessage?.Invoke("BloodPressureMeasurement Characteristic not found.", 1);
                return;
            }
                 
            var resultch = await BateryLevelCharacteristic.GetDescriptorsAsync(BluetoothCacheMode.Uncached);
            if (resultch.Status != GattCommunicationStatus.Success)
            {
                SendConnectionStatusmessageToMCC("Descriptor read failure: " + resultch.Status.ToString());
                NotifyStatusMessage?.Invoke("Descriptor read failure: " + resultch.Status.ToString(), 1);
                return;
            }

            presentationFormat = null;
            if (BateryLevelCharacteristic.PresentationFormats.Count > 0)
            {
                if (BateryLevelCharacteristic.PresentationFormats.Count.Equals(1))
                {
                    // Get the presentation format since there's only one way of presenting it
                    presentationFormat = BateryLevelCharacteristic.PresentationFormats[0];
                }
                else
                {
                    // It's difficult to figure out how to split up a characteristic and encode its different parts properly.
                    // In this case, we'll just encode the whole thing to a string to make it easy to print out.
                }
            }
              SubscribeBateryLevelService();
        }

        async void SubscribeBateryLevelService()
        {
            // initialize status
            GattCommunicationStatus status = GattCommunicationStatus.Unreachable;
            var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.None;
            if (BateryLevelCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
            {
                cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
            }
            else if (BateryLevelCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
            {
                cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
            }

            // BT_Code: Must write the CCCD in order for server to send indications.
            // We receive them in the ValueChanged event handler.
            status = await BateryLevelCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);
            if (status == GattCommunicationStatus.Success)
            {
                AddValueBateryLevelChangedHandler();
                SendConnectionStatusmessageToMCC("Successfully subscribed");
                NotifyStatusMessage?.Invoke("Successfully subscribed for BateryLevel changes");
            }
            else
            {
                SendConnectionStatusmessageToMCC($"Error registering for value changes: {status}");
                NotifyStatusMessage?.Invoke($"Error registering for value changes: {status}", 1);
            }
        }

        void AddValueBateryLevelChangedHandler()
        {

            BloodPressureMeasurementCharacteristic = null;
            BloodPressureMeasurementCharacteristic.ValueChanged += Characteristic_BateryLevelValueChanged;

            //MainPage.TestresultModel.DeviceConnectedTime(MainPage.TestresultModel.bpcuffID, DateTime.Now.ToString(), true);

            //if (subscribedForNotifications)
            //{
            //    BloodPressureMeasurementCharacteristic.ValueChanged -= Characteristic_BateryLevelValueChanged;
            //    BloodPressureMeasurementCharacteristic = null;
            //    subscribedForNotifications = false;
            //}
            //else
            //{
            //    if (!subscribedForNotifications)
            //    {
            //        BloodPressureMeasurementCharacteristic = null;
            //        BloodPressureMeasurementCharacteristic.ValueChanged += Characteristic_BateryLevelValueChanged;
            //        subscribedForNotifications = true;
            //    }
            //}
        }

        private void Characteristic_BateryLevelValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            byte[] data;
            IBuffer buffer = args.CharacteristicValue;
            BPResult bpResult = new BPResult();

            try
            {
                CryptographicBuffer.CopyToByteArray(buffer, out data);
                bpResult.SYS = data[1].ToString();
                bpResult.DIA = data[3].ToString();
                bpResult.Pulse = data[14].ToString();
                bpResult.DateTimeOfTest = DateTime.Now;
                MainPage.TestresultModel.BPCuff(bpResult);
            }
            catch (Exception ex)
            {
                NotifyStatusMessage?.Invoke($"Error : {ex.Message}", 2);
            }
        }

         
        
    }
}
