using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace COG.Device.LightCtrl
{
    public class LvsLightCtrl
    {
        public int Comport { get; set; } = 1;

        private SerialPort _serialPort { get; set; } = null;

        public void Initialize(string portName, int baudRate)
        {
            _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
        }

        public bool Open()
        {
            try
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();

                _serialPort.Open();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Source + err.Message + err.StackTrace);
                return false;
            }

            return true;
        }

        public void LightOnOff()
        {

        }
    }
}
