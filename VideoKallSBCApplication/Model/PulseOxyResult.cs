using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoKallSBCApplication.Model
{
 public   class PulseOxyResult
    {
      public int PatientID { get; set; }
      public DateTime DateTimeOfTest { get; set; }
      public string spo2 { get; set; }//(%)
      public string PR  { get; set; }
      public string RawData { get; set; }
      public bool IsConnected { get; set; } = false;
      public string ConnectionTime { get; set; } = "";
      public int SessionID { get; set; }
    }

    public class ThermometerResult
    {
        public int PatientID { get; set; }
        public DateTime DateTimeOfTest { get; set; }
        public string Mode { get; set; } 
        public decimal Temp { get; set; }
        public TempUnit unit { get; set; } 
        public string RawData { get; set; }
        public bool IsConnected { get; set; } = false;
        public string ConnectionTime { get; set; } = "";
        public bool Status { get; set; }
        public int SessionID { get; set; }
    }

    public class GlucoResult
    {
        public string TestDay { get { return string.Format("{0}/{1}/{2}",  Month.PadLeft(2,'0'), Day.PadLeft(2,'0'),Year); } }
        public string TestTime { get { return string.Format("{0}:{1}  {2}",int.Parse(Hour)>12? ((int.Parse(Hour)-12).ToString()).PadLeft(2,'0') : Hour.PadLeft(2,'0'), Min.PadLeft(2,'0'), int.Parse(Hour)>12?"PM":"AM"); } }
       public string Year { get; set; }
        public String Month { get; set; }// = DateTime.Now.Month.ToString();
        public String Day { get; set; } //= DateTime.Now.Day.ToString();
        public String Hour { get; set; } //= DateTime.Now.Hour.ToString();
        public string Min { get; set; }// = DateTime.Now.Minute.ToString();
        public string TestType { get; set; } //= "GLU";
        public string Mode { get; set; }// = "AC";
        public string TestValue { get { return string.Format("{0} {1}", TestResult, Unit); } }
        public string TestResult { get; set; }// = "150";
        public string Unit { get; set; } //= "mg/dl";
        public string NormalRange { get; set; }
        public int SessionID { get; set; }
        public int PatientID { get; set; }
        public int RecordCount { get; set; }
    }
}
