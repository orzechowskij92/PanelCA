using System;
using System.Windows.Forms;
using System.Threading;
			

namespace PanelCA
{
	public partial class mainForm : Form
	{
		const string NL = "\r\n"; // sekwencja nowej linii w komponencie Text-Multiline
		public Com com; //Obiektu portu COM/UART
		public Dac dac; //Obiekt przetornika DAC na platformie sprzętowej

		public mainForm()
		{
			InitializeComponent();
			com = new Com();
			dac = new Dac(com, consRecivText);
			bitsChecks = new CheckBox[] { b0Check, b1Check, b2Check, 
				b3Check, b4Check, b5Check, b6Check, b7Check, b8Check,
				b9Check, b10Check, b11Check };
			unitCBox.SelectedIndex = 0;
		}

		private void connButt_Click(object sender, EventArgs e)
		{
			if (connCheck.Checked) {
				com.disconn();
				connButt.Text = "Połącz";
				connCheck.Checked = false;
				return;
			}
			try {
				com.connectTo(portsCBox.Text);

				if (com.connected) {
					consSendText.Text += "Hello Board!" + NL;
					consRecivText.Text += com.helloBoard() + NL;
					connButt.Text = "Rozłącz";
					connCheck.Checked = true;
				} else {
					MessageBox.Show("Niepowodzenie w łączeniu!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					refreshButt_Click(this, null);
				}
			}
			catch {
				MessageBox.Show("Niepowodzenie w łączeniu!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				refreshButt_Click(this, null);
			}
		}	

		private void refreshButt_Click(object sender, EventArgs e)
		{
			portsCBox.Items.Clear();

			if (com.refreshPorts() > 0) {
			portsCBox.Items.AddRange(com.availPorts);
			portsCBox.SelectedIndex = 0;
			connButt.Enabled = true;

			connButt_Click (this, null);
			//Auto Conect
			}
			else {
			//MessageBox.Show("Nie wykryto portów COM!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			connButt.Enabled = false;
			}
		}

		private void sampText_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = !((char.IsDigit(e.KeyChar) || char.IsControl(e.KeyChar)) 
				&& (sampText.Lines.Length <= 10 || char.IsControl(e.KeyChar)));
		}

		private void delText_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = !(char.IsDigit(e.KeyChar) || char.IsControl(e.KeyChar));
		}

		private void clearButt_Click(object sender, EventArgs e)
		{
			sampText.Clear();
		}
	}
}
