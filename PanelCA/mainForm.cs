﻿using System;
using System.IO;
using System.Windows.Forms;
using System.Threading;
			

namespace PanelCA
{
	public partial class mainForm : Form
	{
		string NL = Environment.NewLine; // sekwencja nowej linii w komponencie Text-Multiline
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
			//unitCBox.SelectedIndex = 0;
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

		int x = 0;
		private void Butt_Click(object sender, EventArgs e)
		{
			dacGrid.Rows.Add();
			dacGrid.Rows[x].Cells[0].Value = "elo";
			dacGrid.Rows[x].Cells[1].Value = "elo!";
			dacGrid.Rows[x++].Cells[2].Value = "elo!!";


		}

		private void button2_Click(object sender, EventArgs e)
		{
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.Filter = "csv|*.csv|All files|*.*";
			dialog.ShowDialog();
			
			string path = dialog.FileName;
			if (File.Exists(path)) File.Delete(path);

			using (StreamWriter file = File.CreateText(path)) {
				for (int row = 0; row < dacGrid.Rows.Count; row++)
				{
					string line2 = dacGrid.Rows[row].Cells[0].Value.ToString();

					line2 += ";" + dacGrid.Rows[row].Cells[1].Value.ToString();
					line2 += ";" + dacGrid.Rows[row].Cells[2].Value.ToString() + NL;
					file.WriteLine(line2);
				}
			}
			
		}

		private void rekordsCheck_CheckedChanged(object sender, EventArgs e)
		{
			dacGrid.Enabled = rekordsCheck.Checked;
		}
	}
}
