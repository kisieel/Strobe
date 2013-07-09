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
            if (USBClass.GetUSBDevice(0x03EB, 0x2044, ref USBDeviceProperties, true))
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

                byte b1 = 0, b2 = 0;

                if (checkBox1.Checked) b1 |= (1 << 0);
                if (checkBox2.Checked) b1 |= (1 << 1);
                if (checkBox3.Checked) b1 |= (1 << 2);
                if (checkBox4.Checked) b1 |= (1 << 3);
                if (checkBox5.Checked) b1 |= (1 << 4);
                if (checkBox6.Checked) b1 |= (1 << 5);
                if (checkBox7.Checked) b1 |= (1 << 6);
                if (checkBox8.Checked) b1 |= (1 << 7);

                if (checkBox16.Checked) b2 |= (1 << 0);
                if (checkBox15.Checked) b2 |= (1 << 1);
                if (checkBox14.Checked) b2 |= (1 << 2);
                if (checkBox13.Checked) b2 |= (1 << 3);
                if (checkBox12.Checked) b2 |= (1 << 4);
                if (checkBox11.Checked) b2 |= (1 << 5);
                if (checkBox10.Checked) b2 |= (1 << 6);
                if (checkBox9.Checked) b2 |= (1 << 7);

                port.Write(new byte[] { 0x00, b2, b1, (byte)((int)numericUpDown1.Value >> 8), (byte)((int)numericUpDown1.Value & 0xff) }, 0, 5);

                port.Close();

                MessageBox.Show("Schemat został wgrany!");

            }
            else
                MessageBox.Show("Brak urządzenia!");
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
            for(int ii = 0; ii < 2; ii++)
                for (int i = 0; i < 8; i++)
                {
                    SetBulb(pictureBox2, "checkBox" + (i + 1).ToString());
                    SetBulb(pictureBox1, "checkBox" + (16 - i).ToString());

                    Thread.Sleep(iSleep);
                }

            ClearBulb();

            testThread.Abort(); 
        }

        private delegate void ClearBulbDelegate();
        private void ClearBulb()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new ClearBulbDelegate(ClearBulb), new object[] {  });
                return;
            }
            else
            {
                pictureBox1.Image = null;
                pictureBox2.Image = null;
            }
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
