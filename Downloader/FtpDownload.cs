using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;

namespace Downloader
{
    public class FtpDownload : DownloadBase
    {
        private FtpWebRequest ftpRequest;
        private FtpWebResponse ftpResponse;
        private FileStream fileStream;
        public string UserName { get; set; }
        public string Password { get; set; }

        public FtpDownload(DownloadTask downloadTask, int currentThreadIndex)
            : base(downloadTask, currentThreadIndex)
        {
            ftpRequest = null;
            ftpResponse = null;
            fileStream = null;
            UserName = downloadTask.Config.UserName;
            Password = downloadTask.Config.Password;
        }

        public override void Start()
        {
            again:
            string link = downloadTask.Config.Link;
            long start = downloadTask.Config.BlockList[CurrentThreadIndex].StartIndex + downloadTask.Config.BlockList[CurrentThreadIndex].DownloadedSize;
            long thisBlockSize = downloadTask.Config.BlockList[CurrentThreadIndex].BlockSize - downloadTask.Config.BlockList[CurrentThreadIndex].DownloadedSize;
            if (thisBlockSize == 0)
                return;
            try
            {
                ftpRequest = (FtpWebRequest)WebRequest.Create(link);
                ftpRequest.Credentials = new NetworkCredential(downloadTask.Config.UserName, downloadTask.Config.Password);
                ftpRequest.ContentOffset = start;
                ftpRequest.UseBinary = true;
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                if(ftpResponse.StatusCode == FtpStatusCode.CommandOK || ftpResponse.StatusCode == FtpStatusCode.OpeningData
                    || ftpResponse.StatusCode == FtpStatusCode.DataAlreadyOpen || ftpResponse.StatusCode == FtpStatusCode.FileActionOK)
                {
                    fileStream =  new FileStream(downloadTask.SavePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
                    fileStream.Seek(start, SeekOrigin.Begin);
                    Stream dataStream = ftpResponse.GetResponseStream();
                    int receivedSize = 1;
                    byte[] buff = new byte[BufferSize];
                    
                    while (receivedSize > 0 && thisBlockSize > 0)
                    {
                        receivedSize = dataStream.Read(buff, 0, BufferSize);
                        if (receivedSize > 0)
                        {
                            fileStream.Write(buff, 0, receivedSize);
                            UpdateDownloadedSize(receivedSize, 0);
                            thisBlockSize -= receivedSize;
#if DEBUG
                            System.Diagnostics.Debug.WriteLine("Thread {0}: Receive {1} bytes from {2} downloaded {3:F}%", CurrentThreadIndex, receivedSize, "ftp server", downloadTask.DownloadProgress);
#endif
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
            catch (IOException e)
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
                if (ftpResponse != null)
                {
                    ftpResponse.Close();
                    ftpResponse = null;
                }
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream = null;
                }
                if (downloadTask.State == DownloadState.Completed || downloadTask.State == DownloadState.Stop)
                    Interlocked.Decrement(ref downloadTask.CurrentThreads);
#if DEBUG
                if (thisBlockSize == 0)
                    downloadTask.MasterForm.Invoke(downloadTask.OutPutPrint, String.Format("Thread {0} download completed!", CurrentThreadIndex));
#endif
            }
        }
    }
}
