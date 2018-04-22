using System;
using System.Globalization;
using System.Windows.Media.Imaging;
using ZXing;

namespace InvoiceQ
{
    class Invoice
    {
        public Invoice(BitmapSource source)
        {
            Image = source;

            MultiFormatReader reader = new MultiFormatReader();
            LuminanceSource luminanceSource = new BitmapSourceLuminanceSource(Image);
            Result result = reader.decode(new BinaryBitmap(new ZXing.Common.HybridBinarizer(luminanceSource)));
            RawText = result.Text;
            string[] splited = RawText.Split(',');
            Code = splited[2];
            Number = splited[3];
            Amount = double.Parse(splited[4]);
            Date = DateTime.ParseExact(splited[5], "yyyyMMdd", CultureInfo.InvariantCulture);
            CheckCode = splited[6];
        }

        public BitmapSource Image { get; set; }
        public string RawText { get; set; }
        public string Code { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public string CheckCode { get; set; }
        public string BriefCheckCode { get { return CheckCode.Substring(CheckCode.Length - 6); } }

        public override string ToString()
        {
            return RawText;
        }
    }
}
