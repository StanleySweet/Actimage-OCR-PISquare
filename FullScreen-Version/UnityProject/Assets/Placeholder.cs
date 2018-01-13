namespace Assets
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Windows.Speech;
    using UnityEngine.XR;
    using System.Threading.Tasks;

#if !UNITY_EDITOR
    using MediaFrameQrProcessing;
    using MediaFrameQrProcessing.Wrappers;
    using MediaFrameQrProcessing.Entities;
    using System.Threading;
    using System.Linq;
    using AssemblyCSharpWSA;
#endif
    public class Placeholder : MonoBehaviour
    {
        #region Attributes
        private TextMesh m_TextMesh;
        private Transform m_PlaneMesh;
        private DictationRecognizer m_DictationRecognizer;
        private Boolean m_Searching;
        private Boolean m_Validated;
        private Boolean m_Decrypting;
        private Boolean m_StartedRecognition;
        private String m_WordToSearch;
        private List<Rect> m_Rectangles;
        private List<Color> m_Colors;
        #endregion

        /// <summary>
        /// Awake is used to initialize any variables or game state before
        /// the game starts. Awake is called only once during the lifetime
        /// of the script instance. Awake is called after all objects are
        /// initialized so you can safely speak to other objects or query
        /// them using eg. GameObject.FindWithTag. Each GameObject's Awake
        /// is called in a random order between objects. Because of this,
        /// you should use Awake to set up references between scripts,
        /// and use Start to pass any information back and forth. Awake
        /// is always called before any Start functions. This allows you
        /// to order initialization of scripts. Awake can not act as a coroutine.
        /// </summary>
        public void Awake()
        {
            this.m_TextMesh = GetComponentInChildren<TextMesh>();
            this.m_Searching = false;
            this.m_Validated = false;
            this.m_Decrypting = false;
            this.m_WordToSearch = string.Empty;
            this.m_StartedRecognition = false;
            this.m_Rectangles = new List<Rect>();
            this.m_Colors = new List<Color>();
            this.m_DictationRecognizer = null;
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just
        /// before any of the Update methods is called the first time.
        /// Like the Awake function, Start is called exactly once in
        /// the lifetime of the script.However, Awake is called when
        /// the script object is initialised, regardless of whether
        /// or not the script is enabled.Start may not be called on
        /// the same frame as Awake if the script is not enabled 
        /// at initialisation time.
        /// The Awake function is called on all objects in the scene
        /// before any object's Start function is called. This fact
        /// is useful in cases where object A's initialisation code
        /// needs to rely on object B's already being initialised;
        /// B's initialisation should be done in Awake while A's 
        /// should be done in Start.
        /// Where objects are instantiated during gameplay, their
        /// Awake function will naturally be called after the Start
        /// functions of scene objects have already completed.
        /// </summary>
        public void Start()
        {
            if (Constants.DEBUG)
            {
                // VR Causes issues on local windows.
                this.DisableVirtualReality();
                this.m_PlaneMesh = GetDebugPlane();
            }

            this.Reset();
            this.InitDictationRecognizer();
        }


        void Update()
        {
            Screen.fullScreen = true;
        }

        public void ClearLists()
        {
            this.m_Rectangles.Clear();
            this.m_Colors.Clear();
        }

        public void Scan(string text)
        {
            if (!this.m_Searching && Constants.SEARCH_KEYWORD.Equals(text) && !this.m_Decrypting)
            {
                this.m_TextMesh.text = Constants.HOME_PROMPT;
                this.m_Searching = true;
                FunctionProxyOnMainThread(ClearLists);
            }
            else if (!this.m_Searching && Constants.QRCODE_KEYWORD.Equals(text))
            {
                this.m_TextMesh.text = Constants.SEARCHING_FOR_QR_CODES;
                this.m_Decrypting = true;
#if !UNITY_EDITOR
                try
                {
                    Task.Factory.StartNew(() =>
                    {
                        ZXingQrCodeScanner.ScanFirstCameraForQrCode(GetQRCode, TimeSpan.FromSeconds(Constants.TIMEOUT));
                    });
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
#endif
            }
            else if (this.m_Searching)
            {

                if (Constants.VALIDATION_TEXT.Equals(text) && !String.IsNullOrEmpty(m_WordToSearch))
                {
                    if (!this.m_StartedRecognition)
                    {
                        this.m_TextMesh.text = Constants.LOOKING_FOR_WORD + String.Format("'{0}'.", this.m_WordToSearch);
                        this.m_Validated = true;
                    }

                    if (this.m_Validated)
                    {
                        if (!this.m_StartedRecognition)
                        {
                            this.m_StartedRecognition = true;
#if !UNITY_EDITOR
                            Task.Factory.StartNew(() =>
                            {
                                WordScanner.ScanFirstCameraForWords(GetWordsBoundingBoxes, TimeSpan.FromSeconds(Constants.TIMEOUT), this.m_WordToSearch.Equals("42") ? Constants.WILDCARD : this.m_WordToSearch);
                                DelaySecondProxy(() => { }, () =>
                                {
                                    FunctionProxyOnMainThread(Reset);
                                    FunctionProxyOnMainThread(DisplayWebcamPreview);
                                }, (float) Constants.TIMEOUT);

                            });
#endif
                        }
                        if (Constants.STOP_TEXT.Equals(text) && this.m_StartedRecognition)
                        {
                            Task.Factory.StartNew(() =>
                            {
                                DelaySecondProxy(() => { }, () =>
                                {
                                    Reset();
                                    DisplayWebcamPreview();
                                }, 5);

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
        public void Reset()
        {
            DelaySecondProxy(() =>
            {
                this.m_TextMesh.text = Constants.RESET_IN_PROGRESS;
            }, () =>
            {
                this.m_Decrypting = false;
                this.m_Validated = false;
                this.m_Searching = false;
                this.m_StartedRecognition = false;
                this.m_WordToSearch = string.Empty;
                var sentence = string.Format("Dites '{0}' pour commencer \n ou '{1}' pour dechiffrer un QR corde", Constants.SEARCH_KEYWORD, Constants.QRCODE_KEYWORD);
                this.m_TextMesh.text = sentence;
            }, 0.1f);
        }


#if !UNITY_EDITOR
        public void GetQRCode(string result)
        {
            if (!string.IsNullOrEmpty(result))
                FunctionProxyOnMainThread(() => { this.m_TextMesh.text = result; });
            else
            {
                FunctionProxyOnMainThread(() =>
                {
                    this.m_TextMesh.text = Constants.NO_QR_CODE_FOUND;
                    Reset();
                });
            }
        }

        private void GetWordsBoundingBoxes(List<ActiDetectedWord> result)
        {
            String resultStatistics = String.Empty;
            try
            {
                FunctionProxyOnMainThread(ClearLists);
                foreach (var resultat in result)
                {
                    m_Rectangles.Add(new Rect(resultat.PosX, resultat.PosY, resultat.Width, resultat.Height));
                    m_Colors.Add(resultat.IsExactMatch() ? Constants.PERFECT_MATCH_COLOR : Constants.APPROXIMATE_MATCH_COLOR);
                }
                UInt16 numberOfExactMatches = (UInt16)result.Where(r => r.IsExactMatch()).Count();
                resultStatistics = numberOfExactMatches + " exact match(es) were found. \n " + (result.Count - numberOfExactMatches) + " close match(es) were found. \n for the word '" + this.m_WordToSearch + "'";
            }
            catch (Exception ex)
            {
                print("Erreur lors de la recherche de mots" + ex.Message);
                resultStatistics = Constants.ERROR_NO_RESULT_FOUND;
            }

            FunctionProxyOnMainThread(() => { this.m_TextMesh.text = resultStatistics; });
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
                    var borderRectangles = m_Rectangles[j].GetRectOutlines(2, m_PlaneMesh);
                    foreach (var borderRectangle in borderRectangles)
                        GUI.DrawTexture(borderRectangle, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, false, 1, m_Colors[j], 2, 0);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Erreur lors de l'affichage du/des rectangle(s). " + ex.Message);
            }

        }


        public void FunctionProxyOnMainThread(Action callback)
        {
            if (UnityEngine.WSA.Application.RunningOnAppThread())
            {
                callback();
            }
            else
            {
                UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                {
                    callback();
                }, true);
            }
        }


        public void DelaySecondProxy(Action callback, Action callback2, float seconds)
        {
            if (UnityEngine.WSA.Application.RunningOnAppThread())
            {
                StartCoroutine(DelaySeconds(callback, callback2, seconds));
            }
            else
            {
                UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                {
                    StartCoroutine(DelaySeconds(callback, callback2, seconds));
                }, false);
            }
        }

        private IEnumerator DelaySeconds(Action callback, Action callback2, float seconds)
        {
            callback();
            yield return new WaitForSeconds(seconds);
            callback2();
        }

        #region Debug

        public Transform GetDebugPlane()
        {
            var transforms = GetComponentsInChildren<Transform>();
            Transform transform = null;
            foreach (var transformPivots in transforms)
            {
                if (transformPivots.name.Equals("Plane"))
                    transform = transformPivots;
            }
            return transform;
        }

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
            // Important : If this is not disabled dictation recognizer will 
            // not start
            this.DisablePhraseRecognitionSystem();

            m_DictationRecognizer?.Dispose();
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
                    Debug.Log(string.Format("Dictation completed unsuccessfully: {0}.", completionCause));
                    this.InitDictationRecognizer();
                    Reset();
                }
            };

            this.m_DictationRecognizer.DictationError += (error, hresult) =>
            {
                Debug.Log(string.Format("Dictation error: {0}; HResult = {1}.", error, hresult));
            };

            if (SpeechSystemStatus.Stopped == this.m_DictationRecognizer.Status)
                this.m_DictationRecognizer.Start();

            Debug.Log("Microphone is " + (Microphone.IsRecording(string.Empty) ? "" : "not ") + "recording.");
        }
        #endregion
    }
}