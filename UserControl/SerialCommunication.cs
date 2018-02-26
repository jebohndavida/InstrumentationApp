using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UserControl {
    public class SerialCommunication {
        static string time, quat, measures, avgTimeStr = "Not available yet";
        static int count = 0;
        static float avgTime, totalTime = 0;
        SerialPort serialPort = new SerialPort();
        static bool interFlag = false;
        //static TextBox box = new TextBox();
        static System.Diagnostics.Stopwatch watchKalman;
        
        public void Init(string com) {
            serialPort.PortName = "COM8";
            serialPort.BaudRate = 115200;
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;
            serialPort.DataBits = 8;
            serialPort.Handshake = Handshake.None;
            serialPort.RtsEnable = true;
            serialPort.ReadBufferSize = 52;
            //Config receive interrupt
            serialPort.ReceivedBytesThreshold = 52;
            serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

        }

        public void Start() {
            serialPort.Open();
        }

        public void ResquestStop() {
            serialPort.Close();
        }

        public string GetMessage() {
            return "nothing";
        }

        public void Loop(TextBox textBox1, TextBox textBox2) {

            watchKalman = System.Diagnostics.Stopwatch.StartNew();
            while (true) {
                if (interFlag) {
                    textBox1.AppendText(time);
                    textBox1.AppendText("\t");
                    textBox1.AppendText(quat);
                    textBox1.AppendText("\t");
                    textBox1.AppendText(measures);
                    textBox1.AppendText("\n");
                    textBox2.AppendText("\n");
                    textBox2.Text = avgTimeStr;
                    interFlag = false;
                    Thread currentThread = Thread.CurrentThread;
                    currentThread.Priority = ThreadPriority.Lowest;
                    //Thread.Sleep(1000);
                }
            }
        }

        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e) {
            
            watchKalman.Stop();

            SerialPort sp = (SerialPort)sender;
            byte[] buffer = new byte[52];
            float aux = 0;

            count++;
            
            aux = ((float)watchKalman.ElapsedTicks * 1000 / System.Diagnostics.Stopwatch.Frequency);
            time = "Elapsed Time: " + aux.ToString("F4", CultureInfo.InvariantCulture); //nTicks*T

            totalTime += aux;
            if (count == 30) {
                avgTime = totalTime / 30;
                avgTimeStr = avgTime.ToString();
                count = 0;
                totalTime = 0;
            }

            for (int i = 0; i < 52; i++) {
                buffer[i] = (byte)sp.ReadByte();
            }
            
            quat = "q = [  ";
            for (int k = 0; k < 16; k += 4) {
                quat += BitConverter.ToSingle(buffer, k).ToString("F2", CultureInfo.InvariantCulture) + "  ";
            }
            quat += "  ]";

            measures = "m = [  ";
            for (int k = 16; k < 52; k += 4) {
                measures += BitConverter.ToSingle(buffer, k).ToString("F2", CultureInfo.InvariantCulture) + "  ";
            }
            measures += "  ]";

            sp.DiscardInBuffer();

            interFlag = true;
            watchKalman = System.Diagnostics.Stopwatch.StartNew();
        }
    }
}


/* Time analyses
 * Serial communication with baud rate of 115200:
 * Sending n bits results in a minimum period between reads of:
 * t = n*8/115200 (s), n=20 -> 20*8/115200 ~ 1.38ms 
 * The minimum Elapsed ucTime got between interrupts without reading errors was 1.9~2.1ms
 * utilizing FTDI converter. 
 * Utilizing Bluetooth communication, the emmulated COM port runs at maximum speed possible and 
 * the Elapsed ucTime varies since 0.6ms up to 3ms
 * Print on the window the maximum and minimum Elapsed times
 */