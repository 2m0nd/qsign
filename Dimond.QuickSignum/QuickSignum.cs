using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Timers;

namespace Dimond.QuickSignum
{
    public class QuickSignum : IQuickSignum
    {
        private readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private readonly Func<string, string> _licenseRequesAction;
        private readonly Action<Exception> _errorWhenChekcLicense;
        private readonly Action _sucessfulActivating;
        private Timer _timer;

        public QuickSignum(Func<string, string> licenseRequesAction, Action<Exception> errorWhenCheckLicense, Action sucessfulActivating)
        {
            Log.Info("Start initialize QuickSignum");
            _licenseRequesAction = licenseRequesAction;
            _errorWhenChekcLicense = errorWhenCheckLicense;
            _sucessfulActivating = sucessfulActivating;

            //_timer = new Timer(5000);
            //_timer.Elapsed += TimerOnElapsed;
            //_timer.Start();
            Log.Info("End initialize QuickSignum");
        }

        private readonly object locker = new object();

        //private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        //{
        //    TimerCheckLicense();
        //}

        //void TimerCheckLicense()
        //{
        //    lock (locker)
        //    {
        //        try
        //        {
        //            _timer.Stop();
        //            Log.Info("Start TimerCheckLicense TimerCheckLicense");
        //            Licensing.CheckLicensing(_licenseRequesAction, _sucessfulActivating);
        //            Log.Info("End TimerCheckLicense TimerCheckLicense");
        //        }
        //        catch (Exception exc)
        //        {
        //            Log.Error(exc);
        //            _errorWhenChekcLicense(exc);
        //            _timer.Start();
        //        }
        //    }
        //}

        //bool CheckLicense()
        //{
        //    lock (locker)
        //    {
        //        try
        //        {
        //            _timer.Stop();
        //            Log.Info("Start CheckLicense TimerCheckLicense");
        //            Licensing.CheckLicensing(_licenseRequesAction, _sucessfulActivating);
        //            Log.Info("End CheckLicense TimerCheckLicense");
        //            return true;
        //        }
        //        catch (Exception exc)
        //        {
        //            Log.Error(exc);
        //            _errorWhenChekcLicense(exc);
        //            _timer.Start();
        //            return false;
        //        }
        //    }
        //}

        public static String SignExtension { get { return ".sig"; } }
        private X509Certificate2 _currentCert;
        //private static List<String> _nameCertList = new List<string>();
        //private static List<String> _serialCertList = new List<string>();
        private static Dictionary<string, string> _certifacateDictionary;

        ///// <summary>
        /////
        ///// </summary>
        ///// <param name="certIdentity"> Наименование сертификата </param>
        /////<param name="byName"> </param>
        //public QuickSignum()
        //{
        //    if(byName)
        //        _currentCert = this.GetCurrentUserMyCertByName(certIdentity);
        //    else
        //        _currentCert = this.GetCurrentUserMyCertBySerial(certIdentity);
        //}

        public void Dispose()
        {
           // _timer.Dispose();
        }

        public X509Certificate2 CurrentCert
        {
            get
            {
                return _currentCert;
            }
        }

        public X509Certificate2Collection GetCurrentUserMyCerts()
        {
            //хранилище личных сертификатов текущего пользователя
            var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            certStore.Open(OpenFlags.ReadOnly);
            var certs = certStore.Certificates;
            certStore.Close();

            return certs;
        }

        public X509Certificate2 GetCurrentUserMyCertByName(string certName)
        {
            var certs = GetCurrentUserMyCerts();
            var findedCerts = certs.Find(X509FindType.FindBySubjectName, certName, false);
            if (findedCerts.Count == 0)
                throw new Exception(String.Format(
                    "Сертификат с именем '{0}' не обнаружен в хранилище личных сертификатов", certName));
            if (findedCerts.Count > 1)
                throw new Exception(String.Format(
                    "Обнаружено несколько сертификатов с именем '{0}'", certName));

            return findedCerts[0];
        }

        public X509Certificate2 GetCurrentUserMyCertBySerial(string certSerial)
        {
            var certs = GetCurrentUserMyCerts();
            var findedCerts = certs.Find(X509FindType.FindBySerialNumber, certSerial, false);
            if (findedCerts.Count == 0)
                throw new Exception(String.Format(
                    "Сертификат с именем '{0}' не обнаружен в хранилище личных сертификатов", certSerial));
            if (findedCerts.Count > 1)
                throw new Exception(String.Format(
                    "Обнаружено несколько сертификатов с именем '{0}'", certSerial));

            return findedCerts[0];
        }

        /// <summary>
        /// Метод установки сертификата
        /// </summary>
        /// <param name="certName"></param>
        public void SetCurrentCertByName(string certName)
        {
            _currentCert = this.GetCurrentUserMyCertByName(certName);
        }

        /// <summary>
        /// Метод установки сертификата
        /// </summary>
        /// <param name="certName"></param>
        public void SetCurrentCertBySerial(string serial)
        {
            _currentCert = this.GetCurrentUserMyCertBySerial(serial);
        }

        public byte[] Sign(byte[] docByteArray)
        {
            Log.Info("Start Sign[]");
            //if (!CheckLicense())
            //    throw new Exception("Нет лицензии на использование данного метода.");

            var contentInfo = new ContentInfo(docByteArray);
            var signedCms = new SignedCms(contentInfo, true);
            var cmsSigner = new CmsSigner(CurrentCert);
            signedCms.ComputeSignature(cmsSigner, false);
            Log.Info("End Sign[]");
            return signedCms.Encode();
        }

        public bool Verify(byte[] docByteArray, byte[] signature)
        {
            Log.Info("Start Verify[]");
            //if (!CheckLicense())
            //    return false;

            var contentInfo = new ContentInfo(docByteArray);
            var signedCms = new SignedCms(contentInfo, true);
            signedCms.Decode(signature);

            try
            {
                signedCms.CheckSignature(true);
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                return false;
            }
            Log.Info("End Verify[]");
            return true;
        }

        public string Sign(string pathSignedFile)
        {
            var bytesFile = GetByteArrayFromFile(pathSignedFile);
            var bytesSign = Sign(bytesFile);

            var pathSign = pathSignedFile + QuickSignum.SignExtension;
            WriteByteArray(pathSign, bytesSign);
            return pathSign;
        }

        public bool Verify(string pathSignedFile, string pathSignature)
        {
            var bytesFile = GetByteArrayFromFile(pathSignedFile);
            var bytesSign = GetByteArrayFromFile(pathSignature);

            return Verify(bytesFile, bytesSign);
        }

        public byte[] GetByteArrayFromFile(string fullFilePath)
        {
            byte[] bytes;
            using (var fileStream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read))
            {
                bytes = new byte[fileStream.Length];
                fileStream.Position = 0;
                fileStream.Read(bytes, 0, (int)fileStream.Length);
                fileStream.Close();
            }
            return bytes;
        }

        public void WriteByteArray(string fullOutFilePath, byte[] bytes)
        {
            using (var writeStream = new FileStream(fullOutFilePath, FileMode.Create, FileAccess.Write))
            {
                writeStream.Write(bytes, 0, bytes.Count());
                writeStream.Close();
            }
        }

        Dictionary<string, string> IQuickSignum.CertificateCollection
        {
            get { return CertificateCollection; }
        }

        public void ViewCertificateInformation(IntPtr parrent)
        {
            if (CurrentCert != null)
                X509Certificate2UI.DisplayCertificate(CurrentCert, parrent);
        }


        static void UpdateCertDictionary()
        {
            _certifacateDictionary = new Dictionary<string, string>();
            //хранилище личных сертификатов текущего пользователя
            var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            certStore.Open(OpenFlags.ReadOnly);
            var certs = certStore.Certificates;
            certStore.Close();

            foreach (var cert in certs)
            {
                if (cert.SubjectName.Name != null && cert.SerialNumber != null)
                {
                    var nameParts = cert.SubjectName.Name.Split(',');
                    var nameCN = nameParts.FirstOrDefault(name => name.Contains("CN"));
                    if (nameCN != null)
                    {
                        var startIndex = nameCN.IndexOf("=", StringComparison.Ordinal) + 1;
                        var certFrendlyName = nameCN.Substring(startIndex);
                        _certifacateDictionary.Add(cert.SerialNumber, String.Format("{0} | {1}", certFrendlyName, cert.SerialNumber));
                    }
                }
            }
        }


        static Dictionary<string, string> CertificateCollection
        {
            get
            {
                if (_certifacateDictionary == null)
                    UpdateCertDictionary();
                return _certifacateDictionary;
            }
        }

    }
}
