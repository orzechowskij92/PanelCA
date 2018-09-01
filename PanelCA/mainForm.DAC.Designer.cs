using System;
using System.Windows.Forms;
using System.Threading;


namespace PanelCA
{
	public partial class mainForm
	//Część głównej klasy okna odpowiedzialna za sterowanie DAC

	{
		CheckBox[] bitsChecks;
		int samp = 0, delay = 1; 

		float vdd = (float)4.75; //Wartość napięcia zasilania przetwornika
		
		//*****************************************************************************

		private void bitCheck(object sender, EventArgs e)
		//Zaznaczanie bitu w panelu
		{
			CheckBox bitChecked = sender as CheckBox; //rzut parametru sender na kontrolke Check
			bitChecked.Text = (bitChecked.Checked) ? "1" : "0"; // zmiana bitu w nazwie kontrolki
			int mult = (bitChecked.Checked) ? 1 : -1;
			
			//dodanie zaznaczonej/odznaczonej potegi dwojki
			for (int i = 0, val = 1; i <= 11; i++, val <<= 1)
				if (bitChecked == bitsChecks[i])
					samp += mult * val;

			//Aktualizacja pól DEC, HEX i U0teor
			decText.Text = Convert.ToString(samp);
			hexText.Text = Convert.ToString(samp, 16);
			if (vdd > 0 ) vddText_TextChanged(this, null);

			//Ustawienie próbki i wypisanie danych do tablicy
			addRow(dac.setSamp(samp, adcReadCheck.Checked));
		}

		//-----------------------------------------------------------------------------

		Single u0teor = 0;
		private void vddText_TextChanged(object sender, EventArgs e)
		//Zmiana wartości napięcia zasilania
		{
			//Single u0teor = 0;
			if (vddText.Text == "") {
				vdd = -1;
				u0Label.Visible = false;
				return;
			}
			else if (!u0Label.Visible) u0Label.Visible = true; 
			
			vdd = Convert.ToSingle(vddText.Text);
			Decimal d = decimal.Round(Convert.ToDecimal ((float)samp / (float)4095.0 * vdd), 4);
			
			u0teor = Convert.ToSingle(d);
			u0Label.Text = "U0 = " + d.ToString() + "[V]";
			
		}

		//-----------------------------------------------------------------------------

		private void hexText_KeyUp(object sender, KeyEventArgs e)
		//Zmiana wartości hexadecymalnej
		{
			if (hexText.Text == "") return;
			samp = Convert.ToInt32(hexText.Text, 16);
			decText.Text = Convert.ToString(samp);
			
			bitsReload();

			//Ustawienie próbki i wypisanie danych do tablicy
			addRow(dac.setSamp(samp, adcReadCheck.Checked));
		}

		//-----------------------------------------------------------------------------

		private void decText_KeyUp(object sender, KeyEventArgs e)
		//Zmiana wartości dziesiętnej
		{
			if (decText.Text == "") return;
			samp = Convert.ToInt32(decText.Text);
			hexText.Text = Convert.ToString(samp, 16);
			
			bitsReload();

			//Ustawienie próbki i wypisanie danych do tablicy
			addRow(dac.setSamp(samp, adcReadCheck.Checked));
		}

		//-----------------------------------------------------------------------------

		void bitsReload()
		//Ustawia wartość kontrolek typu Check dla poszczególnych bitów. Porównuje wynik operacji
		//maskowania dla kolejnych potęg dwójki, wpisuje do własciwości Enable kontroki oraz
		//ustawia znak na kontrolce.
		{
			for (int i = 0, j = 1; i < 12; j <<= 1, i++)
				bitsChecks[i].Text = (bitsChecks[i].Checked = ((samp & j) != 0) ) ? "1" : "0";
			
			vddText_TextChanged(this, null);
		}
		
		//-----------------------------------------------------------------------------
		
		private void setSamp(int newSamp)
		//Ustawienie zewnetrzne wartosci probki, uzywane przy tabeli wartości.
		{
			samp = newSamp;
			
			//Aktualizacja pól DEC i HEX
			decText.Text = Convert.ToString(samp);
			hexText.Text = Convert.ToString(samp, 16);
			
			//Aktualizacja bitBoeardu
			bitsReload();

			//Ustawienie próbki i wypisanie danych do tablicy
			addRow(dac.setSamp(samp, adcReadCheck.Checked));
		}

		//-----------------------------------------------------------------------------

		int step = -1;
		const string arrowStr = " ←";
		private void nextbackButt_Click(object sender, EventArgs e)
		//Poruszanie się po liście nastaw (sesja pomiarowa)
		{
			string[] lines = sampText.Lines; //zawarość pola

			int dir = ((sender as Button).Name == "nextButt") ? 1 : -1;
			int rowsCnt = Convert.ToInt32(lines.Length); //ilosc wierszy, liczona od 0
			if (dir == -1 && step == -1 || dir == 1 && step == rowsCnt) return;

			//Spr czy aktualnie jest wybrany element
			if (step > -1 && step < rowsCnt) {
				//Usuń znacznik wybrania elementu w sesji
				if (lines[step].Contains(arrowStr))
					lines[step] = lines[step].Substring(0, lines[step].Length - arrowStr.Length);
			}

			step += dir;
			if (dir == -1 && step == (rowsCnt - 1) && lines[step].Contains(arrowStr)) {
				lines[step] = lines[step].Substring(0, lines[step].Length - arrowStr.Length);
				step += dir;
			} 
			else if (dir == 1 && step == 0 && lines[step].Contains(arrowStr)) {
				lines[step] = lines[step].Substring(0, lines[step].Length - arrowStr.Length);
				step += dir;
			}
				
			if (step == -1 || step == rowsCnt) return; //granice poczatku i konca sesji

			//przejdz po pustych liniach, az do konca listy
			while (lines[step] == "") if ((step += dir) == -1 || step == rowsCnt) return;

			//Ustaw wyjscie DAC, przeładuj pozostałe kontrolki stanu DAC
			setSamp (Convert.ToInt32(lines[step]));

			//Dodanie znacznika postepu sesji
			lines[step] += arrowStr;

			//Odswierz liste
			sampText.Lines = lines;
			this.Refresh();
		}
		
		//-----------------------------------------------------------------------------
				
		private void ringButt_Click(object sender, EventArgs e)
		//Generowanie kodu pierścieniowego
		{
			sampText.Clear();

			for (int bit = 1, i = 0; bit <= 0x0800; bit <<= 1, i++)
				sampText.AppendText(Convert.ToString (bit) + (bit < 0x0800 ? NL : ""));
				

		}
		
		//-----------------------------------------------------------------------------

		private void dacScroll_Scroll(object sender, ScrollEventArgs e)
		{
			setSamp(dacScroll.Value);
		}

		//-----------------------------------------------------------------------------

		private void button1_Click(object sender, EventArgs e)
		//Wersja prototypowa generowania
		{
			sampText.Clear();
			int step = Convert.ToInt32(textBox1.Text);
			for (samp = step; samp < 4096; samp += step) {
				sampText.AppendText (Convert.ToString(samp) + NL);
			}
		}

		//-----------------------------------------------------------------------------

		/*
		Wersja sesji pomiarowej przed zmianą wymagań przez promotora



		//int step = 0;
		const string arrowStr = " \u0011\u0006\u0006\u0006";
		private void nextButt_Click(object sender, EventArgs e)
		{
			//pobieranie pola z wartościami
			string[] lines = sampText.Lines; //zawarość pola
			int rowsCnt = Convert.ToUInt16(lines.Length); //ilosc wierszy
			if (rowsCnt == 0 || step >= rowsCnt) return; //spr czy nie jest na końcu
			while (lines[step] == "") if (++step == rowsCnt) return; //inoruj puste linie

			//[] arr = new int[len]; //wartosci probek
			int beforeStep = step - 1;
			if (step > 0 && lines[beforeStep].Contains(arrowStr))
				lines[beforeStep] = lines[beforeStep].Substring(0, lines[beforeStep].Length - arrowStr.Length);
			int samp = Convert.ToInt32(lines[step]);

			//if (j == 0) return;

			//wysylanie danych sesji
			//dac.setSamps(delay, arr, j);
			//dac.setSamp(samp);
			lines[step++] += arrowStr;
			//if (++step == rowsCnt) step = 0;
			sampText.Lines = lines;
			this.Refresh();
			setSamp(samp);
			
			//Wskaznik postepu sesji
			string line = "";
			j = 0;
			for (int i = 0; i < len; i++) {
				if ((line = lines[i]) == "") continue;
				lines[i] += " <--";
				sampText.Lines = lines;
				setSamp(arr[j++]);

				this.Refresh();
				Thread.Sleep (delay);
				
				lines[i] = lines[i].Substring(0, lines[i].Length - 4);
				sampText.Lines = lines;
			}
		}
		private void backButt_Click(object sender, EventArgs e)
		{
			string[] lines = sampText.Lines; //zawarość pola
			int rowsCnt = Convert.ToUInt16(lines.Length); //ilosc wierszy
			if (rowsCnt == 0 || step > 0) return; //spr czy nie jest na poczatku
			while (lines[step] == "") if (--step == 0) return; //inoruj puste linie

		} */
		
		
	}
}
