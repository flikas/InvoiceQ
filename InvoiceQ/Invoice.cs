using Spire.Pdf;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.QrCode;

namespace InvoiceQ
{
    class Invoice : INotifyPropertyChanged
    {
        private BitmapSource result;

        public Invoice(String file)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                PdfDocument doc = new PdfDocument();
                doc.LoadFromFile(file);
                Image img = doc.SaveAsImage(0);
                img.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();
                doc.Close();
                Image = bi;
            }
            QRCodeReader reader = new QRCodeReader();
            LuminanceSource luminanceSource = new BitmapSourceLuminanceSource(Image);
            Result result = reader.decode(new BinaryBitmap(new ZXing.Common.HybridBinarizer(luminanceSource)));
            if ( result == null)
            {
                throw new FileFormatException("未找到发票二维码");
            }
            File = file;
            RawText = result.Text;
            string[] splited = RawText.Split(',');
            Code = splited[2];
            Number = splited[3];
            Amount = double.Parse(splited[4]);
            Date = DateTime.ParseExact(splited[5], "yyyyMMdd", CultureInfo.InvariantCulture);
            CheckCode = splited[6];
        }

        public string File { get; set; }
        public BitmapSource Image { get; set; }
        public string RawText { get; set; }
        public string Code { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public string CheckCode { get; set; }
        public string BriefCheckCode { get { return CheckCode.Substring(CheckCode.Length - 6); } }

        public BitmapSource Result
        {
            get { return result; }
            set { result = value; NotifyPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return RawText;
        }

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            Invoice invoice = obj as Invoice;
            if (invoice == null) return false;
            return this.File.Equals(invoice.File);
        }

        public override int GetHashCode()
        {
            return this.File.GetHashCode();
        }
    }
}
