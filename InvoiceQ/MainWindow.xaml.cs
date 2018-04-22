using Microsoft.Win32;
using mshtml;
using Spire.Pdf;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace InvoiceQ
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private InvoiceList images = new InvoiceList();

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            //browser.Navigate(new Uri("https://inv-veri.chinatax.gov.cn/"));
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.AddExtension = true;
            dlg.DefaultExt = ".pdf";
            dlg.Filter = "PDF Documents(*.pdf)|*.pdf";
            dlg.Title = "打开电子发票文件";
            Nullable<bool> result = dlg.ShowDialog();
            if (result != true) return;

            using (MemoryStream ms = new MemoryStream())
            {
                PdfDocument doc = new PdfDocument(dlg.FileName);
                Image img = doc.SaveAsImage(0);
                img.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = ms;
                bi.EndInit();
                images.Add(new Invoice(bi));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            listView.ItemsSource = images;
        }


        private void listView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            OnPropertyChanged("ReadyToPush");
        }

        private void browser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

            //MessageBox.Show(codeElement.GetType().ToString());
            //    codeElement.setAttribute("value", inv.Code);
        }

        private void btnPush_Click(object sender, RoutedEventArgs e)
        {
          //  BackgroundWorker bgWorker = new BackgroundWorker();
            BgWorkArgs args = new BgWorkArgs();
            Invoice inv = args.Invoice = listView.SelectedItem as Invoice;
            //args.Document = browser.Document as HTMLDocumentClass;
            //bgWorker.DoWork += BgWorker_DoWork;
            //bgWorker.RunWorkerAsync(args);
            string cmd = string.Format(
                "$(\"#fpdm\").val(\"{0}\");" +
                "$(\"#kjje\").focus();"
                , inv.Code);
            browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync(cmd);
            Thread.Sleep(1000);
            cmd = string.Format(
                "$(\"#kjje\").val(\"{0}\");" +
                "$(\"#fphm\").val(\"{1}\");" +
                "$(\"#kprq\").val(\"{2:yyyyMMdd}\");" +
                "$(\"#yzm\").focus();"
                , inv.BriefCheckCode, inv.Number, inv.Date);
            browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync(cmd);

        }
        class BgWorkArgs
        {
            public Invoice Invoice { get; set; }
            public HTMLDocumentClass Document { get; set; }
        }
        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BgWorkArgs args = e.Argument as BgWorkArgs;
            Invoice inv = args.Invoice;
            HTMLDocumentClass doc = args.Document;
            HTMLInputElementClass codeElement = doc.getElementById("fpdm") as HTMLInputElementClass;
            HTMLInputElementClass numberElement = doc.getElementById("fphm") as HTMLInputElementClass;
            HTMLInputElementClass dateElement = doc.getElementById("kprq") as HTMLInputElementClass;
            HTMLInputElementClass checkCodeElement = doc.getElementById("kjje") as HTMLInputElementClass;
            HTMLInputElementClass captchaElement = doc.getElementById("yzm") as HTMLInputElementClass;
            codeElement.focus();
            codeElement.value = inv.Code;
            numberElement.focus();
            Thread.Sleep(1000);
            numberElement.value = inv.Number;
            dateElement.value = inv.Date.ToString("yyyyMMdd");
            checkCodeElement.value = inv.CheckCode.Substring(inv.CheckCode.Length - 6);
            captchaElement.focus();
        }

        private void browser_LoadingStateChanged(object sender, CefSharp.LoadingStateChangedEventArgs e)
        {
            OnPropertyChanged("ReadyToPush");
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public bool ReadyToPush
        {
            get
            {
                return listView?.SelectedIndex != -1 && browser?.IsLoading == false;
            }
        }
    }
}
