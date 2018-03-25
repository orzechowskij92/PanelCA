using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanelCA

{
	public class Com
	//Zawiera obiekt portu szeregowego wraz z dodatkowym oprogramowaniem
	//obsługującym niezbędne operacje I/O.
		
	{
		//*****************************************************************************

		public SerialPort SP = new SerialPort("null", 9600); //obiekt portu COM(UART)
	
		public string[] availPorts; //dostępne porty
		public int portsCount = 0; //ilość portów
		public bool connected = false; //czy połączono
		public string mainPort = ""; //nazwa portu zss który połączono
		const string HELLO  = "\x05"; //Znak trybu powitania
	
		public Com()
		{
			SP.NewLine = "\r\n";
			SP.Encoding = Encoding.GetEncoding(28591);
		}
	
		//*****************************************************************************

		public int refreshPorts() 
		{
			//pobiera dostępne nazwy portów, wykonuje próbe połączenia
			//z kompatybilną aparaturą.
			availPorts = SerialPort.GetPortNames();
			portsCount = availPorts.Length;
			return portsCount;
			//Przeszukiwanie aparatury, jeśli wykryto - połącz
		}

		//-----------------------------------------------------------------------------

		public void connectTo(string port) 
		{
			SP.PortName = mainPort = port;
			SP.Open();
			connected = SP.IsOpen;
		}
		
		//-----------------------------------------------------------------------------

		public bool isConect()
		{
			return connected = SP.IsOpen;
		}

		//-----------------------------------------------------------------------------

		public void disconn()
		{
			SP.Close();
			connected = false;
		}

		//-----------------------------------------------------------------------------
	
		public string helloBoard()
		{
			return sendStr(HELLO, true);
		}

		//-----------------------------------------------------------------------------

		public string sendStr(string comand, bool response)
		{
			if (connected == false) return comand;
		
			SP.WriteLine (comand);
			if (!response) return "";
			
			while (SP.ReadBufferSize == 0) System.Threading.Thread.Sleep(30);
			return SP.ReadLine();
			}
		
		//-----------------------------------------------------------------------------

		public void sendSamp(string samp)
		{
			if (connected == false) return;
		
			SP.Write (samp);
		}
		
	//*****************************************************************************
	}
}
