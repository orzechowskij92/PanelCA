# PanelCA
This is source code of my diploma this is wrote in C# using MS Visual Studio 2015 IDE.

Whole project depends on communication via RS-232 (the basic functionality is stored in module PanelCA/Com.cs), 
which has to controll specified functions of digital-to-analog converter module (based on MCP4725 chip) conected to AVR Atmega 32 
microcontroller (PanelCA/Dac.cs).

All GUI interface's functionality related to port, saving and exporting data is stored in module PanelCA/mainForm.cs. 
Functionality related to DAC stering is stored in file PanelCA/mainForm.DAC.Designer.cs
