using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Forms;
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
            listView.ItemsSource = images;
            //browser.Navigate(new Uri("https://inv-veri.chinatax.gov.cn/"));
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.AddExtension = true;
            dlg.Multiselect = true;
            dlg.DefaultExt = ".pdf";
            dlg.Filter = "PDF Documents(*.pdf)|*.pdf";
            dlg.Title = "打开电子发票文件";
            DialogResult result = dlg.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;

            SynchronizationContext ViewContext = SynchronizationContext.Current;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (o, ea) =>
            {
                foreach (string f in (string[])ea.Argument)
                {
                    try
                    {
                        ViewContext.Post(x => images.Add((Invoice)x), new Invoice(f));
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show("加载文件 " + f + " 失败: " + ex.Message);
                    }
                }
            };

            worker.RunWorkerCompleted += (o, ea) =>
            {
                listViewBusy.IsBusy = false;
            };
            listViewBusy.IsBusy = true;
            worker.RunWorkerAsync(dlg.FileNames);
        }


        private void btnPush_Click(object sender, RoutedEventArgs e)
        {
            Invoice inv = listView.SelectedItem as Invoice;

            string cmd = string.Format(
                "$(\"#fpdm\").val(\"{0}\");" +
                "$(\"#kjje\").focus();",
                inv.Code);
            browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync(cmd);
            Thread.Sleep(1000);
            cmd = string.Format(
                "$(\"#kjje\").val(\"{0}\");" +
                "$(\"#fphm\").val(\"{1}\");" +
                "$(\"#kprq\").val(\"{2:yyyyMMdd}\");" +
                "$(\"#yzm\").focus();",
                inv.BriefCheckCode, inv.Number, inv.Date);
            browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync(cmd);
        }

        private void btnShot_Click(object sender, RoutedEventArgs e)
        {
            int areaHeight = 0, areaWidth = 0;
            Position areaPos = null;
            Position dialogPos;
            JavaScriptSerializer json = new JavaScriptSerializer();
            Invoice inv = listView.SelectedItem as Invoice;

            string cmd = string.Format("JSON.stringify($(\"#dialog-body\").offset())");
            string dialogOffset = (string)browser.GetBrowser().MainFrame.EvaluateScriptAsync(cmd).Result.Result;
            if (dialogOffset == null) return;
            dialogPos = json.Deserialize<Position>(dialogOffset);

            browser.GetBrowser().GetFrameIdentifiers().ForEach(t =>
            {
                CefSharp.IFrame frame = browser.GetBrowser().GetFrame(t);
                if (frame.IsMain) return;
                else
                {
                    cmd = string.Format("JSON.stringify($(\"#print_area\").offset())");
                    string areaOffset = (string)frame.EvaluateScriptAsync(cmd).Result.Result;
                    areaPos = json.Deserialize<Position>(areaOffset);
                    cmd = string.Format("$(\"#print_area\").outerHeight()");
                    areaHeight = (int)frame.EvaluateScriptAsync(cmd).Result.Result;
                    cmd = string.Format("$(\"#print_area\").outerWidth()");
                    areaWidth = (int)frame.EvaluateScriptAsync(cmd).Result.Result;
                }
            });
            if (areaPos != null)
            {
                areaPos += dialogPos;
                Int32Rect rect = new Int32Rect((int)areaPos.Left, (int)areaPos.Top, areaWidth, areaHeight);
                System.Windows.Controls.Image contentImage = browser.Content as System.Windows.Controls.Image;
                BitmapSource bs = contentImage.Source.Clone() as BitmapSource;
                CroppedBitmap img = new CroppedBitmap(bs, rect);
                inv.Result = img;

            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog saveDialog = new FolderBrowserDialog()
            {
                Description = "选择保存路径",
            };
            if (saveDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            String path = saveDialog.SelectedPath;
            foreach (Invoice i in images)
            {
                if (i.Result != null)
                {
                    BmpBitmapEncoder bitmapEncoder = new BmpBitmapEncoder();
                    bitmapEncoder.Frames.Add(BitmapFrame.Create(i.Result));
                    FileStream fs = new FileStream(Path.Combine(path, Path.GetFileName(Path.ChangeExtension(i.File, ".bmp"))), FileMode.Create);
                    bitmapEncoder.Save(fs);
                    fs.Close();
                }
            }
        }

        private void listView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            OnPropertyChanged("ReadyToPush");
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
