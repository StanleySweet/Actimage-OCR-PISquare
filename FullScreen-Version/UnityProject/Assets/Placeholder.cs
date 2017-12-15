
using System;
using UnityEngine;
using UnityEngine.Windows.Speech;

#if !UNITY_EDITOR
    using HoloToolkit.Unity.InputModule;
    using MediaFrameQrProcessing.Wrappers;
    using System.Threading;
    using Windows.Media.MediaProperties;
    using Windows.Media.Ocr;
    using System.Linq;
#endif

public class Placeholder : MonoBehaviour
{
    private TextMesh m_TextMesh;
    private Canvas m_Canvas;
    private DictationRecognizer m_DictationRecognizer;
    private Boolean m_Resetted;

    private void Start()
    {
        m_TextMesh = GetComponentInChildren<TextMesh>();
        m_Canvas = GetComponentInChildren<Canvas>();
        m_Resetted = false;
        InitDictationRecognizer();
        this.m_TextMesh.text = "Reset in progress...";
        this.Reset();
    }

    private void InitDictationRecognizer()
    {
        m_DictationRecognizer = new DictationRecognizer();

        m_DictationRecognizer.DictationResult += (text, confidence) =>
        {
            this.m_TextMesh.text = text;
            this.m_TextMesh.text += RecognizeText(text);
        };

        m_DictationRecognizer.DictationHypothesis += (text) =>
        {
            this.m_TextMesh.text = text;
            if ("stop".Equals(text))
                Reset();
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

        if (SpeechSystemStatus.Running.Equals(PhraseRecognitionSystem.Status))
            PhraseRecognitionSystem.Shutdown();

        while (SpeechSystemStatus.Running.Equals(PhraseRecognitionSystem.Status)) ;

        if (m_Resetted)
        {
            m_Resetted = false;
            InitDictationRecognizer();
        }


        if (SpeechSystemStatus.Stopped.Equals(m_DictationRecognizer.Status))
            m_DictationRecognizer.Start();

    }

    /// <summary>
    /// This function is called on start and each time the users stop the application
    /// The aim is to switch between the different voices recognition systems.
    /// </summary>
    public void Reset()
    {
        this.m_TextMesh.text = "Reset in progress...";

        if (SpeechSystemStatus.Running.Equals(m_DictationRecognizer.Status))
            m_DictationRecognizer.Dispose();

        while (SpeechSystemStatus.Running.Equals(m_DictationRecognizer.Status)) { };
        m_Resetted = true;

        if (SpeechSystemStatus.Stopped.Equals(PhraseRecognitionSystem.Status))
            PhraseRecognitionSystem.Restart();

        this.m_TextMesh.text = "Reset done...";
        this.m_TextMesh.text = "Say 'Search for' to start";
    }

    public string RecognizeText(string text)
    {
        string resultStatistics = string.Empty;
#if !UNITY_EDITOR
        WordScanner.ScanFirstCameraForWords(
        result =>
        {
            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                resultStatistics = result.Where(r => r.IsExactMatch()) + " exact match(es) were found. " + result.Where(r => !r.IsExactMatch()) + "close match(es) were found";
            },
            false);
        },
        TimeSpan.FromSeconds(30), text);
#endif

        return resultStatistics;
    }
}
