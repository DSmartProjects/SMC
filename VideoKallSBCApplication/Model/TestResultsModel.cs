using SBCDBModule;
using SBCDBModule.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VideoKallSBCApplication.BLEDevices;
using VideoKallSBCApplication.Helpers;
using VideoKallSBCApplication.Model;
using VideoKallSBCApplication.Stethosope;
using VideoKallSBCApplication.TestResults;
using VideoKallSMC.Communication;
using Windows.Storage;

namespace VideoKallSBCApplication
{
    public enum DeviceTypesenums
    {
        UNKNOWN,
        BPMONITOR,
        OXIMETER,
        THERMOMETER,
        GLUCOMONITOR
    }
    public enum TempUnit
    {
        Fharenheit,
        Centigrade
    }

    public class GlucoMeterCommand
    {
        public static byte[] latestReadingPart1 = {0x51, 0x25, 0x0, 0x0, 0x0, 0x0, 0xA3, 0x19};
        public static byte[] latestReadingPart2 = {0x51, 0x26, 0x0, 0x0, 0x0, 0x0, 0xA3, 0x1A};
        public static byte[] DeleteAlldata = {0x51, 0x52, 0x0, 0x0, 0x0, 0x0, 0xA3, 0x46};
        public static byte[] NumberOfStoredRecord = { 0x51, 0x2B, 0x0, 0x0, 0x0, 0x0, 0xA3, 0x1F };
        public static byte[] ReadPart1byIndex(int recordindex)
        {
            int sumofbytes = 281+ recordindex;
            int checksum = sumofbytes % 256;
            string s = checksum.ToString("x");
            byte chsumhexvalue = Convert.ToByte( s,16);
            byte indexhexvalue = Convert.ToByte(recordindex.ToString("x"),16);
            byte[] latestReadingPart1 = { 0x51, 0x25, indexhexvalue, 0x0, 0x0, 0x0, 0xA3, chsumhexvalue };
            return latestReadingPart1;
        }

        public static byte[] ReadPart2byIndex(int recordindex)
        {
            int sumofbytes = 282 + recordindex;
            int checksum = sumofbytes % 256;
            byte chsumhexvalue = Convert.ToByte(checksum.ToString("x"),16);
            byte indexhexvalue = Convert.ToByte(recordindex.ToString("x"),16);
            byte[] latestReadingPart2 = { 0x51, 0x26, indexhexvalue, 0x0, 0x0, 0x0, 0xA3, chsumhexvalue };
            return latestReadingPart2;
        }

    }
    public class TestResultsModel
    {
        SBCDB dbmodule = new SBCDB();
        string[] _bleDeviceList = { "BP7000", "BLEsmart_", "AOJ-20A", "TNG ADVANCE", "FORA 6 CONNECT", "OxySmart" };
        public string bpcuffID = "BluetoothLE#BluetoothLE28:56:5a:9e:d2:fa-28:ff:b2:39:8d:29";
        public string OximeterID = "BluetoothLE#BluetoothLE28:56:5a:9e:d2:fa-f3:b1:06:f6:d0:cf";
        public Guid OximeterserviceID = new Guid("6e400001-b5a3-f393-e0a9-e50e24dcca9e");
        public string ThermoMeterId = "BluetoothLE#BluetoothLE28:56:5a:9e:d2:fa-a4:c1:38:48:35:c7";
        public string Glucometerid = "BluetoothLE#BluetoothLE28:56:5a:9e:d2:fa-c0:26:da:0a:e5:3e";

        public int ThermometerHandleid = 12;
        public int OximeterHandleid = 14;
        public string glucoMeterHandleid = "00001524-1212-EFDE-1523-785FEABCD123";
        public Guid BPServiceID = new Guid("00001810-0000-1000-8000-00805f9b34fb");
        //00001810-0000-1000-8000-00805F9B34FB
        public Guid ThermoServiceID = new Guid("0000ffe0-0000-1000-8000-00805f9b34fb");
        public string BPCharacteristicName = "BloodPressureMeasurement";
        public Guid GlucoMeterServiceID = new Guid("00001523-1212-EFDE-1523-785FEABCD123");
        string[] TempMode = { "Invalid", "Adult Forehead", "Child Forehead", "Ear", "Object/Room" };
        public delegate void BPResultCallback(BPResult res);
        public delegate void NotifyDeviceConnection(DeviceTypesenums parm, string parm2 = "", bool status = false);
        public BPResultCallback BpEvent = null;
        public NotifyDeviceConnection DeviceConnectionTimeCallback;
        public BPCuff bpcuff = new BPCuff();
        public ObservableCollection<BLEDeviceInfo> BLEDevices = new ObservableCollection<BLEDeviceInfo>();
        BPResult bpResult = new BPResult();
        /// <summary>
        public delegate void Notify(string msg, int errorOrStatus = 0);
        public Notify NotifyStatusMessage = null;
        /// </summary>
        public OxyMeter oxymeter = new OxyMeter();
        public delegate void OxyMeterDataEvent(PulseOxyResult res);
        public OxyMeterDataEvent oxymeterDataReceiveCallback;
        PulseOxyResult oxymeterResult = new PulseOxyResult();
        //
        List<DeviceType> devtypelist = null;
        //
        Thermometer _thermometer = new Thermometer();
        ThermometerResult thrmoResult = new ThermometerResult();
        public delegate void ThermoResultCallback(ThermometerResult res);
        public ThermoResultCallback ThermoResultcallback;
        //
        public GlucoMonitor glucoMonitor = new GlucoMonitor();
        public ObservableCollection<GlucoResult> GlucoResultscollection = new ObservableCollection<GlucoResult>();
        public delegate void GlucometerResultEvent(GlucoResult res);
        public GlucometerResultEvent GlucometerDataReceiveCallback;
        public Stethoscope StethoscopeTx = new Stethoscope();
        // 
        public TestResultsModel()
        {

            ThermoResult.unit = TempUnit.Fharenheit;
        }

        public string[] BLEDeviceList { get { return _bleDeviceList; } }
        public ThermometerResult ThermoResult { get { return thrmoResult; } }
        public Thermometer Thermo { get { return _thermometer; } }
        public TempUnit TempratureUnit { get { return thrmoResult.unit; } set { thrmoResult.unit = value; } }

        public PulseOxyResult OximeterResult { get { return oxymeterResult; } }
        public List<string> DeviceTypelist
        { 
            get
            {
              if(devtypelist == null)
                devtypelist = dbmodule.DeviceTypeList();
                List<string> dvl = new List<string>();
                if (devtypelist != null)
                {
                    foreach (var dv in devtypelist)
                    {
                        dvl.Add(dv.DeviceTypeName);
                    }
                }
                else
                {
                    Toast.ShowToast("",Constants.Need_DataBase_MSG);
                }
              
                return dvl;
            }
        }
         
        public DeviceType GetDeviceType(int id)
        {
            try
            {
                if (devtypelist == null)
                    devtypelist = dbmodule.DeviceTypeList();
                foreach (var v in devtypelist)
                {
                    if (v.ID == id)
                        return v;
                }
            }catch(Exception ex)
            {
                string s = ex.Message;
            }

            DeviceType dv = new DeviceType();
            switch (id)
            {
                case 1:
                    dv.DeviceName = "BP7000";
                    dv.CharacteristicAttributeid = "BloodPressureMeasurement";
                    dv.ServiceID =  "00001810 - 0000 - 1000 - 8000 - 00805f9b34fb" ;
                    break;
                case 2:
                    dv.DeviceName = "OxySmart";
                    dv.DeviceName = "14";
                    dv.ServiceID = "00001523-1212-EFDE-1523-785FEABCD123";
                    break;
                case 3:
                    dv.DeviceName = "AOJ-20A";
                    dv.CharacteristicAttributeid = "12";
                    dv.ServiceID = "0000ffe0-0000-1000-8000-00805f9b34fb";
                    break;
                case 4:
                    dv.DeviceName = "FORA 6 CONNECT";
                    dv.CharacteristicAttributeid = "00001524-1212-EFDE-1523-785FEABCD123";
                    dv.ServiceID = "00001523-1212-EFDE-1523-785FEABCD123";
                    break;
            }
            return dv;
        }

        public void BPCuff(BPResult res)
        {
            bpResult.DateTimeOfTest = res.DateTimeOfTest;
            bpResult.DIA = res.DIA;
            bpResult.SYS = res.SYS;
            bpResult.Pulse = res.Pulse;
            bpResult.PatientID = res.PatientID;
            // "BPRES>D:{0}>S:{1}>P:{2}>DT:{3}>T:{4}";
           string date =  string.Format("{0}/{1}/{2}", res.DateTimeOfTest.Month, res.DateTimeOfTest.Date, res.DateTimeOfTest.Year);
            string time = string.Format("{0}:{1}:{2}", res.DateTimeOfTest.Month, res.DateTimeOfTest.Date, res.DateTimeOfTest.Year);

           string result =  string.Format(CommunicationCommands.BPRESULT, res.DIA, res.SYS, res.Pulse, date, time);
            
            MainPage.mainPage.commChannel.SendMessageToMCC(result);
            BpEvent?.Invoke(res);
        }

        public string BpCuffConnectionTime { get { return bpResult.BpCuffConnectionTime; } }
        public bool IsBpConnected { get { return bpResult.IsBPCuffConnected; } }
        public void DeviceConnectedTime(DeviceTypesenums type, string datetime,bool status)
        { 
            switch(type)
            {
                case DeviceTypesenums.BPMONITOR:
                    {
                        DeviceConnectionTimeCallback?.Invoke(type, datetime, status);
                        bpResult.IsBPCuffConnected = status;
                        bpResult.BpCuffConnectionTime = datetime;
                    }
                    break;
                case DeviceTypesenums.OXIMETER:
                    {
                        DeviceConnectionTimeCallback?.Invoke(type, datetime, status);
                        oxymeterResult.IsConnected = status;
                        oxymeterResult.ConnectionTime = datetime;
                    }
                    break;
                case DeviceTypesenums.THERMOMETER:
                    {
                        DeviceConnectionTimeCallback?.Invoke(type, datetime, status);
                        thrmoResult.IsConnected = status;
                        thrmoResult.ConnectionTime = datetime;
                    }
                    break;
            } 
        }
       
        public void OxymeterData(string spo2,string pr)
        {
            oxymeterResult.DateTimeOfTest = DateTime.Now ;
            oxymeterResult.spo2 = spo2;
            oxymeterResult.PR = pr;
            FormatPulseOximeter(spo2,pr, string.Format("{0}:{1}:{2}", DateTime.Now.Hour.ToString().PadLeft(2, '0')
                , DateTime.Now.Minute.ToString().PadLeft(2, '0'),
                DateTime.Now.Second.ToString().PadLeft(2, '0')));
           oxymeterDataReceiveCallback?.Invoke(oxymeterResult);
        }

        void FormatPulseOximeter(string spo2, string pr, string datetime)
        {
            string res = string.Format(CommunicationCommands.PUSLEOXIMETERRESULT, spo2, pr, datetime);
            MainPage.mainPage.commChannel.SendMessageToMCC(res);
        }

        public void ThermometerData(decimal temp, int mode, bool status,string raw)
        {
            thrmoResult.DateTimeOfTest = DateTime.Now;
            thrmoResult.Temp = temp;
            thrmoResult.Mode = TempMode[mode];
            string time = string.Format("{0}:{1}:{2}", DateTime.Now.Hour.ToString().PadLeft(2, '0'), DateTime.Now.Minute.ToString().PadLeft(2, '0'),
                  DateTime.Now.Second.ToString().PadLeft(2, '0'));
            //thrmoResult.unit = TempUnit.Centigrade;
            string strth= string.Format(CommunicationCommands.THERMORESULT, temp, TempMode[mode], status, time );
            MainPage.mainPage.commChannel.SendMessageToMCC(strth);
            LogMessage(strth);

            thrmoResult.RawData = raw;
            thrmoResult.Status = status;
            ThermoResultcallback?.Invoke(thrmoResult);
            LogMessage(raw);
        }

        GlucoResult tmpResult = null;
        public void GlucometerResult(GlucoResult res,int datatype, bool updateResult)
        {
           switch(datatype)
            {
                case 0:
                    tmpResult = new GlucoResult();
                    tmpResult.Year = res.Year;
                    tmpResult.Month = res.Month;
                    tmpResult.Day = res.Day;
                    tmpResult.Hour = res.Hour;
                    tmpResult.Min = res.Min;
                  //  GlucometerDataReceiveCallback?.Invoke(tmpResult);
                    break;
                case 1:
                    tmpResult.TestResult = res.TestResult;
                    tmpResult.Unit = res.Unit;
                    tmpResult.TestType = res.TestType;
                    tmpResult.Mode = res.Mode;
                    //"GLUCMDRES>V:{0}>U:{1}>T:{2}>M:{3}>D:{4}>T:{5}";
                    string strres = string.Format(CommunicationCommands.GLUCORESULTRES, res.TestResult, res.Unit, res.TestType,
                        res.Mode, tmpResult.Month + "/" + tmpResult.Day + "/" + tmpResult.Year, tmpResult.Hour + ":" + tmpResult.Min);
                    MainPage.mainPage.commChannel.SendMessageToMCC(strres );
                    GlucometerDataReceiveCallback?.Invoke(tmpResult);
                    break;
                case 2:
                    tmpResult = new GlucoResult();
                    tmpResult.RecordCount = res.RecordCount;
                    GlucometerDataReceiveCallback?.Invoke(tmpResult);
                    break;
            }
           
        }

        public async void LogMessage(string msg)
        {
            try
            {
                // msg = DateTime.Now.ToString() + ":" + Environment.NewLine + msg + Environment.NewLine;
                msg = Environment.NewLine + msg + Environment.NewLine;
                string filename = "commLog.txt";
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile pinfofile = await localFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
              //  await Windows.Storage.FileIO.AppendTextAsync(pinfofile, msg, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                await Windows.Storage.FileIO.WriteTextAsync(pinfofile, msg, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            }
            catch (Exception)
            { }
        }
    }
}
