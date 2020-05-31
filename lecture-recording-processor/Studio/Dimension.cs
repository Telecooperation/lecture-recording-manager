using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RecordingProcessor.Studio
{
    public class Dimension
    {
        public int width;
        public int height;
        public string background;
        public AspectRatio aspectRatio;

        public static Dimension Dim720p = new Dimension
        {
            width = 1280,
            height = 720,
            aspectRatio = AspectRatio.Wide,
            background = Path.Combine("resources", "defaultbg720.png")
        };

        public static Dimension Dim1080p = new Dimension
        {
            width = 1920,
            height = 1080,
            aspectRatio = AspectRatio.Wide,
            background = Path.Combine("resources", "defaultbg1080.png")
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
