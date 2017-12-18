namespace Assets
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Windows.Speech;

#if !UNITY_EDITOR
    using HoloToolkit.Unity.InputModule;
    using System.Threading;
    using Windows.Media.MediaProperties;
    using MediaFrameQrProcessing.Wrappers;
    using MediaFrameQrProcessing.Entities;
    using Windows.Media.Ocr;
    using System.Linq;
#endif
    public class Placeholder : MonoBehaviour
    {
        private TextMesh m_TextMesh;
        private DictationRecognizer m_DictationRecognizer;
        private Boolean m_Searching;
        private Boolean m_Validated;
        private Boolean m_StartedRecognition;
        private String m_WordToSearch;
        private List<Rect> m_Rectangles;
        private List<Color> m_Colors;
        private System.Diagnostics.Stopwatch m_StopWatch;

        private void Start()
        {
            // Important : If this is not disabled dictation recognizer will 
            // not start
            this.DisablePhraseRecognitionSystem();
            this.m_TextMesh = GetComponentInChildren<TextMesh>();
            this.m_Searching = false;
            this.m_Validated = false;
            this.m_StartedRecognition = false;
            this.m_Rectangles = new List<Rect>();
            this.m_Colors = new List<Color>();
            this.m_StopWatch = System.Diagnostics.Stopwatch.StartNew();
            this.InitDictationRecognizer();
            this.Reset();

            if (SpeechSystemStatus.Stopped.Equals(this.m_DictationRecognizer.Status))
                this.m_DictationRecognizer.Start();
        }

        private void DisablePhraseRecognitionSystem()
        {
            if (SpeechSystemStatus.Running.Equals(PhraseRecognitionSystem.Status))
                PhraseRecognitionSystem.Shutdown();
        }

        private void InitDictationRecognizer()
        {
            m_DictationRecognizer = new DictationRecognizer();

            this.m_DictationRecognizer.DictationResult += (text, confidence) =>
            {
                Scan(text);
            };

            this.m_DictationRecognizer.DictationHypothesis += (text) =>
            {
            };

            this.m_DictationRecognizer.DictationComplete += (completionCause) =>
            {
                if (!completionCause.Equals(DictationCompletionCause.Complete))
                    Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);
            };

            this.m_DictationRecognizer.DictationError += (error, hresult) =>
            {
                Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
            };
        }
        public void Scan(string text)
        {
            if (!this.m_Searching && Constants.SEARCH_KEYWORD.Equals(text))
            {
                PromptForSearchWord();
                return;
            }


            if (!this.m_Validated)
            {
                if (!Constants.VALIDATION_TEXT.Equals(text))
                    this.m_WordToSearch = text;

                this.m_TextMesh.text = String.Format("Le mot à rechercher est {0} \n dites 'valider' pour continuer.", this.m_WordToSearch);
            }

            if (Constants.VALIDATION_TEXT.Equals(text))
                ValidateSearch();

            if (this.m_Validated && !this.m_StartedRecognition)
                StartRecognition();

            if (Constants.STOP_TEXT.Equals(text) || TimeoutExceeded())
                Reset();

        }

        private void ValidateSearch()
        {
            this.m_TextMesh.text = String.Format("Recherche en cours pour le mot '{0}'.", this.m_WordToSearch);
            this.m_Validated = true;
        }

        /// <summary>
        /// Init the search by setting the flag
        /// Displays a message to the user.
        /// </summary>
        private void PromptForSearchWord()
        {
            this.m_TextMesh.text = "Donnez le mot à rechercher. \n Dites 'Stop' pour annuler.";
            this.m_Searching = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns> Wether the 30 seconds have been elapsed</returns>
        private bool TimeoutExceeded()
        {
            return this.m_StopWatch.Elapsed.Seconds > Constants.TIMEOUT;
        }

        /// <summary>
        /// Start text recognition.
        /// </summary>
        private void StartRecognition()
        {
            this.m_StartedRecognition = true;
            this.m_StopWatch.Start();
            this.m_TextMesh.text = RecognizeText(this.m_WordToSearch);
        }


        /// <summary>
        /// This function is called on start and each time the users stop the application
        /// The aim is to switch between the different voices recognition systems.
        /// </summary>
        public void Reset()
        {
            this.m_StopWatch.Reset();
            this.m_TextMesh.text = "Reset in progress...";
            this.m_Validated = false;
            this.m_Searching = false;
            this.m_TextMesh.text = "Say 'Rechercher' to start";

        }

        public string RecognizeText(string text)
        {
            if (text == "42")
                text = "quarante deux";


            string resultStatistics = string.Empty;

            try
            {
#if !UNITY_EDITOR
                List<ActiDetectedWord> resultat = new List<ActiDetectedWord>();
                WordScanner.ScanFirstCameraForWords(
                result =>
                {
                    m_Rectangles.Clear();
                    m_Colors.Clear();

                    for (Int16 i = 0; i < result.Count; ++i)
                    {
                        m_Rectangles.Add(new Rect(result[i].PosX, result[i].PosY, result[i].Width, result[i].Height));
                        m_Colors.Add(result[i].IsExactMatch() ? Constants.PERFECT_MATCH_COLOR : Constants.APPROXIMATE_MATCH_COLOR);
                    }
                    resultat = result;
                },
                TimeSpan.FromSeconds(Constants.TIMEOUT), text);
                resultStatistics = resultat.Where(r => r.IsExactMatch()).Count() + " exact match(es) were found. \n " + resultat.Where(r => !r.IsExactMatch()).Count() + " close match(es) were found. \n for the word " + text;
#endif
            }
            catch (Exception ex)
            {
                print("Erreur lors de la recherche de mots" + ex.Message);
                resultStatistics = string.Format("0 exact match(es) were found. 0 close match(es) were found for the word {0}", text);
            }

            return resultStatistics;
        }

        /// <summary>
        /// Called automatically to the engine to refresh the UI.
        /// </summary>
        void OnGUI()
        {
#if UNITY_EDITOR
            m_Rectangles.Add(new Rect(75, 50, 200, 50));
            m_Rectangles.Add(new Rect(200, 500, 200, 50));
            m_Colors.Add(Constants.PERFECT_MATCH_COLOR);
            m_Colors.Add(Constants.APPROXIMATE_MATCH_COLOR);
#endif
            try
            {
                for (Int16 j = 0; j < m_Rectangles.Count; ++j)
                {
                    var borderRectangels = DrawRectOutlines(m_Rectangles[j], 2);
                    for (Int16 i = 0; i < borderRectangels.Count; ++i)
                        GUI.DrawTexture(borderRectangels[i], Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, false, 1, m_Colors[j], 2, 5);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Erreur lors de l'affichage du/des rectangle(s). " + ex.Message);
            }

        }
        /// <summary>
        /// Return a list of rectangles to make the outline.
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="borderWidth"></param>
        /// <returns></returns>
        private List<Rect> DrawRectOutlines(Rect rectangle, int borderWidth)
        {
            var rectangles = new List<Rect>()
        {
            new Rect(rectangle.x, rectangle.y, rectangle.width, borderWidth),
            new Rect(rectangle.x, rectangle.y + rectangle.height - borderWidth, rectangle.width, borderWidth),
            new Rect(rectangle.x + rectangle.width - borderWidth, rectangle.y + borderWidth,borderWidth, rectangle.height - (borderWidth * 2)),
            new Rect(rectangle.x, rectangle.y + borderWidth,borderWidth, rectangle.height - (borderWidth * 2))
        };
            return rectangles;
        }
        private IEnumerator DelaySeconds(int seconds)
        {
            yield return new WaitForSeconds(seconds);
        }
    }

}