using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Microsoft.Extensions.Logging;
using RecordingProcessor.Utils;
using SixLabors.ImageSharp.PixelFormats;

namespace RecordingProcessor.Studio
{
    public class ChromaKeyParamGuesser
    {
        public static int NrOfSamples = 10;

        private readonly ILogger<ChromaKeyParamGuesser> _logger;

        public ChromaKeyParamGuesser(ILogger<ChromaKeyParamGuesser> logger)
        {
            this._logger = logger;
        }

        public string GuessChromaKeyParams(string videoFile)
        {
            List<Color> sampledColors = SampleColors(videoFile);

            float r = 0;
            float g = 0;
            float b = 0;

            foreach (var color in sampledColors)
            {
                r += color.R * color.R;
                g += color.G * color.G;
                b += color.B * color.B;
            }

            Color result = Color.FromArgb((int)Math.Sqrt(r / sampledColors.Count),
                (int)Math.Sqrt(g / sampledColors.Count), (int)Math.Sqrt(b / sampledColors.Count));

            string res = "0x" + result.R.ToString("X2") + result.G.ToString("X2") + result.B.ToString("X2");

            return res;
        }

        private List<Color> SampleColors(string videoFile)
        {
            var result = new List<Color>();

            var outPath = Path.GetDirectoryName(videoFile);
            var len = FFmpegHelper.GetMediaLength(videoFile);
            var stepSize = len.TotalSeconds / NrOfSamples;
            double currentTime = stepSize / 2.0;

            for (int i = 0; i < NrOfSamples - 1; i++)
            {
                Console.WriteLine("Sampling at " + currentTime);
                currentTime += stepSize;

                // create thumbnail
                var tmpFileName = Path.GetRandomFileName();
                FFmpegHelper.ExportThumbnail((float)currentTime, videoFile, outPath, tmpFileName);

                // pick colors
                using (var img = SixLabors.ImageSharp.Image.Load<Rgba32>(Path.Combine(outPath, tmpFileName + ".jpg")))
                {
                    var height = img.Height / 1080;
                    var width = img.Width / 1920;

                    var colors = new List<Color>();

                    var imgColor = img[400 * width, 400 * height].Rgb;
                    colors.Add(Color.FromArgb(imgColor.R, imgColor.G, imgColor.B));

                    imgColor = img[350 * width, 350 * height].Rgb;
                    colors.Add(Color.FromArgb(imgColor.R, imgColor.G, imgColor.B));

                    imgColor = img[200 * width, 800 * height].Rgb;
                    colors.Add(Color.FromArgb(imgColor.R, imgColor.G, imgColor.B));

                    imgColor = img[180 * width, 820 * height].Rgb;
                    colors.Add(Color.FromArgb(imgColor.R, imgColor.G, imgColor.B));

                    imgColor = img[1600 * width, 200 * height].Rgb;
                    colors.Add(Color.FromArgb(imgColor.R, imgColor.G, imgColor.B));

                    imgColor = img[1620 * width, 150 * height].Rgb;
                    colors.Add(Color.FromArgb(imgColor.R, imgColor.G, imgColor.B));

                    imgColor = img[1700 * width, 800 * height].Rgb;
                    colors.Add(Color.FromArgb(imgColor.R, imgColor.G, imgColor.B));

                    imgColor = img[1720 * width, 820 * height].Rgb;
                    colors.Add(Color.FromArgb(imgColor.R, imgColor.G, imgColor.B));

                    // merge together
                    float r = 0, g = 0, b = 0;

                    foreach (var color in colors)
                    {
                        r += color.R * color.R;
                        g += color.G * color.G;
                        b += color.B * color.B;
                    }

                    result.Add(Color.FromArgb((int)Math.Sqrt(r / colors.Count),
                        (int)Math.Sqrt(g / colors.Count), (int)Math.Sqrt(b / colors.Count)));
                }

                File.Delete(Path.Combine(outPath, tmpFileName + ".jpg"));
            }

            return result;
        }
    }
}
