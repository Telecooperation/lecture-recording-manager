using System.IO;

namespace RecordingProcessor.Studio
{
    public class Dimension
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public string Background { get; set; }

        public AspectRatio AspectRatio { get; set; }

        public static Dimension Dim720p = new Dimension
        {
            Width = 1280,
            Height = 720,
            AspectRatio = AspectRatio.Wide,
            Background = Path.Combine("resources", "defaultbg720.png")
        };

        public static Dimension Dim1080p = new Dimension
        {
            Width = 1920,
            Height = 1080,
            AspectRatio = AspectRatio.Wide,
            Background = Path.Combine("resources", "defaultbg1080.png")
        };
    }

    public class AspectRatio
    {
        public int width;
        public int height;

        public static AspectRatio Wide = new AspectRatio
        {
            width = 16,
            height = 9
        };

        public static AspectRatio Traditional = new AspectRatio
        {
            width = 4,
            height = 3
        };
    }
}
