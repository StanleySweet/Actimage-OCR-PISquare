namespace MediaFrameQrProcessing.ZXing
{
    using global::ZXing;
    using global::ZXing.Common;
    using System.Diagnostics;
    using System.Runtime.InteropServices.WindowsRuntime;
    using Windows.Graphics.Imaging;
    using Windows.Storage.Streams;
    using static global::ZXing.RGBLuminanceSource;

    public static class ZXingQRCodeDecoder
    {
        public static BarcodeReader BarcodeReader;

        static ZXingQRCodeDecoder()
        {
            BarcodeReader = new BarcodeReader();
            BarcodeReader.Options.TryHarder = true;
            BarcodeReader.Options.PureBarcode = false;
        }

        public static Result DecodeBufferToQRCode(SoftwareBitmap barcodeBitmap)
        {
            Result result = null;
            try
            {
                result = (BarcodeReader.Decode(barcodeBitmap));
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("Erreur lors du décodage du QR code : "+  ex.Message);
            }
            return result;
        }
        public static Result DecodeBufferToQRCode(byte[] buffer, int width, int height, BitmapFormat bitmapFormat)
        {
            return (BarcodeReader.Decode(buffer, width, height, bitmapFormat));
        }
        public static Result DecodeBufferToQRCode(IBuffer buffer, int width, int height, BitmapFormat bitmapFormat)
        {
            return (DecodeBufferToQRCode(buffer.ToArray(), width, height, bitmapFormat));
        }
    }
}