namespace Assets
{
#if !UNITY_EDITOR
    using MediaFrameQrProcessing.Wrappers;
    using MediaFrameQrProcessing.Entities;
    using System.Linq;
    using Windows.Networking.Connectivity;
#endif

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Windows.Speech;
    using UnityEngine.XR;

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
        private String m_WordSearchedFor;
        private List<Rect> m_Rectangles;
        private List<Color32> m_Colors;
        private HashSet<GameObject> m_ResultBoxes;
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
            this.m_Colors = new List<Color32>();
            this.m_DictationRecognizer = null;
            this.m_ResultBoxes = new HashSet<GameObject>();
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
            if (HasInternet())
            {
                this.Reset();
                this.InitDictationRecognizer();
            }
            else
            {
                this.m_TextMesh.text = Constants.ERROR_NO_INTERNET;
            }
#if UNITY_EDITOR
            m_Rectangles.Add(new Rect(75, 50, 200, 50));
            m_Rectangles.Add(new Rect(200, 500, 200, 50));
            m_Colors.Add(Constants.PERFECT_MATCH_COLOR);
            m_Colors.Add(Constants.APPROXIMATE_MATCH_COLOR);
#endif
        }

        public static bool HasInternet()
        {
#if !UNITY_EDITOR
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            bool internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            return internet;
#endif
#if UNITY_EDITOR
            return true;
#endif
        }


        public void Update()
        {
            Screen.fullScreen = true;
        }

        private void ClearLists()
        {
            this.m_Rectangles.Clear();
            this.m_Colors.Clear();
            foreach (var box in m_ResultBoxes)
                Destroy(box);
            this.m_ResultBoxes.Clear();
        }

        private void Scan(string text)
        {
            if (Constants.STOP_KEYWORD.Equals(text))
            {
                FunctionProxyOnMainThread(Reset);
                if (Constants.DEBUG)
                    FunctionProxyOnMainThread(DisplayWebcamPreview);
                return;
            }

            if (!this.m_Searching && Constants.SEARCH_KEYWORD.Equals(text) && !this.m_Decrypting)
            {
                this.m_TextMesh.text = Constants.SEARCH_PROMPT;
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
                    ZXingQrCodeScanner.ScanFirstCameraForQrCode(GetQRCode, TimeSpan.FromSeconds(Constants.TIMEOUT));
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
#endif
            }
            else if (this.m_Searching)
            {
                if (!string.IsNullOrEmpty(m_WordToSearch))
                {
                    if (Constants.VALIDATION_KEYWORD.Equals(text) && !this.m_StartedRecognition)
                    {

                        this.m_TextMesh.text = String.Format(Constants.LOOKING_FOR_WORD, this.m_WordToSearch);

                        this.m_StartedRecognition = true;
                        ThreadSafeDelaySecondsCoroutine(() =>
                        {
                            this.m_WordSearchedFor = this.m_WordToSearch.Equals(Constants.WILDCARD_HEARD_TEXT) ? Constants.WILDCARD_TEXT : this.m_WordToSearch;
                            Debug.Log(this.m_WordToSearch.Equals(Constants.WILDCARD_HEARD_TEXT) ? Constants.WILDCARD_TEXT : this.m_WordToSearch);
#if !UNITY_EDITOR
                                WordScanner.ScanFirstCameraForWords(GetWordsBoundingBoxes, TimeSpan.FromSeconds(Constants.TIMEOUT), this.m_WordToSearch.Equals(Constants.WILDCARD_HEARD_TEXT) ? Constants.WILDCARD_TEXT : this.m_WordToSearch);
#endif
                        }, () =>
                            {
                        Scan(Constants.STOP_KEYWORD);
                        if (Constants.DEBUG)
                            FunctionProxyOnMainThread(DisplayWebcamPreview);
                    }, 2);


                    }
                }
                else
                {
                    this.m_WordToSearch = text;
                    ThreadSafeDelaySecondsCoroutine(() =>
                    {
                        this.m_TextMesh.text = string.Format(Constants.THE_WORD_YOU_ARE_LOOKING_FOR_IS, this.m_WordToSearch, Constants.FINAL_WORD_SEARCH_PROMPT);
                    }, () =>
                    {
                    }, 1);
                }
            }
        }

        /// <summary>
        /// This function is called on start and each time the users stop the application
        /// The aim is to switch between the different voices recognition systems.
        /// </summary>
        public void Reset()
        {
            ThreadSafeDelaySecondsCoroutine(() =>
            {
                this.m_TextMesh.text = Constants.RESET_IN_PROGRESS;
            }, () =>
            {
                this.m_Decrypting = false;
                this.m_Validated = false;
                this.m_Searching = false;
                this.m_StartedRecognition = false;
                this.m_WordToSearch = string.Empty;
                this.m_WordSearchedFor = string.Empty;
                this.m_TextMesh.text = Constants.HOME_PROMPT;
                ClearLists();
                this.InitDictationRecognizer();
            }, 0.1f);
        }

#if UNITY_EDITOR
        public void OnGUI()
        {
            m_Rectangles.Add(new Rect(75, 50, 200, 50));
            m_Rectangles.Add(new Rect(200, 500, 200, 50));
            m_Colors.Add(Constants.PERFECT_MATCH_COLOR);
            m_Colors.Add(Constants.APPROXIMATE_MATCH_COLOR);
            try
            {
                for (Int16 j = 0; j < m_Rectangles.Count; ++j)
                {
                    var borderRectangles = m_Rectangles[j].GetRectOutlines(2);
                    if (Constants.DEBUG)
                        borderRectangles = m_Rectangles[j].GetRectOutlines(2, m_PlaneMesh);

                    foreach (var borderRectangle in borderRectangles)
                    {
                        GUI.DrawTexture(borderRectangle, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, false, 1, Color.red, 2, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log(Constants.ERROR_DISPLAYING_RECTANGLES + ex.Message);
            }
        }
#endif
#if !UNITY_EDITOR
        public void GetQRCode(string result)
        {
            ThreadSafeDelaySecondsCoroutine(() =>
            {
                if (!string.IsNullOrEmpty(result))
                    FunctionProxyOnMainThread(() => { this.m_TextMesh.text = string.Format(Constants.THE_QR_CODE_MEANS, result); });
                else
                {
                    FunctionProxyOnMainThread(() =>
                    {
                        this.m_TextMesh.text = Constants.NO_QR_CODE_FOUND;
                    });
                }
            }, () =>
            {
                if (Constants.DEBUG)
                    FunctionProxyOnMainThread(DisplayWebcamPreview);
                Scan(Constants.STOP_KEYWORD);
            }, (float)Constants.TIMEOUT);
        }

        private void GetWordsBoundingBoxes(List<ActiDetectedWord> result)
        {
            String resultStatistics = String.Empty;
            try
            {
                FunctionProxyOnMainThread(ClearLists);
                foreach (ActiDetectedWord resultat in result)
                {
                    m_Rectangles.Add(new Rect(resultat.PosX, resultat.PosY, resultat.Width, resultat.Height));
                    m_Colors.Add(resultat.IsExactMatch() ? Constants.PERFECT_MATCH_COLOR : Constants.APPROXIMATE_MATCH_COLOR);
                }
                for (Int16 j = 0; j < m_Rectangles.Count; ++j)
                {
                    var borderRectangles = m_Rectangles[j].GetRectOutlines(2);
                    foreach (var borderRectangle in borderRectangles)
                    {
                        m_ResultBoxes.Add(borderRectangle.CreateRectangleMeshObject(FindObjectOfType<Canvas>().transform.parent, m_Colors[j]));
                    }
                }

                int numberOfExactMatches = result.Count(r => r.IsExactMatch());
                resultStatistics = string.Format(Constants.RESULT_TEXT_RECOGNITION_SENTENCE, numberOfExactMatches, (result.Count - numberOfExactMatches), this.m_WordSearchedFor);

            }
            catch (Exception ex)
            {
                print(Constants.ERROR_LOOKING_FOR_WORDS + ex.Message);
                resultStatistics = Constants.ERROR_NO_RESULT_FOUND;
            }

            FunctionProxyOnMainThread(() => { this.m_TextMesh.text = resultStatistics; });
        }

#endif

        private void DisplayWebcamPreview()
        {
            DisplayWebcam.tex.Play();
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


        public void ThreadSafeDelaySecondsCoroutine(Action callback, Action callback2, float seconds)
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
            yield break;
        }

        #region Debug

        public Transform GetDebugPlane()
        {
            var transforms = GetComponentsInChildren<Transform>();
            Transform transform = null;
            foreach (var transformPivots in transforms)
            {
                if (transformPivots.name.Equals(Constants.PLANE_MESH_NAME))
                    transform = transformPivots;
            }
            return transform;
        }

        private void DisableVirtualReality()
        {
            StartCoroutine(LoadDevice(Constants.DEFAULT_VR_DEVICE_NAME, false));
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
                    Debug.Log(string.Format(Constants.ERROR_COMPLETING_DICTATION, completionCause));
                    Reset();
                }
            };

            this.m_DictationRecognizer.DictationError += (error, hresult) =>
            {
                Debug.Log(string.Format(Constants.ERROR_IN_DICTATION, error, hresult));
            };

            if (SpeechSystemStatus.Stopped == this.m_DictationRecognizer.Status)
                this.m_DictationRecognizer.Start();

            Debug.Log(Constants.IS_MICROPHONE_RECORDING);
        }
        #endregion
    }
}