using Hangfire;
using LectureRecordingManager.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RecordingProcessor.Model;
using RecordingProcessor.Studio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LectureRecordingManager.Jobs
{
    public class ProcessRecordingJob
    {
        private readonly DatabaseContext _context;
        private readonly IConfiguration _config;
        private readonly ChromaKeyParamGuesser chromaKeyParamGuesser;
        private readonly MediaConverter converter;

        public ProcessRecordingJob(DatabaseContext context,
            IConfiguration config,
            ChromaKeyParamGuesser chromaKeyParamGuesser,
            MediaConverter converter)
        {
            this._context = context;
            this._config = config;
            this.chromaKeyParamGuesser = chromaKeyParamGuesser;
            this.converter = converter;
        }

        [AutomaticRetry(Attempts = 2)]
        public async Task Execute(int recordingId)
        {
            // set status
            var recording = await _context.Recordings.FindAsync(recordingId);

            if (recording.Status == RecordingStatus.PROCESSING)
            {
                return;
            }

            recording.Status = RecordingStatus.PROCESSING;
            await _context.SaveChangesAsync();

            // start encoding
            if (recording.Type == RecordingType.GREEN_SCREEN_RECORDING)
            {
                var filePath = Path.Combine(_config["UploadVideoPath"], recording.Id.ToString());
                var outputFolder = Path.Combine(filePath, "output");
                Directory.CreateDirectory(outputFolder);

                var inputFileName = Directory.GetFiles(filePath)
                    .Where(x => x.EndsWith("_meta.json"))
                    .SingleOrDefault();

                if (inputFileName == null)
                {
                    recording.Status = RecordingStatus.ERROR;
                    recording.StatusText = "Could not find studio meta data.";
                    await _context.SaveChangesAsync();

                    throw new Exception("Could not find studio meta data.");
                }

                // convert files
                var metaData = ConvertStudioRecording(inputFileName, outputFolder);
                recording.Duration = metaData.Duration;

                // add slides
                foreach (var slide in metaData.Slides)
                {
                    var chapter = new RecordingChapter()
                    {
                        Recording = recording,
                        StartPosition = slide.StartPosition,
                        Text = slide.Ocr,
                        Thumbnail = slide.Thumbnail
                    };

                    _context.RecordingChapters.Add(chapter);
                }

                recording.Status = RecordingStatus.PROCESSED;
                recording.StatusText = null;
                await _context.SaveChangesAsync();
            }
        }

        private RecordingMetadata ConvertStudioRecording(string inputFileName, string outputFolder)
        {
            // identify file paths
            var slideVideoPath = inputFileName.Replace("_meta.json", "_slides.mp4");
            var thVideoPath = inputFileName.Replace("_meta.json", "_talkinghead.mp4");
            var ckFile = inputFileName.Replace("_meta.json", ".ckparams");

            // setup of recording
            var targetDimension = Dimension.Dim720p;
            var recordingStyle = RecordingStyle.TkStudioStyle(targetDimension);

            if (File.Exists(ckFile))
            {
                dynamic ckInfo = JsonConvert.DeserializeObject(File.ReadAllText(ckFile));

                if (ckInfo["color"] != null)
                    recordingStyle.ChromaKeyParams.color = ckInfo["color"];
                else
                {
                    recordingStyle.ChromaKeyParams.color = chromaKeyParamGuesser.GuessChromaKeyParams(thVideoPath);
                }

                if (ckInfo["similarity"] != null)
                    recordingStyle.ChromaKeyParams.similarity = ckInfo["similarity"];
                if (ckInfo["blend"] != null)
                    recordingStyle.ChromaKeyParams.blend = ckInfo["blend"];
            }
            else
            {
                recordingStyle.ChromaKeyParams.color = chromaKeyParamGuesser.GuessChromaKeyParams(thVideoPath);
            }

            var config = new ConversionConfiguration()
            {
                SlideVideoPath = slideVideoPath,
                TalkingHeadVideoPath = thVideoPath,
                MetadataPath = inputFileName,
                OutputDirectory = outputFolder,
                ProjectName = Path.GetFileName(inputFileName).Replace("_meta.json", ""),
                RecordingStyle = recordingStyle,
                ExportJson = false
            };

            return converter.ConvertMedia(config);
        }

    }
}
