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

		public Com com;
		System.Windows.Forms.TextBox console;
		const string NL = "\r\n";
		const byte MASKL =  0x00FF;
		const byte MASKH =  0x000F;
		
		//*****************************************************************************

		public Dac (Com com, System.Windows.Forms.TextBox console) 
		{
			this.com = com;
			this.console = console;
		}
		
		//*****************************************************************************
		
		private string sampTo2Chars (int samp)
		{
			char L = Convert.ToChar (samp & MASKL);
			char H = Convert.ToChar ((samp >> 8) & MASKH);
			return String.Format ("{0}{1}", H, L);
		}

		//-----------------------------------------------------------------------------
		
		private string intTo2Chars (int x)
		{
			char L = Convert.ToChar (x & MASKL);
			char H = Convert.ToChar ((x >> 8) & MASKL);
			return string.Format ("{0}{1}", H, L);
		}

		//-----------------------------------------------------------------------------

		public void setSamp (int samp)
		{
			string comand = "\x11" + sampTo2Chars (samp);
			console.Text += com.sendStr(comand, true) + NL;
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
			console.Text += com.sendStr(sampTo2Chars(arr[len]), true);
		
		}
	}
}
