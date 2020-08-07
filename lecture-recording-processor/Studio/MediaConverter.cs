using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RecordingProcessor.Model;
using RecordingProcessor.Utils;
using Tesseract;

namespace RecordingProcessor.Studio
{
    public class MediaConverter
    {
        private readonly ILogger<MediaConverter> _logger;

        public MediaConverter(ILogger<MediaConverter> _logger)
        {
            this._logger = _logger;
        }

        public RecordingMetadata ConvertMedia(ConversionConfiguration config)
        {
            Directory.CreateDirectory(config.OutputDirectory);

            // convert file
            ConvertVideoFiles(config, false);

            // generate recording object
            var finalRecording = new RecordingMetadata();
            finalRecording.FileName = "slides.mp4";
            finalRecording.PresenterFileName = "talkinghead.mp4";
            finalRecording.StageVideo = "stage.mp4";
            finalRecording.Slides = BuildThumbnails(config, "slides.mp4");
            finalRecording.Duration = FFmpegHelper.GetMediaLength(config.SlideVideoPath).TotalSeconds;

            return finalRecording;
        }

        public RecordingMetadata ConvertPreviewMedia(ConversionConfiguration config)
        {
            // generate output directory
            if (Directory.Exists(config.OutputDirectory))
                Directory.Delete(config.OutputDirectory, true);

            Directory.CreateDirectory(config.OutputDirectory);

            // convert file
            ConvertVideoFiles(config, true);

            // generate recording object
            var finalRecording = new RecordingMetadata();
            finalRecording.FileName = "slides.mp4";
            finalRecording.PresenterFileName = "talkinghead.mp4";
            finalRecording.StageVideo = "stage.mp4";
            finalRecording.Duration = FFmpegHelper.GetMediaLength(config.SlideVideoPath).TotalSeconds;

            FFmpegHelper.ExportThumbnail(5f, Path.Combine(config.OutputDirectory, "stage.mp4"), config.OutputDirectory, "thumbnail");

            return finalRecording;
        }

        public Slide[] BuildThumbnails(ConversionConfiguration config, string slidesFileName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return BuildThumbnailsUnix(config, slidesFileName);
            }
            else
            {
                return BuildThumbnailsWin(config, slidesFileName);
            }
        }

        private Slide[] BuildThumbnailsWin(ConversionConfiguration config, string slidesFileName)
        {
            var thumbOutDir = Path.Combine(config.OutputDirectory, "thumbs");
            Directory.CreateDirectory(thumbOutDir);

            var ocrEngine = new TesseractEngine(Path.Combine("resources", "tessdata"), "eng", EngineMode.Default);
            List<Slide> result = new List<Slide>();

            try
            {
                dynamic projectJson = JsonConvert.DeserializeObject(File.ReadAllText(config.MetadataPath));
                int currentId = 0;

                var keyframes = new List<TimeSpan>();
                keyframes.Add(TimeSpan.Zero);
                foreach (string timestamp in projectJson["slides"])
                    keyframes.Add(TimeSpan.Parse(timestamp));

                foreach (var keyframe in keyframes)
                {
                    TimeSpan? nextKeyframe = null;

                    if (keyframes.IndexOf(keyframe) != keyframes.Count - 1)
                    {
                        nextKeyframe = keyframes[keyframes.IndexOf(keyframe) + 1];
                    }
                    else
                    {
                        nextKeyframe = FFmpegHelper.GetMediaLength(Path.Combine(config.OutputDirectory, slidesFileName));
                    }

                    string thumbName = FFmpegHelper.ExportThumbnail((float)nextKeyframe.GetValueOrDefault().TotalSeconds - 2.0f, Path.Combine(config.OutputDirectory, slidesFileName), thumbOutDir,
                        (currentId++).ToString());

                    var slide = new Slide
                    {
                        StartPosition = (float)keyframe.TotalSeconds + 0.2f,
                        Thumbnail = "thumbs/" + thumbName,
                        Ocr = PerformOcr(Path.Combine(thumbOutDir, thumbName), ocrEngine)
                    };

                    if (keyframe.Equals(TimeSpan.Zero))
                        slide.StartPosition = 0.0f;
                    result.Add(slide);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            finally
            {
                ocrEngine.Dispose();
            }

            return result.ToArray();
        }

        private Slide[] BuildThumbnailsUnix(ConversionConfiguration config, string slidesFileName)
        {
            var thumbOutDir = Path.Combine(config.OutputDirectory, "thumbs");
            Directory.CreateDirectory(thumbOutDir);

            List<Slide> result = new List<Slide>();

            try
            {
                dynamic projectJson = JsonConvert.DeserializeObject(File.ReadAllText(config.MetadataPath));
                int currentId = 0;

                var keyframes = new List<TimeSpan>();
                keyframes.Add(TimeSpan.Zero);
                foreach (string timestamp in projectJson["slides"])
                    keyframes.Add(TimeSpan.Parse(timestamp));

                foreach (var keyframe in keyframes)
                {
                    TimeSpan? nextKeyframe = null;

                    if (keyframes.IndexOf(keyframe) != keyframes.Count - 1)
                    {
                        nextKeyframe = keyframes[keyframes.IndexOf(keyframe) + 1];
                    }
                    else
                    {
                        nextKeyframe = FFmpegHelper.GetMediaLength(Path.Combine(config.OutputDirectory, slidesFileName));
                    }

                    string thumbName = FFmpegHelper.ExportThumbnail((float)nextKeyframe.GetValueOrDefault().TotalSeconds - 2.0f, Path.Combine(config.OutputDirectory, slidesFileName), thumbOutDir,
                        (currentId++).ToString());

                    var tmpFileName = Path.GetRandomFileName();

                    var process = FFmpegHelper.BuildProcess("tesseract", Path.Combine(thumbOutDir, thumbName) + " \"" + tmpFileName + "\"", false);
                    process.Start();
                    process.WaitForExit();

                    var ocr = File.ReadAllText(tmpFileName);
                    File.Delete(tmpFileName);

                    var slide = new Slide
                    {
                        StartPosition = (float)keyframe.TotalSeconds + 0.2f,
                        Thumbnail = "thumbs/" + thumbName,
                        Ocr = ocr
                    };

                    if (keyframe.Equals(TimeSpan.Zero))
                        slide.StartPosition = 0.0f;
                    result.Add(slide);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return result.ToArray();
        }

        private string PerformOcr(string file, TesseractEngine ocrEngine)
        {
            Console.WriteLine("Performing OCR for " + file);

            var img = Pix.LoadFromFile(file);
            var page = ocrEngine.Process(img);
            var result = page.GetText();
            page.Dispose();

            return result;
        }

        public void ConvertVideoFiles(ConversionConfiguration config, bool preview)
        {
            var lenSlideVideo = FFmpegHelper.GetMediaLength(config.SlideVideoPath);
            var lenTHVideo = FFmpegHelper.GetMediaLength(config.TalkingHeadVideoPath);

            var trimTHVideo = lenTHVideo - lenSlideVideo;

            string args = "-i \"" + config.SlideVideoPath + "\" " +
                            "-i \"" + config.TalkingHeadVideoPath + "\" " +
                            "-i \"" + config.RecordingStyle.TargetDimension.Background + "\" " +
                            "-filter_complex " +
                            "\"" +
                            "[1:v]trim=start=" + trimTHVideo.TotalSeconds.ToString("0.00000", CultureInfo.InvariantCulture) + ",setpts=PTS-STARTPTS[1v];" +
                            "[1:a]atrim=start=" + trimTHVideo.TotalSeconds.ToString("0.00000", CultureInfo.InvariantCulture) + ",asetpts=PTS-STARTPTS,asplit=2[1a1][1a2];" +
                            config.RecordingStyle.getFFmpegFilterString() +
                            "\" " +
                            "-map \"[slides1]\" -f mp4 -vcodec libx264 -crf 23 -preset veryfast -tune stillimage -profile:v baseline -level 3.0 -pix_fmt yuv420p -r 30 " + (preview ? " -t 10 " : "") + "\"" + Path.Combine(config.OutputDirectory, "slides.mp4") + "\" " +
                            "-map \"[th_ck_ct]\" -map \"[1a1]\" -f mp4 -vcodec libx264 -crf 23 -preset veryfast -profile:v baseline -level 3.0 -pix_fmt yuv420p -r 30 -acodec aac -b:a 192k " + (preview ? " -t 10 " : "") + "\"" + Path.Combine(config.OutputDirectory, "talkinghead.mp4") + "\" " +
                            "-map \"[stage]\" -map \"[1a2]\" -f mp4 -vcodec libx264 -crf 23 -preset veryfast -profile:v baseline -level 3.0 -pix_fmt yuv420p -r 30 -acodec aac -b:a 192k " + (preview ? " -t 10 " : "") + "\"" + Path.Combine(config.OutputDirectory, "stage.mp4") + "\" ";

            _logger.LogInformation("Execute ffmpeg: {0}", args);

            Process p = FFmpegHelper.FFmpeg(args, false);
            p.Start();
            p.WaitForExit();
        }
    }
}
