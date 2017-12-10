using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Media.Ocr;

namespace OCR_Library
{
    public class OcrDetectionFrameProcessor : PreviewFrameProcessor<OcrResult>
    {
        #region Attributes
        private OcrEngine m_OcrEngine; 
        #endregion
        #region Constructors
        public OcrDetectionFrameProcessor(MediaCapture capture,
  VideoEncodingProperties videoProperties) : base(capture, videoProperties)
        {

        }
        #endregion
        #region Properties
        protected override BitmapPixelFormat BitmapFormat
        {
            get
            {
                // The OCR Engine supports anything convertible to Gray8 so I'm asking for
                // gray 8 in the first place.
                return (BitmapPixelFormat.Gray8);
            }
        } 
        #endregion
        protected override async Task InitialiseForProcessingLoopAsync()
        {
            this.m_OcrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
        }
        protected override async Task<OcrResult> ProcessBitmapAsync(
          SoftwareBitmap bitmap)
        {
            var results = await this.m_OcrEngine.RecognizeAsync(bitmap);
            return (results);
        }
    }
}
