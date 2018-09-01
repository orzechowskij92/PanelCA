using System;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Text;

namespace PanelCA

{
	public partial class mainForm : Form
	//Głowna klasa okna odpowiedzialna za łączenie z portem, walidacje wprowadzania danych
	//oraz prezentacje/eksportowanie pomiarów w tabeli

	{
		public string NL = Environment.NewLine; // sekwencja nowej linii w komponencie Text-Multiline
		public Com com; //Obiektu portu COM/UART
		public Dac dac; //Obiekt przetornika DAC na platformie sprzętowej
		private bool genMode = false;

		//*****************************************************************************

		public mainForm()
		//Główny konstruktor obiektu okna
		{ 
			InitializeComponent();
			com = new Com();
			dac = new Dac(com, consSendText, consRecivText, adcChart, NL);
			
			//Tablica przycisków dla poszczególnych bitów
			bitsChecks = new CheckBox[] {b0Check, b1Check, b2Check, 
				b3Check, b4Check, b5Check, b6Check, b7Check, b8Check,
				b9Check, b10Check, b11Check};
		}

		//-----------------------------------------------------------------------------

		private void connButt_Click(object sender, EventArgs e)
		//Obsługa zdarzenia dla przycusku Połącz/Rozłącz
		{
			if (connCheck.Checked) {
				com.disconn();
				connButt.Text = "Połącz";
				connCheck.Checked = false;
				return;
			}
			try {
				com.connectTo(portsCBox.Text); //Próba połączenia

				if (com.connected) { //Jeśli połączono
					consSendText.Text += "Hello Board!" + NL; //Wyświetl napis powitający
					consRecivText.Text += com.helloBoard() + NL; //Wyślij wiadomość powitalna (czeka na odpowiedź)
					connButt.Text = "Rozłącz";
					connCheck.Checked = true;
				} else { //Jeśli nie połączono
					MessageBox.Show("Niepowodzenie w łączeniu!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					refreshButt_Click(this, null);
				}
			}
			catch { //Jeśli nastąpił jaki kolwiek wyjątek podczas próby połączenia
				MessageBox.Show("Niepowodzenie w łączeniu!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				refreshButt_Click(this, null);
			}
		}	

		//-----------------------------------------------------------------------------

		private void refreshButt_Click(object sender, EventArgs e)
		//Obsługa zdarzenia dla przycusku Odśwież
		{
			portsCBox.Items.Clear(); //Czyszczenie listy portów

			if (com.refreshPorts() > 0) { //Odśwież liste, jeśli wykryto porty
				portsCBox.Items.AddRange(com.availPorts); //Dodawanie listy portów
				portsCBox.SelectedIndex = 0; //Wybierz pierwszy port
				connButt.Enabled = true;

				connButt_Click (this, null); //Próba połączenia
			} else {
			//MessageBox.Show("Nie wykryto portów COM!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			connButt.Enabled = false;
			}
		}

		//-----------------------------------------------------------------------------

		private void sampText_KeyPress(object sender, KeyPressEventArgs e)
		//Obsługa zdarzenia dla pola z listą próbek 
		//wstrzymywanie znaków nie będącymi cyrą lub znakiem kontrolnym
		{
			e.Handled = !((char.IsDigit(e.KeyChar) || char.IsControl(e.KeyChar)));
		}
		private void vddText_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = !(char.IsDigit(e.KeyChar) || char.IsControl(e.KeyChar) || e.KeyChar == ',')
				|| (e.KeyChar == ',' && (sender as TextBox).Text.IndexOf(',') > -1);
		}
		private void decText_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = !(char.IsDigit(e.KeyChar) || char.IsControl(e.KeyChar));
		}
		private void hexText_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = !(char.IsDigit(e.KeyChar) || char.IsControl(e.KeyChar) 
				|| ((e.KeyChar | ' ') >= 'a' && (e.KeyChar | ' ') <= 'f'));
		}

		private void clearButt_Click(object sender, EventArgs e)
		{
			sampText.Clear();
			step = 0;
		}

		//-----------------------------------------------------------------------------

		int xChart = 1;
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
				Decimal v = decimal.Round(Convert.ToDecimal((float)adcVal / (float)1023.0 * vdd), 4);
				dacGrid.Rows[rows].Cells[3].Value = Convert.ToString(Convert.ToSingle(v));
				adcChart.Series[0].Points.AddXY(xChart++, v);
			}
			else
				dacGrid.Rows[rows].Cells[3].Value = "N/C";
		}
		
		//-----------------------------------------------------------------------------
		
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

		//-----------------------------------------------------------------------------

		private void rekordsCheck_CheckedChanged(object sender, EventArgs e)
		{
			dacGrid.Enabled = rekordsCheck.Checked;
		}

		//-----------------------------------------------------------------------------

		private void consSendText_TextChanged_1(object sender, EventArgs e)
		{
			TextBox sndr = sender as TextBox;
			sndr.SelectionStart = sndr.Text.Length;
			sndr.ScrollToCaret();
		}

		//-----------------------------------------------------------------------------

		private void genButt_Click(object sender, EventArgs e)
		{
			if (!genMode) {
				genMode = true;
				int[] arr = new int[3];
				dac.setSamps(0, arr, 1);
				//Thread.Sleep(30);
				//adcMonit = new Thread(new ThreadStart(dac.adcMonitor(consRecivText)));
				//adcMonit.Start(consRecivText);
			} else {
				dac.stopGen();	
				Thread.Sleep(50);
				genMode = false;
			}
		}
	}
}
