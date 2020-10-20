using SBCDBModule.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoKallSBCApplication.Model;
using VideoKallSMC.Communication;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace VideoKallSBCApplication.BLEDevices
{
   public class GlucoMonitor
    {
        //  string BateryLeveCharacteristicName = "";
        private BluetoothLEDevice bluetoothLeDevice = null;
        // private GattCharacteristic selectedCharacteristic;

        // Only one registered characteristic at a time.
        private static GattCharacteristic GlucometerCharacteristic;
       // private GattCharacteristic BateryLevelCharacteristic;
        private GattPresentationFormat presentationFormat;
        private Dictionary<string, GattDeviceService> GTTServicelist = new Dictionary<string, GattDeviceService>();
        IReadOnlyList<GattCharacteristic> characteristics = null;
        public delegate void Notify(string msg, int errorOrStatus = 0);
        public delegate void ExecuteCommand();
        public ExecuteCommand Execute;
        public async void Connect()
        {
            DeviceType dv = MainPage.TestresultModel.GetDeviceType(4);
            BLEDeviceInfo device = null;
            foreach (var dev in MainPage.TestresultModel.BLEDevices)
            {
               // if (dev.Id.Equals(MainPage.TestresultModel.Glucometerid))
               if(dev.DeviceType == DeviceTypesenums.GLUCOMONITOR && dev.IsPaired)
                {
                    device = dev;
                    break;
                }
               else if(dev.DeviceType == DeviceTypesenums.GLUCOMONITOR && !dev.IsPaired)
                {
                    MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Device is not paired.", 1);
                    MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.GLUCORESULTRESSTATUS,
               "Device is not paired."));

                    return;
                }
            }

            GTTServicelist.Clear();
            if (device == null)
            {
                MainPage.TestresultModel.NotifyStatusMessage?.Invoke("No device found.", 1);
                MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.GLUCORESULTRESSTATUS,
                "Successfully subscribed"));
                return;
            }

            bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(device.DeviceInfo.Id);
            if (bluetoothLeDevice == null)
            {
                MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.GLUCORESULTRESSTATUS,
             " Device Unreachable."));
                MainPage.TestresultModel.NotifyStatusMessage?.Invoke(" Device Unreachable.", 1);
                return;
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
                SelectService(device);
            }
            else
            {
                MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.GLUCORESULTRESSTATUS,
            "Failed to connect. " + result.Status.ToString()));
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
                        MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.GLUCORESULTRESSTATUS,
           "Error accessing service " + result.Status.ToString()));
                        MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Error accessing service " + result.Status.ToString(), 1);
                        return;
                    }
                }
                else
                {
                    MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.GLUCORESULTRESSTATUS,
           "Error accessing service " + accessStatus.ToString()));
                    MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Error accessing service " + accessStatus.ToString(), 1);
                    return;
                }
            }
            catch (Exception ex)
            {
                MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.GLUCORESULTRESSTATUS,
         "Exception " + ex.Message));
                MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Exception " + ex.Message, 2);
                return;
            }

            foreach (GattCharacteristic c in characteristics)
            {
                string characteristicname = DisplayHelpers.GetCharacteristicName(c);
                if (c.Uuid.ToString().ToLower().Contains(dev.DeviceTypeInfo.CharacteristicAttributeid.ToLower()))  //MainPage.TestresultModel.glucoMeterHandleid.ToLower()))
                {
                    GlucometerCharacteristic = c;
                    break;
                }
            }

            if (GlucometerCharacteristic == null)
            {
                MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.GLUCORESULTRESSTATUS,
         "Gluco monitor Characteristic not found."));
                MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Gluco monitor Characteristic not found.", 1);
                return;
            }
            // BloodPressureMeasurementCharacteristic = selectedCharacteristic;
            var resultch = await GlucometerCharacteristic.GetDescriptorsAsync(BluetoothCacheMode.Uncached);
            if (resultch.Status != GattCommunicationStatus.Success)
            {
                MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.GLUCORESULTRESSTATUS, "Descriptor read failure: " + resultch.Status.ToString()));
                MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Descriptor read failure: " + resultch.Status.ToString(), 1);
                return;
            }
            presentationFormat = null;
            if (GlucometerCharacteristic.PresentationFormats.Count > 0)
            {
                if (GlucometerCharacteristic.PresentationFormats.Count.Equals(1))
                {
                    // Get the presentation format since there's only one way of presenting it
                    presentationFormat = GlucometerCharacteristic.PresentationFormats[0];
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
            if (GlucometerCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
            {
                cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
            }
            else if (GlucometerCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
            {
                cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
            }

            // BT_Code: Must write the CCCD in order for server to send indications.
            // We receive them in the ValueChanged event handler.
            status = await GlucometerCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);
            if (status == GattCommunicationStatus.Success)
            {
                MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.GLUCORESULTRESSTATUS,
        "Successfully subscribed."));
                //AddValueBPMeasureChangedHandler();
                MainPage.TestresultModel.NotifyStatusMessage?.Invoke("Successfully subscribed  value  changes");
            }
            else
            {
                MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.GLUCORESULTRESSTATUS,
       $"Error registering for value changes: { status}"));
                MainPage.TestresultModel.NotifyStatusMessage?.Invoke($"Error registering for value changes: {status}", 1);
            }

            GlucometerCharacteristic.ValueChanged += Characteristic_ValueChanged;
            MainPage.TestresultModel.DeviceConnectedTime(DeviceTypesenums.GLUCOMONITOR, DateTime.Now.ToString(), true);
            Execute?.Invoke();
        }

        private void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            byte[] data;
            IBuffer buffer = args.CharacteristicValue;
            try
            {
                GlucoResult Result = new GlucoResult();
                CryptographicBuffer.CopyToByteArray(buffer, out data);
                int cmd = data[0];
                int data0 = data[2];
                int data1 = data[3];
                int data2 = data[4];
                int data3 = data[5];
                int stopbit = data[6];
                string rawdata = string.Format("{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}", cmd, data[1],
                    data0, data1, data2, data3, stopbit, data[7]);
                MainPage.TestresultModel.LogMessage(rawdata);
                if (data[0] == 81  )
                {
                    switch(data[1])
                    {
                        case 37: //date time
                            string strdata0 = Convert.ToString(data0, 2).PadLeft(8, '0');
                           
                            string strdata1 = Convert.ToString(data1, 2).PadLeft(8, '0');
                            strdata1 = strdata1+ strdata0;
                            // 7bit 4bit 5bit
                            //15 14 13 12 11 10 9 8 7 6  5  4  3  2  1    0
                            //1  2  3  4  5  6 , 7 8 9 10 ,11 12   13   14   15
                            //0010100,0111,00101
                            string day = strdata1.Substring(11, 5);
                            string Month = strdata1.Substring(7, 4);
                            string year = strdata1.Substring(0, 7);
                            
                           
                            year = Convert.ToInt32(year, 2).ToString();
                            Month = Convert.ToInt32(Month, 2).ToString();
                            day = Convert.ToInt32(day, 2).ToString();
                            //000,01001
                            string strdata3 = Convert.ToString(data3,2).PadLeft(8,'0');
                            string strdata2 = Convert.ToString(data2, 2).PadLeft(8, '0');
                            string hour = Convert.ToInt32( strdata3.Substring(3, 5),2).ToString();
                            //00,010000
                            string min = Convert.ToInt32(strdata2.Substring(2, 6), 2).ToString();
                            Result.Year = year;
                            Result.Month = Month;
                            Result.Day = day;
                            Result.Hour = hour;
                            Result.Min = min;
                            MainPage.TestresultModel.GlucometerResult(Result, 0, false);
                            break;
                        case 38: //reading
                                 //  100 ,01000
                                 //   3210 43210
                                 //D3 10000001
                                 //D2 01000110
                                 //D3D2
                                 //10000001 01000110
                                 //GTYPE 10
                                 //10, 0110,0101000110

                            //D3 01000001
                            //D2 1000110
                            //01000001 1000110
                            strdata0 = Convert.ToString(data0, 2).PadLeft(8, '0');
                            strdata1 = Convert.ToString(data1, 2).PadLeft(8, '0');
                            strdata1 = strdata1 + strdata0;

                            strdata2 = Convert.ToString(data2, 2).PadLeft(8, '0');
                            strdata3 = Convert.ToString(data3, 2).PadLeft(8, '0');
                            strdata3 = strdata3 + strdata2;
                            string Type1 = strdata3.Substring(0, 2);
                            string Type2 = strdata3.Substring(2, 4);
                            string code = strdata3.Substring(6);

                            //string strmode = Convert.ToString(data3, 2).PadLeft(8, '0');
                            switch (Convert.ToInt32(Type1.Substring(0, 2), 2))
                            {
                                case 0:
                                    Result.Mode = "Gen(Any time)";
                                    break;
                                case 1:
                                    Result.Mode = "AC(Before meal)";
                                    break;
                                case 2:
                                    Result.Mode = "PC(After meal)";
                                    break;
                            }
                            switch (Convert.ToInt32(Type2,2))
                            {
                                case 0:
                                Result.TestResult = Convert.ToInt32(strdata1, 2).ToString();  //data0.ToString();
                                Result.Unit = "mg/dL";
                                Result.TestType = "GLU";
                                
                                    break;
                                case 7:
                                    {                                      
                                        decimal ketone = ((decimal)(Convert.ToInt32(strdata1, 2)) / 30);
                                        string[] decimalpart = ketone.ToString().Split('.');
                                        Result.TestResult = string.Format("{0}.{1}", decimalpart[0], decimalpart.Length >1 ? decimalpart[1].Substring(0,1) : "0");
                                        Result.Unit = "mmol/L";
                                        Result.TestType = "Ketone";
                                      //  Result.Mode = "Gen";
                                    }
                                    break;
                                case 6: //Hematocrit. 
                                    {
                                        decimal ketone = ((decimal)(Convert.ToInt32(strdata1, 2)) / 10);
                                        string[] decimalpart = ketone.ToString().Split('.');
                                        Result.TestResult = string.Format("{0}.{1}", decimalpart[0], decimalpart.Length > 1 ? decimalpart[1].Substring(0, 1) : "0");
                                        Result.Unit = "g/dL";
                                        Result.TestType = "KB";
                                       // Result.Mode = "Gen";
                                    }
                                    break;
                                case 9: // Cholesterol value. 
                                    {
                                        decimal ketone = ((decimal)(Convert.ToInt32(strdata1, 2)) /(decimal) 38.665);
                                        string[] decimalpart = ketone.ToString().Split('.');
                                        Result.TestResult = string.Format("{0}.{1}", decimalpart[0], decimalpart.Length > 1 ? decimalpart[1].Substring(0, 1) : "0");
                                        Result.Unit = "mmol/L";
                                        Result.TestType = "Cholesterol";
                                      //  Result.Mode = "Gen"; 

                                    }
                                    break;
                                case 8: //Uric acid value. 
                                    { 
                                        decimal ketone = ((decimal)(Convert.ToInt32(strdata1, 2)) / 10);
                                        string[] decimalpart = ketone.ToString().Split('.');
                                        Result.TestResult = string.Format("{0}.{1}", decimalpart[0], decimalpart.Length > 1 ? decimalpart[1].Substring(0, 1) : "0");
                                        Result.Unit = " mg/dL";
                                        Result.TestType = "Uric acid";
                                        Result.Mode = "Gen";
                                    }
                                    break;
                                case 11:
                                    {
                                        decimal ketone = ((decimal)(Convert.ToInt32(strdata1, 2)) / 10);
                                        string[] decimalpart = ketone.ToString().Split('.');
                                        Result.TestResult = string.Format("{0}.{1}", decimalpart[0], decimalpart.Length > 1 ? decimalpart[1].Substring(0, 1) : "0");
                                        Result.Unit = " mg/dL";
                                        Result.TestType = "Uric acid";
                                        Result.Mode = "Gen";
                                    }
                                    break;
                                case 12:
                                    {
                                        decimal ketone = ((decimal)(Convert.ToInt32(strdata1, 2)) / 10);
                                        string[] decimalpart = ketone.ToString().Split('.');
                                        Result.TestResult = string.Format("{0}.{1}", decimalpart[0], decimalpart.Length > 1 ? decimalpart[1].Substring(0, 1) : "0");
                                        Result.Unit = " mg/dL";
                                        Result.TestType = "Uric acid";
                                        Result.Mode = "Gen";
                                    }
                                    break;
                                default:
                                    {
                                       decimal ketone = ((decimal)(Convert.ToInt32(strdata1, 2)));
                                        Result.TestResult = ketone.ToString().Substring(0, 3);
                                        Result.Unit = " ";
                                        Result.TestType = " ";
                                        Result.Mode = " ";
                                    }
                                    break;
                            }
                            MainPage.TestresultModel.GlucometerResult(Result, 1, true);
                            break;
                        case 43: //count 2b

                            int sum = data0 + data1;
                            string strdataq0 = sum.ToString("x");
                            Result.RecordCount = Convert.ToInt32(strdataq0)-1;
                            MainPage.TestresultModel.GlucometerResult(Result, 2, true);

                            break;
                    }

                }

            }
            catch(Exception ex)
            {
                string s = ex.Message;
            }
            
        }

        public async void LastestTestResultTime()
        {
            if(GlucometerCharacteristic != null)
            {
                //  GlucoMeterCommand
                var writeBuffer = CryptographicBuffer.CreateFromByteArray(GlucoMeterCommand.latestReadingPart1); 
                // BT_Code: Writes the value from the buffer to the characteristic.
                var result = await GlucometerCharacteristic.WriteValueWithResultAsync(writeBuffer);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    MainPage.TestresultModel.NotifyStatusMessage?.Invoke($"Command written : {result.Status.ToString()}", 1);
                }
                else
                {
                    MainPage.TestresultModel.NotifyStatusMessage?.Invoke($"Command Failed. : {result.Status.ToString()}", 1);
                    return  ;
                }

                //  GlucoMeterCommand
                  writeBuffer = CryptographicBuffer.CreateFromByteArray(GlucoMeterCommand.latestReadingPart2);
                // BT_Code: Writes the value from the buffer to the characteristic.
                  result = await GlucometerCharacteristic.WriteValueWithResultAsync(writeBuffer);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    MainPage.TestresultModel.NotifyStatusMessage?.Invoke($"Command written : {result.Status.ToString()}", 1);
                }
                else
                {
                    MainPage.TestresultModel.NotifyStatusMessage?.Invoke($"Command Failed : {result.Status.ToString()}", 1);
                    return;
                }
            }
        }

        public async void DeleteAll()
        {
            if (GlucometerCharacteristic != null)
            {
                //  GlucoMeterCommand
                var writeBuffer = CryptographicBuffer.CreateFromByteArray(GlucoMeterCommand.DeleteAlldata);
                // BT_Code: Writes the value from the buffer to the characteristic.
                var result = await GlucometerCharacteristic.WriteValueWithResultAsync(writeBuffer);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    MainPage.TestresultModel.NotifyStatusMessage?.Invoke($"Deleted all record : {result.Status.ToString()}", 1);
                }
                else
                {
                    MainPage.TestresultModel.NotifyStatusMessage?.Invoke($"Command Failed. : {result.Status.ToString()}", 1);
                    return;
                } 
            }
        }

        public async void RecordCount()
        {
            if (GlucometerCharacteristic != null)
            {
                //  GlucoMeterCommand
                var writeBuffer = CryptographicBuffer.CreateFromByteArray(GlucoMeterCommand.NumberOfStoredRecord);
                // BT_Code: Writes the value from the buffer to the characteristic.
                var result = await GlucometerCharacteristic.WriteValueWithResultAsync(writeBuffer);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    MainPage.TestresultModel.NotifyStatusMessage?.Invoke($"Number of records all record : {result.Status.ToString()}", 1);
                }
                else
                {
                    MainPage.TestresultModel.NotifyStatusMessage?.Invoke($"Command Failed. : {result.Status.ToString()}", 1);
                    return;
                }
            }
        }

        public async void RecordByIndex(int index)
        {
            if (GlucometerCharacteristic != null)
            {
                //  GlucoMeterCommand
                var writeBuffer = CryptographicBuffer.CreateFromByteArray(GlucoMeterCommand.ReadPart1byIndex(index));
                // BT_Code: Writes the value from the buffer to the characteristic.
                var result = await GlucometerCharacteristic.WriteValueWithResultAsync(writeBuffer);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    MainPage.TestresultModel.NotifyStatusMessage?.Invoke($"Command written : {result.Status.ToString()}", 1);
                }
                else
                {
                    MainPage.TestresultModel.NotifyStatusMessage?.Invoke($"Command Failed. : {result.Status.ToString()}", 1);
                    return;
                }

                //  GlucoMeterCommand
                writeBuffer = CryptographicBuffer.CreateFromByteArray(GlucoMeterCommand.ReadPart2byIndex(index));
                // BT_Code: Writes the value from the buffer to the characteristic.
                result = await GlucometerCharacteristic.WriteValueWithResultAsync(writeBuffer);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    MainPage.TestresultModel.NotifyStatusMessage?.Invoke($"Command written : {result.Status.ToString()}", 1);
                }
                else
                {
                    MainPage.TestresultModel.NotifyStatusMessage?.Invoke($"Command Failed : {result.Status.ToString()}", 1);
                    return;
                }
            }
        }

    }//
}
