using MediaFrameQrProcessing.VideoDeviceFinders;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Media.Capture.Frames;
using Windows.Media.Ocr;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Windows.Media.Capture;

namespace MediaFrameQrProcessing.Processors
{
    public class WordFrameProcessor : MediaCaptureFrameProcessor
    {
        public string Result { get; private set; }
        private string m_RequestWord;
        private OcrEngine m_OcrEngine;

        public WordFrameProcessor(string requestWord, MediaFrameSourceFinder mediaFrameSourceFinder, DeviceInformation videoDeviceInformation, string mediaEncodingSubtype, MediaCaptureMemoryPreference memoryPreference = MediaCaptureMemoryPreference.Cpu)
        : base(mediaFrameSourceFinder, videoDeviceInformation, mediaEncodingSubtype, memoryPreference)
        {
            this.m_RequestWord = requestWord;
        }

        protected override async Task<bool> ProcessFrameAsync(MediaFrameReference frameReference)
        {

            // doc here https://msdn.microsoft.com/en-us/library/windows/apps/xaml/windows.media.capture.frames.videomediaframe.aspx
            // says to dispose this softwarebitmap if you access it.
            using (SoftwareBitmap bitmap = frameReference.VideoMediaFrame.SoftwareBitmap)
            {
                try
                {
                    if (this.m_OcrEngine == null)
                        this.m_OcrEngine = OcrEngine.TryCreateFromUserProfileLanguages();

                    OcrResult ocrResult = await this.m_OcrEngine.RecognizeAsync(bitmap);

                    if (ocrResult == null)
                        return false;

                    // ocrResult.Lines[1].Words[1].BoundingRect
                    IEnumerable<string> matchingWordsPlural = FindSimilarWords(ocrResult.Text, m_RequestWord + 's');

                    if (!ocrResult.Text.Contains(m_RequestWord) 
                        && !ocrResult.Text.Contains(m_RequestWord + 's') && matchingWordsPlural.Count() == 0)
                        return false;

                    this.Result = Regex.Matches(ocrResult.Text, m_RequestWord).Count.ToString() + " exact match(es) and " + matchingWordsPlural.Count() + " approximate matches found";
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }

        private IEnumerable<string> FindSimilarWords(string entry, string requestWord)
        {
            IEnumerable<string> words = entry.Split(' ').ToList();

            //Pick only the words with the same length.
            words = words.Where(w => w.Length == requestWord.Length);

            // Can't use the distance calculation if the word is too short
            if(requestWord.Length > 4)
                words = words.Where(w => LevenshteinDistance.Compute(w, requestWord) <= 2);

            return words;
        }
    }
}
