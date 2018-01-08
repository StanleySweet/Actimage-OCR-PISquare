# Pre-reqs for local machine

1. Unity 2017
2. VS 2017
3. HoloToolkit
4. Having the virutal reality enabled in Windows 10
5. Windows 10 1705

# Installation

1. Open Unity
2. Open the project in the folder named Actimage-OCR-PISquare\FullScreen-Version\UnityProject\Assets
3. Compile the solution
    1. File Menu
    2. Build Settings
    3. Make sure that:
        1. Target device = Hololens
        2. Build type = D3D
        3. Unity C# Project is checked
        3. Unity developpment build is checked
    4. Press build
    5. Select an output folder.
    6. Open the folder location
    7. Open the Project4.sln File
    8. In the Solution Explorer on the left expand references
    9. Delete MediaFrameQrProcessing on every project 
    10. Click on the solution name, and add existing project Actimage-OCR-PISquare\FullScreen-Version\MediaFrameQrProcessing
    11. Click on references on Project 4 and tick MediaFrameQrProcessing
    12. Do the same for Assembly-Csharp
    13. Change the debug build to x86
    14. Either use device or Local computer
    15. Launch the solution if it says it's already installed replace it
    16. You'll have to launch it twice to enable everything (Mike + Camera)

# Notes
1. It won't ask for camera permissions if not in debug mode. Else it won't work on local computer if not in debug mode
    1. To enable debug mode (On by default) in assembly C# set in Constants.cs         
    > public static Boolean DEBUG = true;
