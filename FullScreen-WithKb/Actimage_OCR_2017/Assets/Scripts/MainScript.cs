namespace Assets
{

#if !UNITY_EDITOR
    using MediaFrameQrProcessing.Wrappers;
    using MediaFrameQrProcessing.Entities;
    using Windows.Networking.Connectivity;
#endif

    using Assets;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.Windows.Speech;
    using System.Linq;

    public class MainScript : MonoBehaviour
    {

        #region Attributes
        private Text m_TextPromptMesh;
        private Text m_TextInputMesh;
        private DictationRecognizer m_DictationRecognizer;
        private Boolean m_Searching;
        private Boolean m_Decrypting;
        private Boolean doOnce = true;
        private Boolean m_StartedRecognition;
        private String m_WordToSearch;
        private String m_WordSearchedFor;
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
            this.GetTextMeshes();
            this.m_Searching = false;
            this.m_Decrypting = false;
            this.m_WordToSearch = string.Empty;
            this.m_StartedRecognition = false;
            this.m_DictationRecognizer = null;
            this.m_ResultBoxes = new HashSet<GameObject>();
        }

        public void GetTextMeshes()
        {
            var textMeshes = GetComponentsInChildren<Text>().ToList();
            this.m_TextPromptMesh = textMeshes.FirstOrDefault(a => a.name.Equals(Constants.TEXT_PROMPT_MESH_LABEL));
            this.m_TextInputMesh = textMeshes.FirstOrDefault(a => a.name.Equals(Constants.TEXT_INPUT_MESH_LABEL));
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
            // Dictation Manager requires permanent
            // internet connection.
            if (HasInternet())
                this.Reset();
            else
                this.m_TextPromptMesh.text = Constants.ERROR_NO_INTERNET;
        }

        public static bool HasInternet()
        {
#if !UNITY_EDITOR
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            return connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
#endif
#if UNITY_EDITOR
            return true;
#endif
        }
        public void Update()
        {


            if (doOnce && Constants.DEBUG)
            {
                doOnce = false;
                var tempWidth = DisplayWebcam.WebcamWidthResolution;
                var tempHeight = DisplayWebcam.WebcamHeightResolution;
#if UNITY_EDITOR
                var rectangles = new List<Rect>()
                {
                    new Rect(-663.75F, -540.00F, 180.00F, 2.00F),
                    new Rect(-663.75F, -362.00F, 180.00F, 2.00F),
                    new Rect(-485.75F, -538.00F, 2.00F, 176.00F),
                    new Rect(-663.75F, -538.00F, 2.00F, 176.00F)
                };

                m_ResultBoxes.Add(rectangles.CreateRectangleMeshObject(FindObjectOfType<Canvas>().transform.parent, Color.white, "AA"));

#endif
#if !UNITY_EDITOR
                if (Constants.DEBUG)
                {
                    var temp = new HashSet<ActiDetectedWord>() {
                    new ActiDetectedWord("AA", 25, 0, 80, 80, false),
                    new ActiDetectedWord("BB", 25, tempHeight -50, 50, 50, true),
                    new ActiDetectedWord("CC", tempWidth -50 , 0, 50, 50, true),
                    new ActiDetectedWord("DD", tempWidth -50 , tempHeight -50, 50, 50, true),
                    new ActiDetectedWord("EE", (tempWidth / 2) -25 , (tempHeight / 2) -25,50, 50, true),
                    new ActiDetectedWord("FF", 25, (tempHeight / 2) -25,50, 50, true),
                    new ActiDetectedWord("GG", (tempWidth / 2) -25 , 0,50, 50, true)
                };
                    GetWordsBoundingBoxes(temp);
                }
#endif
            }
            Screen.fullScreen = true;
        }

        private void ClearLists()
        {
            foreach (var box in m_ResultBoxes)
                Destroy(box);
            this.m_TextInputMesh.text = "";
            this.m_ResultBoxes.Clear();
        }

        private void Scan(string text)
        {
            if (Constants.STOP_KEYWORD.Equals(text))
            {
                FunctionProxyOnMainThread(Reset);
                return;
            }

            if (!this.m_Searching && Constants.SEARCH_KEYWORD.Equals(text) && !this.m_Decrypting)
            {
                this.m_TextPromptMesh.text = Constants.SEARCH_PROMPT;
                this.m_Searching = true;
                FunctionProxyOnMainThread(ClearLists);
            }
            else if (!this.m_Searching && Constants.QRCODE_KEYWORD.Equals(text))
            {
                this.m_TextPromptMesh.text = Constants.SEARCHING_FOR_QR_CODES;
                this.m_Decrypting = true;
#if !UNITY_EDITOR
                if (Constants.DEBUG)
                    FunctionProxyOnMainThread(DisplayWebcam.StopWebcamPlayback);
                ZXingQrCodeScanner.ScanFirstCameraForQrCode(GetQRCode, TimeSpan.FromSeconds(Constants.TIMEOUT));
#endif
            }
            else if (this.m_Searching)
            {
                if (!string.IsNullOrEmpty(m_WordToSearch) || !string.IsNullOrEmpty(this.m_TextInputMesh.text))
                {
                    if (Constants.CORRECTION_KEYWORD.Equals(text))
                    {
                        this.m_WordToSearch = string.Empty;
                        this.m_TextPromptMesh.text = Constants.SEARCH_PROMPT;
                    }
                    else if (Constants.VALIDATION_KEYWORD.Equals(text) && !this.m_StartedRecognition)
                    {

                        this.m_TextPromptMesh.text = String.Format(Constants.LOOKING_FOR_WORD, this.m_WordToSearch);

                        this.m_StartedRecognition = true;
                        this.m_WordSearchedFor = this.m_WordToSearch.Equals(Constants.WILDCARD_HEARD_TEXT) ? Constants.WILDCARD_TEXT : this.m_WordToSearch;
#if !UNITY_EDITOR
                        if (Constants.DEBUG)
                            FunctionProxyOnMainThread(DisplayWebcam.StopWebcamPlayback);

                        ThreadSafeDelaySecondsCoroutine(() =>
                        {
                            WordScanner.ScanFirstCameraForWords(GetWordsBoundingBoxes, TimeSpan.FromSeconds(Constants.TIMEOUT), this.m_WordToSearch.Equals(Constants.WILDCARD_HEARD_TEXT) ? Constants.WILDCARD_TEXT : this.m_WordToSearch).ContinueWith((task) => 
                            {
                                FunctionProxyOnMainThread(() => this.m_TextPromptMesh.text += Constants.SEARCH_IS_OVER);
                            });
                        }, () =>
                        {
                            this.m_TextPromptMesh.text += '\n' + Constants.STOP_PROMPT;
                        }, (float)Constants.TIMEOUT);
#endif


                    }
                }
                else
                {
                    this.m_WordToSearch = text;
                    ThreadSafeDelaySecondsCoroutine(() =>
                    {
                        this.m_TextPromptMesh.text = string.Format(Constants.THE_WORD_YOU_ARE_LOOKING_FOR_IS, this.m_WordToSearch, Constants.FINAL_WORD_SEARCH_PROMPT);
                    }, () =>
                    {
                    }, 0.1F);
                }
            }
        }

        public void OnClickTranslate()
        {
            Scan(Constants.QRCODE_KEYWORD);
        }

        public void OnClickScan()
        {
            ThreadSafeDelaySecondsCoroutine(() =>
            {
                Scan(Constants.SEARCH_KEYWORD);
            }, () =>
            {
                ThreadSafeDelaySecondsCoroutine(() =>
                {
                    Scan(this.m_TextPromptMesh.text);
                }, () =>
                {
                    Scan(Constants.VALIDATION_KEYWORD);
                }, 0.1F);
            }, 0.1F);
        }

        public void OnClickStop()
        {
            Scan(Constants.STOP_KEYWORD);
        }

        /// <summary>
        /// This function is called on start and each time the users stop the application
        /// The aim is to switch between the different voices recognition systems.
        /// </summary>
        public void Reset()
        {
            ThreadSafeDelaySecondsCoroutine(() =>
            {
                this.m_TextPromptMesh.text = Constants.RESET_IN_PROGRESS;
            }, () =>
            {
                this.m_Decrypting = false;
                this.m_Searching = false;
                this.m_StartedRecognition = false;
                this.m_WordToSearch = string.Empty;
                this.m_WordSearchedFor = string.Empty;
                this.m_TextPromptMesh.text = Constants.HOME_PROMPT;
                ClearLists();
                this.InitDictationRecognizer();
                if (Constants.DEBUG)
                    FunctionProxyOnMainThread(DisplayWebcam.StartWebcamPlayback);
            }, 0.1f);
        }

#if !UNITY_EDITOR
        public void GetQRCode(string result)
        {
            ThreadSafeDelaySecondsCoroutine(() =>
            {
                if (!string.IsNullOrEmpty(result))
                    FunctionProxyOnMainThread(() => { this.m_TextPromptMesh.text = string.Format(Constants.THE_QR_CODE_MEANS, result); });
                else
                {
                    FunctionProxyOnMainThread(() =>
                    {
                        this.m_TextPromptMesh.text = Constants.NO_QR_CODE_FOUND;
                    });
                }
            }, () =>
            {
            }, (float)Constants.TIMEOUT);
        }

        private void GetWordsBoundingBoxes(HashSet<ActiDetectedWord> result)
        {


            FunctionProxyOnMainThread(() =>
            {
                String resultStatistics = String.Empty;
                try
                {
                    FunctionProxyOnMainThread(ClearLists);

                    int numberOfExactMatches = 0;
                    foreach (ActiDetectedWord resultat in result)
                    {
                        var rect = new Rect(resultat.PosX, resultat.PosY, resultat.Width, resultat.Height);
                        var borderRectangles = rect.GetRectOutlines(2, FindObjectOfType<Canvas>().transform.GetComponent<RectTransform>().rect);
                        m_ResultBoxes.Add(borderRectangles.CreateRectangleMeshObject(FindObjectOfType<Canvas>().transform.parent, resultat.IsExactMatch() ? Constants.PERFECT_MATCH_COLOR : Constants.APPROXIMATE_MATCH_COLOR, resultat.DetectedWord));
                        if (resultat.IsExactMatch())
                            ++numberOfExactMatches;
                    }

                    resultStatistics = string.Format(Constants.RESULT_TEXT_RECOGNITION_SENTENCE, numberOfExactMatches, (result.Count - numberOfExactMatches), this.m_WordSearchedFor);

                }
                catch (Exception ex)
                {
                    print(Constants.ERROR_LOOKING_FOR_WORDS + ex.Message);
                    resultStatistics = Constants.ERROR_NO_RESULT_FOUND;
                }
                this.m_TextPromptMesh.text = resultStatistics;
            });
        }

#endif
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

        #region Utils
        private void InitDictationRecognizer()
        {
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

            this.m_DictationRecognizer.Start();
        }
        #endregion
    }
}
