using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Autowork
{
    public partial class Form1 : Form
    {


        public Form1()
        {
            InitializeComponent();
        }



        /// <summary>
        ///访问网址
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        public string HttpGet(string Url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                return retString;
            }
            catch (WebException ex)//500内部错误
            {
                HttpWebResponse response = (HttpWebResponse)ex.Response;
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                return retString;
            }

        }
        class StateObject
        {
            public HttpWebRequest Request { get; set; }
            public string Type{get;set;}
        //可以添加任何你需要的东西
        //……
        }   
    #region HttpWebRequest异步GET
    public void AsyncGetWithWebRequest(string url,string type)
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri(url));
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            StateObject state = new StateObject();
            state.Request = request;
            state.Type = type;
            request.BeginGetResponse(new AsyncCallback(ReadCallback), state);
        }
        private  void ReadCallback(IAsyncResult asynchronousResult)
        {
            // var request = (HttpWebRequest)asynchronousResult.AsyncState;
            StateObject state = (StateObject)asynchronousResult.AsyncState;
            var request = state.Request;
            try {
                var response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
                using (var streamReader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")))
                {
                    var resultString = streamReader.ReadToEnd();
                    if (resultString != "0")
                    { LogMessage("" + state.Type + "：" + resultString); }
                      
                }
            }
            catch (WebException ex)//500内部错误
            {
                HttpWebResponse response = (HttpWebResponse)ex.Response;
                if (response != null)
                    using (var streamReader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")))
                    {
                        var resultString = streamReader.ReadToEnd();
                        LogMessage(resultString);
                    }
                else {
                    LogMessage("网络错误:"+ex.ToString());
                }
            }
           
        }
        #endregion


        #region 日志记录、支持其他线程访问 
        public delegate void LogAppendDelegate(Color color, string text);
        /// <summary> 
        /// 追加显示文本 
        /// </summary> 
        /// <param name="color">文本颜色</param> 
        /// <param name="text">显示文本</param> 
        public void LogAppend(Color color, string text)
        {
            this.msglog.SelectionColor = color;
            this.msglog.AppendText(text);
            this.msglog.AppendText("\n");
            //scroll滚到底部
            this.msglog.SelectionStart = this.msglog.Text.Length;
            this.msglog.SelectionLength = 0;
            this.msglog.Focus();
            //写入文件
            try
            {
                if (System.IO.Directory.Exists("log"))
                {
                    string logFileName = @"log/LogName" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                    using (TextWriter logFile = TextWriter.Synchronized(File.AppendText(logFileName)))
                    {
                        logFile.WriteLine(text);
                        logFile.Flush();
                        logFile.Close();
                    }
                }
                else
                {
                    Directory.CreateDirectory("log");//创建该文件
                    string logFileName = @"log/LogName" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                    using (TextWriter logFile = TextWriter.Synchronized(File.AppendText(logFileName)))
                    {
                        logFile.WriteLine(text);
                        logFile.Flush();
                        logFile.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Log Create Error:" + ex.Message.ToString());
            }

        }
        /// <summary> 
        /// 显示错误日志 
        /// </summary> 
        /// <param name="text"></param> 
        public void LogError(string text)
        {
            LogAppendDelegate la = new LogAppendDelegate(LogAppend);
            this.msglog.BeginInvoke(la, Color.Red, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + " Error:" + text);
        }
        /// <summary> 
        /// 显示警告信息 
        /// </summary> 
        /// <param name="text"></param> 
        public void LogWarning(string text)
        {
            LogAppendDelegate la = new LogAppendDelegate(LogAppend);
            this.msglog.BeginInvoke(la, Color.Yellow, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + " Warning:" + text);
        }
        /// <summary> 
        /// 显示信息 
        /// </summary> 
        /// <param name="text"></param> 
        public void LogMessage(string text)
        {
            LogAppendDelegate la = new LogAppendDelegate(LogAppend);
            this.msglog.BeginInvoke(la, Color.LightGray, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + " Message:" + text);
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            LogMessage("自动化程序打开");
            this.label2.Text = this.numericUpDown1.Value.ToString();
            this.label9.Text = this.numericUpDown2.Value.ToString();
           
        }

        //开始每日开奖生成
        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Start();
            this.label2.Text = this.numericUpDown1.Value.ToString();
            this.groupBox1.BackColor = Color.GreenYellow;
            LogMessage("开始每日开奖生成");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Convert.ToInt32(this.label2.Text) > 0)
            {
                this.label2.Text = (Convert.ToInt32(this.label2.Text) - 1).ToString();
            }
            else {
                this.label2.Text = this.numericUpDown1.Value.ToString();
                /////访问url
                ///
                AsyncGetWithWebRequest(this.textBox1.Text, "每日开奖生成：");
                /*
                string res=HttpGet(this.textBox1.Text);

                if (res!="0")
                {
                    LogMessage("车位回收：" + res);
                }
               */

            }
        }
        //停止每日开奖生成
        private void button2_Click(object sender, EventArgs e)
        {
            this.timer1.Stop();
            this.groupBox1.BackColor = Color.White;
            LogMessage("停止每日开奖生成");
        }

        //开始任务2
        private void button4_Click(object sender, EventArgs e)
        {
            timer2.Start();
            this.label9.Text = this.numericUpDown2.Value.ToString();
            this.groupBox2.BackColor = Color.GreenYellow;
            LogMessage("开始任务2");
        }

        //停止任务2
        private void button3_Click(object sender, EventArgs e)
        {
            this.timer2.Stop();
            this.groupBox2.BackColor = Color.White;
            LogMessage("停止任务2");
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (Convert.ToInt32(this.label9.Text) > 0)
            {
                this.label9.Text = (Convert.ToInt32(this.label9.Text) - 1).ToString();
            }
            else
            {
                this.label9.Text = this.numericUpDown2.Value.ToString();
                /////访问url
                //string res = HttpGet(this.textBox2.Text);
                AsyncGetWithWebRequest(this.textBox2.Text, "任务2：");
                /*
                if (res != "0")
                {
                    LogMessage("提前续费提醒：" + res);
                }
                */
               

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            LogMessage("自动化程序关闭");
        
  
            //MessageBox.Show("");
        }

    }


}
