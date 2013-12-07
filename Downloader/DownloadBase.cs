using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Downloader
{
    public abstract class DownloadBase
    {
        public DownloadTask downloadTask;//此线程所属的下载任务对象
        public int CurrentThreadIndex;//当前线程索引
        protected readonly int BufferSize;

        public DownloadBase(DownloadTask downloadTask, int currentThreadIndex)
        {
            this.downloadTask = downloadTask;
            CurrentThreadIndex = currentThreadIndex;
            BufferSize = 5120;//每次最多接收5KB
        }

        public abstract void Start();
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
