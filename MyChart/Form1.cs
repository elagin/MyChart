using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MyChart
{
    public partial class Form1 : Form
    {
        //todo: https://habrahabr.ru/post/204308/
        public Form1()
        {
            InitializeComponent();
            draw();
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

            // Add data points to the series that have the specified X and Y values
            for (double t = 0; t <= (2.5 * Math.PI); t += Math.PI / 6)
            {
                double ch1 = Math.Sin(t);
                double ch2 = Math.Sin(t - Math.PI / 2);
                chart1.Series["Series1"].Points.AddXY(t, ch1);
                chart1.Series["Series2"].Points.AddXY(t, ch2);
            }
        }
    }
}
