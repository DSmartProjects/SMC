using System;
using System.Threading.Tasks;

namespace VideoKallSBCApplication.Helpers
{
    public class Utility
    {
        string NPT_ConfigFile = "NPTConfig.txt";
        public async Task<bool> ReadNPTConfig()
        {
            try
            {
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile IpAddressFile = await localFolder.GetFileAsync(NPT_ConfigFile);
                var alltext = await Windows.Storage.FileIO.ReadLinesAsync(IpAddressFile);
                foreach (var line in alltext)
                {
                    string[] data = line.Split(':');
                    switch (data[0])
                    {
                        case "NPT-IP-Address":
                            MainPage.mainPage.mainpagecontext.NPT_IPAddress = data[1];
                            break;
                    }
                }
            }
            catch (Exception)
            {
                try
                {
                    var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                    Windows.Storage.StorageFile pinfofile = await localFolder.CreateFileAsync(NPT_ConfigFile);
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
