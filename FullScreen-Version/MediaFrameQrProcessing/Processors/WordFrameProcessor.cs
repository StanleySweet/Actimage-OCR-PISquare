namespace MediaFrameQrProcessing.Processors
{
    using MediaFrameQrProcessing.Entities;
    using MediaFrameQrProcessing.VideoDeviceFinders;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.Devices.Enumeration;
    using Windows.Graphics.Imaging;
    using Windows.Media.Capture;
    using Windows.Media.Capture.Frames;
    using Windows.Media.Ocr;

    public class WordFrameProcessor : MediaCaptureFrameProcessor
    {
        public HashSet<ActiDetectedWord> Result { get; private set; }
        public string RequestWord { get; }
        private static OcrEngine m_OcrEngine = OcrEngine.TryCreateFromUserProfileLanguages();

        public WordFrameProcessor(string requestWord, MediaFrameSourceFinder mediaFrameSourceFinder, DeviceInformation videoDeviceInformation, string mediaEncodingSubtype, MediaCaptureMemoryPreference memoryPreference = MediaCaptureMemoryPreference.Cpu) : base(mediaFrameSourceFinder, videoDeviceInformation, mediaEncodingSubtype, memoryPreference)
        {
            Result = new HashSet<ActiDetectedWord>();
            this.RequestWord = requestWord;
        }

        protected override async Task<bool> ProcessFrameAsync(MediaFrameReference frameReference)
        {
            Boolean success = false;
            // Takes at worst 60ms
            await Task.Run(async () =>
            {
                // doc here https://msdn.microsoft.com/en-us/library/windows/apps/xaml/windows.media.capture.frames.videomediaframe.aspx
                // says to dispose this softwarebitmap if you access it.
                using (SoftwareBitmap bitmap = frameReference.VideoMediaFrame.SoftwareBitmap)
                {
                    try
                    {
                        
                        OcrResult ocrResult = await m_OcrEngine.RecognizeAsync(bitmap);
                        Result?.Clear();

                        if (ocrResult == null)
                            return;

                        foreach (OcrLine line in ocrResult.Lines)
                        {
                            foreach (OcrWord word in line.Words)
                            {
                                if ("quarante deux".Equals(RequestWord.ToLower()))
                                    Result.Add(new ActiDetectedWord(word, false));
                                else if (word.Text.ToLower().Equals(RequestWord.ToLower()))
                                    Result.Add(new ActiDetectedWord(word, true));
                                else if (word.Text.ToLower().Contains(RequestWord.ToLower()))
                                    Result.Add(new ActiDetectedWord(word, true));
                                else if (RequestWord.ToLower().Contains(word.Text.ToLower()))
                                    Result.Add(new ActiDetectedWord(word, true));
                                else if (LevenshteinDistance.Compute(RequestWord.ToLower(), word.Text.ToLower()) <= 2)
                                    Result.Add(new ActiDetectedWord(word, false));
                            }
                        }
                        success = Result.Count > 0;
                    }
                    catch (Exception ex)
                    {
                        Debug.Write("Erreur lors de la récupération du mot. " + ex.Message);
                    }
                }
            });
            return success;
        }
    }
}
