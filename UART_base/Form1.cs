using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;
using System.Threading;

namespace UART_base
{
    public partial class Form1 : Form
    {
        

        public Form1()
        {
            InitializeComponent();
            cmdClose.Enabled = false; //dezaktywacja klawisza zamykajacego polaczenie
            cmdSend.Enabled = false;
            cmbBaudrate.SelectedIndex = 1;   //domyslanie aby uzytkownik nigdy nie zapomnial
            cmbParity.SelectedIndex = 0;
            cmbDatabits.SelectedIndex = 0;
            cmbStopbits.SelectedIndex = 0;
            foreach (string s in System.IO.Ports.SerialPort.GetPortNames())  //przypisuje do s znalezione porty i dodaje do panelu. jest tak dlugo az bedzie puste getport
            {
                txtPort.Items.Add(s);
            }  

           
        }

        public System.IO.Ports.SerialPort sport;  //spot to klasa, krotsza nazwa poprostu
        string InputData = String.Empty;
        internal delegate void SetTextDelegate(string Text);

        public void serialport_connect(string port, int baudRate, Parity parity, int dataBits, StopBits stopbits)
        {
            DateTime dt =  DateTime.Now; //pobranie aktualnej daty z struktury
            string dtn = dt.ToShortTimeString();
            sport = new System.IO.Ports.SerialPort(port,baudRate,parity,dataBits,stopbits); //nowy obiekt klasy o tych parametrach          

            try
            {
                sport.Open(); //otwarcie portu
                cmdOpen.Enabled = false; //zablokowanie przed ponownym otwarciem
                cmdClose.Enabled = true; //odblokowanie aby zamknac polaczenie
                cmdSend.Enabled = true;
                txtReceive.AppendText("[" + dtn + "] " + " Connected\n");                                 
                sport.DataReceived += new SerialDataReceivedEventHandler(sport_DataReceived); // handler do przerwania gdy nastapi nadejscie danych. datareceived tak samo jak w F12 napisane
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error ;(");
            }
        }

        private void sport_DataReceived(object sender, SerialDataReceivedEventArgs e) //gdy event zglosi ze cos przyszlo to przechodzi do tej metody
        {
            try
            {
                InputData = sport.ReadExisting();
                IAsyncResult result = this.BeginInvoke(new SetTextDelegate(SetText), new object[] { InputData }); //wywolanie asynchroniczne (DELEGAT, ARGUMENT)
                this.EndInvoke(result);
            }
            catch (Exception)
            {
                MessageBox.Show("error");
            }
          
        }

        private void SetText(string text)
        {
            this.txtReceive.AppendText(text);            
        }




        private void cmdOpen_Click(object sender, EventArgs e)
        {
            try
            {

                int baudrate = Convert.ToInt32(cmbBaudrate.Text);
                int databits = Convert.ToInt32(cmbDatabits.Text);
                Parity parity = (Parity)Enum.Parse(typeof(Parity), cmbParity.Text);
                string port = txtPort.Text;
                StopBits stopbits = (StopBits)Enum.Parse(typeof(StopBits), cmbStopbits.Text);

                serialport_connect(port, baudrate, parity, databits, stopbits);
                flControl.BackColor = System.Drawing.Color.DarkGreen;
            }
            catch (Exception)
            {
                MessageBox.Show("Check your settings\n Port can't be open", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);                   
                
            }
           

        }

        private void cmdSend_Click(object sender, EventArgs e)
        {
           try
            {

                DateTime dt = DateTime.Now;
                string dtn = dt.ToShortTimeString();
                string data = txtDatatoSend.Text;
                InputData = txtDatatoSend.Text;
                sport.Write(data);                
                IAsyncResult result = this.BeginInvoke(new SetTextDelegate(SetText), data);         
                this.EndInvoke(result);
                this.txtReceive.AppendText("\n");
 
            }
            catch (Exception)
            {
                MessageBox.Show("Port is close\n Can't send data", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);                  
            } 

           
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            DateTime dt = DateTime.Now;
            string dtn = dt.ToShortTimeString();

            if (sport.IsOpen)
            {
                sport.Close();
                cmdClose.Enabled = false;
                cmdOpen.Enabled = true;
                cmdSend.Enabled = false;
                txtReceive.AppendText("[" + dtn + "]" + " Disconnected\n");    
                flControl.BackColor = System.Drawing.Color.DarkRed;  //zmiana koloru kontrolki
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            txtReceive.Clear();
        }   
       
    }
}


