using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project4
{
    using Windows.Media;

    class PreviewFrameProcessedEventArgs<T> : EventArgs
    {
        public PreviewFrameProcessedEventArgs()
        {

        }
        public PreviewFrameProcessedEventArgs(
          T processingResults,
          VideoFrame frame)
        {
            this.Results = processingResults;
            this.Frame = frame;
        }
        public T Results { get; set; }
        public VideoFrame Frame { get; set; }
    }
}
