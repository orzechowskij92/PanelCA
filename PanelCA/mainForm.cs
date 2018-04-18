using System;
using System.IO;
using System.Windows.Forms;
using System.Threading;
			

namespace PanelCA
{
	public partial class mainForm : Form
	{
		public string NL = Environment.NewLine; // sekwencja nowej linii w komponencie Text-Multiline
		public Com com; //Obiektu portu COM/UART
		public Dac dac; //Obiekt przetornika DAC na platformie sprzętowej

		public mainForm()
		{ 
			InitializeComponent();
			com = new Com();
			dac = new Dac(com, consSendText, consRecivText, this);
			bitsChecks = new CheckBox[] { b0Check, b1Check, b2Check, 
				b3Check, b4Check, b5Check, b6Check, b7Check, b8Check,
				b9Check, b10Check, b11Check };
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
			e.Handled = !((char.IsDigit(e.KeyChar) || char.IsControl(e.KeyChar)) || char.IsControl(e.KeyChar));
		}

		private void clearButt_Click(object sender, EventArgs e)
		{
			sampText.Clear();
			step = 0;
		}

		
		private void addRow(int adcVal)
		{
			if (!rekordsCheck.Checked) return;
			int rows = dacGrid.Rows.Add();
			//BIN
			dacGrid.Rows[rows].Cells[0].Value = Convert.ToString(samp, 2);
			
			//DEC
			dacGrid.Rows[rows].Cells[1].Value = Convert.ToString(samp);
			
			//U0teor
			dacGrid.Rows[rows].Cells[2].Value = Convert.ToString(u0teor);

			//U0zm
			if (adcVal != -1) {
				Decimal d = decimal.Round(Convert.ToDecimal((float)adcVal / (float)1023.0 * vdd), 4);
				dacGrid.Rows[rows].Cells[3].Value = Convert.ToString(Convert.ToSingle(d));
			}
			else
				dacGrid.Rows[rows].Cells[3].Value = "N/C";
		}
		private void exportButt_Click(object sender, EventArgs e)
		{
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.Filter = "csv|*.csv|All files|*.*";
			dialog.ShowDialog();
			
			string path = dialog.FileName;
			if (File.Exists(path)) File.Delete(path);

			using (StreamWriter file = File.CreateText(path)) {
				file.WriteLine("RegVal(BIN);RegVal(DEC);U0teor[V];U0zm[V]");
				for (int row = 0; row < dacGrid.Rows.Count; row++)
				{
					string line = "";
					try
					{
						line = dacGrid.Rows[row].Cells[0].Value.ToString()
							+ ";" + dacGrid.Rows[row].Cells[1].Value.ToString()
							+ ";" + dacGrid.Rows[row].Cells[2].Value.ToString()
							+ ";" + dacGrid.Rows[row].Cells[3].Value.ToString();
					}
					catch (Exception) { };
					file.WriteLine(line);
				}
			}
			
		}

		private void rekordsCheck_CheckedChanged(object sender, EventArgs e)
		{
			dacGrid.Enabled = rekordsCheck.Checked;
		}


		private void consSendText_TextChanged_1(object sender, EventArgs e)
		{
			TextBox sndr = sender as TextBox;
			sndr.SelectionStart = sndr.Text.Length;
			sndr.ScrollToCaret();
		}
	}
}
