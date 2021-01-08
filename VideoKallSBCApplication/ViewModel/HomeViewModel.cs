using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoKallMCCST.ViewModel;

namespace VideoKallSBCApplication.ViewModel
{
    public class HomeViewModel: NotifyPropertyChanged
    {
        private string _npt_IPAddress = null;
        public string NPT_IPAddress { get { return _npt_IPAddress; } set { _npt_IPAddress = value; OnPropertyChanged("NPT_IPAddress"); } }
    }
}
