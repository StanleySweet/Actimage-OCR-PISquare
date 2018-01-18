namespace MediaFrameQrProcessing.Processors
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using VideoDeviceFinders;
    using Windows.Devices.Enumeration;
    using Windows.Media.Capture;
    using Windows.Media.Capture.Frames;
    using Windows.Media.Devices;

    public abstract class MediaCaptureFrameProcessor : IDisposable
    {
        #region Attributes
        private Action<VideoDeviceController> videoDeviceControllerInitialiser;
        private readonly string mediaEncodingSubtype;
        private readonly MediaFrameSourceFinder mediaFrameSourceFinder;
        private readonly DeviceInformation videoDeviceInformation;
        private readonly MediaCaptureMemoryPreference memoryPreference;
        public MediaFrameReader mediaFastFrameReader { get; private set; }
        private MediaCapture mediaCapture;
        #endregion

        protected MediaCaptureFrameProcessor(MediaFrameSourceFinder mediaFrameSourceFinder, DeviceInformation videoDeviceInformation, string mediaEncodingSubtype, MediaCaptureMemoryPreference memoryPreference = MediaCaptureMemoryPreference.Cpu)
        {
            this.mediaFrameSourceFinder = mediaFrameSourceFinder;
            this.videoDeviceInformation = videoDeviceInformation;
            this.mediaEncodingSubtype = mediaEncodingSubtype;
            this.memoryPreference = memoryPreference;
            this.mediaFastFrameReader = null;
        }

        public void SetVideoDeviceControllerInitialiser(Action<VideoDeviceController> initialiser)
        {
            this.videoDeviceControllerInitialiser = initialiser;
        }

        protected abstract Task<bool> ProcessFrameAsync(MediaFrameReference frameReference);

        /// <summary>
        /// Note: the natural thing to do here is what I used to do which is to create the
        /// MediaCapture inside of a using block.
        /// Problem is, that seemed to cause a situation where I could get a crash (AV) in
        ///
        /// Windows.Media.dll!Windows::Media::Capture::Frame::MediaFrameReader::CompletePendingStopOperation
        ///
        /// Which seemed to be related to stopping/disposing the MediaFrameReader and then
        /// disposing the media capture immediately after.
        /// 
        /// Right now, I've promoted the media capture to a member variable and held it around
        /// and instead of creating/disposing an instance each time one instance is kept
        /// indefinitely.
        /// It's not what I wanted...
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task ProcessFramesAsync(TimeSpan timeout)
        {
            try
            {
                await Task.Run(async () =>
                {
                    var startTime = DateTime.Now;
                    if (this.mediaCapture == null)
                        this.mediaCapture = await this.CreateMediaCaptureAsync();

                    var mediaFrameSource = this.mediaCapture.FrameSources[this.mediaFrameSourceFinder.FrameSourceInfo.Id];
                    using (var frameReader = await this.mediaCapture.CreateFrameReaderAsync(mediaFrameSource, this.mediaEncodingSubtype))
                    {
                        await frameReader.StartAsync();
                        await this.ProcessFramesAsyncLoop(frameReader, timeout, startTime);
                        await frameReader.StopAsync();
                    }
                });
            }catch(Exception ex)
            {
                Debug.WriteLine("Erreur : lors du traitement lent des images : " + ex);
            }

        }

        public async Task CreateMediaFastFrameReader()
        {
            if (this.mediaCapture == null) this.mediaCapture = await this.CreateMediaCaptureAsync();
            var mediaFrameSource = this.mediaCapture.FrameSources[this.mediaFrameSourceFinder.FrameSourceInfo.Id];

            if (mediaFastFrameReader == null)
            {
                mediaFastFrameReader = await this.mediaCapture.CreateFrameReaderAsync(mediaFrameSource, this.mediaEncodingSubtype);
                await mediaFastFrameReader.StartAsync();
            }
        }

        public async Task DisposeMediaFastFrameReader()
        {
            if (mediaFastFrameReader != null)
            {
                await mediaFastFrameReader.StopAsync();
                mediaFastFrameReader.Dispose();
                mediaFastFrameReader = null;
            }
        }

        /// <summary>
        /// Iterates until a result is found or timeout
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public async Task ProcessFramesAsyncLoop(MediaFrameReader frameReader, TimeSpan timeout, DateTime startTime)
        {
            bool done = false;
            while (!done)
            {
                using (var frame = frameReader.TryAcquireLatestFrame())
                {
                    if (frame != null)
                    {
                        done = await this.ProcessFrameAsync(frame);
                    }
                }
                if (!done)
                {
                    done = (DateTime.Now - startTime) > timeout;
                }
            }
        }

        async Task<MediaCapture> CreateMediaCaptureAsync()
        {
            var settings = new MediaCaptureInitializationSettings()
            {
                VideoDeviceId = this.videoDeviceInformation.Id,
                SourceGroup = this.mediaFrameSourceFinder.FrameSourceGroup,
                MemoryPreference = this.memoryPreference
            };

            var tempMediaCapture = new MediaCapture();

            await tempMediaCapture.InitializeAsync(settings);

            this.videoDeviceControllerInitialiser?.Invoke(tempMediaCapture.VideoDeviceController);

            return (tempMediaCapture);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.mediaCapture != null)
            {
                this.mediaCapture.Dispose();
                this.mediaCapture = null;
            }

            DisposeMediaFastFrameReader().Wait();
        }
    }
}