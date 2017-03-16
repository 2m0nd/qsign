using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Dimond.LicenseClient;

namespace Dimond.LicenseServer
{
    public class ServerCore
    {
        private string _publicXmlKey = null;
        private string _privateXmlKey = null;
        
        private string PublicXmlKey
        {
            get
            {
                if (_publicXmlKey == null)
                    _publicXmlKey = Encoding.UTF8.GetString(KeyResources._private);
                return _publicXmlKey;
            }
        }
        private string PrivateXmlKey
        {
            get
            {
                if (_privateXmlKey == null)
                    _privateXmlKey = Encoding.UTF8.GetString(KeyResources._private);
                return _privateXmlKey;
            }
        }


        public string KeyQueryXmlStringFromModel(KeyQueryModel keyQueryModelModel)
        {
            var KeyQueryXmlString = KeyQueryModel.GetKeyQueryAsXmlString(keyQueryModelModel);
            return KeyQueryXmlString;
        }

        public string GetKeyQueryFromModel(KeyQueryModel keyQueryModelModel)
        {
            var xmlstr = KeyQueryXmlStringFromModel(keyQueryModelModel);
            var sn = EncryptStringToBase64Url(xmlstr);
            return sn;
        }

        private string EncryptStringToBase64Url(string openString)
        {
            var encrytedString = CryptUtils.Encrypt(PublicXmlKey, openString);
            var encryptedBytes = Encoding.UTF8.GetBytes(encrytedString);
            var base64String = Convert.ToBase64String(encryptedBytes);
            var base64StringUrl = HttpServerUtility.UrlTokenEncode(Convert.FromBase64String(base64String));
            return base64StringUrl;
        }

        #region Decrypt on Server

        // Decrypt a file
        private string DecryptString(string utf8String)
        {
            var openString = CryptUtils.Decrypt(PrivateXmlKey, utf8String);
            return openString;
        }

        #endregion
            
        #region Sign and verify

        private byte[] SignData(byte[] data)
        {
            // Create a new key pair on target CSP
            var cspParams = new CspParameters();
            cspParams.ProviderType = 1; // PROV_RSA_FULL 
            //cspParams.ProviderName; // CSP name
            // cspParams.Flags = CspProviderFlags.UseArchivableKey;
            cspParams.KeyNumber = (int)KeyNumber.Exchange;

            var rsaProvider = new RSACryptoServiceProvider(cspParams);
            // Import public key
            rsaProvider.FromXmlString(PrivateXmlKey);

            var sha1 = SHA1Managed.Create();
            var hash = sha1.ComputeHash(data);
            // Encrypt plain stringData
            rsaProvider.PersistKeyInCsp = true;
            var sign = rsaProvider.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));
            var verifyResult = rsaProvider.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA1"), sign);
            return sign;
        }

        private string SignStringData(string stringData)
        {
            var data = Encoding.UTF8.GetBytes(stringData);
            var signatureBytes = SignData(data);
            return HttpServerUtility.UrlTokenEncode(signatureBytes);
        }

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

        #endregion

        public string GetKeyQueryAsXmlString(string keyQuery)
        {
            var base64KeyQuery = HttpServerUtility.UrlTokenDecode(keyQuery);
            var utf8KeyQuery = Encoding.UTF8.GetString(base64KeyQuery);
            var utf8KeyQueryAsXml = DecryptString(utf8KeyQuery);
            return utf8KeyQueryAsXml;
        }

        #region Public contract

        public KeyQueryModel GetLicenseKeyQueryModelByKeyQuery(string KeyQuery)
        {
            var snAsXml = GetKeyQueryAsXmlString(KeyQuery);
            var snAsObject = KeyQueryModel.GetKeyQueryFromXmlString(snAsXml);
            return snAsObject;
        }



        public string GenerateKeyByKeyQueryBase64Url(string KeyQueryBase64Url)
        {
            var KeyQuerybytes = HttpServerUtility.UrlTokenDecode(KeyQueryBase64Url);
            var utf8KeyQuery = Encoding.UTF8.GetString(KeyQuerybytes);

            var KeyQueryAsXmlString = DecryptString(utf8KeyQuery);
            var key = SignStringData(KeyQueryAsXmlString);
            return key;
        }

        public bool VerifyKeyQueryAndKeyPair(string KeyQueryBase64Url, string keyBase64Url)
        {
            var utf8SerilaNumberAsXml = GetKeyQueryAsXmlString(KeyQueryBase64Url);
            var KeyQueryAsXmlBytes = Encoding.UTF8.GetBytes(utf8SerilaNumberAsXml);

            var keyBytes = HttpServerUtility.UrlTokenDecode(keyBase64Url);

            return VerifyData(KeyQueryAsXmlBytes, keyBytes);
        }

        #endregion

    }
}
