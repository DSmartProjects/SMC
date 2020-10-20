using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VideoKallSBCApplication.Stethosope;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace VideoKallSBCApplication.ViewModel
{
    class SthethoscopeTX : INotifyPropertyChanged
    {
        Stethoscope tx = new Stethoscope();
        public SthethoscopeTX()
        {
            tx.TXevents += Tx_TXevents;
        }

        private async  void Tx_TXevents(object sender, EventArgs e)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
             await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TxNotification = (string)sender;
                if(TxNotification.Contains("Streaming"))
                {
                    IsStreaming = false;
                    OnPropertyChanged("IsStreaming");
                }
                else
                {
                    IsStreaming = true;
                    OnPropertyChanged("IsStreaming");
                }
                OnPropertyChanged("TxNotification");
            });
           
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool IsStreaming { get; set; } = true;

        ICommand _StartTXCommand = null;
        public ICommand StartTXCommand
        {
            get
            {
                if (_StartTXCommand == null)
                    _StartTXCommand = new RelayCommand(ExecuteStartTxCommand);
                return _StartTXCommand;
            }
        }

 
        public string TxNotification{ get; set; }
        void ExecuteStartTxCommand()
        {
            
            tx.Initialize();
        }

        ICommand _RecordTXCommand = null;
        public ICommand RecordTXCommand
        {
            get
            {
                if (_RecordTXCommand == null)
                    _RecordTXCommand = new RelayCommand(ExecuteRecordTxCommand);
                return _RecordTXCommand;
            }
        }

        bool toggle = false;
        void ExecuteRecordTxCommand()
        {
            toggle = !toggle;
            if (toggle)
                tx.StartTxRecording();
            else
                tx.StopTxRecording();
        }
    }
}
