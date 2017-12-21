
namespace Assets
{
    using UnityEngine;

    public class DisplayWebcam : MonoBehaviour
    {
        public static Renderer rend;
        public static WebCamTexture tex;


        // Use this for initialization
        public void Start()
        {
            if (Constants.DEBUG)
                DisplayWebcamPreview();
            else
            {
                rend = this.GetComponentInChildren<Renderer>();
                rend.enabled = false;
            }

        }

        private void DisplayWebcamPreview()
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            rend = this.GetComponentInChildren<Renderer>();
            tex = new WebCamTexture(devices[0].name);
            rend.material.mainTexture = tex;
            tex.Play();

        }

        // Update is called once per frame
        void Update()
        {


        }
    }
}