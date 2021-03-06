﻿using VideoKallMCCST.ViewModel;
using VideoKallSBCApplication.Helpers;
using Windows.UI.Xaml;

namespace VideoKallSBCApplication.ViewModel
{
    public class TestPanelViewModel: NotifyPropertyChanged
    {
        #region properties
        private Visibility _isMsgConnected = Visibility.Collapsed;
        public Visibility IsMsgConnected { get { return _isMsgConnected; } set { _isMsgConnected = value; OnPropertyChanged("IsMsgConnected"); } }
        
        private bool _isConnected_THRM = false;
        public bool IsConnected_THRM { get { return _isConnected_THRM; } set { _isConnected_THRM = value; OnPropertyChanged("IsConnected_THRM"); } }

        private bool _isConnected_Oxy = false;
        public bool IsConnected_Oxy { get { return _isConnected_Oxy; } set { _isConnected_Oxy = value; OnPropertyChanged("IsConnected_Oxy"); } }

        private string _instruction_Note = Constants.RE_CONNECT;
        public string Instruction_Note { get { return _instruction_Note; } set { _instruction_Note = value; OnPropertyChanged("Instruction_Note"); } }

        private bool _isFromSMC_THRM = false;
        public bool IsFromSMC_THRM { get { return _isFromSMC_THRM; } set { _isFromSMC_THRM = value; OnPropertyChanged("IsFromSMC_THRM"); } }

        private bool _isFromSMC_Oxy = false;
        public bool IsFromSMC_Oxy { get { return _isFromSMC_Oxy; } set { _isFromSMC_Oxy = value; OnPropertyChanged("IsFromSMC_Oxy"); } }


        #endregion

        public delegate void InstuctionNoteCallBack(DeviceTypesenums parm, string parm2 = "");      
        public InstuctionNoteCallBack InstuctionNoteCallBackCompleted;
    }
}
