using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Dimond.LicenseClient;
using Dimond.LicenseServer;
using Microsoft.Practices.Prism.Commands;

namespace Dimond.LicenserWPF.ViewModel
{
    public class KeyQueryViewModel : BaseViewModel
    {
        public KeyQueryViewModel()
        {
            ProductName = "qsign";
        }

        #region Binding properties

        private string _computerName;
        private string _processorId;
        private string _serialOs;
        private string _keyQuery;
        private string _productName;

        public string ProductName
        {
            get { return _productName; }
            set
            {
                if (value == _productName) return;
                _productName = value;
                RaisePropertyChanged(() => ProductName);
                RaisePropertyChanged(() => GenerateKeyQueryCommand);
            }
        }

        public string ComputerName
        {
            get { return _computerName; }
            set
            {
                if (value == _computerName) return;
                _computerName = value;
                RaisePropertyChanged(() => ComputerName);
                RaisePropertyChanged(() => GenerateKeyQueryCommand);
            }
        }

        public string ProcessorId
        {
            get { return _processorId; }
            set
            {
                if (value == _processorId) return;
                _processorId = value;
                RaisePropertyChanged(() => ProcessorId);
                RaisePropertyChanged(() => GenerateKeyQueryCommand);
            }
        }

        public string SerialOs
        {
            get { return _serialOs; }
            set
            {
                if (value == _serialOs) return;
                _serialOs = value;
                RaisePropertyChanged(() => SerialOs);
                RaisePropertyChanged(() => GenerateKeyQueryCommand);
            }
        }

        public string KeyQuery
        {
            get { return _keyQuery; }
            set
            {
                if (value == _keyQuery) return;
                _keyQuery = value;
                RaisePropertyChanged(() => KeyQuery);
                RaisePropertyChanged(() => GenerateKeyQueryCommand);
            }
        }

        #endregion

        #region Commands

        public ICommand GenerateKeyQueryCommand
        {
            get
            {
                return new DelegateCommand(GenerateKeyQueryExecuteCommand, () =>
                                                                           !string.IsNullOrWhiteSpace(ComputerName) &&
                                                                           !string.IsNullOrWhiteSpace(ProcessorId) &&
                                                                           !string.IsNullOrWhiteSpace(SerialOs) &&
                                                                           !string.IsNullOrWhiteSpace(ProductName));
            }
        }

        private void GenerateKeyQueryExecuteCommand()
        {
            var serverCore = new ServerCore();
            var sn = serverCore.GetKeyQueryFromModel(new KeyQueryModel
                {
                    ComputerName = ComputerName,
                    ProcessorId = ProcessorId,
                    ProductName = ProductName,
                    SystemSerial = SerialOs,
                });

            KeyQuery = sn;
        }

        #endregion

    }
}
