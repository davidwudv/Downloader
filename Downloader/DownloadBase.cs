using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Downloader
{
    public abstract class DownloadBase
    {
        public DownloadTask downloadTask;//当前线程所属的下载任务对象
        public int CurrentThreadIndex;//当前线程索引
        protected readonly int BufferSize;

        public DownloadBase(DownloadTask downloadTask, int currentThreadIndex)
        {
            this.downloadTask = downloadTask;
            CurrentThreadIndex = currentThreadIndex;
            BufferSize = 5120;//每次最多接收5KB
        }

        public abstract void Start();
        /// <summary>
        /// 更新已下载的数据信息
        /// </summary>
        /// <param name="receivedSize">刚下载的字节数</param>
        /// <param name="headSize">Response head的大小</param>
        protected void UpdateDownloadedSize(int receivedSize, int headSize)
        {
            downloadTask.Config.BlockList[CurrentThreadIndex].DownloadedSize += (receivedSize - headSize);//更新本分块已下载的大小
            downloadTask.Config.SumDownloadedSize += (receivedSize - headSize);//更行已下载的总大小
            if (downloadTask.Config.SumDownloadedSize == downloadTask.Config.FileLength)
                downloadTask.State = DownloadState.Completed;//下载完成
            downloadTask.DownloadProgress = 100 * ((double)downloadTask.Config.SumDownloadedSize / downloadTask.Config.FileLength);
        }
    }

    //public enum DownloadProtocol
    //{
    //    HTTP = 0,
    //    HTTPS = 1,
    //    FTP = 2,
    //    FILE = 3
    //}

    ///// <summary>
    ///// 请求分流协助下载时发送给其他主机的数据包
    ///// </summary>
    //public class DownloadHelpRequest
    //{
    //    public DownloadProtocol Protocol { get; set; }
    //    public string Link { get; set; }
    //    public IPHostEntry Host { get; set; }

    //    public DownloadHelpRequest(DownloadProtocol protocol)
    //    {

    //    }
    //}
}
