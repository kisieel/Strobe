using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using USBClassLibrary;
using System.Threading;
using System.Resources;

namespace Brama_soft
{
    public partial class Form1 : Form
    {

        SerialPort port;
        private USBClass USBPort;
        private USBClass.DeviceProperties USBDeviceProperties;
        public Form1()
        {
            InitializeComponent();

            USBPort = new USBClass();
            USBDeviceProperties = new USBClass.DeviceProperties();
        }   

        
        private void button9_Click(object sender, EventArgs e)
        {
            if (USBClass.GetUSBDevice(0x16C0, 0x05E1, ref USBDeviceProperties, true))
            {

                if (port != null && port.IsOpen)
                    port.Close();

                port = new SerialPort(USBDeviceProperties.COMPort);
                port.BaudRate = 9600;
                port.Parity = Parity.None;
                port.DataBits = 8;
                port.StopBits = StopBits.One;
                port.Handshake = Handshake.None;
                port.Open();

            }
        }

        Thread testThread;

        private void button1_Click(object sender, EventArgs e)
        {
            if (testThread != null && testThread.IsAlive)
                testThread.Abort();

            iSleep = (int)numericUpDown1.Value;
            testThread = new Thread(new ThreadStart(Test));
            testThread.IsBackground = true;
            testThread.Start();
        }

        int iSleep = 0;
        private void Test()
        {
            for (int i = 0; i < 8; i++)
            {
                SetBulb(pictureBox2, "checkBox" + (i + 1).ToString());
                SetBulb(pictureBox1, "checkBox" + (16 - i).ToString());

                Thread.Sleep(iSleep);
            }


            testThread.Abort();
        }



        private delegate void SetBulbDelegate(PictureBox p,  string str);
        private void SetBulb(PictureBox p,string str)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SetBulbDelegate(SetBulb), new object[] { p, str });
                return;
            }
            else
            {
                bool b = false;

                foreach (Control c in this.Controls)
                {
                    if (c is CheckBox && c.Name == str)
                    {
                       b = (((CheckBox)c).Checked);
                    }
                }


                p.Image = b ? Strobe_soft.Properties.Resources._1372723693_light_bulb_basic_yellow : Strobe_soft.Properties.Resources._1372723697_light_bulb_sticker;
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (testThread != null && testThread.IsAlive)
                testThread.Abort();
        }
    }
}
