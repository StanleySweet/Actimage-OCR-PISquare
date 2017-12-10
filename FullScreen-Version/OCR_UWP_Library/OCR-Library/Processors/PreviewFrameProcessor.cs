using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
namespace OCR_Library
{

    public abstract class PreviewFrameProcessor<T>
    {
        #region Attributes
        private PreviewFrameProcessedEventArgs<T> m_EventArgs;
        private MediaCapture m_MediaCapture;
        private Rect m_VideoSize;
        #endregion
        #region Properties
        public event EventHandler<PreviewFrameProcessedEventArgs<T>> FrameProcessed;
        protected abstract BitmapPixelFormat BitmapFormat
        {
            get;
        }
        #endregion

        #region Constructors
        public PreviewFrameProcessor(MediaCapture mediaCapture,
  VideoEncodingProperties videoEncodingProperties)
        {
            this.m_MediaCapture = mediaCapture;

            try
            {
                this.m_VideoSize = new Rect(0, 0, videoEncodingProperties.Width,
  videoEncodingProperties.Height);
            }
            catch (Exception)
            {
            }


            this.m_EventArgs = new PreviewFrameProcessedEventArgs<T>();
        } 
        #endregion
        public async Task RunFrameProcessingLoopAsync(CancellationToken token)
        {
            await Task.Run(async () =>
            {
                await this.InitialiseForProcessingLoopAsync();

                VideoFrame frame = new VideoFrame(this.BitmapFormat,
                  (int)this.m_VideoSize.Width, (int)this.m_VideoSize.Height);

                TimeSpan? lastFrameTime = null;

                try
                {
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();

                        await this.m_MediaCapture.GetPreviewFrameAsync(frame);

                        if ((!lastFrameTime.HasValue) ||
                          (lastFrameTime != frame.RelativeTime))
                        {
                            T results = await this.ProcessBitmapAsync(frame.SoftwareBitmap);

                            this.m_EventArgs.Frame = frame;
                            this.m_EventArgs.Results = results;

                            // This is going to fire on our thread here. Up to the caller to 
                            // 'do the right thing' which is a bit risky really.
                            this.FireFrameProcessedEvent();
                        }
                        lastFrameTime = frame.RelativeTime;
                    }
                }
                finally
                {
                    frame.Dispose();
                }
            },
            token);
        }
        protected abstract Task InitialiseForProcessingLoopAsync();

        protected abstract Task<T> ProcessBitmapAsync(SoftwareBitmap bitmap);
        private void FireFrameProcessedEvent()
        {
            var handlers = this.FrameProcessed;

            if (handlers != null)
            {
                handlers(this, this.m_EventArgs);
            }
        }
    }
}
