namespace RecordingProcessor.Studio
{
    public class CropDimensions
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public CropDimensions(float x, float y, float width, float height, Dimension dimension)
        {
            this.X = (int)(x * dimension.Width);
            this.Y = (int)(y * dimension.Height);
            this.Width = (int)(width * dimension.Width);
            this.Height = (int)(height * dimension.Height);
        }

    }
}
