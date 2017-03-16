using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Dimond.LicenseClient
{
    public class KeyQueryModel
    {
        public string ProductName { get; set; }

        public string ComputerName { get; set; }

        public string SystemSerial { get; set; }

        public string ProcessorId { get; set; }

        public static KeyQueryModel GetCurrentKeyQuery()
        {
            var snm = new KeyQueryModel();

            var atr = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false)
                    .Single(a => a is AssemblyProductAttribute);


            var productAtr = (AssemblyProductAttribute)atr;
            if (string.IsNullOrWhiteSpace(productAtr.Product))
                throw new Exception("У приложения должен быть реализован аттрибут AssemblyProductAttribute");

            snm.ProductName = productAtr.Product;


            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_MotherboardDevice");
                var paramName = "SystemName";
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (queryObj[paramName] != null && !string.IsNullOrWhiteSpace(queryObj[paramName].ToString()))
                        snm.ComputerName = queryObj[paramName].ToString();
                }
            }
            catch (ManagementException e)
            {

            }

            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_OperatingSystem");

                var paramName = "KeyQuery";
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (queryObj[paramName] != null && !string.IsNullOrWhiteSpace(queryObj[paramName].ToString()))
                    {
                        var serial = queryObj[paramName].ToString();
                        int replacemntCount = 5;
                        if (serial.Length > replacemntCount)
                        {
                            serial = (new int[replacemntCount]).Select(i => i.ToString())
                                .Aggregate((i, i1) => i + i1) + serial.Substring(5);
                        }
                        snm.SystemSerial = serial;
                    }
                }
            }
            catch (ManagementException e)
            {

            }


            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_Processor");
                var paramName = "ProcessorId";
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (queryObj[paramName] != null && !string.IsNullOrWhiteSpace(queryObj[paramName].ToString()))
                        snm.ProcessorId = queryObj[paramName].ToString();
                }
            }
            catch (ManagementException e)
            {

            }

            return snm;
        }

        public static KeyQueryModel GetKeyQueryFromXmlString(string xmlData)
        {
            var serializer = new XmlSerializer(typeof(KeyQueryModel));
            KeyQueryModel result;

            using (TextReader reader = new StringReader(xmlData))
            {
                result = serializer.Deserialize(reader) as KeyQueryModel;
            }
            return result;
        }

        public static string GetCurrentKeyQueryAsXmlString()
        {
            var keyQueryModel = GetCurrentKeyQuery();
            var serilaNumberXmlString = GetKeyQueryAsXmlString(keyQueryModel);
            return serilaNumberXmlString;
        }

        public static string GetKeyQueryAsXmlString(KeyQueryModel keyQueryModel)
        {
            var serializer = new XmlSerializer(typeof(KeyQueryModel));
            var strBuilder = new Utf8StringWriter();
            var xmlWriter = XmlWriter.Create(strBuilder);
            serializer.Serialize(xmlWriter, keyQueryModel);
            var outStr = strBuilder.ToString();
            return outStr;
        }

        class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
        }
    }
}
