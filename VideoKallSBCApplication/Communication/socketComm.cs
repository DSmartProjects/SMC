using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoKallSMC.Communication;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace VideoKallSBCApplication.Communication
{
    class socketComm
    {
        static int PortNumber = 8888;

        DatagramSocket selfhost = null;
        uint inboundBufferSize = 2048;
        public string DataacquistionPortno { get; set; } = "8888";
        public event EventHandler<CommunicationMsg> MessageReceived;
        public   void Initialize()
        {

            selfhost = new DatagramSocket();
            selfhost.MessageReceived += MessageFromDataacqAppReceived;
            // Refer to the DatagramSocketControl class' MSDN documentation for the full list of control options.
            selfhost.Control.InboundBufferSizeInBytes = inboundBufferSize;

            // Set the IP DF (Don't Fragment) flag.
            // Refer to the DatagramSocketControl class' MSDN documentation for the full list of control options.
            selfhost.Control.DontFragment = true;
        }

        public async void  Connect()
        {
            try
            {
                HostName host = new HostName(MainPage.mainPage.commChannel.IPAddress);
                await selfhost.ConnectAsync(host, DataacquistionPortno);
                MainPage.TestresultModel.NotifyStatusMessage(MainPage.mainPage.commChannel.IPAddress + ":" + DataacquistionPortno);
                SendMessageToDataacquistionapp("test-sujit from smc");

                //HostName hostName = new HostName("192.168.0.17");
                //await selfhost.BindEndpointAsync(hostName, DataacquistionPortno);
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                MainPage.TestresultModel.NotifyStatusMessage("connection: " + ex.Message);
            }
            
        }

        DataWriter writer = null;
        public void SendMessageToDataacquistionapp(string msg)
        {

            if (selfhost != null)
            {
                try
                {
                    if (writer == null)
                        writer = new DataWriter(selfhost.OutputStream);
                    string stringToSend = msg;
                    writer.WriteString(stringToSend);

                    writer?.StoreAsync();
                    // writer.Dispose();
                    MainPage.TestresultModel.NotifyStatusMessage(msg);
                }
                catch (Exception e)
                {
                    string s = e.ToString();
                    MainPage.TestresultModel.NotifyStatusMessage(e.Message);
                }

            }
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
                SendMessageToDataacquistionapp("received:" + str);
                //remove
                MainPage.TestresultModel.NotifyStatusMessage("received: " + str);

            }
            catch (Exception e)
            {
                string s = e.ToString();
                MainPage.TestresultModel.NotifyStatusMessage(e.Message);
            }
        }
 
    }
}
