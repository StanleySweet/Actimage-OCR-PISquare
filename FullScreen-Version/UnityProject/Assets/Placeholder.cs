namespace Assets
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Windows.Speech;
    using UnityEngine.XR;

#if !UNITY_EDITOR
    using MediaFrameQrProcessing.Wrappers;
    using MediaFrameQrProcessing.Entities;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;

#endif
    public class Placeholder : MonoBehaviour
    {
        #region Attributes
        private TextMesh m_TextMesh;
        private DictationRecognizer m_DictationRecognizer;
        private Boolean m_Searching;
        private Boolean m_Validated;
        private Boolean m_StartedRecognition;
        private String m_WordToSearch;
        private List<Rect> m_Rectangles;
        private List<Color> m_Colors;
        #endregion

        private void Start()
        {
            // Important : If this is not disabled dictation recognizer will 
            // not start
            // VR Causes issues on local windows.
            if (Constants.DEBUG)
                DisableVirtualReality();
            this.DisablePhraseRecognitionSystem();
            this.m_TextMesh = GetComponentInChildren<TextMesh>();
            this.m_Searching = false;
            this.m_Validated = false;
            this.m_WordToSearch = string.Empty;
            this.m_StartedRecognition = false;
            this.m_Rectangles = new List<Rect>();
            this.m_Colors = new List<Color>();
            this.InitDictationRecognizer();
            this.Reset();

            if (SpeechSystemStatus.Stopped.Equals(this.m_DictationRecognizer.Status))
                this.m_DictationRecognizer.Start();
        }

        //Rect ScaleBoxToCanvas(Rect boundingRect)
        //{
        //    float x = (boundingRect.x / (double)this.videoProperties.Width) * this.drawCanvas.ActualWidth;
        //    float y = (boundingRect.y / (double)this.videoProperties.Height) * this.drawCanvas.ActualHeight;
        //    float width = (boundingRect.width / (double)this.videoProperties.Width) * this.drawCanvas.ActualWidth;
        //    float height = (boundingRect.height / (double)this.videoProperties.Height) * this.drawCanvas.ActualHeight;
        //    return (new Rect(x, y, width, height));
        //}

        void Update()
        {
            Screen.fullScreen = true;
        }

        public void Scan(string text)
        {
            if (!this.m_Searching && Constants.SEARCH_KEYWORD.Equals(text))
            {
                AskForSearchWord();
            }
            else if (this.m_Searching)
            {

                if (Constants.VALIDATION_TEXT.Equals(text) && !String.IsNullOrEmpty(m_WordToSearch))
                {
                    ValidateSearch();
                    if (this.m_Validated)
                    {
                        if (!this.m_StartedRecognition)
                        {
                            this.m_StartedRecognition = true;
                            Task.Factory.StartNew(() =>
                            {
                                WordScanner.ScanFirstCameraForWords(GetWordsBoundingBoxes, TimeSpan.FromSeconds(Constants.TIMEOUT), this.m_WordToSearch.Equals("42") ? Constants.WILDCARD : this.m_WordToSearch);
                            });
                        }
                    }
                }
                else
                {
                    if (!Constants.VALIDATION_TEXT.Equals(text))
                        this.m_WordToSearch = text;

                    this.m_TextMesh.text = String.Format("Le mot à rechercher est {0} \n dites '{2}' pour continuer. \n Dites '{1}' pour annuler", this.m_WordToSearch, Constants.STOP_TEXT, Constants.VALIDATION_TEXT);
                }
            }

            if (Constants.STOP_TEXT.Equals(text))
                Reset();
        }

        /// <summary>
        /// This function is called on start and each time the users stop the application
        /// The aim is to switch between the different voices recognition systems.
        /// </summary>
        public void Reset(bool mainThread = true)
        {

            if (!mainThread)
            {
                UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                {
                    this.m_TextMesh.text = "Reset in progress...";
                }, false);
            }
            else
            {
                this.m_TextMesh.text = "Reset in progress...";
            }

            DelaySeconds(0.1f);

            this.m_Validated = false;
            this.m_Searching = false;
            this.m_StartedRecognition = false;

            if (!mainThread)
            {
                UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                {
                    this.m_TextMesh.text = string.Format("Dites '{0}' pour commencer", Constants.SEARCH_KEYWORD);
                }, false);
            }
            else
            {
                this.m_TextMesh.text = string.Format("Dites '{0}' pour commencer", Constants.SEARCH_KEYWORD);
            }


        }
#if !UNITY_EDITOR
        public void GetWordsBoundingBoxes(List<ActiDetectedWord> result)
        {
            String resultStatistics = String.Empty;
            try
            {
                m_Rectangles.Clear();
                m_Colors.Clear();
                foreach (var resultat in result)
                {
                    m_Rectangles.Add(new Rect(resultat.PosX, resultat.PosY, resultat.Width, resultat.Height));
                    m_Colors.Add(resultat.IsExactMatch() ? Constants.PERFECT_MATCH_COLOR : Constants.APPROXIMATE_MATCH_COLOR);
                }

                var numberOfExactMatches = result.Where(r => r.IsExactMatch()).Count();
                resultStatistics = numberOfExactMatches + " exact match(es) were found. \n " + (result.Count - numberOfExactMatches) + " close match(es) were found. \n for the word";
            }
            catch (Exception ex)
            {
                print("Erreur lors de la recherche de mots" + ex.Message);
                resultStatistics = string.Format("ERROR : 0 exact match(es) were found. 0 close match(es) were found for the word");
            }

            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                this.m_TextMesh.text = resultStatistics;
            }, false);

            Task.Factory.StartNew(() =>
            {
                DelaySeconds(5);
                Reset(false);
                DisplayWebcamPreview();
            });
        }
#endif

        private void DisplayWebcamPreview()
        {
            DisplayWebcam.tex.Play();
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
                    var borderRectangles = DrawRectOutlines(m_Rectangles[j], 2);
                    foreach (var borderRectangle in borderRectangles)
                        GUI.DrawTexture(borderRectangle, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, false, 1, m_Colors[j], 2, 5);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Erreur lors de l'affichage du/des rectangle(s). " + ex.Message);
            }

        }

        #region Debug

        private void DisableVirtualReality()
        {
            StartCoroutine(LoadDevice("", false));
        }

        IEnumerator LoadDevice(string newDevice, bool enable)
        {
            XRSettings.LoadDeviceByName(newDevice);
            yield return null;
            XRSettings.enabled = enable;
        }
        #endregion


        #region Utils
        private void DisablePhraseRecognitionSystem()
        {
            if (SpeechSystemStatus.Running.Equals(PhraseRecognitionSystem.Status))
                PhraseRecognitionSystem.Shutdown();
        }
        private void InitDictationRecognizer()
        {
            m_DictationRecognizer = new DictationRecognizer()
            {
                AutoSilenceTimeoutSeconds = -1,
                InitialSilenceTimeoutSeconds = -1
            };


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
                {
                    Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);
                    this.m_DictationRecognizer.Dispose();
                    InitDictationRecognizer();
                    Reset();
                }
            };

            this.m_DictationRecognizer.DictationError += (error, hresult) =>
            {
                Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
            };
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
        private void AskForSearchWord()
        {
            this.m_TextMesh.text = string.Format("Donnez le mot à rechercher. \n Dites '{0}' pour annuler.", Constants.STOP_TEXT);
            this.m_Searching = true;
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
        private IEnumerator DelaySeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }
       #endregion
    }
}