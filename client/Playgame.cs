﻿using DTOProject.DTO;
using Guna.UI2.WinForms;
using MyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace client
{
    public partial class Playgame : Form
    {

        #region DEFINE
        const int NUMBER_CONF_ID = 5000;
        const string strIPAddress = "10.126.4.53";
        const int numPortVideo = 1333;
        const int numPortData = 5000;

        const int COUNTDOWN_TIME = 15;
        bool choosed = false;   //The player has chosen the answer = True else False; 
        int OrigTime = COUNTDOWN_TIME; // count down time
        string nameClient;
        NetComm.Client client; //The client object used for the communication
        Form currentChildForm = null;
        Random rd = new Random();

        #endregion
        public Playgame(string name)
        {

            InitializeComponent();
            pnlLeft.Width = this.Width - pnlRight.Width;
            //Connecting to the host
            pnlInforQuestion.Hide();
            nameClient = name;
        }
        public Playgame()
        {
            InitializeComponent();
            pnlLeft.Width = this.Width - pnlRight.Width;
        }

        #region Function

        void client_Connected()
        {
           
        }
        void client_Disconnected()
        {
           var result = MessageBox.Show("Disconnected from host!\nDo you want exit game?","Disconected",MessageBoxButtons.OK,MessageBoxIcon.Warning );
            if (result == DialogResult.OK) { this.Close(); }
            
        }

        void client_DataReceived(byte[] Data, string ID)
        {

            var data = Utils.ByteArrayToObject(Data);
            if(data is int)
            {
                connectStream((int)data, NUMBER_CONF_ID, strIPAddress, numPortVideo);
            }
            else if (data is DataChart)
            {
                updateChart((DataChart)data);
            }
            else if (data is Question)
            {
                choosed = false;
                pnlInforQuestion.Show();
                setQuestion((Question)data);
                enableButtonAnswer();
                tmrCountDown.Enabled = true;
                resetClock();
            }
            else if (data is AnswerCorrect)
            {
                showAnswerCorrect((AnswerCorrect)data);
            }
            else if (data is Dictionary<string, int>)
            {

                updateRank((Dictionary<string, int>)data);
            }
            else if (data is string)
            {
                string signal = (string)data;
                if (signal == "EndGame")
                {
                    showWinner(lvScores);
                }
                else
                {
                    npwBox.Number = int.Parse(signal);
                }
            }

        }
        public void showWinner(ListView lvScores)
        {
            if (lvScores.Items.Count == 1)
                OpenChildForm(new Winner(lvScores.Items[0].Text, "", ""));
            if (lvScores.Items.Count == 2)
                OpenChildForm(new Winner(lvScores.Items[0].Text, lvScores.Items[1].Text, ""));
            if (lvScores.Items.Count > 2)
                OpenChildForm(new Winner(lvScores.Items[0].Text, lvScores.Items[1].Text, lvScores.Items[2].Text));
        }

        public void updateChart(DataChart dataChart)
        {
            chartCountPlayerAnswer.Series["numberOfPlayerChoose"].Points.Clear();
            chartCountPlayerAnswer.Series["numberOfPlayerChoose"].Points.AddXY("A", dataChart.CountA);
            chartCountPlayerAnswer.Series["numberOfPlayerChoose"].Points.AddXY("B", dataChart.CountB);
            chartCountPlayerAnswer.Series["numberOfPlayerChoose"].Points.AddXY("C", dataChart.CountC);
            chartCountPlayerAnswer.Series["numberOfPlayerChoose"].Points.AddXY("D", dataChart.CountD);
        }

        public void updateRank(Dictionary<string, int> listScores)
        {
            lvScores.Items.Clear();
            foreach (var player in listScores)
            {

                ListViewItem item = new ListViewItem();
                item.Text = player.Key;
                item.SubItems.Add(player.Value.ToString());
                if (player.Key == nameClient) item.BackColor = Color.FromArgb(187, 225, 250);
                lvScores.Items.Add(item);

            }
        }

        public void showAnswerCorrect(AnswerCorrect answerCorrect)
        {
            foreach (var ctl in pnlAnwserGroup.Controls)
            {
                Guna2Button btn = (Guna2Button)ctl;
                if (btn.Text == answerCorrect.Content) btn.FillColor = Color.Lime;
            }
        }

        /// <summary>
        /// Set infor of question into controls
        /// </summary>
        /// <param name="question"></param>
        public void setQuestion(Question question)
        {
            int i = 0;
            lblQuestion.Text = question.Content;
            foreach (Control ctl in pnlAnwserGroup.Controls)
            {

                Guna2Button btn = (Guna2Button)ctl;
                btn.Text = question.listAnswer[i++];
            }
        }
        public void resetClock()
        {
            OrigTime = COUNTDOWN_TIME;
            proCountDown.Value = 0;
            lblCountDown.Text = OrigTime / 60 + ":" + ((OrigTime % 60) >= 10 ? (OrigTime % 60).ToString() : "0" + OrigTime % 60);
        }

        public void unableButtonAnswer()
        {
            foreach (var ctl in pnlAnwserGroup.Controls)
            {
                Guna2Button btn = (Guna2Button)ctl;
                btn.Click -= new System.EventHandler(this.btn_Click);
                btn.FillColor = Color.Gray;
            }
        }
        public void enableButtonAnswer()
        {
            foreach (var ctl in pnlAnwserGroup.Controls)
            {
                Guna2Button btn = (Guna2Button)ctl;
                btn.Click += new System.EventHandler(this.btn_Click);
                btn.FillColor = Color.FromArgb(0, 122, 204);
            }
        }


        public void connectStream(int userId,int confID,string strIPAddress,int numPortVideo)
        {
            axVideoChatReceiver1.ReceiveAudioStream = true;
            axVideoChatReceiver1.ReceiveVideoStream = true;
            axVideoChatReceiver1.VideoWindowAutoMax = true;
            axVideoChatReceiver1.ConferenceNumber = confID;
            axVideoChatReceiver1.ConferenceUserID = userId;
            axVideoChatReceiver1.Listen(strIPAddress, numPortVideo);
            return;
        }

        private void OpenChildForm(Form childForm)
        {
            //open only form
            if (currentChildForm != null)
            {
                currentChildForm.Close();
            }
            currentChildForm = childForm;
            //End
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Width = this.Width;
            childForm.Height = this.Height;
            childForm.Location = new Point(0, 0);
            this.Controls.Add(childForm);
            this.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        #endregion

        #region EVENT
        private void btn_Click(object sender, EventArgs e)
        {
            choosed = true; //Check The player has chosen the answer 
            unableButtonAnswer();
            Guna2Button btn = (Guna2Button)sender;
            btn.FillColor = Color.Red;
            string nameBtn = (btn).Name;
            client.SendData(Utils.ObjectToByteArray(nameBtn[nameBtn.Length - 1].ToString()));
        }
        private void Playgame_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (client.isConnected)
                client.Disconnect(); //Disconnects if the 
                                     //client is connected, closing the communication thread
        }

        private void tmrCountDown_Tick(object sender, EventArgs e)
        {
            OrigTime--;
            lblCountDown.Text = OrigTime / 60 + ":" + ((OrigTime % 60) >= 10 ? (OrigTime % 60).ToString() : "0" + OrigTime % 60);
            proCountDown.Value++;
            if (OrigTime <= 0)
            {
                tmrCountDown.Enabled = false;
                if (!choosed) unableButtonAnswer();
            }
        }
        private void Playgame_ClientSizeChanged(object sender, EventArgs e)
        {
            pnlLeft.Width = this.Width - pnlRight.Width;
            pnlInforQuestion.Location = new Point((pnlLeft.Width - pnlInforQuestion.Width) / 2, pnlLeft.Height - pnlInforQuestion.Height - 30);
            if (currentChildForm != null) currentChildForm.Size = this.Size;
            axVideoChatReceiver1.MP4Height = (short)axVideoChatReceiver1.Height;
            axVideoChatReceiver1.MP4Width = (short)axVideoChatReceiver1.Width;
        }

        private void btnClosed_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void Playgame_Load(object sender, EventArgs e)
        {
            client = new NetComm.Client(); //Initialize the client object
            //Adding event handling methods for the client
            client.Connected += new NetComm.Client.ConnectedEventHandler(client_Connected);
            client.Disconnected += new NetComm.Client.DisconnectedEventHandler(client_Disconnected);
            client.DataReceived += new NetComm.Client.DataReceivedEventHandler(client_DataReceived);
            client.Connect(strIPAddress, numPortData, nameClient); //Connecting to the host (on the same machine) with port 5000 and ID is variable name in contrustor function
        }

        private void Playgame_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (client.isConnected) client.Disconnect(); //Disconnects if the 
                                                         //client is connected, closing the communication thread
        }
        #endregion

    }
}
