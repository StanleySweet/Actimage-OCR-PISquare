﻿namespace MediaFrameQrProcessing.Processors
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
        public List<ActiDetectedWord> Result { get; private set; }
        public readonly string RequestWord;
        private OcrEngine m_OcrEngine;

        public WordFrameProcessor(string requestWord, MediaFrameSourceFinder mediaFrameSourceFinder, DeviceInformation videoDeviceInformation, string mediaEncodingSubtype, MediaCaptureMemoryPreference memoryPreference = MediaCaptureMemoryPreference.Cpu) : base(mediaFrameSourceFinder, videoDeviceInformation, mediaEncodingSubtype, memoryPreference)
        {
            Result = new List<ActiDetectedWord>();
            this.RequestWord = requestWord;
        }

        protected override async Task<bool> ProcessFrameAsync(MediaFrameReference frameReference)
        {
            bool success = false;
            Result?.Clear();

            await Task.Run(async () =>
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
                            throw new NullReferenceException("Ocr Result is null");

                        foreach (OcrWord word in ocrResult.Lines.SelectMany(l => l.Words))
                        {
                            if ("quarante deux".Equals(RequestWord.ToLower()))
                                Result.Add(new ActiDetectedWord(word.Text, word.BoundingRect.X, word.BoundingRect.Y, word.BoundingRect.Width, word.BoundingRect.Height, false));
                            else if (word.Text.ToLower().Equals(RequestWord.ToLower()))
                                Result.Add(new ActiDetectedWord(word.Text, word.BoundingRect.X, word.BoundingRect.Y, word.BoundingRect.Width, word.BoundingRect.Height, true));
                            else if (word.Text.ToLower().Contains(RequestWord.ToLower()))
                                Result.Add(new ActiDetectedWord(word.Text, word.BoundingRect.X, word.BoundingRect.Y, word.BoundingRect.Width, word.BoundingRect.Height, true));
                            else if (RequestWord.ToLower().Contains(word.Text.ToLower()))
                                Result.Add(new ActiDetectedWord(word.Text, word.BoundingRect.X, word.BoundingRect.Y, word.BoundingRect.Width, word.BoundingRect.Height, true));
                            else if (LevenshteinDistance.Compute(RequestWord.ToLower(), word.Text.ToLower()) <= 2)
                                Result.Add(new ActiDetectedWord(word.Text, word.BoundingRect.X, word.BoundingRect.Y, word.BoundingRect.Width, word.BoundingRect.Height, false));
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
