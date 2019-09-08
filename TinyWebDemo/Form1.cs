using System;
using System.Net;
using System.Windows.Forms;
using TinyWeb;
namespace TinyWebDemo
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			comboBox1.Items.Add("(Any)");
			comboBox1.Items.Add(IPAddress.Loopback.ToString());
			comboBox1.Items.Add(IPAddress.IPv6Loopback.ToString());
			foreach (var addr in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
				comboBox1.Items.Add(addr.ToString());
			comboBox1.SelectedIndex = 0;
			textBox1.Text = "8080";
		}

		void webServer_ProcessRequest(object sender, ProcessRequestEventArgs args)
		{
			var r = args.Response;
			r.ContentType = "text/html";
			r.WriteLine("<html><h1>Hello World!</h1></html>");
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			webServer.IsStarted = checkBox1.Checked;
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			timer1.Enabled = true;
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			timer1.Enabled = false;
			
			IPAddress addr = "(Any)" == comboBox1.Text?IPAddress.Any:IPAddress.Parse(comboBox1.Text);

			int port;
			if(int.TryParse(textBox1.Text,out port))
			{
				webServer.EndPoint=new IPEndPoint(addr, port);
			}
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			timer1.Enabled = true;
		}
	}
}
