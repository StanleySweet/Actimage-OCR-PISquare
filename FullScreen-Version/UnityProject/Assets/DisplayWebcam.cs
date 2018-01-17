
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
            DisplayWebcamPreview();

            // If not in debug mode do not display the webcam.
            if (!Constants.DEBUG)
            {
                rend.enabled = false;
                tex.Stop();
            }
            Debug.Log(Constants.WEBCAM_HAS_INITIALIZED_SUCESSFULLY);
        }

        private void DisplayWebcamPreview()
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            rend = this.GetComponentInChildren<Renderer>();
            tex = new WebCamTexture(devices[0].name);
            rend.material.mainTexture = tex;
            tex.Play();
            WebcamHeightResolution = tex.height;
            WebcamWidthResolution = tex.width;
        }
    }
}