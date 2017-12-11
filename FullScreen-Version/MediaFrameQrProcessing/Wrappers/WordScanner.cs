using MediaFrameQrProcessing.Processors;
using MediaFrameQrProcessing.VideoDeviceFinders;
using System;
using Windows.Devices.Enumeration;
using Windows.Media.MediaProperties;

namespace MediaFrameQrProcessing.Wrappers
{
    public static class WordScanner
    {
        public static WordFrameProcessor m_FrameProcessor;

        public static async void ScanFirstCameraForWords(Action<string> resultCallback, TimeSpan timeout, string requestWord)
        {
            string result = string.Empty;

            // Note - I keep this frame processor around which means keeping the
            // underlying MediaCapture around because when I didn't keep it
            // around I ended up with a crash in Windows.Media.dll related
            // to disposing of the MediaCapture.
            // So...this isn't what I wanted but it seems to work better :-(
            if (m_FrameProcessor == null)
            {
                MediaFrameSourceFinder mediaFrameSourceFinder = new MediaFrameSourceFinder();
                // We want a source of media frame groups which contains a color video
                // preview (and we'll take the first one).
                var populated = await mediaFrameSourceFinder.PopulateAsync(MediaFrameSourceFinder.ColorVideoPreviewFilter, MediaFrameSourceFinder.FirstOrDefault);

                if (populated)
                {
                    // We'll take the first video capture device.
                    DeviceInformation videoCaptureDevice = await VideoCaptureDeviceFinder.FindFirstOrDefaultAsync();

                    if (videoCaptureDevice != null)
                    {
                        // Make a processor which will pull frames from the camera and run
                        m_FrameProcessor = new WordFrameProcessor(requestWord, mediaFrameSourceFinder, videoCaptureDevice, MediaEncodingSubtypes.Bgra8);
                        // Remember to ask for auto-focus on the video capture device.
                        m_FrameProcessor.SetVideoDeviceControllerInitialiser(vd => vd.Focus.TrySetAuto(true));
                    }
                }
            }
            if (m_FrameProcessor != null)
            {
                // Process frames for up to 30 seconds to see if we get any words...
                await m_FrameProcessor.ProcessFramesAsync(timeout);

                // See what result we got.
                result = m_FrameProcessor.Result;
            }
            // Call back with whatever result we got.
            resultCallback(result);
        }
    }
}
