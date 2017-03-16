using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace Dimond.LicenseClient
{
    public class ClientCore
    {
        private string _publicXmlKey = null;
        private string _currentKeyQueryXmlString = null;

        private string PublicXmlKey
        {
            get
            {
                if (_publicXmlKey == null)
                    _publicXmlKey = Encoding.UTF8.GetString(KeyResources._public);
                return _publicXmlKey;
            }
        }

        public string CurrentKeyQueryXmlString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_currentKeyQueryXmlString))
                    _currentKeyQueryXmlString = KeyQueryModel.GetCurrentKeyQueryAsXmlString();
                return _currentKeyQueryXmlString;
            }
        }

        #region Encrypt data

        private string EncryptStringToBase64Url(string openString)
        {
            var encrytedString = CryptUtils.Encrypt(PublicXmlKey, openString);
            var encryptedBytes = Encoding.UTF8.GetBytes(encrytedString);
            var base64String = Convert.ToBase64String(encryptedBytes);
            var base64StringUrl = HttpServerUtility.UrlTokenEncode(Convert.FromBase64String(base64String));
            return base64StringUrl;
        }

        #endregion

        public string GetKeyQueryBase64Url()
        {
            var sn = EncryptStringToBase64Url(CurrentKeyQueryXmlString);
            return sn;
        }

        #region Verify signature

        private bool VerifyData(byte[] signedData, byte[] signature)
        {
            // Create a new key pair on target CSP
            var cspParams = new CspParameters();
            cspParams.ProviderType = 1; // PROV_RSA_FULL 
            //cspParams.ProviderName; // CSP name
            // cspParams.Flags = CspProviderFlags.UseArchivableKey;
            cspParams.KeyNumber = (int)KeyNumber.Exchange;

            var rsaProvider = new RSACryptoServiceProvider(cspParams);
            // Import private/public key pair
            rsaProvider.FromXmlString(PublicXmlKey);
            var sha1 = SHA1Managed.Create();
            var hash = sha1.ComputeHash(signedData);
            var verifyResult = rsaProvider.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA1"), signature);
            return verifyResult;
        }

        private bool VerifyStringData(string stringData, string stringSigntaure)
        {
            var dataBytes = Encoding.UTF8.GetBytes(stringData);
            var signatureBytes = HttpServerUtility.UrlTokenDecode(stringSigntaure);
            var reslut = VerifyData(dataBytes, signatureBytes);
            return reslut;
        }

        #endregion
        
        public bool CheckLicenseKeyAsBase64Url(string key)
        {
             return VerifyStringData(CurrentKeyQueryXmlString, key);
        }
    }
}
