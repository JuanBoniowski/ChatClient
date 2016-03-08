using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.Xml;

namespace Client
{
    public partial class Main : Form
    {
        Socket sck;

        public Main()
        {
            InitializeComponent();
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            
            
            if (!sck.Connected)
            {
                sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                sck.Connect("172.16.3.100", 23);
                if (sck.Connected)
                    txtReceived.AppendText("YOU ARE NOW CONNECTED"+ Environment.NewLine);

                sck.BeginReceive(new byte[] { 0 }, 0, 0, 0, callback, null);
                sck.Send(Encoding.UTF8.GetBytes("Welcome " + txtName.Text));
                txtMsg.Focus();
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            sck.Send(Encoding.UTF8.GetBytes(txtName.Text + ">>> " + txtMsg.Text));
            
            txtMsg.Text = "";
            txtMsg.Focus();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            closeAll();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            closeAll();
        }

        void callback(IAsyncResult ar)
        {
            string msgHolder = null;

            try
            {
                if (sck.Available == 0)
                {

                }

                sck.EndReceive(ar);

                byte[] buf = new byte[8192];

                int rec = sck.Receive(buf); //, buf.Length, 0);

                if (rec < buf.Length)
                    Array.Resize<byte>(ref buf, rec);
                sck.BeginReceive(new byte[] { 0 }, 0, 0, 0, callback, null);

                xmlParser(Encoding.UTF8.GetString(buf));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private void txtMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int s = sck.Send(Encoding.UTF8.GetBytes(txtName.Text + ">>> " + txtMsg.Text));

                txtMsg.Text = "";
                txtMsg.Focus();
            }
        }

        private void xmlParser(string xmlString)
        { 

              XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);
                   Invoke((MethodInvoker)delegate
                {
                    lstUsers.Items.Clear();
                });
            ReadAllNodes(doc.SelectSingleNode("*"));

        }

        private void ReadAllNodes(XmlNode nodes)
        {

            if (nodes is XmlElement)
            {
                if (nodes.HasChildNodes)
                    ReadAllNodes(nodes.FirstChild);
                if (nodes.NextSibling != null)
                {
                    ReadAllNodes(nodes.NextSibling);
                }
            }

            else if (nodes is XmlText)
            {
                Invoke((MethodInvoker)delegate
                {
                    

                if (nodes.ParentNode.Name.ToString() == "MainMsg")
                {

                        txtReceived.AppendText(nodes.InnerText.ToString()+Environment.NewLine);
                }
                else if (nodes.ParentNode.Name.ToString().Equals("UserName"))
                {
                    if(!nodes.InnerText.ToString().Equals("NULL"))
                        lstUsers.Items.Add(nodes.InnerText.ToString());
                }
                     });
            }
            else if (nodes is XmlComment)
            { }
        }

        private void closeAll()
        {
            //send Msg to remove current user from list.
            sck.Send(Encoding.UTF8.GetBytes(txtName.Text + ">>> " + "DisconnectAllRightNow"));
            sck.Close();
            Invoke((MethodInvoker)delegate
            {
                txtReceived.AppendText("YOU ARE NOW DISCONNECTED");
            });
        }
    }
}
