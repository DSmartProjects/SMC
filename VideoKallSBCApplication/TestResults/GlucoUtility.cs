using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoKallSBCApplication.TestResults
{
  public  class GlucoUtility
    {
        int commandType = 0;
        public GlucoUtility()
        {
           MainPage.TestresultModel.glucoMonitor.Execute += Execute;
        }

        public void DeleteAll()
        {
            commandType = 2;
            MainPage.TestresultModel.glucoMonitor.Connect();
        }
        public void LatestTestResult()
        {
            try
            {
                commandType = 0;
                MainPage.TestresultModel.glucoMonitor.Connect();
            }
            catch (Exception ex)
            {
                MainPage.TestresultModel.NotifyStatusMessage("Exception: " + ex.Message);
            }
        }
        public void Execute()
        {
            try
            {
                switch (commandType)
                {
                    case 0:
                        MainPage.TestresultModel.glucoMonitor.LastestTestResultTime();
                        break;
                    case 2:
                        MainPage.TestresultModel.glucoMonitor.DeleteAll();
                        break;
                    case 3:
                        MainPage.TestresultModel.glucoMonitor.RecordCount();
                        break;
                    case 4:
                     //   MainPage.TestresultModel.glucoMonitor.RecordByIndex(Convert.ToInt32(TxtTestDataByIndex.Text));
                     //   TxtTestDataByIndex.IsEnabled = true;
                        break;
                }
                commandType = 0;
            }
            catch (Exception ex)
            {
                MainPage.TestresultModel.NotifyStatusMessage("Exception: " + ex.Message);
            }
        }
        public void NumberOfRecords()
        {
            commandType = 3;
            MainPage.TestresultModel.glucoMonitor.Connect();
        }
        public void ResultbyIndex()
        {
           
                commandType = 4;
            //    TxtTestDataByIndex.IsEnabled = false;
                MainPage.TestresultModel.glucoMonitor.Connect(); 
        }
            
    }
}
