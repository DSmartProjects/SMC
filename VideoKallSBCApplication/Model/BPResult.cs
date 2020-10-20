using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoKallSBCApplication
{
   public class BPResult
    {
       public int PatientID { get; set; } 
       public DateTime DateTimeOfTest { get; set; }
       public string SYS { get; set; }//(mmHg)
       public string DIA { get; set; }
       public string Pulse { get; set; }
       public bool IsBPCuffConnected { get; set; }= false;
       public string BpCuffConnectionTime { get; set; }="";
      public int SessionID { get; set; }
    }
}
