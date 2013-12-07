using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace Downloader
{
    public enum DownloadState
    {
        Downloading = 0,
        Stop = 1,
        Completed = 2
    }

    public class DownloadTask
    {
        private delegate void DownloadCompletedDelegate(DownloadTask myTask);
        public delegate void OutPutDelegate(string output);
        private DownloadCompletedDelegate downloadCompletedDelegate;
        private DownloadState state;//当前任务的状态
        public OutPutDelegate OutPutPrint;
        public Form_Download MasterForm { get; set; }
        public TaskConfig Config { get; private set; }//当前任务的配置信息
        public string SavePath { get; set; }//下载文件的保存路径
        public object CriticalSectionLock;//临界区锁
        public int CurrentThreads;//当前任务运行的线程数
        public double DownloadProgress { get; set; }//下载进度(百分比)
        public List<Thread> ThreadList;//当前运行的线程列表

        public DownloadTask(string savePath, TaskConfig taskconfig, Form_Download masterForm)
        {
            MasterForm = masterForm;
            downloadCompletedDelegate = MasterForm.DownloadCompleted;
            OutPutPrint = MasterForm.PrintToListBox;
            CriticalSectionLock = new object();
            this.SavePath = savePath;
            Config = taskconfig;
            state = DownloadState.Stop;
            CurrentThreads = 0;
            DownloadProgress = 0.0;
            ThreadList = new List<Thread>(10);
            if (Config.SupportResume)
                SaveConfig();
            if (!File.Exists(SavePath))
            {
                FileStream file = new FileStream(SavePath, FileMode.Create, FileAccess.Write);
                file.SetLength((long)Config.FileLength);
                file.Close();
            }
        }

        public DownloadState State
        {
            get { return state; }
            set
            {
                if (state == DownloadState.Downloading && value == DownloadState.Completed)
                {
                    DeleteConfig();
                    MasterForm.Invoke(downloadCompletedDelegate, this);
                }
                else if (state == DownloadState.Downloading && value == DownloadState.Stop)
                {
                    state = value;
#if DEBUG
                    int i = 0;
#endif
                    foreach (var item in ThreadList)
                    {
#if DEBUG
                        MasterForm.Invoke(OutPutPrint, String.Format("Thread {0} has been stop!",i.ToString()));
                        ++i;
#endif
                        item.Abort();
                    }
                    ThreadList.Clear();
                    if (Config.SupportResume)
                        SaveConfig();
                    else
                    {
                        while (CurrentThreads > 0)
                            Thread.Sleep(30);
                        File.Delete(SavePath);
                    }
                }
                state = value;
            }
        }

        public void SaveConfig() { Config.Save(SavePath + ".conf"); }
        public void LoadConfig() { Config.Load(SavePath + ".conf"); }
        public void DeleteConfig() { File.Delete(SavePath + ".conf"); }//删除配置文件
        public void SetStop() { State = DownloadState.Stop; }

        public void Start()
        {
            int threadsSum = Config.BlockSum;
            int i = 0;
            HttpDownload download = null;
            Thread downloadThread = null;
            if (MasterForm.OtherMachineIP.Count > 0)
            {
                foreach (var item in MasterForm.OtherMachineIP)
                {
                    download = new HttpDownload(this, i);
                    downloadThread = new Thread(download.StartEx) { IsBackground = true };
                    ThreadList.Add(downloadThread);
                    downloadThread.Start(item);
                    #if DEBUG
                MasterForm.Invoke(OutPutPrint, String.Format("Thread {0} start!",i.ToString()));
#endif
                    ++i;
                    ++CurrentThreads;
                }
            }
            int surplusBlock = threadsSum - i;//发送给协助主机后还剩余的文件块数
            for (; i < threadsSum; ++i)
            {
                download = new HttpDownload(this, i);
                //if (surplusBlock > 2)
                //{
                    downloadThread = new Thread(download.StartEx) { IsBackground = true };
                    downloadThread.Start(null);
                //}
                //else
                //{
                //    downloadThread = new Thread(download.Start) { IsBackground = true };
                //    downloadThread.Start();
                //}
                ThreadList.Add(downloadThread);
                ++CurrentThreads;
#if DEBUG
                MasterForm.Invoke(OutPutPrint, String.Format("Thread {0} start!",i.ToString()));
#endif
            }

            State = DownloadState.Downloading;
        }
    }
}
