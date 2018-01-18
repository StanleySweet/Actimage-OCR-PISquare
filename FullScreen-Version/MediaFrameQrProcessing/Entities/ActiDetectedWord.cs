using Windows.Media.Ocr;

namespace MediaFrameQrProcessing.Entities
{
    public class ActiDetectedWord
    {
        private double _posX;
        private double _posY;
        private double _width;
        private double _height;
        private bool _exactMatch;
        private string _originalText;

        public ActiDetectedWord(string originalText, double posX, double posY, double width, double height, bool exactMatch)
        {
            _originalText = originalText;
            _posX = posX;
            _posY = posY;
            _height = height;
            _width = width;
            _exactMatch = exactMatch;
        }

        public ActiDetectedWord(OcrWord word, bool exactMatch = false)
        {
            _originalText = word.Text;
            _posX = word.BoundingRect.X;
            _posY = word.BoundingRect.Y;
            _height = word.BoundingRect.Height;
            _width = word.BoundingRect.Width;
            _exactMatch = exactMatch;
        }
        public string DetectedWord => _originalText;
        public bool IsExactMatch() => _exactMatch;
        public float PosX => (float)_posX;
        public float PosY => (float)_posY;
        public float Width => (float)_width;
        public float Height => (float)_height;
    }
}
