using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VideoKallSMC.Communication;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;
namespace VideoKallSBCApplication.Communication
{
   public class SpiroBankIIadv
    {
        string Id = "";
        RfcommDeviceService SpirobanktoothService = null;
        StreamSocket streamSocket;
        private  DataWriter cmdWriter = null;
         DataReader respReader;
        bool stopDataReaderLoop = false;
        uint unreaddata = 32;
        
        public const byte SdpServiceNameAttributeType = (4 << 3) | 5;
        /// <summary>
        /// results
        /// </summary>
        uint cod_FLU = 0x10;
        uint cod_VOL = 0x20;
        uint TestHeadercod_TEMP = 0x50;
        uint TestHeaderFVC_cod_STEPV = 0x30;
        uint TestHeaderVC_cod_STEPT = 0x40;

        Queue<byte> LiveData { get; set; } = new Queue<byte>();
        public bool IsFVCTest { get; set; }
        public bool IsLiveTestDataInProgress { get; set; }
        bool IsTestHeaderDataReceived { get; set; }
        bool EndTest { get; set; }
        byte[] TestHeaderTemp = null;
        byte[] TestHeaderFVCorVC = null;
        byte[] TestData = null;
        double Vol = 0;
        double time = 0;
        private readonly object synchlock = new object();

        public delegate void ShowLiveData(string msg);
        public ShowLiveData LivedataDisplayEvent;
        public ShowLiveData EndTestEvent; 
        public SpiroBankIIadv()
        {
            EndTestEvent += EndTestMsg;
        }

        public void EndTestMsg(string msg)
        {
            EndTest = true;
            lock (synchlock)
            {
                LiveData.Clear();
            }
            GetResultCmd();
              Vol = 0;
              time = 0;
        }
        public void StartFVCTestcmd()
        {

            StartFVCTest();
            IsFVCTest = true;
            Task.Run(() =>  ParseLiveData());
            Sendcmd(cmdWriter, 0x88);
        }


        public void StartVCTestcmd()
        { 
              StartVCTest();

            Task.Run(() =>  ParseLiveData());

            Sendcmd(cmdWriter, 0x82);
        }

        public void InfoCmd()
        {
            Sendcmd(cmdWriter, 0x00);
           // Sendcmd(cmdWriter, 0xFF);
        }

        public void StopTestAndCalculateResultcmd()
        {
            
            Sendcmd(cmdWriter, 0x1B);
        }
        public void GetResultCmd()
        { 
            Sendcmd(cmdWriter, 0xA2);//0xD1, 
        }
        public void BestResultTest()
        {
            Sendcmd(cmdWriter, 0xD1);
        }

        async void Sendcmd(DataWriter sw, byte bv = 0x00)
        {
            try
            {
                if (sw == null)
                    return;
                sw.WriteByte(bv);
                await sw.StoreAsync(); 
            }
            catch (Exception e)
            {
                 
            }
        }
        private void StartFVCTest()
        {
            Vol = 0;
            time = 0;
            EndTest = false;
            IsTestHeaderDataReceived = false;
            IsFVCTest = true;
            TestHeaderTemp = new byte[3];
            TestHeaderFVCorVC = new byte[3];
            TestData = new byte[3];
           
            IsLiveTestDataInProgress = true;
            LiveData.Clear();

        }

        private void StartVCTest()
        {
            Vol = 0;
            time = 0;
            EndTest = false;
            IsTestHeaderDataReceived = false;
            TestHeaderTemp = new byte[3];
            TestHeaderFVCorVC = new byte[3];
            TestData = new byte[3];
            IsFVCTest = false;
            IsLiveTestDataInProgress = true;
            LiveData.Clear();

        }

        private void StopTest()
        {
            EndTest = true;
            IsTestHeaderDataReceived = false;
            //   TestHeaderTemp = new byte[3];
            //   TestHeaderFVCorVC = new byte[3];
            //  IsFVCTest = false;
            IsLiveTestDataInProgress = false;
            LiveData.Clear();
        }

        private void AddToLiveDataCollection(byte data)
        {
            lock (synchlock)
            {
                LiveData.Enqueue(data);
            }
        }

        public async Task ParseLiveData()
        {
            if (!IsDeviceConnected)
                return;
            try
            {
                while (true)
                {
                    if (IsLiveTestDataInProgress)
                    {
                        #region Headers
                        if (!IsTestHeaderDataReceived)
                        {
                            if (LiveData.Count >= 6)
                            {
                                lock (synchlock)
                                {
                                    byte data = LiveData.ElementAt(0);
                                    bool TempheaderRceived = false;
                                    bool TestheaderRceived = false;
                                    if (data == 80)
                                    {
                                        TestHeaderTemp[0] = LiveData.Dequeue();
                                        TestHeaderTemp[1] = LiveData.Dequeue();
                                        TestHeaderTemp[2] = LiveData.Dequeue();
                                        TempheaderRceived = true;
                                    }

                                    data = LiveData.ElementAt(0);
                                    if (data == 48 || data == 64)
                                    {
                                        TestHeaderFVCorVC[0] = LiveData.Dequeue();
                                        TestHeaderFVCorVC[1] = LiveData.Dequeue();
                                        TestHeaderFVCorVC[2] = LiveData.Dequeue();
                                        TestheaderRceived = true;
                                    }

                                    if ((TestheaderRceived) && (TempheaderRceived))
                                        IsTestHeaderDataReceived = true;
                                }
                            }
                        }
                        #endregion
                        #region LiaveData
                        if (IsTestHeaderDataReceived )
                        {
                            double flow = 0;
                            string res = "";
                            lock (synchlock)
                            {
                                if (LiveData.Count() > 2)
                                {
                                    byte data = LiveData.ElementAt(0);
                                    if ((((data == 16) && IsFVCTest) || (data == 32 && !IsFVCTest)) && LiveData.Count >= 3)
                                    {

                                        TestData[0] = LiveData.Dequeue();
                                        TestData[1] = LiveData.Dequeue();
                                        TestData[2] = LiveData.Dequeue();
                                        if (IsFVCTest)
                                        {
                                            flow = (double)(TestData[1] * 256 + TestData[2]);
                                            if (flow > 32768)
                                            {
                                                flow = (flow / 100) - 655.33;
                                                Vol -= (((double)(TestHeaderFVCorVC[1] * 256 + TestHeaderFVCorVC[2])) / 10000);

                                            }
                                            else
                                            {
                                                flow = (double)flow / 100;
                                                Vol += ((double)((double)(TestHeaderFVCorVC[1] * 256) + (double)TestHeaderFVCorVC[2])) / 10000;
                                            }

                                            string strflow = string.Format("{0:0.00}", flow);
                                            string strvol = string.Format("{0:0.00}", Vol);
                                            string s = strflow + "," + strvol;
                                            res = "FVC Flow,Vol" + flow.ToString() + ",  " + Vol.ToString() + "-";

                                                LivedataDisplayEvent?.Invoke(s);
                                            MainPage.mainPage.commwithChartapp.SendMsg(string.Format(CommunicationCommands.SpirometerFVCdata, s));
                                            MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.SpirometerFVCdata, s));

                                        }
                                        else
                                        {
                                            //VC Test
                                            // Time(X):
                                            //Value of Time Step in Sec = (STEPt_MSB * 256 + STEPt_LSB) / 1000
                                            //Current Time = Current Time + Time Step
                                            //Volume(Y):
                                            //Volume Value in L = ((VOL_MSB * 256 + VOL_LSB) - 5000) / 100

                                            double timeval = (double)(TestHeaderFVCorVC[1] * 256 + TestHeaderFVCorVC[2]);
                                            time += (double)timeval / 1000;
                                            Vol = (double)((TestData[1] * 256 + TestData[2]) - 5000);
                                            Vol = (double)(Vol / 100);

                                            string strvol = string.Format("{0:0.000}", Vol);
                                            string strtime = string.Format("{0:0.000}", time);
                                            string s = strvol + "," + strtime;
                                            res = "VC  Vol, time" + Vol.ToString() + ",  " + time.ToString() + "-";


                                            MainPage.mainPage.commwithChartapp.SendMsg(string.Format(CommunicationCommands.SpirometerVC, s));
                                            MainPage.mainPage.commChannel.SendMessageToMCC(string.Format(CommunicationCommands.SpirometerVC, s));
                                              LivedataDisplayEvent?.Invoke(s);

                                        }

                                    }
                                    else
                                    if (LiveData.Count() > 2)
                                    {
                                        data = LiveData.ElementAt(0);
                                        if(data == 6 && LiveData.Count>2)
                                        {
                                            Vol = 0;
                                            
                                            int data1 = LiveData.Dequeue();
                                            data1 = LiveData.Dequeue();
                                            data1 = LiveData.Dequeue();
                                        }
                                        else if(data == 1)
                                        {
                                            int data2 = LiveData.Dequeue();
                                            data2 = LiveData.Dequeue();
                                            data2 = LiveData.Dequeue();
                                        }
                                        if (data == 4)
                                            EndTest = true;
                                    }
                                }
                                #region EndTest

                               
                                #endregion

                            }

                            #endregion
                            //if (tmplog)
                            //    await LogMessage("Test Data" + TestData[0].ToString() + "," + TestData[1].ToString() + "," + TestData[2].ToString() + "header: " + TestHeaderFVCorVC[1].ToString() + ": " + TestHeaderFVCorVC[2].ToString() + res);
                          //  tmplog = false;
                        }
                    }

                    if (EndTest)
                        break;
                   // Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                 await LogMessage(ex.Message);
                EndTest = true;

            }

        }

        static async Task LogMessage(string msg)
        {
            msg = msg + "\n";
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile pinfofile = await localFolder.CreateFileAsync("data.txt", CreationCollisionOption.OpenIfExists);
            await Windows.Storage.FileIO.AppendTextAsync(pinfofile, msg, Windows.Storage.Streams.UnicodeEncoding.Utf8);
        }
        public async Task<string> GetPairedDevices()
        {
            var devselector = BluetoothDevice.GetDeviceSelector();
            var devcol = await DeviceInformation.FindAllAsync(devselector);
            string devicename = "";
            foreach (var v in devcol)
            {
                if (v.Name.ToLower().Contains("spirobank"))
                {
                    BluetoothDevice bluDevice = await BluetoothDevice.FromIdAsync(v.Id);

                    Id = v.Id;
                    if (bluDevice != null)
                    {
                        devicename = v.Name;
                        break;
                    }
                }
            }

            return devicename;
        }


     public   async Task<string> ConnectSpiroBankII()
        {
            string res = "Device not found.";
            try
            {
                if (Id.Length > 0)
                {
                    BluetoothDevice bluDevice = await BluetoothDevice.FromIdAsync(Id);
                    var rfcomm = await bluDevice.GetRfcommServicesAsync(BluetoothCacheMode.Uncached);

                    if (rfcomm.Services.Count() > 0)
                    {
                        SpirobanktoothService = rfcomm.Services[0];
                        streamSocket = new StreamSocket();

                        await streamSocket.ConnectAsync(SpirobanktoothService.ConnectionHostName, SpirobanktoothService.ConnectionServiceName);

                        var attributes = await SpirobanktoothService.GetSdpRawAttributesAsync();
                        var attributeReader = DataReader.FromBuffer(attributes[0x100]);
                        var attributeType = attributeReader.ReadByte();
                        if (attributeType != SdpServiceNameAttributeType)
                        {

                        }

                        var serviceNameLength = attributeReader.ReadByte();

                        // The Service Name attribute requires UTF-8 encoding.
                        attributeReader.UnicodeEncoding = UnicodeEncoding.Utf8;

                        cmdWriter = new DataWriter(streamSocket.OutputStream);

                        respReader = new DataReader(streamSocket.InputStream);


                        ReceivedataLoop(respReader);

                        res = "Connected.";
                        IsDeviceConnected = true;
                    }
                    else
                    {
                        res= "Device not found.";
                    }

                }
            }catch(Exception ex)
            {
                res= ex.Message;
            }

            return res;
        }

        public bool IsDeviceConnected { get; set; } = false;
        private  async void ReceivedataLoop(DataReader dataReader)
        {
            string strmsg = "";
            while (true)
            {
                if (stopDataReaderLoop)
                    break;

                try
                {
                    dataReader.InputStreamOptions = InputStreamOptions.Partial;


                    uint size = await dataReader.LoadAsync(32);

                    unreaddata = dataReader.UnconsumedBufferLength;
                   // await LogMessage("UnReadData:  " + unreaddata.ToString());

                    byte[] ReceiveData = new byte[unreaddata];

                    dataReader.ReadBytes(ReceiveData);
                    strmsg = "";

                    foreach (byte Data in ReceiveData)
                    {
                        strmsg += Data.ToString() + "-";
                      AddToLiveDataCollection(Data);
                    }
                //    await LogMessage(strmsg);
                 //   Datadl?.Invoke(strmsg);
                }
                catch (Exception ex)
                {
                    string s = ex.Message;
                }
            }
        }
    }
}
