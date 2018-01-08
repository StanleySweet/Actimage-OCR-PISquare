using System.Threading;
using UnityEngine;
using UnityEngine.Windows.Speech;

#if !UNITY_EDITOR
    using OCR_Library;
    using Windows.Media.MediaProperties;
    using Windows.UI.Xaml.Controls;
    using Windows.Media.Ocr;
#endif

public class Placeholder : MonoBehaviour
{
    private TextMesh m_TextMesh;
    private DictationRecognizer m_DictationRecognizer;


#if !UNITY_EDITOR
    private CancellationTokenSource m_CancellationTokenSource;
    private OcrDetectionFrameProcessor m_OcrProcessor;
    private CameraPreviewManager m_CameraPreviewManager;
    private VideoEncodingProperties m_VideoProperties;
    private CaptureElement m_CaptureElement;
#endif



    private async void DisplayCameraPreview()
    {

    }




    private async void Start()
    {
        m_TextMesh = GetComponentInChildren<TextMesh>();
        m_DictationRecognizer = new DictationRecognizer();

#if !UNITY_EDITOR
        // TODO : Add something to the unity scene broken
        this.m_CameraPreviewManager = new CameraPreviewManager(this.m_CaptureElement);

        this.m_VideoProperties =
          await this.m_CameraPreviewManager.StartPreviewToCaptureElementAsync(
            vcd => vcd.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front);

        this.m_OcrProcessor = new OcrDetectionFrameProcessor(
          this.m_CameraPreviewManager.MediaCapture, this.m_VideoProperties);

        this.m_OcrProcessor.FrameProcessed += (s, e) =>
        {
            //this.Dispatcher.RunAsync(
            //  CoreDispatcherPriority.Normal,
            //  () =>
            //  {
            //      this.DrawOcrResults(e.Results);
            //  });
        };

#endif
        InitDictationRecognizer();
        this.OnReset();
    }

#if !UNITY_EDITOR
    //void DrawOcrResults(OcrResult ocrResult)
    //{
    //    if ((ocrResult == null) ||
    //      (ocrResult.Lines == null) ||
    //      (ocrResult.Lines.Count == 0))
    //    {
    //        this.drawCanvas.Children.Clear();
    //        this.txtWholeText.Text = string.Empty;
    //    }
    //    else
    //    {
    //        var words = ocrResult.Lines.SelectMany(l => l.Words).ToList();

    //        // draw any words that we have recognised, doing our best to
    //        // make use of rectangles that are already on the canvas.
    //        for (int i = 0; i < words.Count; i++)
    //        {
    //            var word = words[i];
    //            DisplayBox drawBox = CreateOrReuseDiplayBox(i);

    //            var scaledBox = this.ScaleBoxToCanvas(word.BoundingRect);
    //            Canvas.SetLeft(drawBox, scaledBox.Left);
    //            Canvas.SetTop(drawBox, scaledBox.Top);
    //            drawBox.Width = scaledBox.Width;
    //            drawBox.Height = scaledBox.Height;
    //            drawBox.Text = word.Text;
    //            this.rotateTransform.Angle = ocrResult.TextAngle ?? 0.0d;
    //        }

    //        // Get rid of any rectangles that we have which no longer represent words
    //        // that we have recognised.
    //        for (int i = this.drawCanvas.Children.Count - 1; i >= words.Count; i--)
    //        {
    //            this.drawCanvas.Children.RemoveAt(i);
    //        }
    //        this.txtWholeText.Text = ocrResult.Text;
    //    }
    //}
#endif

    private void InitDictationRecognizer()
    {
        m_DictationRecognizer.DictationResult += (text, confidence) =>
        {
            this.m_TextMesh.text = text;
        };

        m_DictationRecognizer.DictationHypothesis += (text) =>
        {
            this.m_TextMesh.text = text;
            if ("stop".Equals(text))
                OnReset();
        };

        m_DictationRecognizer.DictationComplete += (completionCause) =>
        {
            if (completionCause != DictationCompletionCause.Complete)
                Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);
        };

        m_DictationRecognizer.DictationError += (error, hresult) =>
        {
            Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
        };

    }
    public void OnScan()
    {
        this.m_TextMesh.text = "Scanning for 30s";

        if (PhraseRecognitionSystem.Status.Equals(SpeechSystemStatus.Running))
            PhraseRecognitionSystem.Shutdown();

        if (m_DictationRecognizer.Status.Equals(SpeechSystemStatus.Stopped))
            m_DictationRecognizer.Start();

        RecognizeText();

    }

    /// <summary>
    /// This function is called on start and each time the users stop the application
    /// The aim is to switch between the different voices recognition systems.
    /// </summary>
    public void OnReset()
    {
        this.m_TextMesh.text = "Reset in progress...";

        if (m_DictationRecognizer.Status.Equals(SpeechSystemStatus.Running))
            m_DictationRecognizer.Stop();

        if (PhraseRecognitionSystem.Status.Equals(SpeechSystemStatus.Stopped))
            PhraseRecognitionSystem.Restart();

#if !UNITY_EDITOR
        m_CancellationTokenSource?.Cancel();
#endif

        this.m_TextMesh.text = "Say 'Search for' to start";
    }

    public void RecognizeText()
    {



#if !UNITY_EDITOR
    m_CancellationTokenSource = new CancellationTokenSource();
    this.m_OcrProcessor.RunFrameProcessingLoopAsync(m_CancellationTokenSource.Token);
#endif



    }
}
