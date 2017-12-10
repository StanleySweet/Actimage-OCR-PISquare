using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml.Controls;

namespace OCR_Library
{
    public class CameraPreviewManager
    {
        #region Attributes
        private CaptureElement m_CaptureElement;
        private MediaCapture m_MediaCapture;
        #endregion
        #region Properties
        public MediaCapture MediaCapture
        {
            get
            {
                return (this.m_MediaCapture);
            }
        }
        public VideoEncodingProperties VideoProperties
        {
            get
            {
                return (this.m_MediaCapture.VideoDeviceController.GetMediaStreamProperties(
                  MediaStreamType.VideoPreview) as VideoEncodingProperties);
            }
        }
        #endregion
        #region Constructors
        public CameraPreviewManager(CaptureElement captureElement)
        {
            this.m_CaptureElement = captureElement;
        }
        #endregion

        public async Task<VideoEncodingProperties> StartPreviewToCaptureElementAsync(
          Func<DeviceInformation, bool> deviceFilter)
        {
            var preferredCamera = await this.GetFilteredCameraOrDefaultAsync(deviceFilter);

            MediaCaptureInitializationSettings initialisationSettings = new MediaCaptureInitializationSettings()
            {
                StreamingCaptureMode = StreamingCaptureMode.Video,
                VideoDeviceId = preferredCamera.Id
            };
            this.m_MediaCapture = new MediaCapture();

            await this.m_MediaCapture.InitializeAsync(initialisationSettings);

            this.m_MediaCapture.VideoDeviceController.Focus.TrySetAuto(true);

            this.m_CaptureElement.Source = this.m_MediaCapture;

            await this.m_MediaCapture.StartPreviewAsync();

            return (this.m_MediaCapture.VideoDeviceController.GetMediaStreamProperties(
              MediaStreamType.VideoPreview) as VideoEncodingProperties);
        }
        public async Task StopPreviewAsync()
        {
            await this.m_MediaCapture.StopPreviewAsync();
            this.m_CaptureElement.Source = null;
        }
        private async Task<DeviceInformation> GetFilteredCameraOrDefaultAsync(
          Func<DeviceInformation, bool> deviceFilter)
        {
            var videoCaptureDevices = await DeviceInformation.FindAllAsync(
              DeviceClass.VideoCapture);

            var selectedCamera = videoCaptureDevices.SingleOrDefault(deviceFilter);

            if (selectedCamera == null)
            {
                // we fall back to the first camera that we can find.
                selectedCamera = videoCaptureDevices.FirstOrDefault();
            }
            return (selectedCamera);
        }

    }
}
