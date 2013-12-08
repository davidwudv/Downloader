using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Net.Sockets;


namespace Downloader
{
    public class HttpDownload : DownloadBase
    {
        private string Referer;//重定向的源地址

        public HttpDownload(DownloadTask downloadTask, int currentThreadIndex)
            : base(downloadTask, currentThreadIndex)
        {
            Referer = null;
        }

        /// <summary>
        /// 用于1-2个线程的下载任务
        /// 此方法最多只能同时运行2个下载线程，
        /// 多余的会被投入睡眠队列
        /// </summary>
        public override void Start()//开始下载
        {
            again:
            HttpWebRequest httpRequest = null;
            HttpWebResponse httpResponse = null;
            FileStream fileStream = null;
            string link = downloadTask.Config.Link;
            long start = downloadTask.Config.BlockList[CurrentThreadIndex].StartIndex + downloadTask.Config.BlockList[CurrentThreadIndex].DownloadedSize;
            long end = downloadTask.Config.BlockList[CurrentThreadIndex].StartIndex + downloadTask.Config.BlockList[CurrentThreadIndex].BlockSize - 1;
            long thisBlockSize = downloadTask.Config.BlockList[CurrentThreadIndex].BlockSize - downloadTask.Config.BlockList[CurrentThreadIndex].DownloadedSize;
            if (thisBlockSize == 0)
                return;
            try
            {
                httpRequest = (HttpWebRequest)WebRequest.Create(link);
                if (downloadTask.Config.SupportResume && downloadTask.CurrentThreads > 1)
                    httpRequest.AddRange(start, end);
                httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                if (httpResponse.StatusCode == HttpStatusCode.OK || httpResponse.StatusCode == HttpStatusCode.PartialContent)
                {
                    Stream DataStream = httpResponse.GetResponseStream();
                    fileStream = new FileStream(downloadTask.SavePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
                    fileStream.Seek(start, SeekOrigin.Begin);
                    byte[] buffer = new byte[BufferSize + 1];
                    int receivedSize = 1;
                    while (receivedSize > 0 && thisBlockSize > 0)
                    {
                        receivedSize = DataStream.Read(buffer, 0, BufferSize);
                        if (receivedSize > 0)
                        {
                            lock (downloadTask.CriticalSectionLock)
                            {
                                fileStream.Write(buffer, 0, receivedSize);
                                UpdateDownloadedSize(receivedSize, 0);
#if DEBUG
                                System.Diagnostics.Debug.WriteLine("Thread {0}: Receive {1} bytes from {2} downloaded {3:F}%", CurrentThreadIndex, receivedSize, httpRequest.Host, downloadTask.DownloadProgress);
#endif
                            }
                            thisBlockSize -= receivedSize;
                            if(downloadTask.CurrentThreads > 1)
                                Thread.Sleep(5);//让出时间片
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Thread {0} has been stop.", CurrentThreadIndex);
#endif
            }
            catch(IOException e)
            {
#if DEBUG
                System.Windows.Forms.MessageBox.Show("IOException: " + e.Message);
#endif
                if (thisBlockSize > 0)
                    goto again;
            }
            catch (Exception e)
            {
#if DEBUG
                System.Windows.Forms.MessageBox.Show("Exception: " + e.Message);
#endif
            }
            finally
            {
                if (httpResponse != null)
                    httpResponse.Close();
                if (fileStream != null)
                    fileStream.Close();
                if(downloadTask.State == DownloadState.Completed || downloadTask.State == DownloadState.Stop)
                    Interlocked.Decrement(ref downloadTask.CurrentThreads);
            }
        }

        /// <summary>
        /// 用于2个线程以上的下载任务
        /// 使用Socket编写
        /// </summary>
        public void StartEx(object OtherMachineIP)
        {
        again:
            if (downloadTask.Config.BlockList[CurrentThreadIndex].Completed)
                return;
            Uri DownloadUri = new Uri(downloadTask.Config.Link);
            Socket socket = null;
            FileStream fileStream = null;
            long start = downloadTask.Config.BlockList[CurrentThreadIndex].StartIndex + downloadTask.Config.BlockList[CurrentThreadIndex].DownloadedSize;
            long end = downloadTask.Config.BlockList[CurrentThreadIndex].StartIndex + downloadTask.Config.BlockList[CurrentThreadIndex].BlockSize - 1;
            long thisBlockSize = downloadTask.Config.BlockList[CurrentThreadIndex].BlockSize - downloadTask.Config.BlockList[CurrentThreadIndex].DownloadedSize;

            try
            {
                StringBuilder strBuilder = new StringBuilder("GET ");
                strBuilder.Append(DownloadUri.LocalPath +  " HTTP/1.1\r\n");
                strBuilder.Append("Host: " + DownloadUri.Host + "\r\n");
                strBuilder.Append("Accept: */*\r\n");
                strBuilder.Append("Pragma: no-cache\r\n");
                strBuilder.Append("Cache-Control: no-cache\r\n");
                strBuilder.Append("Connection: Keep-Alive\r\n");
                if (Referer != null)
                    strBuilder.Append("Referer: " + Referer + "\r\n");
                if (downloadTask.Config.SupportResume && downloadTask.Config.BlockSum > 1)
                {
                    if (downloadTask.Config.BlockList[CurrentThreadIndex].IsLastBlock)
                        strBuilder.AppendFormat("Range: bytes={0}-\r\n", start);
                    else
                        strBuilder.AppendFormat("Range: bytes={0}-{1}\r\n", start, end);
                }
                strBuilder.Append("\r\n");

                string strHttpRequestHead = strBuilder.ToString();
                IPAddress ipAddress = null;
                int Port = -1;
                if (OtherMachineIP == null)
                {
                    IPHostEntry ipHost = Dns.GetHostEntry(DownloadUri.Host);
                    ipAddress = ipHost.AddressList[0];
                    Port = DownloadUri.Port;
                }
                else
                {
                    ipAddress = (IPAddress)OtherMachineIP;
                    Port = 9394;
                }
                
                byte[] buffer = new byte[BufferSize];
                int receivedSize = 1;
                int responseHeadSize = 0;
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.ReceiveTimeout = 10000;//10s
                fileStream = new FileStream(downloadTask.SavePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
                fileStream.Seek(start, SeekOrigin.Begin);
                socket.Connect(ipAddress, Port);
                socket.Send(Encoding.ASCII.GetBytes(strHttpRequestHead));
                receivedSize = socket.Receive(buffer);
                string strResponse = Encoding.ASCII.GetString(buffer);
                string strResponseHead = null;
                int nStatusCode;
                if (!QueryStatusCode(strResponse, out nStatusCode))
                {
#if DEBUG
                    System.Windows.Forms.MessageBox.Show("无法获取状态码!");
#endif
                    return;
                }
                if (nStatusCode >= 200 && nStatusCode < 300)
                {
                    /***截取Http Response head，此部分不写入文件***/
                    int index = strResponse.IndexOf("\r\n\r\n");
                    strResponseHead = strResponse.Substring(0, index + 4);
                    responseHeadSize = strResponseHead.Length;
                    lock(downloadTask.CriticalSectionLock)
                    {
                        fileStream.Write(buffer, responseHeadSize, receivedSize - responseHeadSize);
                        UpdateDownloadedSize(receivedSize, responseHeadSize);
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("Thread {0}: Receive {1} bytes from {2} downloaded {3:F}%", CurrentThreadIndex, receivedSize, ipAddress.ToString(), downloadTask.DownloadProgress);
#endif
                    }
                    thisBlockSize -= (receivedSize - responseHeadSize);

                    while (receivedSize > 0 && thisBlockSize > 0)
                    {
                        receivedSize = socket.Receive(buffer);
                        if (receivedSize > 0)
                        {
                            lock (downloadTask.CriticalSectionLock)
                            {
                                fileStream.Write(buffer, 0, receivedSize);
                                UpdateDownloadedSize(receivedSize, 0);
#if DEBUG
                                System.Diagnostics.Debug.WriteLine("Thread {0}: Receive {1} bytes from {2} downloaded {3:F}%", CurrentThreadIndex, receivedSize, ipAddress.ToString(), downloadTask.DownloadProgress);
#endif
                            }
                        }
                        thisBlockSize -= receivedSize;
                        if (downloadTask.CurrentThreads > 1)
                            Thread.Sleep(5);
                    }
                }
                else if (nStatusCode >= 300 && nStatusCode < 400)//重定向
                {
                    int index1 = strResponseHead.IndexOf("Location: ");
                    int index2 = strResponseHead.IndexOf("\r\n", index1);
                    Referer = strResponseHead.Substring(index1, index2 - index1);
                    goto again;
                }
                else
                    return;
            }
            catch (ThreadAbortException)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Thread {0} has been stop.", CurrentThreadIndex);
#endif
                if(socket != null && socket.Connected)
                    socket.Shutdown(SocketShutdown.Both);
            }
            catch(SocketException e)
            {
                if (e.SocketErrorCode == SocketError.ConnectionReset || e.SocketErrorCode == SocketError.TimedOut)
                    goto again;
                else if (e.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    if(OtherMachineIP != null)
                        System.Windows.Forms.MessageBox.Show("{0} 拒绝连接，可能是对方没有开启协助功能！", (OtherMachineIP as IPAddress).ToString());
                    else
                        System.Windows.Forms.MessageBox.Show("服务器拒绝连接!");
                }
#if DEBUG
                else
                    System.Windows.Forms.MessageBox.Show("SocketException: " + e.Message);
#endif
            }
            catch (IOException e)
            {
#if DEBUG
                System.Windows.Forms.MessageBox.Show("IOException: " + e.Message);
#endif
            }
            catch(Exception e)
            {
#if DEBUG
                System.Windows.Forms.MessageBox.Show("Exception: " + e.Message);
#endif
            }
            finally
            {
                if (socket != null)
                    socket.Close();
                if (fileStream != null)
                    fileStream.Close();
                if (downloadTask.State == DownloadState.Completed || downloadTask.State == DownloadState.Stop)
                    Interlocked.Decrement(ref downloadTask.CurrentThreads);
#if DEBUG
                if(thisBlockSize == 0)
                    downloadTask.MasterForm.Invoke(downloadTask.OutPutPrint, String.Format("Thread {0} download completed!",CurrentThreadIndex));
#endif
            }
        }

        public static bool QueryStatusCode(string strResponse, out int statusCode)
        {
            if (!strResponse.Contains("HTTP"))
            {
                statusCode = -1;
                return false;
            }
            string strStatusCode = strResponse.Substring(9, 3);
            statusCode = Int32.Parse(strStatusCode);
            return statusCode > 0 ? true : false;
        }

        

    }
}
