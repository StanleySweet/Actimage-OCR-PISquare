using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media;
namespace OCR_Library
{


    public class PreviewFrameProcessedEventArgs<T> : EventArgs
    {
        #region Properties
        public T Results { get; set; }
        public VideoFrame Frame { get; set; }
        #endregion
        #region Constructors
        public PreviewFrameProcessedEventArgs()
        {

        }
        #endregion

        public PreviewFrameProcessedEventArgs(
  T processingResults,
  VideoFrame frame)
        {
            this.Results = processingResults;
            this.Frame = frame;
        }
    }
}
