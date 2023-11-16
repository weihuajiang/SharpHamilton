using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Huarui.STARLine.ThirdParty
{
    /// <summary>
    /// Metrologic barcode scanner Orbit/MS7120 for read operations over the serial Interface RS232.
    /// </summary>
    public class OrbitBarcodeReader
    {
        private OrbitBarcodeReader()
        {

        }
        SerialPort reader;
        void OpenComPort(string comPort, int timeout=2000)
        {
            if (reader != null)
            {
                if (reader.IsOpen)
                    reader.Close();
                reader = null;
            }
            reader = new SerialPort(comPort, 9600, Parity.Space, 7, StopBits.One);
            reader.Handshake = Handshake.RequestToSend;
            reader.NewLine = "\r\n";
            reader.Encoding = Encoding.ASCII;
            reader.ReadTimeout = timeout;
            reader.Open();
        }
        /// <summary>
        /// Open the serial port for barcode reader
        /// </summary>
        /// <param name="comPort">Serial port to be used (string), 
        /// The Metrologic barcode scanner Orbit/MS7120 may be plugged on one of the following ports: "COM1", "COM2", "COM3", "COM4"
        /// Default ComPort setting: COM2</param>
        /// <param name="timeout">timeout for serial port reading</param>
        public static OrbitBarcodeReader Open(string comPort, int timeout = 2000)
        {
            OrbitBarcodeReader orbit = new OrbitBarcodeReader();
            orbit.OpenComPort(comPort, timeout);
            return orbit;
        }
        /// <summary>
        /// Close the barcode reader
        /// </summary>
        public void Close()
        {
            if (reader != null)
            {
                if (reader.IsOpen)
                    reader.Close();
                reader = null;
            }
        }
        /// <summary>
        /// Get read barcode from Metrologic barcode scanner Orbit/MS7120.
        /// </summary>
        /// <returns>barcode string</returns>
        public string Read()
        {
            string bc= reader.ReadLine();
            bc = bc.Trim();
            return bc;
        }
        /// <summary>
        /// Deletes the input buffer of the used serial port
        /// </summary>
        public void DelComBuffer()
        {
            reader.ReadExisting();
        }
    }
}
