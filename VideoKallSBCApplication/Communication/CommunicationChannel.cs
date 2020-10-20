using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace VideoKallSMC.Communication
{
   public class CommunicationChannel
    {
        DatagramSocket _socketUdp = new DatagramSocket();
        public event EventHandler<CommunicationMsg> MessageReceived;
        public event EventHandler<CommunicationMsg> ErrorMessage;
       private ConnectionInfo MCCConnection { get; set; } = null;
    
        public void Initialize()
        {
            _socketUdp.MessageReceived += _socketUdp_MessageReceived;
            IPAddress = LocalIPAddress();
            PortNo = "9856";
           
        }

        private async void _socketUdp_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            try
            {
                uint stringLength = args.GetDataReader().UnconsumedBufferLength;
                string receivedMessage = args.GetDataReader().ReadString(stringLength);

               if (receivedMessage.ToLower().Equals(CommunicationCommands.MCCConnection.ToLower()))
                {
                    IOutputStream outputStream = await _socketUdp.GetOutputStreamAsync(
                    args.RemoteAddress,
                    args.RemotePort);
                    MCCConnection = new ConnectionInfo(args.RemoteAddress.ToString(), args.RemotePort, outputStream);
                }
                
                MessageReceived?.Invoke(this, new CommunicationMsg(receivedMessage));
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }

        public async void Listen()
        {
            if (_socketUdp == null)
                return;
            try
            {
                HostName hostName = new HostName(IPAddress);
                await _socketUdp.BindEndpointAsync(hostName, PortNo);
            }catch(Exception ex)
            {
                ErrorMessage?.Invoke(this, new CommunicationMsg(ex.Message));
            }
        }


        public async void SendMessageToMCC(string msg)
        {
            try
            {
                if (MCCConnection != null)
                {
                    await MCCConnection.WriteData(msg);
                }
            }catch(Exception ex)
            {
                ErrorMessage?.Invoke(this, new CommunicationMsg(ex.Message));
            }
        }

       

        public void Cleanup()
        {
            if(_socketUdp != null)
            {
                _socketUdp.Dispose();
                _socketUdp = null;
            }
        }

        public string LocalIPAddress()
        {
            try
            {
                var icp = NetworkInformation.GetInternetConnectionProfile();

                if (icp?.NetworkAdapter == null) return "";
                var hostname =
                    NetworkInformation.GetHostNames()
                        .SingleOrDefault(
                            hn =>
                                hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                                == icp.NetworkAdapter.NetworkAdapterId);

                return hostname?.CanonicalName;
            }
            catch (Exception EX)
            {

            }
            return null;
        }
        public string IPAddress { get; set; }
        public string PortNo { get; set; }
    }

    class ConnectionInfo
    {
        string ipaddress = string.Empty;
        string portNo = string.Empty;
        IOutputStream outputstream = null;
        DataWriter writer = null;
        public ConnectionInfo(string ip, string port, IOutputStream io)
        {
            outputstream = io;
            ipaddress = ip;
            portNo = port;
        }

        public async Task WriteData(string msg)
        {
            try
            {
                if (writer == null)
                    writer = new DataWriter(outputstream);
                writer.WriteString(msg);
                await writer?.StoreAsync();
            }
            catch (Exception)
            {

            }

        }
    }

   public class CommunicationMsg: EventArgs
    {
        public string Msg { get; set; }
        public CommunicationMsg(string msg)
        {
            Msg = msg;
        }
    }
}
