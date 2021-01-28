using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VideoKallSBCApplication.Communication
{
   public class CommwithChartApp
    {
        UdpClient udpClient;
        IPAddress Ipaddress = null;
        public int Portnumber { get; set; } = 9889;
        IPEndPoint ep;
        public void ConnectwithChartApp()
        {
            if (MainPage.mainPage.commChannel.MCCConnection != null)
            {
                if (udpClient == null)
                    udpClient = new UdpClient();
                try
                {
                    Ipaddress = IPAddress.Parse( MainPage.mainPage.commChannel.MCCConnection.ipaddress);
                    ep = new IPEndPoint(Ipaddress, Portnumber);
                    udpClient.Connect(ep); 

                }
                catch (Exception ex)
                {
                    string str = "test" + ex.Message;
                }
            }
        }


        public void SendMsg(string msg)
        {
            if (udpClient != null)
            {
                byte[] buffer = Encoding.Unicode.GetBytes(msg);
                udpClient.Send(buffer, buffer.Length);
            }
        }
    }
}
