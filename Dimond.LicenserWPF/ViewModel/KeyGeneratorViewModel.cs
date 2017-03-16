using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Dimond.LicenseServer;
using Microsoft.Practices.Prism.Commands;

namespace Dimond.LicenserWPF.ViewModel
{
    public class KeyGeneratorViewModel : BaseViewModel
    {
        #region Binding properties

        private string _computerName;
        private string _processorId;
        private string _serialOs;
        private string _keyQuery;
        private string _productName;
        private string _key;

        public string ProductName
        {
            get { return _productName; }
            set
            {
                if (value == _productName) return;
                _productName = value;
                RaisePropertyChanged(() => ProductName);
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
                RaisePropertyChanged(() => GenerateKeyCommand);
            }
        }

        public string Key
        {
            get { return _key; }
            set
            {
                if (value == _key) return;
                _key = value;
                RaisePropertyChanged(() => Key);
            }
        }

        #endregion


        public ICommand GenerateKeyCommand
        {
            get { return new DelegateCommand(GenerateKeyExecuteCommand, () => !string.IsNullOrWhiteSpace(KeyQuery)); }
        }

        private void GenerateKeyExecuteCommand()
        {
            var serverCore = new ServerCore();
            var modelKeyQuery = serverCore.GetLicenseKeyQueryModelByKeyQuery(KeyQuery);
            ComputerName = modelKeyQuery.ComputerName;
            ProductName = modelKeyQuery.ProductName;
            ProcessorId = modelKeyQuery.ProcessorId;
            SerialOs = modelKeyQuery.SystemSerial;

            var key = serverCore.GenerateKeyByKeyQueryBase64Url(KeyQuery);
            Key = key;
        }
    }
}
