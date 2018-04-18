using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PanelCA
{
	public class Dac
	{
		//*****************************************************************************

		public Com com; //Obiekt komunikacji po porcie COM
		System.Windows.Forms.TextBox consoleResp, consoleReq; //Konsola do wypisywania inf zwrotnej
		//private string NL = "\r\n"; //Nowa linia dla komunikacji z platformą
		private string NLform; //Nowa linia dla komunikacji z platformą
		
		const byte MASKL =  0x00FF; //Maska dla młodszej części słowa dla DAC 
		const byte MASKH =  0x000F; //Maska dla starszej części słowa dla DAC
		
		//*****************************************************************************
		//Konstruktor parametryczy, przyjmuje jako parametry: obiekt typu Com umożliwiający
		//wysłanie rozkazów do platformy, obiekt TextBox do wypisywania informacji zwrotnej.
		
		public Dac (Com com, System.Windows.Forms.TextBox consoleReq, System.Windows.Forms.TextBox consoleResp, mainForm form) 
		{
			NLform = form.NL;
			this.com = com;
			this.consoleResp = consoleResp;
			this.consoleReq = consoleReq;

		}
		
		//*****************************************************************************
		//Konwertuje wartość do postaci 12b próbki (2 znaki 1B) dla DAC
		//Zwraca rozkaz w postaci ciągu 2 znaków

		private string sampTo2Chars (int samp)
		{
			char L = Convert.ToChar (samp & MASKL); //Młodszy znak
			char H = Convert.ToChar ((samp >> 8) & MASKH); //Starszy znak
			return String.Format ("{0}{1}", H, L); //Składanie słowa
		}

		//-----------------------------------------------------------------------------
		//Konwetuje wartość do postaci 2 słów 1B
		//Zwraca rozkaz w postaci ciągu 2 znaków

		private string intTo2Chars (int x)
		{
			char L = Convert.ToChar (x & MASKL);
			char H = Convert.ToChar ((x >> 8) & MASKL);
			return string.Format ("{0}{1}", H, L);
		}

		//-----------------------------------------------------------------------------
		//Składa rozkaz dla trybu ustawiania pojedyńczej próbki, wypisuje wiadomość powrotną
		//w konsoli (TextBox)
		public int setSamp (int samp, bool returnADC)
		{
			consoleReq.Text += "Ustaw DAC: " + Convert.ToString(samp) + NLform;
			  
			if (returnADC) {
				string response = "", comand = "\x11" + "\x01" + sampTo2Chars (samp);
				consoleResp.Text += (response = com.sendStr(comand, true) + NLform);
				int pos = response.IndexOf("ADC:") + 5;
				response = response.Substring(pos);
				return Convert.ToInt32(response);
			}
			else {
				string comand = "\x11" + "\x02" + sampTo2Chars (samp);
				com.sendStr(comand, false);
				return -1;
			}
			
		}
		
		//-----------------------------------------------------------------------------
	
		public void setSamps (int delay, int[] arr, int len)
		{
			if (len == 0) return;
			//Wysylanie trybu sesji ilosci bajtow (l. probek*2) oraz opoznienia
			string comand = "\x12" + intTo2Chars(len * 2) + intTo2Chars(delay);
			byte x = Convert.ToByte(comand[4]);
			com.sendStr(comand, false);
			
			len--;
			for (int i = 0; i < len; i++) {
				com.sendSamp(sampTo2Chars(arr[i]));
				Thread.Sleep(3);
			}
			//Wysyłanie ostatniej próbki
			consoleResp.Text += com.sendStr(sampTo2Chars(arr[len]), true);
		
		}
	}
}
