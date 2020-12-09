using CefSharp;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TestNetJs.Helper;

namespace TestNetJs
{
    //public static class DispatcherHelper
    //{
    //    [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
    //    public static void DoEvents()
    //    {
    //        DispatcherFrame frame = new DispatcherFrame();
    //        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrames), frame);
    //        try { Dispatcher.PushFrame(frame); }
    //        catch (InvalidOperationException) { }
    //    }
    //    private static object ExitFrames(object frame)
    //    {
    //        ((DispatcherFrame)frame).Continue = false;
    //        return null;
    //    }
    //}
    public class BoundObject
    {
        public int MyProperty { get; set; }
        List<SynFileInfo> m_SynFileInfoList;
        public string MyReadOnlyProperty { get; internal set; }
        public Type MyUnconvertibleProperty { get; set; }
        public SubBoundObject SubObject { get; set; }
        public ExceptionTestBoundObject ExceptionTestObject { get; set; }

       

        public BoundObject()
        {
            MyProperty = 42;
            m_SynFileInfoList = new List<SynFileInfo>();
            MyReadOnlyProperty = "I'm immutable!";
           // IgnoredProperty = "I am an Ignored Property";
            MyUnconvertibleProperty = GetType();
            SubObject = new SubBoundObject();
            ExceptionTestObject = new ExceptionTestBoundObject();
        }
        void AddBatchDownload(string str)
        {
            //添加列表(建立多个任务)
            List<ArrayList> arrayListList = new List<ArrayList>();
            arrayListList.Add(new ArrayList(){
                "0",//文件id
                "PPTV客户端.exe",//文件名称
                "0",//文件大小
                "0 KB/S",//下载速度
                "0%",//下载进度
                 str,//远程服务器下载地址
               string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "qq.exe"),//本地保存地址
                true//是否异步
            });
            foreach (ArrayList arrayList in arrayListList)
            {
                
                m_SynFileInfoList.Add(new SynFileInfo(arrayList.ToArray()));
            }
        }
        public void OpenInstalledSoft(string filename)
        {
            Helper.InstallHelper.OpenSoft(filename);
        }
        public List<InstallSoftWare> GetInstallSoftWares()
        {
            string jsonstr = HttpHelper.httpGet(@"https://software-test.cfnet.org.cn/soft/product/softList");
            List<InstallSoftWare> softWareInfo = JsonConvert.DeserializeObject<List<InstallSoftWare>>(jsonstr);
            return softWareInfo;

        }
        public void UpdateInstallSoft(string str, string url)
        {
            try
            {
                List<InstallSoftWare> applist = GetInstallSoftWares();
                //software.Minimum = 1;
                //software.Maximum = applist.Count;
                RegistryKey regKey = Registry.LocalMachine;
                int i = 1;
                foreach (InstallSoftWare item in applist)

                {
                    RegistryKey regSubKey = regKey.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\WeChat", false);
                    string strKey = string.Empty;
                    object objResult = regSubKey.GetValue(strKey);
                    RegistryValueKind regValueKind = regSubKey.GetValueKind(strKey);
                    if (objResult != null)
                    {
                       // software.Value = i;
                        i++;
                    }
                }

            }
            catch
            { 
            
            }
        }
        public void OpenInstallPackage(string str, string url)
        {
            //判断网络连接是否正常
            if (isConnected())
            {
                
                //设置最大活动线程数以及可等待线程数
                ThreadPool.SetMaxThreads(3, 3);
                //判断是否还存在任务
                if (m_SynFileInfoList.Count <= 0) AddBatchDownload(url);
                foreach (SynFileInfo m_SynFileInfo in m_SynFileInfoList)
                {
                    //启动下载任务
                    StartDownLoad(m_SynFileInfo);
                }
            }
            else
            {
                MessageBox.Show("网络异常!");
            }
        }
        #region 检查网络状态
        #region 使用WebClient下载文件

        /// <summary>
        /// HTTP下载远程文件并保存本地的函数
        /// </summary>
        void StartDownLoad(object o)
        {
            SynFileInfo m_SynFileInfo = (SynFileInfo)o;
            m_SynFileInfo.LastTime = DateTime.Now;
            //再次new 避免WebClient不能I/O并发 
            WebClient client = new WebClient();
            if (m_SynFileInfo.Async)
            {
                //异步下载
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                client.DownloadFileAsync(new Uri(m_SynFileInfo.DownPath), m_SynFileInfo.SavePath, m_SynFileInfo);
            }
            else client.DownloadFile(new Uri(m_SynFileInfo.DownPath), m_SynFileInfo.SavePath);
        }

        /// <summary>
        /// 下载进度条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            SynFileInfo m_SynFileInfo = (SynFileInfo)e.UserState;
            m_SynFileInfo.SynProgress = e.ProgressPercentage + "%";
            double secondCount = (DateTime.Now - m_SynFileInfo.LastTime).TotalSeconds;
            m_SynFileInfo.SynSpeed = FileOperate.GetAutoSizeString(Convert.ToDouble(e.BytesReceived / secondCount), 2) + "/s";
            
        }

        /// <summary>
        /// 下载完成调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //到此则一个文件下载完毕
            SynFileInfo m_SynFileInfo = (SynFileInfo)e.UserState;
            m_SynFileInfoList.Remove(m_SynFileInfo);
            Helper.InstallHelper.OpenInstallPackage("qq");
        }

        #endregion
        //检测网络状态
        [DllImport("wininet.dll")]
        extern static bool InternetGetConnectedState(out int connectionDescription, int reservedValue);
        /// <summary>
        /// 检测网络状态
        /// </summary>
        bool isConnected()
        {
            int I = 0;
            bool state = InternetGetConnectedState(out I, 0);
            return state;
        }

        #endregion
     
        #region old
        //public uint[] MyUintArray
        //{
        //    get { return new uint[] { 7, 8 }; }
        //}

        //public int[] MyIntArray
        //{
        //    get { return new[] { 1, 2, 3, 4, 5, 6, 7, 8 }; }
        //}

        //public Array MyArray
        //{
        //    get { return new short[] { 1, 2, 3 }; }
        //}

        //public byte[] MyBytes
        //{
        //    get { return new byte[] { 3, 4, 5 }; }
        //}
        //{
        //    return "lowercase";
        //}

        //public string ReturnJsonEmployeeList()
        //{
        //    return "{\"employees\":[{\"firstName\":\"John\", \"lastName\":\"Doe\"},{\"firstName\":\"Anna\", \"lastName\":\"Smith\"},{\"firstName\":\"Peter\", \"lastName\":\"Jones\"}]}";
        //}

        //[JavascriptIgnore]
        //public string IgnoredProperty { get; set; }

        //[JavascriptIgnore]
        //public string IgnoredMethod()
        //{
        //    return "I am an Ignored Method";
        //}

        //public string ComplexParamObject(object param)
        //{
        //    if (param == null)
        //    {
        //        return "param is null";
        //    }
        //    return "The param type is:" + param.GetType();
        //}

        //public SubBoundObject GetSubObject()
        //{
        //    return SubObject;
        //}

        ///// <summary>
        ///// Demonstrates the use of params as an argument in a bound object
        ///// </summary>
        ///// <param name="name">Dummy Argument</param>
        ///// <param name="args">Params Argument</param>
        //public string MethodWithParams(string name, params object[] args)
        //{
        //    return "Name:" + name + ";Args:" + string.Join(", ", args.ToArray());
        //}

        //public string MethodWithoutParams(string name, string arg2)
        //{
        //    return string.Format("{0}, {1}", name, arg2);
        //}

        //public string MethodWithoutAnything()
        //{
        //    return "Method without anything called and returned successfully.";
        //}

        //public string MethodWithThreeParamsOneOptionalOneArray(string name, string optionalParam = null, params object[] args)
        //{
        //    return "MethodWithThreeParamsOneOptionalOneArray:" + (name ?? "No Name Specified") + " - " + (optionalParam ?? "No Optional Param Specified") + ";Args:" + string.Join(", ", args.ToArray());
        //}
        //public void TestCallback(IJavascriptCallback javascriptCallback)
        //{
        //    const int taskDelay = 3000;
        //    Task.Factory.StartNew(async () =>
        //    {
        //        Delay(taskDelay);
        //        using (javascriptCallback)
        //        {
        //            //var response = new CallbackResponseStruct("This callback from C# was delayed " + taskDelay + "ms");
        //            await javascriptCallback.ExecuteAsync(" 来自C#的返回值，在当前延迟" + taskDelay + "ms");
        //        }
        //    });
        //    /*
        //     //.net 4.5的写法
        //    const int taskDelay = 1500;
        //    Task.Run(async () =>
        //    {
        //        await Task.Delay(taskDelay);

        //        using (javascriptCallback)
        //        {
        //            //NOTE: Classes are not supported, simple structs are
        //            var response = new CallbackResponseStruct("This callback from C# was delayed " + taskDelay + "ms");
        //            await javascriptCallback.ExecuteAsync(response);
        //        }
        //    });
        //     */
        //}
        ///// <summary>
        ///// 延时函数
        ///// </summary>
        ///// <param name="delayTime">需要延时多少毫秒</param>
        ///// <returns></returns>
        //public static bool Delay(int delayTime)
        //{
        //    DateTime now = DateTime.Now;
        //    double s;
        //    do
        //    {
        //        TimeSpan spand = DateTime.Now - now;
        //        s = spand.TotalMilliseconds;
        //        DispatcherHelper.DoEvents();
        //    }
        //    while (s < delayTime);
        //    return true;
        //}

        //public int EchoMyProperty()
        //{
        //    return MyProperty;
        //}



        //public string EchoParamOrDefault(string param = "This is the default value")
        //{
        //    return param;
        //}

        //public void EchoVoid()
        //{
        //}

        //public Boolean EchoBoolean(Boolean arg0)
        //{
        //    return arg0;
        //}

        //public Boolean? EchoNullableBoolean(Boolean? arg0)
        //{
        //    return arg0;
        //}

        //public SByte EchoSByte(SByte arg0)
        //{
        //    return arg0;
        //}

        //public SByte? EchoNullableSByte(SByte? arg0)
        //{
        //    return arg0;
        //}

        //public Int16 EchoInt16(Int16 arg0)
        //{
        //    return arg0;
        //}

        //public Int16? EchoNullableInt16(Int16? arg0)
        //{
        //    return arg0;
        //}

        //public Int32 EchoInt32(Int32 arg0)
        //{
        //    return arg0;
        //}

        //public Int32? EchoNullableInt32(Int32? arg0)
        //{
        //    return arg0;
        //}

        //public Int64 EchoInt64(Int64 arg0)
        //{
        //    return arg0;
        //}

        //public Int64? EchoNullableInt64(Int64? arg0)
        //{
        //    return arg0;
        //}

        //public Byte EchoByte(Byte arg0)
        //{
        //    return arg0;
        //}

        //public Byte? EchoNullableByte(Byte? arg0)
        //{
        //    return arg0;
        //}

        //public UInt16 EchoUInt16(UInt16 arg0)
        //{
        //    return arg0;
        //}

        //public UInt16? EchoNullableUInt16(UInt16? arg0)
        //{
        //    return arg0;
        //}

        //public UInt32 EchoUInt32(UInt32 arg0)
        //{
        //    return arg0;
        //}

        //public UInt32? EchoNullableUInt32(UInt32? arg0)
        //{
        //    return arg0;
        //}

        //public UInt64 EchoUInt64(UInt64 arg0)
        //{
        //    return arg0;
        //}

        //public UInt64? EchoNullableUInt64(UInt64? arg0)
        //{
        //    return arg0;
        //}

        //public Single EchoSingle(Single arg0)
        //{
        //    return arg0;
        //}

        //public Single? EchoNullableSingle(Single? arg0)
        //{
        //    return arg0;
        //}

        //public Double EchoDouble(Double arg0)
        //{
        //    return arg0;
        //}

        //public Double? EchoNullableDouble(Double? arg0)
        //{
        //    return arg0;
        //}

        //public Char EchoChar(Char arg0)
        //{
        //    return arg0;
        //}

        //public Char? EchoNullableChar(Char? arg0)
        //{
        //    return arg0;
        //}

        //public DateTime EchoDateTime(DateTime arg0)
        //{
        //    return arg0;
        //}

        //public DateTime? EchoNullableDateTime(DateTime? arg0)
        //{
        //    return arg0;
        //}

        //public Decimal EchoDecimal(Decimal arg0)
        //{
        //    return arg0;
        //}

        //public Decimal? EchoNullableDecimal(Decimal? arg0)
        //{
        //    return arg0;
        //}

        //public String EchoString(String arg0)
        //{
        //    return arg0;
        //}

        // TODO: This will currently not work, as it causes a collision w/ the EchoString() method. We need to find a way around that I guess.
        //public String echoString(String arg)
        //{
        //    return "Lowercase echo: " + arg;
        //}
        #endregion


        // public String lowercaseMethod()

    }
    /// <summary>
    /// 文件有关的操作类
    /// </summary>
    public class FileOperate
    {
        #region 相应单位转换常量

        private const double KBCount = 1024;
        private const double MBCount = KBCount * 1024;
        private const double GBCount = MBCount * 1024;
        private const double TBCount = GBCount * 1024;

        #endregion

        #region 获取适应大小

        /// <summary>
        /// 得到适应大小
        /// </summary>
        /// <param name="size">字节大小</param>
        /// <param name="roundCount">保留小数(位)</param>
        /// <returns></returns>
        public static string GetAutoSizeString(double size, int roundCount)
        {
            if (KBCount > size) return Math.Round(size, roundCount) + "B";
            else if (MBCount > size) return Math.Round(size / KBCount, roundCount) + "KB";
            else if (GBCount > size) return Math.Round(size / MBCount, roundCount) + "MB";
            else if (TBCount > size) return Math.Round(size / GBCount, roundCount) + "GB";
            else return Math.Round(size / TBCount, roundCount) + "TB";
        }

        #endregion
    }

    class SynFileInfo
    {
        public string DocID { get; set; }
        public string DocName { get; set; }
        public long FileSize { get; set; }
        public string SynSpeed { get; set; }
        public string SynProgress { get; set; }
        public string DownPath { get; set; }
        public string SavePath { get; set; }
        public bool Async { get; set; }
        public DateTime LastTime { get; set; }

        public SynFileInfo(object[] objectArr)
        {
            int i = 0;
            DocID = objectArr[i].ToString(); i++;
            DocName = objectArr[i].ToString(); i++;
            FileSize = Convert.ToInt64(objectArr[i]); i++;
            SynSpeed = objectArr[i].ToString(); i++;
            SynProgress = objectArr[i].ToString(); i++;
            DownPath = objectArr[i].ToString(); i++;
            SavePath = objectArr[i].ToString(); i++;
            Async = Convert.ToBoolean(objectArr[i]); i++;
        }
    }
    public class InstallSoftWare
    {
        public int id { get; set; }
        public int product_type { get; set; }
        public string product_name { get; set; }
        public string package_file { get; set; }
        public string version { get; set; }
        public string registry_address { get; set; }
    }
}