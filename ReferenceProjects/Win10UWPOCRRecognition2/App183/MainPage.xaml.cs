namespace App183
{
  using System;
  using System.Linq;
  using System.Threading;
  using Windows.Foundation;
  using Windows.Media.Capture;
  using Windows.Media.MediaProperties;
  using Windows.Media.Ocr;
  using Windows.UI.Core;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;

  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
      this.Loaded += OnLoaded;
    }
    async void OnLoaded(object sender, RoutedEventArgs args)
    {
      // NB: this example never sets this but it could.
      CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

      this.cameraPreviewManager = new CameraPreviewManager(this.captureElement);

      this.videoProperties =
        await this.cameraPreviewManager.StartPreviewToCaptureElementAsync(
          vcd => vcd.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front);

      this.ocrProcessor = new OcrDetectionFrameProcessor(
        this.cameraPreviewManager.MediaCapture, videoProperties);

      this.ocrProcessor.FrameProcessed += (s, e) =>
      {
        this.Dispatcher.RunAsync(
          CoreDispatcherPriority.Normal,
          () =>
          {
            this.DrawOcrResults(e.Results);
          });
      };
      // NB: we provide no way to cancel this - we just run forever.
      await this.ocrProcessor.RunFrameProcessingLoopAsync(
        cancellationTokenSource.Token);
    }
    void DrawOcrResults(OcrResult ocrResult)
    {
      if ((ocrResult == null) || 
        (ocrResult.Lines == null) || 
        (ocrResult.Lines.Count == 0))
      {
        this.drawCanvas.Children.Clear();
        this.txtWholeText.Text = string.Empty;
      }
      else
      {
        var words = ocrResult.Lines.SelectMany(l => l.Words).ToList();

        // draw any words that we have recognised, doing our best to
        // make use of rectangles that are already on the canvas.
        for (int i = 0; i < words.Count; i++)
        {
          var word = words[i];
          DisplayBox drawBox = CreateOrReuseDiplayBox(i);

          var scaledBox = this.ScaleBoxToCanvas(word.BoundingRect);
          Canvas.SetLeft(drawBox, scaledBox.Left);
          Canvas.SetTop(drawBox, scaledBox.Top);
          drawBox.Width = scaledBox.Width;
          drawBox.Height = scaledBox.Height;
          drawBox.Text = word.Text;
          this.rotateTransform.Angle = ocrResult.TextAngle ?? 0.0d;
        }

        // Get rid of any rectangles that we have which no longer represent words
        // that we have recognised.
        for (int i = this.drawCanvas.Children.Count - 1; i >= words.Count; i--)
        {
          this.drawCanvas.Children.RemoveAt(i);
        }
        this.txtWholeText.Text = ocrResult.Text;
      }
    }
    Rect ScaleBoxToCanvas(Rect boundingRect)
    {
      double x = (boundingRect.X / (double)this.videoProperties.Width) * this.drawCanvas.ActualWidth;
      double y = (boundingRect.Y / (double)this.videoProperties.Height) * this.drawCanvas.ActualHeight;
      double width = (boundingRect.Width / (double)this.videoProperties.Width) * this.drawCanvas.ActualWidth;
      double height = (boundingRect.Height / (double)this.videoProperties.Height) * this.drawCanvas.ActualHeight;
      return (new Rect(x, y, width, height));
    }
    DisplayBox CreateOrReuseDiplayBox(int index)
    {
      DisplayBox box = null;
      if (index < this.drawCanvas.Children.Count)
      {
        box = this.drawCanvas.Children[index] as DisplayBox;
      }
      else
      {
        box = new DisplayBox();
        this.drawCanvas.Children.Add(box);
      }
      return (box);
    }
    OcrDetectionFrameProcessor ocrProcessor;
    CameraPreviewManager cameraPreviewManager;
    VideoEncodingProperties videoProperties;
  }
}