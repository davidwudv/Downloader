using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;

namespace Downloader
{
    
    public partial class Form_Download : Form
    {
        private DownloadTask downloadTask;
        private delegate void UpdateProgressDelegate(DownloadTask mytask, double speed);
        private Thread updateProgressThread;
        private List<Thread> HelpDownloadThreadList;
        public List<IPAddress> OtherMachineIP;
        private string ServerUserName;
        private string ServerPassword;
#if DEBUG
        private delegate void OutPutDelegate(string output);
        OutPutDelegate OutPutPrint;
#endif

        public Form_Download()
        {
            InitializeComponent();
            HelpDownloadThreadList = new List<Thread>();
            OtherMachineIP = new List<IPAddress>();
            listBox_IPList.Items.Add("172.18.175.247");
            OtherMachineIP.Add(IPAddress.Parse("172.18.175.247"));
#if DEBUG
            OutPutPrint = PrintToListBox;
#endif
        }

        private void button_Start_Click(object sender, EventArgs e)
        {
            if (downloadTask != null)
            {
                MessageBox.Show("暂不支持多任务同时下载！");
                return;
            }
            TaskConfig myTaskConfig;
            int result = Exist(textBox_Path.Text);
            if (result == 0)
            {
                myTaskConfig = new TaskConfig();
                myTaskConfig.Load(textBox_Path.Text + ".conf");
            }
            else if (result == 1)
            {
                MessageBox.Show("此文件已经下载过了！");
                return;
            }
            else
            {
                int threads = 0;
                if (checkBox_Prox.Checked)
                    threads = listBox_IPList.Items.Count;
                else if (Int32.Parse(textBox_Threads.Text) > listBox_IPList.Items.Count)
                    threads = Int32.Parse(textBox_Threads.Text);
                else
                    threads = 1 + listBox_IPList.Items.Count;
                myTaskConfig = new TaskConfig(new Uri(textBox_Url.Text), threads);
                if (ServerPassword != null && ServerUserName != null && ServerPassword.Length > 0 && ServerUserName.Length > 0)
                {
                    myTaskConfig.UserName = ServerUserName;
                    myTaskConfig.Password = ServerPassword;
                }
                myTaskConfig.InitConfig();
            }
            downloadTask = new DownloadTask(textBox_Path.Text, myTaskConfig, this);
            downloadTask.Start();
            updateProgressThread = new Thread(SetProgress);
            updateProgressThread.Start();

            button_Start.Enabled = false;
            button_Stop.Enabled = true;
            button_AddIP.Enabled = false;
            button_DeleteIP.Enabled = false;
            textBox_AddIP.Enabled = false;
            textBox_Path.Enabled = false;
            textBox_Threads.Enabled = false;
            textBox_Url.ReadOnly = true;
            checkBox_Prox.Enabled = false;
        }

        private void button_Stop_Click(object sender, EventArgs e)
        {
            if (downloadTask != null)
            {
                if (downloadTask.Config.SupportResume)
                {
                    downloadTask.SetStop();
                    updateProgressThread.Abort();
                    downloadTask = null;
                }
                else
                {
                    ModleForm modileForm = new ModleForm();
                    if (modileForm.ShowDialog(this) == DialogResult.Yes)
                    {
                        downloadTask.SetStop();
                        updateProgressThread.Abort();
                        downloadTask = null;
                    }
                    else
                        return;
                }
                
            }
            button_Start.Enabled = true;
            button_Stop.Enabled = false;
            button_AddIP.Enabled = true;
            button_DeleteIP.Enabled = true;
            textBox_AddIP.Enabled = true;
            textBox_Path.Enabled = true;
            textBox_Url.ReadOnly = false;
            checkBox_Prox.Enabled = true;
            if(!checkBox_Prox.Checked)
                textBox_Threads.Enabled = true;
        }

        /************************************************************************/
        /* 返回0表示此文件曾经下载过但未下载完成；    */
        /* 返回1表示此文件曾经下载过且已经下载完成；*/
        /* 返回-1表示此文件从未下载过；                        */
        /************************************************************************/
        private int Exist(string strPath)
        {
            if (File.Exists(strPath))
            {
                if (File.Exists(strPath + ".conf"))
                    return 0;
                else
                    return 1;
            }
            return -1;
        }

        private void SetProgress()
        {
            try
            {
                if (this.InvokeRequired)
                {
                    long a = 0;//1.8秒前的已下载大小
                    long b = 0;//1.8秒后的已下载大小
                    double speed = 0;//下载速度
                    while (downloadTask.State == DownloadState.Downloading)
                    {
                        if (b > 0)
                        {
                            speed = (b - a) / 1000 / 1.6;//0.2是下载线程的睡眠时间造成的误差值
                        }
                        a = downloadTask.Config.SumDownloadedSize;
                        UpdateProgressDelegate updateProgress = new UpdateProgressDelegate(UpdateProgress);
                        progressBar_Download.BeginInvoke(updateProgress, downloadTask, speed);
                        Thread.Sleep(1800);
                        b = downloadTask.Config.SumDownloadedSize;
                    }

                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
        }

        private void UpdateProgress(DownloadTask myTask, double speed)
        {
            string str = String.Format("{0:F}%", myTask.DownloadProgress);
            this.progressBar_Download.Value = (int)myTask.DownloadProgress;
            this.label_downloadPrecent.Text = str;
            this.label_DownloadSpeed.Text = String.Format("{0:F}KB/S", speed);

        }

        private void Form_Download_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (downloadTask != null)
            {
                downloadTask.SetStop();
                downloadTask = null;
            }
            foreach (var thread in HelpDownloadThreadList)
            {
                thread.Abort();
            }
        }

        /**多任务同时下载时需要修改**/
        public void DownloadCompleted(DownloadTask myTask)
        {
            MessageBox.Show("下载完成！");
            updateProgressThread.Abort();
            downloadTask = null;
            button_Start.Enabled = true;
            button_Stop.Enabled = false;
            button_DeleteIP.Enabled = true;
            textBox_AddIP.Enabled = true;
            textBox_Path.Enabled = true;
            textBox_Threads.Enabled = true;
            textBox_Url.Enabled = true;
            checkBox_Prox.Enabled = true;
        }

        public void PrintToListBox(string output)
        {
            listBox_DownloadOutput.Items.Add(output);
        }

        /// <summary>
        /// 启动协助下载监听线程
        /// </summary>
        private void ListenDownload()
        {
            Socket listener = null;
            try
            {
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(new IPEndPoint(IPAddress.Any, 9394));
                listener.Listen(5);
                while (true)
                {
                    Socket acceptSocket = listener.Accept();
#if DEBUG
                    this.BeginInvoke(OutPutPrint, String.Format("{0} connected.", (acceptSocket.RemoteEndPoint as IPEndPoint).Address));
#endif
                    Thread downloadThread = new Thread(HelpDownload) { IsBackground = true };
                    downloadThread.Start(acceptSocket);
                    HelpDownloadThreadList.Add(downloadThread);
                    //HelpDownload(acceptSocket);
                }
            }
            catch (ThreadAbortException)
            {

            }
            catch (SocketException e)
            {
#if DEBUG
                MessageBox.Show(String.Format("SocketException: {0}", e.Message));
#endif
            }
            catch(Exception e)
            {
#if DEBUG
                MessageBox.Show(String.Format("Exception: {0}", e.Message));
#endif
            }
            finally
            {
                if (listener != null)
                    listener.Close();
            }
            
        }

        private void HelpDownload(object socket)
        {
            Socket acceptSocket = (Socket)socket;
            byte[] buff = new byte[5120];
            int receivedSize = acceptSocket.Receive(buff);
            HelpDownloadThread(acceptSocket, buff, receivedSize);
        }

        /// <summary>
        /// 开始帮助发送请求的主机下载
        /// 此函数功能只是作为数据中转
        /// </summary>
        /// <param name="TagetMachineSocket">连接请求帮助下载的主机的Socket</param>
        /// <param name="buff">接收缓冲区</param>
        /// <param name="length">缓冲区长度</param>
        private void HelpDownloadThread(Socket TagetMachineSocket, byte[] buff, int length)
        {
            string strRequest = Encoding.ASCII.GetString(buff);
            IPHostEntry ipHost = null;
            int Port = -1;
            int receivedSize = 1;

        again:
            Socket downloadServerSocket = null;
            bool wantToAgain = false;
            try
            {
                if (strRequest.Contains("HTTP/1.1\r\n"))//HTTP协议
                {
                    int hostStartIndex = strRequest.IndexOf("Host: ");
                    int hostEndIndex = strRequest.IndexOf("\r\n", hostStartIndex);
                    ipHost = Dns.GetHostEntry(strRequest.Substring(hostStartIndex + 6, hostEndIndex - hostStartIndex - 6));
                    Port = 80;
                    downloadServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    downloadServerSocket.ReceiveTimeout = 10000;//10s
                    IPAddress ipAddress = ipHost.AddressList[0];
                    downloadServerSocket.Connect(ipAddress, Port);
                    downloadServerSocket.Send(buff, length, SocketFlags.None);//将Request发送给下载服务器
                    receivedSize = downloadServerSocket.Receive(buff);
                    string strResponse = Encoding.ASCII.GetString(buff);
                    while (receivedSize > 0)
                    {
                        TagetMachineSocket.Send(buff, receivedSize, SocketFlags.None);//将从下载数据传回给请求帮助下载的主机
                        receivedSize = downloadServerSocket.Receive(buff);//继续从下载服务器接收数据
                    }
                }
                else//此处添加其他协议的处理
                {
                    return;
                }
            }
            #region
            catch (ThreadAbortException)
            {
            }
             catch (SocketException e)
            {
                //若downloadServerSocket.Connected == true，则说明是发送帮助下载请求的主机断开连接，而不是与下载服务器的连接断开了
                if ((e.SocketErrorCode == SocketError.ConnectionReset || e.SocketErrorCode == SocketError.TimedOut) && !downloadServerSocket.Connected && TagetMachineSocket.Connected)
                {
                    wantToAgain = true;
                    goto again;
                }
#if DEBUG
                else if ((e.SocketErrorCode == SocketError.ConnectionRefused) && !downloadServerSocket.Connected)
                    MessageBox.Show("服务器拒绝连接!");
                else if (downloadServerSocket.Connected)
                    this.BeginInvoke(OutPutPrint, String.Format("{0} connection has been closed.", (TagetMachineSocket.RemoteEndPoint as IPEndPoint).Address));
                else
                    MessageBox.Show("SocketException: " + e.Message);
#endif
            }
            catch(Exception e)
            {
#if DEBUG
                MessageBox.Show(String.Format("Exception: {0}", e.Message));
#endif
            }
            finally
            {
                if(downloadServerSocket != null)
                {
                    downloadServerSocket.Shutdown(SocketShutdown.Both);
                    downloadServerSocket.Close();
                }
                if (!wantToAgain)
                    TagetMachineSocket.Close();
            }
        }
            #endregion

        private void button_AddIP_Click(object sender, EventArgs e)
        {
            if (CheckIP(textBox_AddIP.Text))
            {
                listBox_IPList.Items.Add(textBox_AddIP.Text);
                OtherMachineIP.Add(IPAddress.Parse(textBox_AddIP.Text));
                textBox_AddIP.Text = "";
            }
            else
                MessageBox.Show("请输入正确的IP地址！");
        }

        private bool CheckIP(string strIP)
        {
            string strPattern = @"^(\d{1,2}|[01]?\d\d|2[0-4]\d|25[0-5])\."
                + @"(\d{1,2}|[01]?\d\d|2[0-4]\d|25[0-5])\."
                + @"(\d{1,2}|[01]?\d\d|2[0-4]\d|25[0-5])\."
                + @"(\d{1,2}|[01]?\d\d|2[0-4]\d|25[0-5])$";
            Regex regex = new Regex(strPattern);
            return regex.IsMatch(strIP);
        }

        private void button_DeleteIP_Click(object sender, EventArgs e)
        {
            if (listBox_IPList.SelectedIndex >= 0)
            {
                OtherMachineIP.RemoveAt(listBox_IPList.SelectedIndex);
                listBox_IPList.Items.RemoveAt(listBox_IPList.SelectedIndex);
            }
        }

        private void textBox_AddIP_TextChanged(object sender, EventArgs e)
        {
            if (String.Empty == textBox_AddIP.Text)
                button_AddIP.Enabled = false;
            else
                button_AddIP.Enabled = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_OpenListenThread.Checked)
            {
                Thread listenThread = new Thread(ListenDownload) { IsBackground = true };
                listenThread.Start();
                HelpDownloadThreadList.Add(listenThread);
            }
            else
            {
                foreach (var thread in HelpDownloadThreadList)
                    thread.Abort();
            }
        }

        private void checkBox_Prox_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_Prox.Checked)
                textBox_Threads.Enabled = false;
            else
                textBox_Threads.Enabled = true;
        }

        private void textBox_Url_TextChanged(object sender, EventArgs e)
        {
            int index;
            if ((index = textBox_Url.Text.LastIndexOf('/')) > 0)
                textBox_Path.Text = @"D:\" + textBox_Url.Text.Substring(index + 1);
        }

        private void button_Credentials_Click(object sender, EventArgs e)
        {
            Form_Credentials fc = new Form_Credentials();
            if (ServerUserName != null && ServerPassword != null)
            {
                fc.UserName = ServerUserName;
                fc.Password = ServerPassword;
            }
            if (fc.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ServerUserName = fc.UserName;
                ServerPassword = fc.Password;
            }
        }
        

    }
}
