using System;
using Eto.Forms;
using Eto.Drawing;
using nucs.JsonSettings;
using System.Net.Sockets;
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using System.Drawing.Printing;

namespace RemoteTicketPrinter
{
	public partial class MainForm : Form
    {
		SettingsBag Settings;

		private DropDown printersDropdown;
		private TextBox textBoxServer;
		private TextBox textBoxApiKey;
		private TextBox textBoxCutCommand;
		private TextBox textBoxOpenCommand;

		public MainForm()
        {
            InitializeComponent();
            LoadAppSettings();

            RunServer();
        }		
		void InitializeComponent()
		{
			Title = "RemotePrinter Server";
			ClientSize = new Size(500, -1);

			printersDropdown = new DropDown();
			LoadInstalledPrinters(printersDropdown);

			Button saveButton;
			Button testButton;
			Content = new TableLayout
			{
				Spacing = new Size(10, 10), // space between each cell
				Padding = new Padding(10, 10, 10, 10), // space around the table's sides
				Rows =
				{
					new TableRow(
						new TableCell(new Label { Text = "API URL" }, true)
					),
					new TableRow(
						textBoxServer = new TextBox { PlaceholderText = "http://localhost/facturascritpts", Width = -1}
					),
					new TableRow(
						new TableCell(new Label { Text = "API Key" }, true)
					),
					new TableRow(
						textBoxApiKey = new TextBox { PlaceholderText = "La clave de acceso a la API", Width = -1}
					),
					new TableRow(
						new Label { Text = "Impresora" }
					),
					new TableRow(
						printersDropdown
					),
					new TableRow(
						new StackLayout
						{
							Orientation = Orientation.Horizontal,
							Spacing = 10,
							Size = new Size(-1, -1),
							//BackgroundColor = new Color(28, 70, 215),
							Items =
							{
								new StackLayoutItem { 
									Expand = true,
									Control = textBoxCutCommand = new TextBox { 
										PlaceholderText = "Comando de corte",
									} 
								},
								new StackLayoutItem {
									Expand = true,
									Control = textBoxOpenCommand = new TextBox { 
										PlaceholderText = "Comando de apertura"
									} 
								}
							}
						}
					),
					new TableRow(
						saveButton = new Button { Text = "Save"}						
					),
					new TableRow(
						testButton = new Button { Text = "Test"}
					)
				}
			};

			saveButton.Click += SaveButton_Click;
			testButton.Click += TestButton_Click;
		}

		private void TestButton_Click(object sender, EventArgs e)
		{
			NetworkStream ns = null;
			Socket socket = null;

			//try
			//{
			//	IPEndPoint printerIP = null;
			//	if (printerIP == null)
			//	{
			//		/* IP is a string property for the printer's IP address. */
			//		/* 6101 is the common port of all our Zebra printers. */
			//		printerIP = new IPEndPoint(IPAddress.Parse("192.168.1.1"), 9100);
			//	}

			//	socket = new Socket(AddressFamily.InterNetwork,
			//		SocketType.Stream,
			//		ProtocolType.Tcp);
			//	socket.Connect(printerIP);

			//	ns = new NetworkStream(socket);

			//	byte[] toSend = Encoding.Unicode.GetBytes("Prueba de impresion");
			//	ns.Write(toSend, 0, toSend.Length);
			//}
			//finally
			//{
			//	if (ns != null)
			//		ns.Close();

			//	if (socket != null && socket.Connected)
			//		socket.Close();
			//}
			NewPrintMethod();
		}

		private void SaveButton_Click(object sender, EventArgs e)
		{
			Settings["cutcommand"] = textBoxCutCommand.Text;
			Settings["opencommand"] = textBoxOpenCommand.Text;
			Settings["key"] = textBoxApiKey.Text;
			Settings["printer"] = printersDropdown.SelectedKey;
			Settings["server"] = textBoxServer.Text;

			var dlg = new Dialog
			{
				ClientSize = new Size(200, 100),

				Content = new StackLayout
				{
					Items =
					{
						new StackLayoutItem { Control = new Label { Text = "Configuracion guardada"} },
					}
				},
			};

			dlg.ShowModal(this);
		}

		private void LoadAppSettings()
        {
            Settings = JsonSettings.Load<SettingsBag>("config.json").EnableAutosave();

			textBoxApiKey.Text = Settings["key"] as String;
			textBoxServer.Text = Settings["server"] as String;
			textBoxCutCommand.Text = Settings.Get("cutcommand", "27.105");
			textBoxOpenCommand.Text = Settings.Get("opencommand", "27.112.48");

			if (Settings.Get("printer", "") != "")
            {
                printersDropdown.SelectedKey = Settings["printer"] as String;
			}
        }

        private void LoadInstalledPrinters(DropDown dropDown)
        {
            foreach (string pName in PrinterSettings.InstalledPrinters)
            {
                dropDown.Items.Add(pName);
				Console.WriteLine(pName);
            }
        }

        private void RunServer()
        {
            PrintServer printserver = new PrintServer(UpdateStatus)
            {
                Prefix = "http://127.0.0.1:10080/"
            };

            printserver.Start();
        }

        private void UpdateStatus(string serverStatus)
        {

        }

		private void NewPrintMethod()
		{
			PrinterSettings ps = new PrinterSettings();
			ps.PrinterName = Settings["printer"] as String;
			Console.WriteLine(ps);

			BasePrinter printer;
			ICommandEmitter e;

			string ip = "192.168.1.1";
			int port = 9100;

			printer = new FilePrinter("test.txt");
			e = new EPSON();

			byte[] bytes = System.Text.Encoding.Unicode.GetBytes("Prueba de impresion");
			printer.Write(bytes);
			printer.Dispose();
		}
    }
}
