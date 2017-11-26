﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Placeholder : MonoBehaviour
{
    public Transform textMeshObject;

    private void Start()
    {
        this.textMesh = this.textMeshObject.GetComponent<TextMesh>();
        this.OnReset();
    }
    public void OnScan()
    {
        this.textMesh.text = "Scanning for 30s";

#if !UNITY_EDITOR
        MediaFrameQrProcessing.Wrappers.IPAddressScanner.ScanFirstCameraForIPAddress(
        result =>
        {
            // result here is a System.Net.IPAddress...
            this.textMesh.text = result?.ToString() ?? "not found";
        },
        TimeSpan.FromSeconds(30));
#endif
    }

    public void OnReset()
    {
        this.textMesh.text = "Say scan to start";
    }
    TextMesh textMesh;
}
