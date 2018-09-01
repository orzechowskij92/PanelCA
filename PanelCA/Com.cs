using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanelCA

{
	public class Com
	//Zawiera definijcje klasy obiektu portu szeregowego z jego funkcjonalnością
	//obsługującą niezbędne operacje I/O.

	{
		private SerialPort SP = new SerialPort("null", 9600); //obiekt portu COM(UART)
	
		public string[] availPorts; //dostępne porty
		public int portsCount = 0; //ilość portów
		public bool connected = false; //czy połączono
		private string mainPort = ""; //nazwa portu zss który połączono
		private const string HELLO  = "\x05"; //Znak trybu powitania

		//*****************************************************************************
	
		public Com()
		//Główny konstruktor obiektu portu, wyłącza sterownie pinów odpowiedzialnych za
		//kontrolę przepływu, przypisuje znak nowej lini dla portu oraz ustawia strone
		//kodową
		{
			//SP.ReadTimeout = 1000;
			SP.RtsEnable = false;
			SP.DtrEnable = false;
			
			SP.NewLine = "\r\n";
			SP.Encoding = Encoding.GetEncoding(28591);
		}
	
		//*****************************************************************************

		public int refreshPorts() 
		//pobiera dostępne nazwy portów, wykonuje próbe połączenia
		//z kompatybilną aparaturą.
		{
			availPorts = SerialPort.GetPortNames();
			portsCount = availPorts.Length;
			return portsCount;
			//TODO - Przeszukiwanie aparatury, jeśli wykryto - połącz
		}

		//-----------------------------------------------------------------------------

		public void connectTo(string port) 
		//Procedura wykonująca łączenie z wybranym portem
		{
			SP.PortName = mainPort = port;
			SP.Open();
			connected = SP.IsOpen;
		}

		//-----------------------------------------------------------------------------

		public void disconn()
		//Rozłączanie portu
		{
			SP.Close();
			connected = false;
		}

		//-----------------------------------------------------------------------------
	
		public string helloBoard()
		//Wysyłanie zapytania powitalnego do platformy
		{
			string response = sendStr(HELLO, true);
			
			return response;
		}

		//-----------------------------------------------------------------------------

		public string sendStr(string comand, bool response)
		//Wysyłanie ciągu znaków do platformy jako rozkaz(zakończone NL)
		//w zależności od parametru response pobierana i zwracana zostaje odpowiedź zwrotna
		{
			SP.WriteLine (comand);
			if (!response) return "";
			
			if (connected == false) return "N/C";
			
			//while (SP.ReadBufferSize == 0) System.Threading.Thread.Sleep(30);
			return SP.ReadLine();
			}
		
		//-----------------------------------------------------------------------------
		//Wysyłanie ciągu znaków do platformy jako dane(bez NL), nie oczekuje na odpowiedź
		public void sendSamp(string samp)
		{
			if (connected == false) return;
		
			SP.Write (samp);
		}
		
		//-----------------------------------------------------------------------------

		public string readWhenResponse()
		//Pobieranie odpowiedzi zwrotnej z platformy
		{
			while (SP.ReadBufferSize == 0);
			return SP.ReadLine();
		}
	}
}
