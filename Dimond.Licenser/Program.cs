using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dimond.LicenseClient;
using Dimond.LicenseServer;

namespace Dimond.Licenser
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = ConsoleParametrs(args);
            
                switch (p.CommandType)
                {
                    case Parametrs.GenLic:
                        {
                            GeneratorLicese(p.Parameter);
                            break;
                        }
                    case Parametrs.GenSn:
                        {
                            GeneratorKeyQuery();
                            break;
                        }
                }
        }

        static void GeneratorKeyQuery()
        {
            if (Properties.Settings.Default.IsFullScreen)
                Console.WindowWidth = 120;

            var pcName = InputParametr("Input PC name");
            var systemKey = InputParametr("Input systemKey");
            var cpuId = InputParametr("Input cpuId");
            var prodName = InputParametr("Input prodName");

            var serverCore = new ServerCore();
            var sn = serverCore.GetKeyQueryFromModel(new KeyQueryModel
                 {
                     ComputerName = pcName,
                     ProcessorId = cpuId,
                     ProductName = prodName,
                     SystemSerial = systemKey,
                 });
            Console.WriteLine(sn);
            Console.ReadLine();
        }

        static string InputParametr(string description)
        {
            Console.WriteLine(description + ": ");
            var parametr = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(parametr))
                InputParametr(description);
            return parametr;
        }

        static void GeneratorLicese(string id)
        {
            if (Properties.Settings.Default.IsFullScreen)
                Console.WindowWidth = 120;

            Console.WriteLine("Sn:\n{0}", id);
            Console.WriteLine();

            var srvLic = new ServerCore();

            var xml = srvLic.GetKeyQueryAsXmlString(id);
            string formattedXml = XElement.Parse(xml).ToString();
            Console.WriteLine(formattedXml);
            Console.WriteLine();

            var key = srvLic.GenerateKeyByKeyQueryBase64Url(id);
            Console.WriteLine("Key:\n{0}", key);
            Console.ReadLine();
        }

        static ConsoleCommand ConsoleParametrs(string[] args)
        {
            for (int index = 0; index < args.Length; index++)
            {
                try
                {
                    var s = args[index];
                    if (!String.IsNullOrWhiteSpace(s))
                    {
                        s = s.Trim().Remove(0, 1);
                        var command = s;
                            
                        var commandType = (Parametrs) Enum.Parse(typeof (Parametrs), command);

                        if (args.Count() > 1)
                        {
                            var param = args[index + 1];
                            var c = new ConsoleCommand {CommandType = commandType, Parameter = param};
                            return c;
                        }
                        return new ConsoleCommand{CommandType = commandType, Parameter = null};
                    }

                }
                catch (Exception exc)
                {
                    Console.WriteLine("Не правильные параметры команды");
                    Console.WriteLine("Возможные команды");
                    var commands = Enum.GetValues(typeof(Parametrs));
                    foreach (var command in commands)
                    {
                        Console.WriteLine(command);
                        Console.ReadLine();
                    }
                }
            }
            return null;
        }

        class ConsoleCommand
        {
            public Parametrs CommandType { get; set; }
            public string Parameter { get; set; }
        }

        [Flags]
        enum Parametrs
        {
            g,
            GenLic = g,

            s,
            GenSn = s,
        }
    }
}
