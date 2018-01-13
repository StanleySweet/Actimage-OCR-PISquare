﻿namespace MediaFrameQrProcessing.Wrappers
{
    using MediaFrameQrProcessing.Processors;
    using MediaFrameQrProcessing.VideoDeviceFinders;
    using System;
    using Windows.Media.MediaProperties;

    public static class ZXingQrCodeScanner
    {
        private static QrCaptureFrameProcessor m_frameProcessor;


        /// <summary>
        /// Brings together the pieces to do a scan for a QR code from the first
        /// camera that it finds on the system. You might have more success/
        /// flexibility by just using the pieces directly but I was trying to
        /// package things up into a simple, single method here.
        /// </summary>
        /// <param name="resultCallback">Your function to be called back when we 
        /// find a QR code. Note that you get called with null if we time out
        /// while looking for one and you will get called multiple times if
        /// you have not chosen to pass a timeout</param>
        /// <param name="timeout">An optional timeout. If you pass it then we
        /// will stop after that period. Otherwise, we'll run continually.
        /// </param>
        public static async void ScanFirstCameraForQrCode(Action<string> resultCallback, TimeSpan timeout)
        {
            string result = null;

            // Note - I keep this frame processor around which means keeping the
            // underlying MediaCapture around because when I didn't keep it
            // around I ended up with a crash in Windows.Media.dll related
            // to disposing of the MediaCapture.
            // So...this isn't what I wanted but it seems to work better :-(
            if (m_frameProcessor == null)
            {
                var mediaFrameSourceFinder = new MediaFrameSourceFinder();

                // We want a source of media frame groups which contains a color video
                // preview (and we'll take the first one).
                var populated = await mediaFrameSourceFinder.PopulateAsync(
                  MediaFrameSourceFinder.ColorVideoPreviewFilter,
                  MediaFrameSourceFinder.FirstOrDefault);

                if (populated)
                {
                    // We'll take the first video capture device.
                    var videoCaptureDevice =
                      await VideoCaptureDeviceFinder.FindFirstOrDefaultAsync();

                    if (videoCaptureDevice != null)
                    {
                        // Make a processor which will pull frames from the camera and run
                        // ZXing over them to look for QR codes.
                        m_frameProcessor = new QrCaptureFrameProcessor(mediaFrameSourceFinder, videoCaptureDevice, MediaEncodingSubtypes.Bgra8);

                        // Remember to ask for auto-focus on the video capture device.
                        m_frameProcessor.SetVideoDeviceControllerInitialiser(
                          vd => vd.Focus.TrySetAuto(true));
                    }
                }
            }
            if (m_frameProcessor != null)
            {
                // Process frames for up to 30 seconds to see if we get any QR codes...
                await m_frameProcessor.ProcessFramesAsync(timeout);
                // See what result we got.
                result = m_frameProcessor.Result;
            }
            // Call back with whatever result we got.
            resultCallback(result);
        }
    }
}