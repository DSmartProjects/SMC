using SBCDBModule.DB;
using System;
using System.Collections.Generic;
using VideoKallSMC.Communication;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace VideoKallSBCApplication.BLEDevices
{
    public class OxyMeter
    {
       // Guid serviceID = new Guid("6e400001-b5a3-f393-e0a9-e50e24dcca9e");
    
         private BluetoothLEDevice bluetoothLeDevice = null;
        // Only one registered characteristic at a time.
        private GattCharacteristic oxymeterCharacteristic;
        private GattPresentationFormat presentationFormat;
        private Dictionary<string, GattDeviceService> GTTServicelist = new Dictionary<string, GattDeviceService>();
        IReadOnlyList<GattCharacteristic> characteristics = null;
       
        public   async void Connect()
        {
            DeviceType dv = MainPage.TestresultModel.GetDeviceType(2);
            BLEDeviceInfo device = null;
            foreach (var dev in MainPage.TestresultModel.BLEDevices)
            {
               // if (dev.Id.Equals(MainPage.TestresultModel.OximeterID))
               if( dev.DeviceType == DeviceTypesenums.OXIMETER && dev.IsPaired)
                {
                    device = dev;
                    break;
                }else if(dev.DeviceType == DeviceTypesenums.OXIMETER && !dev.IsPaired)
                {
                    MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.PUSLEOXIMETERCONNECTIONMSG,
                         "Device is not paired.","Error"));

                    MainPage.TestresultModel.NotifyStatusMessage?.Invoke(" Device is not paired.", 1);
                    return;
                }
            }

            GTTServicelist.Clear();
            if (device == null)
            {
                try
                {
                    MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.PUSLEOXIMETERCONNECTIONMSG,
                     "No device found.","Error"));
                    MainPage.TestresultModel.NotifyStatusMessage?.Invoke("No device found.", 1);
                    return;
                }
                catch (Exception)
                {
                    MainPage.TestPanelVM.InstuctionNoteCallBackCompleted?.Invoke(DeviceTypesenums.OXIMETER, "");

                }
             
            }
            try
            {
                bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(device.DeviceInfo.Id);
            }
            catch (Exception)
            {
                MainPage.TestPanelVM.InstuctionNoteCallBackCompleted?.Invoke(DeviceTypesenums.THERMOMETER, "");
                return;
            }          
            if (bluetoothLeDevice == null)
            {
                MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.PUSLEOXIMETERCONNECTIONMSG,
                       " Device Unreachable.", "Error"));
                MainPage.TestresultModel.NotifyStatusMessage?.Invoke(" Device Unreachable.", 1);
            }

            // Note: BluetoothLEDevice.GattServices property will return an empty list for unpaired devices. For all uses we recommend using the GetGattServicesAsync method.
            // BT_Code: GetGattServicesAsync returns a list of all the supported services of the device (even if it's not paired to the system).
            // If the services supported by the device are expected to change during BT usage, subscribe to the GattServicesChanged event.
            GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
            if (result.Status == GattCommunicationStatus.Success)
            {
                
                var services = result.Services;
                foreach (var svc in services)
                {
                    GTTServicelist.Add(svc.Uuid.ToString().ToLower(), svc);
                }
                SelectService(device); // MainPage.TestresultModel.OximeterserviceID);
            }
            else
            {
                try
                {
                    MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.PUSLEOXIMETERCONNECTIONMSG, "Failed to connect. " + result.Status.ToString(),"Error"));
                    MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Failed to connect. " + result.Status.ToString(), 1);
                    MainPage.TestPanelVM.InstuctionNoteCallBackCompleted?.Invoke(DeviceTypesenums.OXIMETER, "");
                }
                catch (Exception)
                {
                    MainPage.TestPanelVM.InstuctionNoteCallBackCompleted?.Invoke(DeviceTypesenums.OXIMETER, "");
                }

            }
        }

       async void SelectService(BLEDeviceInfo dev)
        {
            try
            {
                var service = GTTServicelist[dev.DeviceTypeInfo.ServiceID.ToString().ToLower()];
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
                        MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.PUSLEOXIMETERCONNECTIONMSG,
                      "Error accessing service " + result.Status.ToString(), "Error"));
                        MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Error accessing service " + result.Status.ToString(), 1);
                        return;
                    }
                }
                else
                {
                    MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.PUSLEOXIMETERCONNECTIONMSG,
                    "Error accessing service " + accessStatus.ToString(), "Error"));
                    MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Error accessing service " + accessStatus.ToString(), 1);
                    return;
                }
            }
            catch(Exception ex)
            {
                MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.PUSLEOXIMETERCONNECTIONMSG,
                  "Exception " + ex.Message, "Error"));
                MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Exception " + ex.Message, 2);
                return;
            }

            foreach (GattCharacteristic c in characteristics)
            {
                string characteristicname = DisplayHelpers.GetCharacteristicName(c);
                if ( c.AttributeHandle.ToString().Equals(dev.DeviceTypeInfo.CharacteristicAttributeid))//MainPage.TestresultModel.OximeterHandleid)
                {
                    oxymeterCharacteristic = c;
                    break;
                }
            }

            if (oxymeterCharacteristic == null)
            {
                MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.PUSLEOXIMETERCONNECTIONMSG,
                 "OxyMeter Characteristic not found.", "Error"));
                MainPage.TestresultModel.NotifyStatusMessage?.Invoke("OxyMeter Characteristic not found.", 1);
                return;
            }
            // BloodPressureMeasurementCharacteristic = selectedCharacteristic;
            var resultch = await oxymeterCharacteristic.GetDescriptorsAsync(BluetoothCacheMode.Uncached);
            if (resultch.Status != GattCommunicationStatus.Success)
            {
                MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.PUSLEOXIMETERCONNECTIONMSG,
        "Descriptor read failure: " + resultch.Status.ToString(), "Error"));
                MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Descriptor read failure: " + resultch.Status.ToString(), 1);
                return;
            }
            presentationFormat = null;
            if (oxymeterCharacteristic.PresentationFormats.Count > 0)
            {
                if (oxymeterCharacteristic.PresentationFormats.Count.Equals(1))
                {
                    // Get the presentation format since there's only one way of presenting it
                    presentationFormat = oxymeterCharacteristic.PresentationFormats[0];
                }
                else
                {
                    // It's difficult to figure out how to split up a characteristic and encode its different parts properly.
                    // In this case, we'll just encode the whole thing to a string to make it easy to print out.
                }
            }
            SubscribeDeviceReading();
        }///

          async void SubscribeDeviceReading()
        {
            // initialize status
            GattCommunicationStatus status = GattCommunicationStatus.Unreachable;
            var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.None;
            if (oxymeterCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
            {
                cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
            }
            else if (oxymeterCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
            {
                cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
            }

            // BT_Code: Must write the CCCD in order for server to send indications.
            // We receive them in the ValueChanged event handler.
            status = await oxymeterCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);
            if (status == GattCommunicationStatus.Success)
            {
                try
                {
                    
                        //AddValueBPMeasureChangedHandler();
                        MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.PUSLEOXIMETERCONNECTIONMSG,
                        "Successfully subscribed","success"));
                        MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Successfully subscribed for oximeter value  changes");                    

                }
                catch (Exception)
                {
                    MainPage.TestPanelVM.InstuctionNoteCallBackCompleted?.Invoke(DeviceTypesenums.OXIMETER, "");
                }

            }
            else
            {
                MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.PUSLEOXIMETERCONNECTIONMSG,
               $"Error registering for value changes: {status}", "Error"));
                MainPage.TestresultModel.NotifyStatusMessage?.Invoke($"Error registering for value changes: {status}", 1);
            }
            oxymeterCharacteristic.ValueChanged += Characteristic_ValueChanged;
            MainPage.TestresultModel.DeviceConnectedTime(DeviceTypesenums.OXIMETER, DateTime.Now.ToString(), true);


        }

        private void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
           byte[] data;
           IBuffer buffer=  args.CharacteristicValue;
              BPResult bpResult = new BPResult();

            try
            {
                CryptographicBuffer.CopyToByteArray(buffer, out data);
                string rawdata = "";
                //"170-85-15-8-1"
                int datapos = 0; 
                for (int i=0; i<data.Length;i++)
                {
                    if (i + 4 >= data.Length)
                        break;

                    if( data[i] == 170 &&
                        data[i+1]== 85 &&
                        data[i + 2]== 15 &&
                        data[i+3]== 8 &&
                        data[i+4]== 1)
                    {
                        datapos = i + 4;
                      
                        break;
                    }
                }

                for(int i=0; i<data.Length; i++)
                {
                    rawdata += data[i].ToString() + "-";
                }
                if (datapos>0 && datapos < data.Length - 2  )
                {
                     
                    MainPage.TestresultModel.OxymeterData(data[datapos+1].ToString(), data[datapos + 2].ToString());
                }
                MainPage.TestresultModel.LogMessage(rawdata);
            }
            catch(Exception ex)
            {
                MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.PUSLEOXIMETERCONNECTIONMSG,
              $"Error from OXymeter data reading : {ex.Message}", "Error"));
                MainPage.TestresultModel.NotifyStatusMessage?.Invoke($"Error from OXymeter data reading : {ex.Message}", 2);
            }
        }

      
    }//
}
