using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace RemoteTicketPrinter.PrintUtils
{
    class LinuxPrint
    {
        public static string SendStringToPrinter(string PrinterName, string String)
        {
            File.WriteAllText("ticket.txt", String);

            var args = string.Format("lpr -P {0} {1}", PrinterName, "ticket.txt");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    //Arguments = $"-c \"{args}\"",
                    Arguments = "-c \" " + args + " \"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();            

            return result;
        }
    }
}
