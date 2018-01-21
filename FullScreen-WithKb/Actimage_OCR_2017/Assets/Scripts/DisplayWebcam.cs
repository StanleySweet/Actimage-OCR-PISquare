
namespace Assets
{
    using UnityEngine;

    public class DisplayWebcam : MonoBehaviour
    {
        private static Renderer rend;
        public static WebCamTexture tex;
        public static int WebcamHeightResolution;
        public static int WebcamWidthResolution;

        // Use this for initialization
        public void Start()
        {
            // We need to start the webcam in order to get a correct height.
            // Else it would return a 16x16 resolution.
            var success = DisplayWebcamPreview();

            // If not in debug mode do not display the webcam.
            if (!Constants.DEBUG)
            {
                StopWebcamPlayback();
            }
            Debug.Log(success ? Constants.WEBCAM_HAS_BEEN_INITIALIZED_SUCESSFULLY : Constants.WEBCAM_HAS_NOT_BEEN_INITIALIZED_SUCESSFULLY);
        }

        public void Update()
        {
            
        }

        private bool DisplayWebcamPreview()
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            rend = this.GetComponentInChildren<Renderer>();
            if (devices.Length <= 0)
                return false;

            tex = new WebCamTexture(devices[0].name);
            rend.material.mainTexture = tex;
            StartWebcamPlayback();
            WebcamHeightResolution = tex.height;
            WebcamWidthResolution = tex.width;
            return true;
        }

        public static void StartWebcamPlayback()
        {
            tex?.Play();
            rend.enabled = true;
        }
        public static void StopWebcamPlayback()
        {
            tex?.Stop();
            rend.enabled = false;
        }
    }
}