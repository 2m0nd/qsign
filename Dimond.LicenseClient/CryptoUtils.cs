using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Dimond.LicenseClient
{
    public static class CryptUtils
    {
        public static void GenerateRsaKeyPair(string outPathPrivateKey, string outPathPublicKey)
        {
            // Variables
            CspParameters cspParams = null;
            RSACryptoServiceProvider rsaProvider = null;
            StreamWriter publicKeyFile = null;
            StreamWriter privateKeyFile = null;
            string publicKey = "";
            string privateKey = "";

            try
            {
                // Create a new key pair on target CSP
                cspParams = new CspParameters();
                cspParams.ProviderType = 1; // PROV_RSA_FULL 
                //cspParams.ProviderName; // CSP name
                cspParams.Flags = CspProviderFlags.UseArchivableKey;
                cspParams.KeyNumber = (int)KeyNumber.Exchange;
                rsaProvider = new RSACryptoServiceProvider(2024, cspParams);

                // Export public key
                publicKey = rsaProvider.ToXmlString(false);

                // Write public key to file
                publicKeyFile = File.CreateText(outPathPublicKey);
                publicKeyFile.Write(publicKey);

                // Export private/public key pair 
                privateKey = rsaProvider.ToXmlString(true);

                // Write private/public key pair to file
                privateKeyFile = File.CreateText(outPathPrivateKey);
                privateKeyFile.Write(privateKey);
            }
            catch (Exception ex)
            {
                // Any errors? Show them
                Console.WriteLine("Exception generating a new key pair! More info:");
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Do some clean up if needed
                if (publicKeyFile != null)
                {
                    publicKeyFile.Close();
                }
                if (privateKeyFile != null)
                {
                    privateKeyFile.Close();
                }
            }
        }

        public static string Encrypt(string publicKeyXmlString, string data)
        {
            // full array of bytes to encrypt
            byte[] bytesToEncrypt;

            // worker byte array
            byte[] block;

            // encrypted bytes
            byte[] encryptedBytes;

            // length of bytesToEncrypt
            var dataLength = 0;

            // number of bytes in key                
            var keySize = 0;

            // maximum block length to encrypt          
            var maxLength = 0;

            // how many blocks must we encrypt to encrypt entire message?
            var iterations = 0;

            // the encrypted data
            var encryptedData = new StringBuilder();

            // instantiate the crypto provider with the correct key length
            var rsaCryptoServiceProvider = new RSACryptoServiceProvider();

            // initialize the RSA object from the given public key
            rsaCryptoServiceProvider.FromXmlString(publicKeyXmlString);

            // convert data to byte array
            bytesToEncrypt = Encoding.UTF8.GetBytes(data);

            // get length of byte array
            dataLength = bytesToEncrypt.Length;

            // convert length of key from bits to bytes
            keySize = rsaCryptoServiceProvider.KeySize / 8;

            // .NET RSACryptoServiceProvider uses SHA1 Hash function
            // use this to work out the maximum length to encrypt per block
            maxLength = ((keySize - 2) - (2 * SHA1.Create().ComputeHash(bytesToEncrypt).Length));

            // how many blocks do we need to encrypt?
            iterations = dataLength / maxLength;

            // encrypt block by block
            for (int index = 0; index <= iterations; index++)
            {
                // is there more than one full block of data left to encrypt?
                if ((dataLength - maxLength * index) > maxLength)
                {
                    block = new byte[maxLength];
                }
                else
                {
                    block = new byte[dataLength - maxLength * index];
                }

                // copy the required number of bytes from the array of bytes to encrypt to our worker array
                Buffer.BlockCopy(bytesToEncrypt, maxLength * index, block, 0, block.Length);

                // encrypt the current worker array block of bytes
                encryptedBytes = rsaCryptoServiceProvider.Encrypt(block, true);

                // RSACryptoServiceProvider reverses the order of encrypted bytesToEncrypt after encryption and before decryption.
                // Undo this reversal for compatibility with other implementations
                Array.Reverse(encryptedBytes);

                // convert to base 64 string
                encryptedData.Append(Convert.ToBase64String(encryptedBytes));
            }

            return encryptedData.ToString();
        }

        public static string Decrypt(string privateKeyXmlString, string data)
        {
            var rsaCryptoServiceProvider = new RSACryptoServiceProvider();
            rsaCryptoServiceProvider.FromXmlString(privateKeyXmlString);

            var dwKeySize = rsaCryptoServiceProvider.KeySize;

            int base64BlockSize = ((dwKeySize / 8) % 3 != 0) ?
              (((dwKeySize / 8) / 3) * 4) + 4 : ((dwKeySize / 8) / 3) * 4;

            int iterations = data.Length / base64BlockSize;

            var arrayList = new ArrayList();
            for (int i = 0; i < iterations; i++)
            {
                byte[] encryptedBytes = Convert.FromBase64String(
                     data.Substring(base64BlockSize * i, base64BlockSize));

                // Be aware the RSACryptoServiceProvider reverses the order of 
                // encrypted bytes after encryption and before decryption.
                // If you do not require compatibility with Microsoft Cryptographic 
                // API (CAPI) and/or other vendors.
                // Comment out the next line and the corresponding one in the 
                // EncryptString function.
                Array.Reverse(encryptedBytes);
                arrayList.AddRange(rsaCryptoServiceProvider.Decrypt(encryptedBytes, true));
            }

            return Encoding.UTF8.GetString(arrayList.ToArray(typeof(byte)) as byte[]);
        }
    }

    public enum RsaKeyLengths
    {
        Bit1024 = 1024,
        Bit2048 = 2048,
        Bit4096 = 4096
    }
}
