using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using nucs.JsonSettings;


namespace RemoteTicketPrinter
{    
    class PrintController
    {
        private string OS = "na";
        private string printerName;
        private static SettingsBag Settings = JsonSettings.Load<SettingsBag>("config.json");
        public static string openCommand;
        public static string cutCommand;


        public PrintController()
        {
            printerName = Settings["printer"] as String;

            string command = Settings.Get("cutcommand", "27.105");
            foreach (var c in command.Split('.'))
            {
                int unicode = Convert.ToInt32(c);
                char character = (char)unicode;

                cutCommand += character.ToString();
            }

            command = Settings.Get("opencommand", "27.112.48");
            foreach (var c in command.Split('.'))
            {
                int unicode = Convert.ToInt32(c);
                char character = (char)unicode;

                openCommand += character.ToString();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                OS = "windows";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                OS = "linux";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                OS = "OSX";
            }
        }

        public void Print(string text)
        {
            StringBuilder builder = new StringBuilder(text);
            builder.Replace("[[cut]]", cutCommand);
            builder.Replace("[[opendrawer]]", openCommand);

            text = builder.ToString();
            Console.WriteLine(text);

            switch (OS)
            {
                default:
                    break;

                case "windows":
                    PrintUtils.WindowsRawPrint.SendStringToPrinter(printerName, text);
                    break;

                case "linux":
                    PrintUtils.LinuxPrint.SendStringToPrinter(printerName, text);
                    break;

                case "OSX":
                    break;
            }
        }

        public void extras()
        {
            //BasePrinter printer;
            //ICommandEmitter e;

            //string ip = "192.168.1.1";
            //int port = 9100;

            //printer = new FilePrinter("test.txt");
            //printer = new NetworkPrinter(ip, port, false);
            //e = new EPSON();

            //byte[] bytes = System.Text.Encoding.Unicode.GetBytes(text);

            //printer.Write(ByteSplicer.Combine(e.Print(text)));
            //printer.Dispose();
        }
    }
}
