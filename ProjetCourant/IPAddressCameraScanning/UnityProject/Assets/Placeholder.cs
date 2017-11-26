using MediaFrameQrProcessing.Wrappers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Windows.Media.SpeechRecognition;

public class Placeholder : MonoBehaviour
{
    public Transform textMeshObject;
    TextMesh textMesh;

    private async void Start()
    {
        this.textMesh = this.textMeshObject.GetComponent<TextMesh>();
        this.OnReset();
    }
    public void OnScan()
    {
        this.textMesh.text = "Scanning for 30s";
        FindIp();

    }




public void OnReset()
    {
        this.textMesh.text = "Say scan to start";
    }

    public void FindIp()
    {
#if !UNITY_EDITOR
        IPAddressScanner.ScanFirstCameraForIPAddress(
        result =>
        {
            // result here is a System.Net.IPAddress...
            this.textMesh.text = result?.ToString() ?? "not found";
        },
        TimeSpan.FromSeconds(30));
#endif
    }
}
