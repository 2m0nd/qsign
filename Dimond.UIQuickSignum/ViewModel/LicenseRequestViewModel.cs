
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace UIQuickSignum.ViewModel
{
    public class LicenseRequestViewModel : ViewModelBase
    {
        private readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public Window Owner { get; set; }
        public LicenseRequestViewModel(Window owner, string serialNumber)
        {
            Owner = owner;
            SerialNumber = serialNumber;
        }
        private string _licenseKey;
        private string _serialNumber;
        public string SerialNumber
        {
            get { return _serialNumber; }
            set
            {
                if (value == _serialNumber) return;
                _serialNumber = value;
                RaisePropertyChanged(() => SerialNumber);
            }
        }

        public string LicenseKey
        {
            get { return _licenseKey; }
            set
            {
                if (value == _licenseKey) return;
                _licenseKey = value;
                RaisePropertyChanged(() => LicenseKey);
            }
        }

        public ICommand CheckLicenseCommand
        {
            get { return new RelayCommand(CheckLicenseExecuteCommand); }
        }

        private void CheckLicenseExecuteCommand()
        {
            Owner.DialogResult = true;
        }

        public ICommand BuyCommand
        {
            get { return new RelayCommand(BuyExecuteCommand); }
        }

        private void BuyExecuteCommand()
        {
            Log.Info("Start BuyExecuteCommand");
            var url = string.Format("{0}{1}", Properties.Settings.Default.MarketUrl, SerialNumber);

            try
            {
                System.Diagnostics.Process.Start("chrome", url);
            }
            catch (Exception excC)
            {
                try
                {
                    System.Diagnostics.Process.Start("opera", url);
                }
                catch (Exception excO)
                {
                    try
                    {
                        System.Diagnostics.Process.Start("firefox", url);
                    }
                    catch (Exception excF)
                    {
                        try
                        {
                            System.Diagnostics.Process.Start("iexplore", url);
                        }
                        catch (Exception excIE)
                        {
                            MessageBox.Show(
                                "Скопируйте содержимое запроса ключа активации и запросите ключ на сайте магазина программного обеспечения.");
                        }
                    }
                }
            }

            Log.Info("End BuyExecuteCommand");
        }
    }
}
