namespace App183
{
  using System;
  using System.Threading.Tasks;
  using Windows.Graphics.Imaging;
  using Windows.Media.Capture;
  using Windows.Media.MediaProperties;
  using Windows.Media.Ocr;

  class OcrDetectionFrameProcessor : PreviewFrameProcessor<OcrResult>
  {
    public OcrDetectionFrameProcessor(MediaCapture capture,
      VideoEncodingProperties videoProperties) : base(capture, videoProperties)
    {

    }
    protected override BitmapPixelFormat BitmapFormat
    {
      get
      {
        // The OCR Engine supports anything convertible to Gray8 so I'm asking for
        // gray 8 in the first place.
        return (BitmapPixelFormat.Gray8);
      }
    }
    protected override async Task InitialiseForProcessingLoopAsync()
    {
      this.ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
    }
    protected override async Task<OcrResult> ProcessBitmapAsync(
      SoftwareBitmap bitmap)
    {
      var results = await this.ocrEngine.RecognizeAsync(bitmap);
      return (results);
    }
    OcrEngine ocrEngine;
  }
}