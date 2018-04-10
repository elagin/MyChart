using System;
using System.Data;
using System.Data.OleDb;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;

namespace MyChart
{
    public partial class Form1 : Form
    {
        SerialPort serialPort;
        readonly string DATE_FORMAT = "dd.MM.yyyy HH:mm:ss";
        readonly string dateFormatFileName = "yyyy-MM-dd-HH-mm-ss";
        private int iter = 0;
        private String fileName;

        //todo: https://habrahabr.ru/post/204308/
        public Form1()
        {
            InitializeComponent();
            this.FormClosed += new FormClosedEventHandler(this.MainForm_FormClosed);
            /*
                        int xCalibration = -28081;
                        int yCalibration = -23732;
                        int zCalibration = -12798;

                        int calibrationCount = 100;
                        int dx = xCalibration / calibrationCount;
                        int dy = yCalibration / calibrationCount;
                        int dz = zCalibration / calibrationCount;*/
            scanPorts();
        }

        private void loadData()
        {
            // Full path to the data source file
            string file = "DataFile.csv";
            string path = "";

            // Create a connection string.
            string ConStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + ";Extended Properties=\"Text;HDR=No;FMT=Delimited\"";
            OleDbConnection myConnection = new OleDbConnection(ConStr);

            // Create a database command on the connection using query
            string mySelectQuery = "Select * from " + file;
            OleDbCommand myCommand = new OleDbCommand(mySelectQuery, myConnection);

            // Open the connection and create the reader
            myCommand.Connection.Open();
            OleDbDataReader myReader = myCommand.ExecuteReader(CommandBehavior.CloseConnection);

            // Column 1 is a time value, column 2 is a double
            // databind the reader to the chart using the DataBindXY method
            chart1.Series[0].Points.DataBindXY(myReader, "1", myReader, "2");

            // Close connection and data reader
            myReader.Close();
            myConnection.Close();
        }

        private void draw()
        {
            // Set primary x-axis properties
            chart1.ChartAreas["ChartArea1"].AxisX.LabelStyle.Interval = Math.PI;
            chart1.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "##.##";
            chart1.ChartAreas["ChartArea1"].AxisX.MajorGrid.Interval = Math.PI;
            chart1.ChartAreas["ChartArea1"].AxisX.MinorGrid.Interval = Math.PI / 4;
            chart1.ChartAreas["ChartArea1"].AxisX.MinorTickMark.Interval = Math.PI / 4;
            chart1.ChartAreas["ChartArea1"].AxisX.MajorTickMark.Interval = Math.PI;
            chart1.ChartAreas["ChartArea1"].AxisY.MinorGrid.Interval = 0.25;
            chart1.ChartAreas["ChartArea1"].AxisY.MajorGrid.Interval = 0.5;
            chart1.ChartAreas["ChartArea1"].AxisY.LabelStyle.Interval = 0.5;
            /*
            // Add data points to the series that have the specified X and Y values
            for (double t = 0; t <= (2.5 * Math.PI); t += Math.PI / 6)
            {
                double ch1 = Math.Sin(t);
                double ch2 = Math.Sin(t - Math.PI / 2);
                chart1.Series["Series1"].Points.AddXY(t, ch1);
                chart1.Series["Series2"].Points.AddXY(t, ch2);
            }
            */
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            try
            {
                String comName = comboPorts.SelectedItem.ToString();
                serialPort = new SerialPort();
                serialPort.PortName = comName;
                serialPort.BaudRate = 9600;
                serialPort.ReadTimeout = 500;
                serialPort.WriteTimeout = 500;
                serialPort.Open();
                serialPort.DataReceived += serialPort2_DataReceived;
                buttonStart.Enabled = false;
                buttonStop.Enabled = true;
                buttonRescan.Enabled = false;

                if (checkBoxSaveFile.CheckState == CheckState.Checked)
                {
                    fileName = DateTime.Now.ToString(dateFormatFileName);
                }
                else
                {
                    fileName = null;
                }
            }
            catch (Exception ex)
            {
                serialPort = null;
                string caption = "Ошибка.";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result;
                result = MessageBox.Show(ex.Message, caption, buttons, MessageBoxIcon.Error);
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            closePort();
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
            buttonRescan.Enabled = true;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            closePort();
        }

        private void closePort()
        {
            if (serialPort != null)
            {
                serialPort.Close();
                serialPort = null;
            }
        }

        private void serialPort2_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                string pot = serialPort.ReadLine();
                BeginInvoke(new LineReceivedEvent(LineReceived), pot);
            }
            catch (Exception ex)
            {

            }
        }

        private delegate void LineReceivedEvent(string POT);

        int currentCount = 0;
        int calibrationCount = 100;
        int xCalibration = 0;
        int yCalibration = 0;
        int zCalibration = 0;

        readonly double K = 0.1;
        int newX, oldX;
        int newY, oldY;
        int newZ, oldZ;

        private void LineReceived(string line)
        {
            string[] values = line.Remove(line.Length - 1, 1).Split(':');
            int x;
            int y;
            int z;
            if (!Int32.TryParse(values[1], out x))
                Console.WriteLine("String could not be parsed.");
            if (!Int32.TryParse(values[2], out y))
                Console.WriteLine("String could not be parsed.");
            if (!Int32.TryParse(values[3], out z))
                Console.WriteLine("String could not be parsed.");

            if (currentCount < calibrationCount)
            {
                xCalibration = xCalibration + x;
                yCalibration = yCalibration + y;
                zCalibration = zCalibration + z;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"WriteLines.csv", true))
                {
                    /*
                    StringBuilder save = new StringBuilder();
                    save.Append(x);
                    save.Append(";");
                    save.Append(y);
                    save.Append(";");
                    save.Append(z);
                    save.Append(";");
                    save.Append(xCalibration);
                    save.Append(";");
                    save.Append(yCalibration);
                    save.Append(";");
                    save.Append(zCalibration);
                    file.WriteLine(save.ToString());
                    */
                    currentCount++;
                    if (currentCount == calibrationCount)
                    {
                        xCalibration = xCalibration / calibrationCount;
                        yCalibration = yCalibration / calibrationCount;
                        zCalibration = zCalibration / calibrationCount;
                        /*
                                                file.WriteLine("===============");
                                                StringBuilder save2 = new StringBuilder();
                                                save2.Append(xCalibration);
                                                save2.Append(";");
                                                save2.Append(yCalibration);
                                                save2.Append(";");
                                                save2.Append(zCalibration);
                                                file.WriteLine(save2.ToString());
                                                file.WriteLine("===============");
                        */
                    }
                }
            }
            else
            {
                int xPoint = x - xCalibration;
                int yPoint = y - yCalibration;
                int zPoint = z - zCalibration;

                //http://www.g0l.ru/blog/n3136
                // low-pass filter   
                int newX = Convert.ToInt32((xPoint * K) + (oldX * (1.0 - K)));
                int newY = Convert.ToInt32((yPoint * K) + (oldY * (1.0 - K)));
                int newZ = Convert.ToInt32((zPoint * K) + (oldZ * (1.0 - K)));

                oldX = newX;
                oldY = newY;
                oldZ = newZ;

                // high-pass filter   
                //newX = x - (x * K) + (oldX * (1.0 -  K));  
                //newY = y - (y * K) + (oldY * (1.0 -  K));  
                //newZ = z - (z * K) + (oldZ * (1.0 -  K));  


                if (fileName != null)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName + ".csv", true))
                    {
                        StringBuilder save = new StringBuilder();
                        String now = DateTime.Now.ToString(DATE_FORMAT);
                        save.Append(now);
                        save.Append(";");
                        save.Append(xPoint);
                        save.Append(";");
                        save.Append(yPoint);
                        save.Append(";");
                        save.Append(zPoint);
                        //save.Append(";");
                        //save.Append(newX);
                        //save.Append(";");
                        //save.Append(newY);
                        //save.Append(";");
                        //save.Append(newZ);
                        file.WriteLine(save.ToString());
                    }
                }
                chart1.Series["Name X"].Points.AddXY(iter, newX);
                chart1.Series["Name Y"].Points.AddXY(iter, newY);
                chart1.Series["Name Z"].Points.AddXY(iter, newZ);

                /*
                chart1.Series["Name X"].Points.AddXY(iter, xPoint);
                chart1.Series["Name Y"].Points.AddXY(iter, yPoint);
                chart1.Series["Name Z"].Points.AddXY(iter, zPoint);
                */
                iter++;
            }
        }

        private void buttonRescan_Click(object sender, EventArgs e)
        {
            scanPorts();
        }

        private void scanPorts()
        {
            comboPorts.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            for (int i = 0; i < ports.Length; i++)
            {
                comboPorts.Items.Add(ports[i]);
            }
            if (ports.Length > 0)
                comboPorts.SelectedIndex = 0;
            if (ports.Length == 1 || ports.Length == 0)
                comboPorts.Enabled = false;
        }
    }
}

