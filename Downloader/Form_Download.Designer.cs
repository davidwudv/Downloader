namespace Downloader
{
    partial class Form_Download
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.button_Start = new System.Windows.Forms.Button();
            this.progressBar_Download = new System.Windows.Forms.ProgressBar();
            this.textBox_Url = new System.Windows.Forms.TextBox();
            this.textBox_Path = new System.Windows.Forms.TextBox();
            this.label_Url = new System.Windows.Forms.Label();
            this.label_Path = new System.Windows.Forms.Label();
            this.label_AddIP = new System.Windows.Forms.Label();
            this.textBox_AddIP = new System.Windows.Forms.TextBox();
            this.button_AddIP = new System.Windows.Forms.Button();
            this.button_DeleteIP = new System.Windows.Forms.Button();
            this.listBox_IPList = new System.Windows.Forms.ListBox();
            this.button_Stop = new System.Windows.Forms.Button();
            this.listBox_DownloadOutput = new System.Windows.Forms.ListBox();
            this.textBox_Threads = new System.Windows.Forms.TextBox();
            this.label_Blocks = new System.Windows.Forms.Label();
            this.label_downloadPrecent = new System.Windows.Forms.Label();
            this.label_DownloadSpeed = new System.Windows.Forms.Label();
            this.checkBox_OpenListenThread = new System.Windows.Forms.CheckBox();
            this.checkBox_Prox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // button_Start
            // 
            this.button_Start.Location = new System.Drawing.Point(338, 286);
            this.button_Start.Name = "button_Start";
            this.button_Start.Size = new System.Drawing.Size(75, 23);
            this.button_Start.TabIndex = 8;
            this.button_Start.Text = "Start";
            this.button_Start.UseVisualStyleBackColor = true;
            this.button_Start.Click += new System.EventHandler(this.button_Start_Click);
            // 
            // progressBar_Download
            // 
            this.progressBar_Download.Location = new System.Drawing.Point(13, 13);
            this.progressBar_Download.Name = "progressBar_Download";
            this.progressBar_Download.Size = new System.Drawing.Size(220, 23);
            this.progressBar_Download.TabIndex = 1;
            // 
            // textBox_Url
            // 
            this.textBox_Url.Location = new System.Drawing.Point(338, 43);
            this.textBox_Url.Name = "textBox_Url";
            this.textBox_Url.Size = new System.Drawing.Size(224, 21);
            this.textBox_Url.TabIndex = 1;
            this.textBox_Url.Text = "http://skype.gmw.cn/software/SkypeSetupFull.6.11.99.102.exe";
            // 
            // textBox_Path
            // 
            this.textBox_Path.Location = new System.Drawing.Point(338, 71);
            this.textBox_Path.Name = "textBox_Path";
            this.textBox_Path.Size = new System.Drawing.Size(224, 21);
            this.textBox_Path.TabIndex = 2;
            this.textBox_Path.Text = "D:\\SkypeSetupFull.6.11.99.102.exe";
            // 
            // label_Url
            // 
            this.label_Url.AutoSize = true;
            this.label_Url.Location = new System.Drawing.Point(303, 46);
            this.label_Url.Name = "label_Url";
            this.label_Url.Size = new System.Drawing.Size(29, 12);
            this.label_Url.TabIndex = 4;
            this.label_Url.Text = "URL:";
            // 
            // label_Path
            // 
            this.label_Path.AutoSize = true;
            this.label_Path.Location = new System.Drawing.Point(296, 74);
            this.label_Path.Name = "label_Path";
            this.label_Path.Size = new System.Drawing.Size(35, 12);
            this.label_Path.TabIndex = 4;
            this.label_Path.Text = "Path:";
            // 
            // label_AddIP
            // 
            this.label_AddIP.AutoSize = true;
            this.label_AddIP.Location = new System.Drawing.Point(284, 128);
            this.label_AddIP.Name = "label_AddIP";
            this.label_AddIP.Size = new System.Drawing.Size(47, 12);
            this.label_AddIP.TabIndex = 4;
            this.label_AddIP.Text = "Add IP:";
            // 
            // textBox_AddIP
            // 
            this.textBox_AddIP.Location = new System.Drawing.Point(338, 125);
            this.textBox_AddIP.Name = "textBox_AddIP";
            this.textBox_AddIP.Size = new System.Drawing.Size(120, 21);
            this.textBox_AddIP.TabIndex = 4;
            this.textBox_AddIP.TextChanged += new System.EventHandler(this.textBox_AddIP_TextChanged);
            // 
            // button_AddIP
            // 
            this.button_AddIP.Enabled = false;
            this.button_AddIP.Location = new System.Drawing.Point(464, 125);
            this.button_AddIP.Name = "button_AddIP";
            this.button_AddIP.Size = new System.Drawing.Size(46, 23);
            this.button_AddIP.TabIndex = 5;
            this.button_AddIP.Text = "Add";
            this.button_AddIP.UseVisualStyleBackColor = true;
            this.button_AddIP.Click += new System.EventHandler(this.button_AddIP_Click);
            // 
            // button_DeleteIP
            // 
            this.button_DeleteIP.Location = new System.Drawing.Point(516, 125);
            this.button_DeleteIP.Name = "button_DeleteIP";
            this.button_DeleteIP.Size = new System.Drawing.Size(46, 23);
            this.button_DeleteIP.TabIndex = 6;
            this.button_DeleteIP.Text = "Del";
            this.button_DeleteIP.UseVisualStyleBackColor = true;
            this.button_DeleteIP.Click += new System.EventHandler(this.button_DeleteIP_Click);
            // 
            // listBox_IPList
            // 
            this.listBox_IPList.FormattingEnabled = true;
            this.listBox_IPList.ItemHeight = 12;
            this.listBox_IPList.Location = new System.Drawing.Point(338, 153);
            this.listBox_IPList.Name = "listBox_IPList";
            this.listBox_IPList.Size = new System.Drawing.Size(224, 124);
            this.listBox_IPList.TabIndex = 7;
            // 
            // button_Stop
            // 
            this.button_Stop.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Stop.Enabled = false;
            this.button_Stop.Location = new System.Drawing.Point(487, 286);
            this.button_Stop.Name = "button_Stop";
            this.button_Stop.Size = new System.Drawing.Size(75, 23);
            this.button_Stop.TabIndex = 9;
            this.button_Stop.Text = "Stop";
            this.button_Stop.UseVisualStyleBackColor = true;
            this.button_Stop.Click += new System.EventHandler(this.button_Stop_Click);
            // 
            // listBox_DownloadOutput
            // 
            this.listBox_DownloadOutput.FormattingEnabled = true;
            this.listBox_DownloadOutput.ItemHeight = 12;
            this.listBox_DownloadOutput.Location = new System.Drawing.Point(13, 46);
            this.listBox_DownloadOutput.Name = "listBox_DownloadOutput";
            this.listBox_DownloadOutput.Size = new System.Drawing.Size(265, 268);
            this.listBox_DownloadOutput.TabIndex = 10;
            // 
            // textBox_Threads
            // 
            this.textBox_Threads.Location = new System.Drawing.Point(338, 98);
            this.textBox_Threads.Name = "textBox_Threads";
            this.textBox_Threads.Size = new System.Drawing.Size(98, 21);
            this.textBox_Threads.TabIndex = 3;
            this.textBox_Threads.Text = "4";
            // 
            // label_Blocks
            // 
            this.label_Blocks.AutoSize = true;
            this.label_Blocks.Location = new System.Drawing.Point(284, 101);
            this.label_Blocks.Name = "label_Blocks";
            this.label_Blocks.Size = new System.Drawing.Size(47, 12);
            this.label_Blocks.TabIndex = 8;
            this.label_Blocks.Text = "Blocks:";
            // 
            // label_downloadPrecent
            // 
            this.label_downloadPrecent.AutoSize = true;
            this.label_downloadPrecent.BackColor = System.Drawing.Color.Transparent;
            this.label_downloadPrecent.Location = new System.Drawing.Point(111, 18);
            this.label_downloadPrecent.Name = "label_downloadPrecent";
            this.label_downloadPrecent.Size = new System.Drawing.Size(17, 12);
            this.label_downloadPrecent.TabIndex = 9;
            this.label_downloadPrecent.Text = "0%";
            // 
            // label_DownloadSpeed
            // 
            this.label_DownloadSpeed.AutoSize = true;
            this.label_DownloadSpeed.Location = new System.Drawing.Point(243, 18);
            this.label_DownloadSpeed.Name = "label_DownloadSpeed";
            this.label_DownloadSpeed.Size = new System.Drawing.Size(35, 12);
            this.label_DownloadSpeed.TabIndex = 10;
            this.label_DownloadSpeed.Text = "0KB/S";
            // 
            // checkBox_OpenListenThread
            // 
            this.checkBox_OpenListenThread.AutoSize = true;
            this.checkBox_OpenListenThread.Location = new System.Drawing.Point(338, 13);
            this.checkBox_OpenListenThread.Name = "checkBox_OpenListenThread";
            this.checkBox_OpenListenThread.Size = new System.Drawing.Size(168, 16);
            this.checkBox_OpenListenThread.TabIndex = 11;
            this.checkBox_OpenListenThread.Text = "开启帮助其主机下载的线程";
            this.checkBox_OpenListenThread.UseVisualStyleBackColor = true;
            this.checkBox_OpenListenThread.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // checkBox_Prox
            // 
            this.checkBox_Prox.AutoSize = true;
            this.checkBox_Prox.Location = new System.Drawing.Point(442, 100);
            this.checkBox_Prox.Name = "checkBox_Prox";
            this.checkBox_Prox.Size = new System.Drawing.Size(120, 16);
            this.checkBox_Prox.TabIndex = 12;
            this.checkBox_Prox.Text = "使用完全代理下载";
            this.checkBox_Prox.UseVisualStyleBackColor = true;
            this.checkBox_Prox.CheckedChanged += new System.EventHandler(this.checkBox_Prox_CheckedChanged);
            // 
            // Form_Download
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Stop;
            this.ClientSize = new System.Drawing.Size(574, 337);
            this.Controls.Add(this.checkBox_Prox);
            this.Controls.Add(this.checkBox_OpenListenThread);
            this.Controls.Add(this.label_DownloadSpeed);
            this.Controls.Add(this.label_downloadPrecent);
            this.Controls.Add(this.label_Blocks);
            this.Controls.Add(this.listBox_DownloadOutput);
            this.Controls.Add(this.listBox_IPList);
            this.Controls.Add(this.button_DeleteIP);
            this.Controls.Add(this.button_AddIP);
            this.Controls.Add(this.label_AddIP);
            this.Controls.Add(this.label_Path);
            this.Controls.Add(this.label_Url);
            this.Controls.Add(this.textBox_AddIP);
            this.Controls.Add(this.textBox_Threads);
            this.Controls.Add(this.textBox_Path);
            this.Controls.Add(this.textBox_Url);
            this.Controls.Add(this.progressBar_Download);
            this.Controls.Add(this.button_Stop);
            this.Controls.Add(this.button_Start);
            this.MaximizeBox = false;
            this.Name = "Form_Download";
            this.Text = "Download";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Download_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;

        }

        #endregion

        private System.Windows.Forms.Button button_Start;
        private System.Windows.Forms.ProgressBar progressBar_Download;
        private System.Windows.Forms.TextBox textBox_Url;
        private System.Windows.Forms.TextBox textBox_Path;
        private System.Windows.Forms.Label label_Url;
        private System.Windows.Forms.Label label_Path;
        private System.Windows.Forms.Label label_AddIP;
        private System.Windows.Forms.TextBox textBox_AddIP;
        private System.Windows.Forms.Button button_AddIP;
        private System.Windows.Forms.Button button_DeleteIP;
        private System.Windows.Forms.ListBox listBox_IPList;
        private System.Windows.Forms.Button button_Stop;
        private System.Windows.Forms.ListBox listBox_DownloadOutput;
        private System.Windows.Forms.TextBox textBox_Threads;
        private System.Windows.Forms.Label label_Blocks;
        private System.Windows.Forms.Label label_downloadPrecent;
        private System.Windows.Forms.Label label_DownloadSpeed;
        private System.Windows.Forms.CheckBox checkBox_OpenListenThread;
        private System.Windows.Forms.CheckBox checkBox_Prox;
    }
}

