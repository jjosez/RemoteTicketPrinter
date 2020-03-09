/*
 * This file is part of RemotePrinter.
 * Copyright (C) 2018-2019 Juan José Prieto Dzul <juanjoseprieto88@gmail.com>
 * 
 * RemotePrinter is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * RemotePrinter is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with RemotePrinter.  If not, see<https://www.gnu.org/licenses/>.
 */
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using nucs.JsonSettings;
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;

namespace RemoteTicketPrinter
{
    public class PrintServer
    {
        private HttpListener httpListener;
        private static AutoResetEvent listenForNextRequest = new AutoResetEvent(false);
        private static SettingsBag Settings = JsonSettings.Load<SettingsBag>("config.json");

        public string Prefix { get; set; }
        public bool IsRunning { get; private set; }
        public static string openCommand;
        public static string cutCommand;

        //Delegate of type Action<string>
        private static Action<string> StatusUpdater;

        public PrintServer(Action<string> UpdateStatusDelegate)
        {
            httpListener = new HttpListener();
            StatusUpdater = UpdateStatusDelegate;

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
        }

        public void Start()
        {
            // Verify HttpListener is supported
            if (!HttpListener.IsSupported)
                throw new NotSupportedException("HttpListener not supported.");

            // Add prefix
            if (String.IsNullOrEmpty(Prefix))
                throw new InvalidOperationException("Specify prefix");
            httpListener.Prefixes.Clear();
            httpListener.Prefixes.Add(Prefix);

            // Start server
            httpListener.Start();
            ThreadPool.SetMaxThreads(10, 10);
            ThreadPool.QueueUserWorkItem(ListenRequest);
            //Thread.Sleep(3000);
            //StatusUpdater("Server runing fine");
        }

        internal void Stop()
        {
            httpListener.Stop();
            IsRunning = false;
        }

        private void ListenRequest(object state)
        {
            // Loop here to begin processing of new requests.
            while (httpListener.IsListening)
            {
                StatusUpdater("Servidor iniciado.");

                httpListener.BeginGetContext(new AsyncCallback(ListenerCallback), httpListener);
                listenForNextRequest.WaitOne();

                IsRunning = true;
            }
        }

        private static void ListenerCallback(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            HttpListenerContext context = listener.EndGetContext(result);

            if (context.Request.HttpMethod.Equals("GET"))
            {
                GetRequestProcess(context);
            }

            // Process next request
            result = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
        }

        private static void GetRequestProcess(HttpListenerContext context)
        {            
            var query = context.Request.QueryString["documento"];
            var url = Settings.Get("server", "http://localhoost/facturascripts");
            var token = Settings.Get("key", "");
            var ticket = "Ticket no encontrado";

            RestClient cliente = new RestClient(url);
            RestRequest request = new RestRequest("api/3/ticketes/{id}", Method.GET);

            request.AddHeader("Token", token);
            request.AddUrlSegment("id", query);
            request.Timeout = 2000;

            IRestResponse<Ticket> response = cliente.Execute<Ticket>(request);

            if (response.ErrorException != null)
            {
                Console.WriteLine("Error {0}: {1}", response.StatusCode, response.ErrorMessage);
                context.Response.StatusCode = 408;
                context.Response.StatusDescription = "Request Timeout";
            }

            if (response.IsSuccessful)
            {
                ticket = response.Data.text;
                Console.WriteLine("Imprimiendo ticket...");
                Print(ticket);
                context.Response.StatusCode = 200;
                context.Response.StatusDescription = "OK";
            }

            Console.WriteLine(ticket);

            // Response
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Methods", "GET");

            context.Response.Close();
        }

        private static void Print(string data)
        {
            string printerName = Settings.Get("printer", "");

            StringBuilder builder = new StringBuilder(data);
            builder.Replace("[[cut]]", cutCommand);
            builder.Replace("[[opendrawer]]", openCommand);

            data = builder.ToString();

            if (!string.IsNullOrEmpty(data))
            {
                SendToPrinter(printerName, data);
            }
            else
            {
                Console.WriteLine("La respuesta esta vacia.");
            }
        }

        private static void SendToPrinter(string printerName, string text)
        {
            BasePrinter printer;
            ICommandEmitter e;

            string ip = "192.168.1.1";
            int port = 9100;

            //printer = new FilePrinter("test.txt");
            printer = new NetworkPrinter(ip, port, false);
            e = new EPSON();

            //byte[] bytes = System.Text.Encoding.Unicode.GetBytes(text);

            printer.Write(ByteSplicer.Combine(e.Print(text)));
            printer.Dispose();
        }
    }
}


