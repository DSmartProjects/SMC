using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VideoKallSBCApplication.Stethosope
{
  public  class Stethoscope
    {
        //TX
        [UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall, SetLastError = true)]
        public delegate void CallbackDelegate(string x);

        [DllImport("ssoipTXLib.dll", CallingConvention = CallingConvention.Cdecl)]
        extern static void ReadTXConfigurationFile();

        [DllImport("ssoipTXLib.dll", CallingConvention = CallingConvention.Cdecl)]
        extern static void GenerateRecordingDeviceFile();

        [DllImport("ssoipTXLib.dll", CallingConvention = CallingConvention.Cdecl)]
        extern static void StartStreaming(int stethoscopeIndx, bool lungs);

        [DllImport("ssoipTXLib.dll", CallingConvention = CallingConvention.Cdecl)]
        extern static void StopStreaming();

        [DllImport("ssoipTXLib.dll", CallingConvention = CallingConvention.Cdecl)]
        extern static void RegisterCallback(IntPtr callbackfunc);

        [DllImport("ssoipTXLib.dll", CallingConvention = CallingConvention.Cdecl)]
        extern static void SetTXLogFolder(String filewithfolder);

        [DllImport("ssoipTXLib.dll", CallingConvention = CallingConvention.Cdecl)]
        extern static void StartRecording();
        [DllImport("ssoipTXLib.dll", CallingConvention = CallingConvention.Cdecl)]
        extern static void StopRecording();

        string RXLogfile = "RXlog.txt";
        public event EventHandler<EventArgs> TXevents;
        IntPtr handle;
        CallbackDelegate NotificationHandle;

        public void Initialize()
        {
            try
            {
                NotificationHandle = new CallbackDelegate(CallBackTX);
                handle = Marshal.GetFunctionPointerForDelegate(NotificationHandle);
                RegisterCallback(handle);
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                RXLogfile = localFolder.Path;
                SetTXLogFolder(RXLogfile);

                //GenerateRecordingDeviceFile();
                ReadTXConfigurationFile();

                StartStreaming(0, false);
            }catch(Exception ex)
            {
                MainPage.mainPage.LogExceptions(ex.Message);
            }
        }

        public void StopTx() {
            StopStreaming();

        }
        public void StartTxRecording()
        {
            try
            {
                StartRecording();
            }
            catch(Exception ex)
            {
                MainPage.mainPage.LogExceptions(ex.Message);
            }
        }

        public void StopTxRecording()
        {
            try
            {
                StopRecording();
            }
            catch(Exception ex)
            {
                MainPage.mainPage.LogExceptions(ex.Message);
            }
          
        }

        void CallBackTX(string message)
        {
            TXevents?.Invoke(message, null);
        }
    }
}
