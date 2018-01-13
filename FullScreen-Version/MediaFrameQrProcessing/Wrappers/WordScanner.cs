namespace MediaFrameQrProcessing.Wrappers
{
    using MediaFrameQrProcessing.Entities;
    using MediaFrameQrProcessing.Processors;
    using MediaFrameQrProcessing.VideoDeviceFinders;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Windows.Devices.Enumeration;
    using Windows.Media.MediaProperties;

    public static class WordScanner
    {
        private static WordFrameProcessor m_FrameProcessor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resultCallback"></param>
        /// <param name="timeout"></param>
        /// <param name="requestWord"></param>
        public static async void ScanFirstCameraForWords(Action<List<ActiDetectedWord>> resultCallback, TimeSpan timeout, string requestWord)
        {

            List<ActiDetectedWord> result = null;

            // Note - I keep this frame processor around which means keeping the
            // underlying MediaCapture around because when I didn't keep it
            // around I ended up with a crash in Windows.Media.dll related
            // to disposing of the MediaCapture.
            // So...this isn't what I wanted but it seems to work better :-(
            if (m_FrameProcessor == null)
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
                        m_FrameProcessor = new WordFrameProcessor(requestWord,
                          mediaFrameSourceFinder,
                          videoCaptureDevice,
                          MediaEncodingSubtypes.Bgra8);

                        // Remember to ask for auto-focus on the video capture device.
                        m_FrameProcessor.SetVideoDeviceControllerInitialiser(
                          vd => vd.Focus.TrySetAuto(true));
                    }
                }
            }
            if (m_FrameProcessor != null)
            {
                DateTime startTime = DateTime.Now;
                Boolean done = false;
                TimeSpan currentTimeout = timeout;

                // Process frames for up to 30 seconds to see if we get any Words...
                // every frame that return something will bring us
                // back in this loop. so we call the call back to display what we found.
                // and we continue iterating until all time has run out.
                while (!done)
                {
                    // Get data from the frames 
                    await m_FrameProcessor.ProcessFramesAsync(currentTimeout);
                    // See what result we got.
                    result = m_FrameProcessor.Result;
                    // Call back with whatever result we got.
                    resultCallback(result);
                    // If we timed out just leave.
                    done = (DateTime.Now - startTime) > timeout;
                    // Continue scanning with the time we have left
                    currentTimeout = (DateTime.Now - startTime);
                }
            }
        }
    }
}
