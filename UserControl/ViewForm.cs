using OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UserControl {

    public partial class ViewForm : Form {
        public delegate void AddDataDelegate();
        public AddDataDelegate textBoxDelegate, chartDelegate, quatDelegate;
        SerialPort bluetooth = new SerialPort();
        int loopCount = 0;
        bool opened = false;
        static string ucTime, osTime, quatStr, euler, measuresStr, ngLoops, ngFactor;
        private static System.Diagnostics.Stopwatch watch;
        float[] quat = new float[4];
        
        /* Charts data vectors */
        public double[] elapsedTime = new double[30];
        public double[] yaw = new double[30];
        public double[] pitch = new double[30];
        public double[] roll = new double[30];
        public double[] accX = new double[30];
        public double[] accY = new double[30];
        public double[] accZ = new double[30];
        public double[] gyroX = new double[30];
        public double[] gyroY = new double[30];
        public double[] gyroZ = new double[30];
        public double[] magX = new double[30];
        public double[] magY = new double[30];
        public double[] magZ = new double[30];
        public double[] intGyroX = new double[30];
        public double[] intGyroY = new double[30];
        public double[] intGyroZ = new double[30];
        
        public ViewForm() {
            InitializeComponent();
        }

        static void Main() {
            Application.Run(new ViewForm());
        }
        
        private void button1_Click(object sender, EventArgs e) {
            OpenGLDraw openGLDraw = new OpenGLDraw();
            Thread openGLThread = new Thread(openGLDraw.View);
            openGLThread.Start();
        }

        private void button2_Click(object sender, EventArgs e) {
            SerialInit();
        }

        private void button3_Click(object sender, EventArgs e) {
            bluetooth.Close();
            opened = false;
            loopCount = 0;
        }

        public void SerialInit() {
            if (!bluetooth.IsOpen) {
                opened = true;
                bluetooth.PortName = "COM5";
                bluetooth.DataBits = 8;
                bluetooth.Handshake = Handshake.None;
                bluetooth.ReadTimeout = 5000;
                bluetooth.ReceivedBytesThreshold = 88;
                bluetooth.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

                textBoxDelegate = new AddDataDelegate(AddDataToTextBoxMethod);
                chartDelegate = new AddDataDelegate(AddDataToChartDelegate);
                quatDelegate = new AddDataDelegate(UpdateQuaternion);

                try {
                    bluetooth.Open();
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.ToString());
                    return;
                }
            }
            
            watch = System.Diagnostics.Stopwatch.StartNew();
        }

        public void AddDataToTextBoxMethod() {
            textBox4.AppendText(quatStr);
            textBox4.AppendText("\n");
            textBox2.AppendText(osTime);
            textBox2.AppendText("\n");
            textBox3.AppendText(ucTime);
            textBox3.AppendText("\n");
            textBox1.AppendText(euler);
            textBox1.AppendText("\n");
            measuresText.AppendText(measuresStr);
            ngLoopsTextBox.AppendText(ngLoops);
            ngLoopsTextBox.AppendText("\n");
            ngFactorTextBox.AppendText(ngFactor);
            ngFactorTextBox.AppendText("\n");
        }

        public void AddDataToChartDelegate(){
            /* chartEuler */
            chartEuler.ChartAreas[0].AxisX.LabelStyle.Format = "##.##";
            chartEuler.ChartAreas[0].AxisX.Title = "Time [s]";
            chartEuler.ChartAreas[0].AxisY.Title = "Yaw, Pitch e Roll [°]";
            chartEuler.Series["Yaw"].Points.Clear();
            chartEuler.Series["Pitch"].Points.Clear();
            chartEuler.Series["Roll"].Points.Clear();

            /* chartAcc */
            chartAcc.ChartAreas[0].AxisX.LabelStyle.Format = "##.##";
            chartAcc.ChartAreas[0].AxisX.Title = "Time [s]";
            chartAcc.ChartAreas[0].AxisY.Title = "Measurements [g]";
            chartAcc.Series["accX"].Points.Clear();
            chartAcc.Series["accY"].Points.Clear();
            chartAcc.Series["accZ"].Points.Clear();

            /* chartGyro */
            chartGyro.ChartAreas[0].AxisX.LabelStyle.Format = "##.##";
            chartGyro.ChartAreas[0].AxisX.Title = "Time [s]";
            chartGyro.ChartAreas[0].AxisY.Title = "Measurements [rad/s]";
            chartGyro.Series["gyroX"].Points.Clear();
            chartGyro.Series["gyroY"].Points.Clear();
            chartGyro.Series["gyroZ"].Points.Clear();

            /* chartMag */
            chartMag.ChartAreas[0].AxisX.LabelStyle.Format = "##.##";
            chartMag.ChartAreas[0].AxisX.Title = "Time [s]";
            chartMag.ChartAreas[0].AxisY.Title = "Measurements [uT]";
            chartMag.Series["magX"].Points.Clear();
            chartMag.Series["magY"].Points.Clear();
            chartMag.Series["magZ"].Points.Clear();
            
            /* integratedGyro */
            chartIntGyro.ChartAreas[0].AxisX.LabelStyle.Format = "##.##";
            chartIntGyro.ChartAreas[0].AxisX.Title = "Time [s]";
            chartIntGyro.ChartAreas[0].AxisY.Title = "Integrated gyro [°]";
            chartIntGyro.Series["gyroIntX"].Points.Clear();
            chartIntGyro.Series["gyroIntY"].Points.Clear();
            chartIntGyro.Series["gyroIntZ"].Points.Clear();


            for (int i = 0; i < roll.Length; i++) {
                /* chartEuler */
                chartEuler.Series["Yaw"].Points.AddXY(elapsedTime[i], yaw[i]);
                chartEuler.Series["Pitch"].Points.AddXY(elapsedTime[i], pitch[i]);
                chartEuler.Series["Roll"].Points.AddXY(elapsedTime[i], roll[i]);

                /* chartAcc */
                chartAcc.Series["accX"].Points.AddXY(elapsedTime[i], accX[i]);
                chartAcc.Series["accY"].Points.AddXY(elapsedTime[i], accY[i]);
                chartAcc.Series["accZ"].Points.AddXY(elapsedTime[i], accZ[i]);

                /* chartGyro */
                chartGyro.Series["gyroX"].Points.AddXY(elapsedTime[i], gyroX[i]);
                chartGyro.Series["gyroY"].Points.AddXY(elapsedTime[i], gyroY[i]);
                chartGyro.Series["gyroZ"].Points.AddXY(elapsedTime[i], gyroZ[i]);

                /* chartMag */
                chartMag.Series["magX"].Points.AddXY(elapsedTime[i], magX[i]);
                chartMag.Series["magY"].Points.AddXY(elapsedTime[i], magY[i]);
                chartMag.Series["magZ"].Points.AddXY(elapsedTime[i], magZ[i]);

                /* chartIntGyro */
                chartIntGyro.Series["gyroIntX"].Points.AddXY(elapsedTime[i], intGyroX[i]);
                chartIntGyro.Series["gyroIntY"].Points.AddXY(elapsedTime[i], intGyroY[i]);
                chartIntGyro.Series["gyroIntZ"].Points.AddXY(elapsedTime[i], intGyroZ[i]);
            }

        }

        public void UpdateQuaternion() {
            UserControl.OpenGLDraw.q = new Quaternion(quat[0], quat[1], quat[2], quat[3]);
        }
                
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e) {
            watch.Stop();

            SerialPort sp = (SerialPort)sender;
            byte[] buffer = new byte[88];

            if (sp.BytesToRead < 88)
                return;

            try {

                /* Get date from serial buffer */
                for (int i = 0; i < 88; i++)
                    buffer[i] = (byte)sp.ReadByte();

                /* Update graph data vectors */
                int length = roll.Length;

                if (loopCount == 0) {
                    sp.DiscardInBuffer();
                    loopCount++;
                    return;
                }

                if (loopCount < length) {

                    loopCount++;

                    yaw[loopCount - 1] = BitConverter.ToSingle(buffer, 52);
                    pitch[loopCount - 1] = BitConverter.ToSingle(buffer, 56);
                    roll[loopCount - 1] = BitConverter.ToSingle(buffer, 60);

                    elapsedTime[loopCount - 1] =  (BitConverter.ToSingle(buffer, 64))*loopCount;
                    
                    euler = "  R: ";
                    euler += roll[loopCount - 1].ToString("F2", CultureInfo.InvariantCulture) + "      ";
                    euler += "P: ";
                    euler += pitch[loopCount - 1].ToString("F2", CultureInfo.InvariantCulture) + "      ";
                    euler += "Y: ";
                    euler += yaw[loopCount - 1].ToString("F2", CultureInfo.InvariantCulture) + " ";

                    accX[loopCount - 1] = BitConverter.ToSingle(buffer, 16);
                    accY[loopCount - 1] = BitConverter.ToSingle(buffer, 20);
                    accZ[loopCount - 1] = BitConverter.ToSingle(buffer, 24);
                    gyroX[loopCount - 1] = BitConverter.ToSingle(buffer, 28);
                    gyroY[loopCount - 1] = BitConverter.ToSingle(buffer, 32);
                    gyroZ[loopCount - 1] = BitConverter.ToSingle(buffer, 36);
                    magX[loopCount - 1] = BitConverter.ToSingle(buffer, 40);
                    magY[loopCount - 1] = BitConverter.ToSingle(buffer, 44);
                    magZ[loopCount - 1] = BitConverter.ToSingle(buffer, 48);
                    intGyroX[loopCount - 1] = BitConverter.ToSingle(buffer, 68);
                    intGyroY[loopCount - 1] = BitConverter.ToSingle(buffer, 72);
                    intGyroZ[loopCount - 1] = BitConverter.ToSingle(buffer, 76);
                }
                else {
                    /* Move data vectors */
                    for (int i = 0; i < length - 1; i++) {
                        roll[i] = roll[i + 1];
                        pitch[i] = pitch[i + 1];
                        yaw[i] = yaw[i + 1];
                        elapsedTime[i] = elapsedTime[i + 1];
                        accX[i] = accX[i + 1];
                        accY[i] = accY[i + 1];
                        accZ[i] = accZ[i + 1];
                        gyroX[i] = gyroX[i + 1];
                        gyroY[i] = gyroY[i + 1];
                        gyroZ[i] = gyroZ[i + 1];
                        magX[i] = magX[i + 1];
                        magY[i] = magY[i + 1];
                        magZ[i] = magZ[i + 1];
                        intGyroX[i] = intGyroX[i + 1];
                        intGyroY[i] = intGyroY[i + 1];
                        intGyroZ[i] = intGyroZ[i + 1];
                    }

                    /* Update values */
                    yaw[length - 1] = BitConverter.ToSingle(buffer, 52);
                    pitch[length - 1] = BitConverter.ToSingle(buffer, 56);
                    roll[length - 1] = BitConverter.ToSingle(buffer, 60);

                    elapsedTime[length - 1] += (BitConverter.ToSingle(buffer, 64));
                    
                    euler = "  R: ";
                    euler += roll[length - 1].ToString("F2", CultureInfo.InvariantCulture) + "      ";
                    euler += "P: ";
                    euler += pitch[length - 1].ToString("F2", CultureInfo.InvariantCulture) + "      ";
                    euler += "Y: ";
                    euler += yaw[length - 1].ToString("F2", CultureInfo.InvariantCulture) + " ";
                    
                    accX[length - 1] = BitConverter.ToSingle(buffer, 16);
                    accY[length - 1] = BitConverter.ToSingle(buffer, 20);
                    accZ[length - 1] = BitConverter.ToSingle(buffer, 24);
                    gyroX[length - 1] = BitConverter.ToSingle(buffer, 28);
                    gyroY[length - 1] = BitConverter.ToSingle(buffer, 32);
                    gyroZ[length - 1] = BitConverter.ToSingle(buffer, 36);
                    magX[length - 1] = BitConverter.ToSingle(buffer, 40);
                    magY[length - 1] = BitConverter.ToSingle(buffer, 44);
                    magZ[length - 1] = BitConverter.ToSingle(buffer, 48);
                    intGyroX[length - 1] = BitConverter.ToSingle(buffer, 68);
                    intGyroY[length - 1] = BitConverter.ToSingle(buffer, 72);
                    intGyroZ[length - 1] = BitConverter.ToSingle(buffer, 76);
                }
                
                /* Update strings */
                osTime = (((float)watch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency) * 1000).ToString("F2", CultureInfo.InvariantCulture) + "ms";
                ucTime = (BitConverter.ToSingle(buffer, 64) * 1000).ToString("F2", CultureInfo.InvariantCulture) + "ms";

                quat[0] = BitConverter.ToSingle(buffer, 0);
                quat[1] = BitConverter.ToSingle(buffer, 4);
                quat[2] = BitConverter.ToSingle(buffer, 8);
                quat[3] = BitConverter.ToSingle(buffer, 12);
                quatStr = "  [";
                for (int k = 0; k < 16; k += 4) {
                    quatStr += quat[k/4].ToString("F2", CultureInfo.InvariantCulture);
                    if (k < 12) {
                        quatStr += ", ";
                    }
                }
                quatStr += "]";


                measuresStr = "acc = [ ";
                for (int k = 16; k < 28; k += 4) {
                    measuresStr += BitConverter.ToSingle(buffer, k).ToString("F2", CultureInfo.InvariantCulture) + "  ";
                }
                measuresStr += " ]  ";

                measuresStr += "gyro = [ ";
                for (int k = 28; k < 40; k += 4) {
                    measuresStr += BitConverter.ToSingle(buffer, k).ToString("F2", CultureInfo.InvariantCulture) + "  ";
                }
                measuresStr += " ]   ";

                measuresStr += "mag = [ ";
                //measuresStr = "";
                for (int k = 40; k < 52; k += 4) {
                    measuresStr += BitConverter.ToSingle(buffer, k).ToString("F2", CultureInfo.InvariantCulture) + "  ";
                }
                measuresStr += " ] ";
                measuresStr += "\r\n";

                ngLoops = BitConverter.ToSingle(buffer, 80).ToString("F0", CultureInfo.InvariantCulture);
                ngFactor = BitConverter.ToSingle(buffer, 84).ToString("F2", CultureInfo.InvariantCulture);

                watch.Reset();
                watch.Restart();

                sp.DiscardInBuffer();
                Invoke(this.textBoxDelegate);
                Invoke(this.chartDelegate);
                Invoke(this.quatDelegate);
            }

            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
                //sp.DiscardInBuffer();
            }
        }
    }
}
