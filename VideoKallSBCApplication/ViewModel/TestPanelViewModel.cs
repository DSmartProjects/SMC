using VideoKallMCCST.ViewModel;
using Windows.UI.Xaml;

namespace VideoKallSBCApplication.ViewModel
{
    public class TestPanelViewModel: NotifyPropertyChanged
    {
        #region properties
        private Visibility _isMsgConnected = Visibility.Collapsed;
        public Visibility IsMsgConnected { get { return _isMsgConnected; } set { _isMsgConnected = value; OnPropertyChanged("IsMsgConnected"); } }

        private bool _isConnectedEnable = false;
        public bool IsConnectedEnable { get { return _isConnectedEnable; } set { _isConnectedEnable = value; OnPropertyChanged("IsConnectedEnable"); } }

        #endregion
    }
}
