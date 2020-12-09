using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Permissions;
using CefSharp.Wpf;
using CefSharp;
using TestNetJs.Handels;

namespace TestNetJs
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    [PermissionSet(SecurityAction.Demand,Name ="FullTrust")]
    [System.Runtime.InteropServices.ComVisible(true)]
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        ChromiumWebBrowser webBrower = null;
        string rootPath = AppDomain.CurrentDomain.BaseDirectory;
        public void ShowMessage(string msg)
        {
            MessageBox.Show("Come on baby"+msg);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string path = "";
            //显示一个html页面
            //path = "www.baidu.com";
            //path = rootPath + "\\html\\fengche.html";
            //JavaScript异步调用C#方法 
            //JavaScript带参数调用C#方法
            path = rootPath + "\\html\\1.html";
            path = "file://" + path.Replace("\\", "/");
            webBrower = new ChromiumWebBrowser();
            webBrower.Address = @" https://software-test.cfnet.org.cn/soft/product/index";
            webBrower.RequestHandler = new MyRequestHandler();
            webBrower.KeyboardHandler = new CEFKeyBoardHander();
            //CefSharpSettings.LegacyJavascriptBindingEnabled = true;
            CefSharpSettings.WcfEnabled = true;
            webBrower.RegisterJsObject("bound", new BoundObject(), false);
           // webBrower.JavascriptObjectRepository.Register("bound", new BoundObject(), isAsync: false, options: BindingOptions.DefaultBinder);
            jstest.Content = webBrower;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Cef.Shutdown();
        }
    }
}
