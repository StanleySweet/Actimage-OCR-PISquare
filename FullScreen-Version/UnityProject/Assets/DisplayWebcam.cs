
namespace Assets
{
    using UnityEngine;

    public class DisplayWebcam : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            Renderer rend = this.GetComponentInChildren<Renderer>();
            // assuming the first available WebCam is desired
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