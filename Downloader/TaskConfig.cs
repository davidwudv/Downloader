using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Net;

namespace Downloader
{
    public class FileBlock
    {
        public long StartIndex;//文件分块的开始坐标
        public long BlockSize;//分块的大小;
        public bool IsLastBlock;//标识是否是最后一个分块
        public bool Completed;//标识此分块是否下载完成
        private long downloadedSize;//已经下载的大小

        public long DownloadedSize
        {
            get { return downloadedSize; }
            set 
            { 
                downloadedSize = value;
                if (downloadedSize == BlockSize)
                    Completed = true;
            }
        }

        public FileBlock() 
        {
            StartIndex = 0;
            BlockSize = 0;
            DownloadedSize = 0;
            IsLastBlock = false;
            Completed = false;
        }
        public FileBlock(long start, long blockSize)
        {
            StartIndex = start;
            BlockSize = blockSize;
            DownloadedSize = 0L;
            IsLastBlock = false;
            Completed = false;
        }
    }
    public class TaskConfig
    {
        public string Link { get; private set; }//下载链接
        public string FileName { get; private set; }//文件名
        public long FileLength { get; private set; }//文件总大小
        public long SumDownloadedSize { get; set; }//已下载的总大小
        public int BlockSum { get; private set; }//文件分块总数
        public bool SupportResume;//是否支持断点续传
        public List<FileBlock> BlockList;


        public TaskConfig()
        {
            Link = null; FileName = null;
            FileLength = 0L; SumDownloadedSize = 0L;
            BlockSum = 0; SupportResume = true;
            BlockList = new List<FileBlock>();
        }

        public TaskConfig(string link, int threadSum)
        {
            Link = link;
            int index = link.LastIndexOf('/') + 1;
            FileName = link.Substring(index);
            BlockSum = threadSum;
            SupportResume = true;
            BlockList = new List<FileBlock>();
        }

        public bool InitConfig()
        {
        begin:
            HttpWebRequest httpRequest = null;
            HttpWebResponse httpResponse = null;
            try
            {
                httpRequest = (HttpWebRequest)WebRequest.Create(Link);
                httpRequest.Method = "HEAD";
                httpRequest.KeepAlive = false;
                if (SupportResume)
                    httpRequest.AddRange(100);
                httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    FileLength = (long)httpResponse.ContentLength;
                    InitBlockInfo();
                }
                else if ((int)httpResponse.StatusCode >= 400)
                    return false;
                else
                {
                    SupportResume = false;
                    BlockSum = 1;
                    goto begin;
                }
            }
            catch (WebException e)
            {
                System.Windows.Forms.MessageBox.Show(String.Format("获取文件信息出错！WebException: {0}", e.Message));
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(String.Format("获取文件信息出错！Exception: {0}", e.Message));
            }
            finally
            {
                if (httpResponse != null)
                    httpResponse.Close();
            }
            return true;
        }

        private void InitBlockInfo()
        {
            if (BlockSum == 1)
            {
                FileBlock block = new FileBlock(0L, FileLength);
                block.IsLastBlock = true;
                BlockList.Add(block);
                return;
            }
            else
            {
                long blockSize = FileLength / (long)BlockSum;
                long lastBlockSize = blockSize + (FileLength % (long)BlockSum);//余下部分由最后一个线程下载
                for (int i = 0; i < BlockSum; ++i)
                {
                    FileBlock block;
                    if (i == 0)
                        block = new FileBlock(0L, blockSize);
                    else if (i == BlockSum - 1)
                    {
                        block = new FileBlock((long)i * blockSize, lastBlockSize);
                        block.IsLastBlock = true;
                    }
                    else
                        block = new FileBlock((long)i * blockSize, blockSize);
                    BlockList.Add(block);
                }
            }
        }

        public bool Save(string strPath)
        {
            Stream writeStream = null;
            BinaryWriter binaryWriter = null;
            try
            {
                writeStream = new FileStream(strPath, FileMode.Create, FileAccess.Write);
                binaryWriter = new BinaryWriter(writeStream, Encoding.ASCII);

                binaryWriter.Write(this.Link);
                binaryWriter.Write(this.FileName);
                binaryWriter.Write(this.FileLength);
                binaryWriter.Write(this.SumDownloadedSize);
                binaryWriter.Write(this.BlockSum);
                binaryWriter.Write(this.SupportResume);

                foreach (var item in BlockList)
                {
                    binaryWriter.Write(item.StartIndex);
                    binaryWriter.Write(item.BlockSize);
                    binaryWriter.Write(item.DownloadedSize);
                    binaryWriter.Write(item.IsLastBlock);
                    binaryWriter.Write(item.Completed);
                }
#if DEBUG
                Debug.WriteLine("保存配置文件完毕！");
#endif
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.Message);
#endif
            }
            finally
            {
                if(writeStream != null && binaryWriter != null)
                    binaryWriter.Close();
                else if (writeStream != null)
                    writeStream.Close();
            }

            return true;
        }

        public bool Load(string strPath)
        {
            Stream readStream = null;
            BinaryReader binaryReader = null;
            try
            {
                readStream = new FileStream(strPath, FileMode.Open, FileAccess.Read);
                binaryReader = new BinaryReader(readStream);

                Link = binaryReader.ReadString();
                FileName = binaryReader.ReadString();
                FileLength = binaryReader.ReadInt64();
                SumDownloadedSize = binaryReader.ReadInt64();
                BlockSum = binaryReader.ReadInt32();
                SupportResume = binaryReader.ReadBoolean();
                for (int i = 0; i < BlockSum; ++i)
                {
                    FileBlock block = new FileBlock();
                    block.StartIndex = binaryReader.ReadInt64();
                    block.BlockSize = binaryReader.ReadInt64();
                    block.DownloadedSize = binaryReader.ReadInt64();
                    block.IsLastBlock = binaryReader.ReadBoolean();
                    block.Completed = binaryReader.ReadBoolean();
                    BlockList.Add(block);
                }

            }
            catch(EndOfStreamException ex)
            {
#if DEBUG
                Debug.WriteLine("The end of the stream is reached.");
#endif
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            catch(ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (readStream != null && binaryReader != null)
                    binaryReader.Close();
                else if (readStream != null)
                    readStream.Close();
            }
#if DEBUG
            Debug.WriteLine("读取配置文件完毕！");
#endif
            return true;
        }
    }
}
