
namespace Assets
{
    using UnityEngine;

    public class DisplayWebcam : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            if (Constants.DEBUG)
                DisplayWebcamPreview();
        }

        private void DisplayWebcamPreview()
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            Renderer rend = this.GetComponentInChildren<Renderer>();
            WebCamTexture tex = new WebCamTexture(devices[0].name);
            rend.material.mainTexture = tex;
            tex.Play();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}