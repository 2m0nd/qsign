using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dimond.LicenseClient;

namespace Dimond.QuickSignum
{
    internal class Licensing
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private const string LicenseFileName = "quicksignum.license";
        private const string LicenseService = "http://www.2m0nd.ru/market/GetLicenseKey";
        private static readonly ClientCore ClientCoreLicensing = new ClientCore();
        private static string _serialNumber;
        private static string _machineId;

        public static string SerialNumber
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_serialNumber))
                {
                    var serialNumber = ClientCoreLicensing.GetKeyQueryBase64Url();
                    _serialNumber = serialNumber;
                }
                return _serialNumber;
            }
        }
        static object locker = new object();
        public static void CheckLicensing(Func<string, string> licenseRequesAction, Action sucessfulActivating)
        {
            Log.Info("START CHECK LICENSE CheckLicensing");
            if (licenseRequesAction == null)
            {
                Log.Error("In CheckLicensing licenseRequesAction == null");
                throw new Exception("Должно передаваться действие для получения лицензии.");
            }
            var exeLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var liscenseFileFullPath = Path.Combine(Path.GetDirectoryName(exeLocation),
                                                    LicenseFileName);
            Log.Info(string.Format("Check file {0} exist", liscenseFileFullPath));
            if (File.Exists(liscenseFileFullPath))
            {
                try
                {
                    Log.Info(string.Format("Read license file"));
                    var key = File.ReadAllText(liscenseFileFullPath);
                    //MessageBox.Show(key);
                    Log.Info("Check license license file");
                    var resultLicenseVirefy = ClientCoreLicensing.CheckLicenseKeyAsBase64Url(key);
                    Log.Info("Check license license file result = " + resultLicenseVirefy);
                    if (resultLicenseVirefy)
                    {
                        return;
                    }
                }
                catch (Exception exception)
                {
                    Log.Error(exception);
                    throw;
                }
            }
            else
            {
                Log.Info(string.Format("File {0} not exist", liscenseFileFullPath));
            }
            Log.Info("Start request license");
            var recivedKey = licenseRequesAction(SerialNumber);
            if (!string.IsNullOrWhiteSpace(recivedKey))
            {
                bool resultVirefyRecivedKey;
                try
                {
                    Log.Info("Check license license key");
                    resultVirefyRecivedKey = ClientCoreLicensing.CheckLicenseKeyAsBase64Url(recivedKey);
                    Log.Info("Check license key result = " + resultVirefyRecivedKey);
                }
                catch (Exception exc)
                {
                    Log.Error(exc);
                    throw new Exception("Введен не валидный ключ лицензии.");
                }
                if (resultVirefyRecivedKey)
                {

                    try
                    {
                        Log.Info("Start wrinting license key");
                        File.WriteAllText(liscenseFileFullPath, recivedKey);
                        Log.Info("End wrinting license key");
                    }
                    catch (Exception)
                    {
                        throw new Exception("Ошибка активации, требуется активация программы с правами администратора.");
                    }

                    sucessfulActivating();
                    return;
                }

            }
            else
            {
                Log.Error("Введите лицензионнный ключ.");
                throw new Exception("Введите лицензионнный ключ.");
            }
            Log.Error("Введен не валидный ключ лицензии.");
            throw new Exception("Введен не валидный ключ лицензии.");

        }
    }
}
