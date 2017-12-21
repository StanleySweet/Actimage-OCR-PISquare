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
        /// Note - I keep this frame processor around which means keeping the
        /// underlying MediaCapture around because when I didn't keep it
        /// around I ended up with a crash in Windows.Media.dll related
        /// to disposing of the MediaCapture.
        /// So...this isn't what I wanted but it seems to work better :-(
        /// </summary>
        /// <param name="requestWord">The word you want to look for</param>
        /// <returns></returns>
        private static async Task<WordFrameProcessor> GetFrameProcessor(string requestWord)
        {
            WordFrameProcessor frameProcessor = null;
            try
            {
                MediaFrameSourceFinder mediaFrameSourceFinder = null;
                DeviceInformation videoCaptureDevice = null;
                try
                {
                    mediaFrameSourceFinder = new MediaFrameSourceFinder();
                    // We want a source of media frame groups which contains a color video
                    // preview (and we'll take the first one).
                    bool populated = await mediaFrameSourceFinder.PopulateAsync(MediaFrameSourceFinder.ColorVideoPreviewFilter, MediaFrameSourceFinder.FirstOrDefault);
                    if (!populated)
                        return null;
                }
                catch (Exception ex)
                {
                    Debug.Write("Erreur lors de la récupération du MediaFrameSourceFinder. \n" + ex.Message);
                }
                try
                {
                    // We'll take the first video capture device.
                    videoCaptureDevice = await VideoCaptureDeviceFinder.FindFirstOrDefaultAsync();
                    if (videoCaptureDevice == null)
                        return null;
                }
                catch (Exception ex)
                {
                    Debug.Write("Erreur lors de la récupération du DeviceInformation. \n" + ex.Message);
                }

                // Make a processor which will pull frames from the camera and run
                frameProcessor = new WordFrameProcessor(requestWord, mediaFrameSourceFinder, videoCaptureDevice, MediaEncodingSubtypes.Bgra8);
                // Remember to ask for auto-focus on the video capture device.
                frameProcessor.SetVideoDeviceControllerInitialiser(vd => vd.Focus.TrySetAuto(true));


            }

            catch (Exception e)
            {
                Debug.Write("Erreur lors de la récupération du frame processor. \n" + e.Message);
            }
            return frameProcessor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resultCallback"></param>
        /// <param name="timeout"></param>
        /// <param name="requestWord"></param>
        //public static async void ScanFirstCameraForWords(Action<List<ActiDetectedWord>> resultCallback, TimeSpan timeout, string requestWord)
        //{
        //    List<ActiDetectedWord> result = new List<ActiDetectedWord>();
        //    try
        //    {

        //        if (m_FrameProcessor == null)
        //            m_FrameProcessor = await GetFrameProcessor(requestWord);

        //        if (m_FrameProcessor != null)
        //        {
        //            // Process frames for up to 30 seconds to see if we get any words...
        //            await m_FrameProcessor.ProcessFramesAsync(timeout);
        //            result = m_FrameProcessor.Result;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.Write("Erreur lors du scan de la webcam. " + ex.Message);
        //    }
        //    // Call back with whatever result we got.
        //    resultCallback(result);
        //}

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
                // Process frames for up to 30 seconds to see if we get any QR codes...
                await m_FrameProcessor.ProcessFramesAsync(timeout);

                // See what result we got.
                result = m_FrameProcessor.Result;
            }
            // Call back with whatever result we got.
            resultCallback(result);
        }
    }
}
