using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoKallSMC.Communication;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;

namespace VideoKallSBCApplication.Communication
{
   public class DataacquistionappComm
    {
        DatagramSocket selfhost = null;
        uint inboundBufferSize = 2048; 
        public string DataacquistionPortno { get; set; } =  "9854" ;
        public event EventHandler<CommunicationMsg> MessageReceived;

        public  void Initialize()
        {
            selfhost = new DatagramSocket();
            selfhost.MessageReceived += MessageFromDataacqAppReceived;
            // Refer to the DatagramSocketControl class' MSDN documentation for the full list of control options.
            selfhost.Control.InboundBufferSizeInBytes = inboundBufferSize;

            // Set the IP DF (Don't Fragment) flag.
            // Refer to the DatagramSocketControl class' MSDN documentation for the full list of control options.
            selfhost.Control.DontFragment = true;
        }

        private void MessageFromDataacqAppReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            try
            {
                uint stringLength = args.GetDataReader().UnconsumedBufferLength; 
                byte[] buffer = new byte[stringLength];
                args.GetDataReader().ReadBytes(buffer);
                string str = Encoding.Unicode.GetString(buffer);
                MessageReceived?.Invoke(this, new CommunicationMsg(str));
                //remove
                MainPage.TestresultModel.NotifyStatusMessage("received: "+str);
            }
            catch (Exception e)
            {
                string s = e.ToString();
                MainPage.TestresultModel.NotifyStatusMessage(e.Message);
            }
        }

        public async Task<int> Connect()
        {
            try
            { 
                HostName host = new HostName(MainPage.mainPage.commChannel.IPAddress);
                await selfhost.ConnectAsync(host, DataacquistionPortno);
                MainPage.TestresultModel.NotifyStatusMessage(MainPage.mainPage.commChannel.IPAddress+":"+ DataacquistionPortno);
                //  SendMessageToDataacquistionapp("test-sujit from smc");
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                MainPage.TestresultModel.NotifyStatusMessage("connection: "+ex.Message);
            }
            return 0;
        }

        DataWriter writer = null;
      public void  SendMessageToDataacquistionapp(string msg)
        {
            WriteToDataAcquisitionTool(msg);
            //if (selfhost != null)
            //{
            //    try
            //    {
            //        if(writer == null)
            //          writer    = new DataWriter(selfhost.OutputStream);
            //        string stringToSend = msg;
            //        writer.WriteString(stringToSend);

            //        writer?.StoreAsync();
            //        // writer.Dispose();
            //        MainPage.TestresultModel.NotifyStatusMessage(msg);
            //    }
            //    catch (Exception e)
            //    {
            //        string s = e.ToString();
            //        MainPage.TestresultModel.NotifyStatusMessage(e.Message);
            //    }

            //}
        }

      public async void    ReadCommFile()
        {
            try
            {
                StorageFolder storageFolder = KnownFolders.PicturesLibrary;
                StorageFile file = await storageFolder.GetFileAsync("tosmc.bin");
                string msg = await Windows.Storage.FileIO.ReadTextAsync(file);
                //(commFile, msg, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                string test = msg;
                
                if (!string.IsNullOrEmpty(msg))
                {
                    MessageReceived?.Invoke(this, new CommunicationMsg(msg));
                    MainPage.TestresultModel.NotifyStatusMessage("received: " + msg);
                    await storageFolder.CreateFileAsync("tosmc.bin", CreationCollisionOption.ReplaceExisting);
                }
               
                
                
            }
            catch(Exception ex)
            {
                MainPage.TestresultModel.NotifyStatusMessage("fileread: " + ex.Message);

            }

        }
        public async Task ResetCommFiles()
        {
            try
            {
                StorageFolder storageFolder = KnownFolders.PicturesLibrary;
                StorageFile commFile = await storageFolder.CreateFileAsync("todataacq.bin", CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteTextAsync(commFile, "", Windows.Storage.Streams.UnicodeEncoding.Utf8);

                commFile = await storageFolder.CreateFileAsync("tosmc.bin", CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteTextAsync(commFile, "", Windows.Storage.Streams.UnicodeEncoding.Utf8);
            }
            catch(Exception ex)
            {
                MainPage.TestresultModel.NotifyStatusMessage("ResetComm: " + ex.Message);
            }
        }
       public async void WriteToDataAcquisitionTool(string msg)
        {
            try
            {
                StorageFolder storageFolder = KnownFolders.PicturesLibrary;
                StorageFile commFile = await storageFolder.CreateFileAsync("todataacq.bin", CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteTextAsync(commFile, msg, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            }
            catch(Exception ex)
            {
                MainPage.TestresultModel.NotifyStatusMessage("file write: " + ex.Message);

            }

        }

    }
}
