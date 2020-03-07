using System;
using Eto.Forms;
using Eto.Drawing;
using nucs.JsonSettings;
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
					)
				}
			};

			saveButton.Click += SaveButton_Click;
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

            //textBoxApiKey.Text = Settings.Get("key", "otherkey");
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
            }
        }

        private void RunServer()
        {
            PrintServer printserver = new PrintServer(UpdateStatus)
            {
                Prefix = "http://localhost:10080/"
            };

            printserver.Start();
        }

        private void UpdateStatus(string serverStatus)
        {

        }
    }
}
