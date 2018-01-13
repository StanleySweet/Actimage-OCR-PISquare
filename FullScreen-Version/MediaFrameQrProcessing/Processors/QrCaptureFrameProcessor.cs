namespace MediaFrameQrProcessing.Processors
{
    using global::ZXing;
    using MediaFrameQrProcessing.VideoDeviceFinders;
    using MediaFrameQrProcessing.ZXing;
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading.Tasks;
    using Windows.Devices.Enumeration;
    using Windows.Media.Capture;
    using Windows.Media.Capture.Frames;
    using static global::ZXing.RGBLuminanceSource;

    public class QrCaptureFrameProcessor : MediaCaptureFrameProcessor
    {
        public String Result { get; private set; }

        public QrCaptureFrameProcessor(MediaFrameSourceFinder mediaFrameSourceFinder, DeviceInformation videoDeviceInformation, string mediaEncodingSubtype, MediaCaptureMemoryPreference memoryPreference = MediaCaptureMemoryPreference.Cpu) : base(mediaFrameSourceFinder, videoDeviceInformation, mediaEncodingSubtype, memoryPreference)
        {
            this.Result = string.Empty;
        }

        protected override async Task<bool> ProcessFrameAsync(MediaFrameReference frameReference)
        {
            await Task.Run(() =>
            {
                // doc here https://msdn.microsoft.com/en-us/library/windows/apps/xaml/windows.media.capture.frames.videomediaframe.aspx
                // says to dispose this softwarebitmap if you access it.
                using (var bitmap = frameReference.VideoMediaFrame.SoftwareBitmap)
                {
                    try
                    {
                        Result zxingResult = ZXingQRCodeDecoder.DecodeBufferToQRCode(bitmap);

                        if (zxingResult != null)
                        {
                            this.Result = zxingResult.Text;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Write("Erreur lors de la récupération du QR code " + ex.Message);
                    }
                }
            });
            return (this.Result != string.Empty);
        }
    }
}