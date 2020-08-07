using Hangfire;
using LectureRecordingManager.Hubs;
using LectureRecordingManager.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RecordingProcessor.Model;
using RecordingProcessor.Studio;
using RecordingProcessor.Studio.Styles;
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
        private readonly IHubContext<MessageHub> hub;
        private readonly ChromaKeyParamGuesser chromaKeyParamGuesser;
        private readonly MediaConverter converter;

        public ProcessRecordingJob(DatabaseContext context,
            IConfiguration config,
            IHubContext<MessageHub> hub,
            ChromaKeyParamGuesser chromaKeyParamGuesser,
            MediaConverter converter)
        {
            this._context = context;
            this._config = config;
            this.hub = hub;
            this.chromaKeyParamGuesser = chromaKeyParamGuesser;
            this.converter = converter;
        }

        [AutomaticRetry(Attempts = 2)]
        public async Task Execute(int recordingId)
        {
            // set status
            var recording = await _context.Recordings
                .Include(x => x.Chapters)
                .FirstOrDefaultAsync(x => x.Id == recordingId);

            if (recording == null || recording.Status == RecordingStatus.PROCESSING)
            {
                return;
            }

            recording.Status = RecordingStatus.PROCESSING;
            await _context.SaveChangesAsync();
            await UpdateLectureRecordingStatus();

            // start encoding
            if (recording.Type == RecordingType.GREEN_SCREEN_RECORDING || recording.Type == RecordingType.SIMPLE_RECORDING)
            {
                // file path is file?
                string outputFolder = "";
                string inputFileName = "";

                if (File.Exists(recording.FilePath))
                {
                    outputFolder = Path.Combine(Path.GetDirectoryName(recording.FilePath), "output");
                    inputFileName = recording.FilePath;
                }
                else
                {
                    outputFolder = Path.Combine(recording.FilePath, "output");
                    inputFileName = Directory.GetFiles(recording.FilePath)
                        .Where(x => x.EndsWith("_meta.json"))
                        .SingleOrDefault();
                }

                Directory.CreateDirectory(outputFolder);

                if (inputFileName == null)
                {
                    recording.Status = RecordingStatus.ERROR;
                    recording.StatusText = "Could not find studio meta data.";
                    await _context.SaveChangesAsync();
                    await UpdateLectureRecordingStatus();

                    throw new Exception("Could not find studio meta data.");
                }

                try
                {
                    // convert files
                    RecordingMetadata metaData = null;

                    if (recording.Type == RecordingType.GREEN_SCREEN_RECORDING)
                    {
                        metaData = ConvertStudioRecording(inputFileName, outputFolder, false);
                    }
                    else if (recording.Type == RecordingType.SIMPLE_RECORDING)
                    {
                        metaData = ConvertSimpleRecording(inputFileName, outputFolder, false);
                    }

                    recording.Duration = metaData.Duration;
                    _context.RecordingChapters.RemoveRange(recording.Chapters);

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

                    // copy thumbnails
                    var thumbsDirectory = Path.Combine(outputFolder, "..", "thumbs");
                    if (Directory.Exists(thumbsDirectory))
                        Directory.Delete(thumbsDirectory, true);

                    Directory.CreateDirectory(thumbsDirectory);

                    foreach (var thumb in Directory.GetFiles(Path.Combine(outputFolder, "thumbs")))
                    {
                        File.Copy(thumb, Path.Combine(outputFolder, "..", "thumbs", Path.GetFileName(thumb)));
                    }

                    recording.Status = RecordingStatus.PROCESSED;
                    recording.StatusText = null;
                    recording.Published = false;

                    await _context.SaveChangesAsync();
                    await UpdateLectureRecordingStatus();
                }
                catch (Exception ex)
                {
                    recording.Status = RecordingStatus.ERROR;
                    recording.StatusText = ex.Message;
                    await _context.SaveChangesAsync();
                    await UpdateLectureRecordingStatus();

                    throw ex;
                }
            }
        }

        [AutomaticRetry(Attempts = 1)]
        public async Task Preview(int recordingId)
        {
            // set status
            var recording = await _context.Recordings
                .Include(x => x.Chapters)
                .FirstOrDefaultAsync(x => x.Id == recordingId);

            if (recording == null)
            {
                return;
            }

            // start encoding preview
            if (recording.Type == RecordingType.GREEN_SCREEN_RECORDING || recording.Type == RecordingType.SIMPLE_RECORDING)
            {
                // file path is file?
                string outputFolder = "";
                string inputFileName = "";

                if (File.Exists(recording.FilePath))
                {
                    outputFolder = Path.Combine(Path.GetDirectoryName(recording.FilePath), "preview");
                    inputFileName = recording.FilePath;
                }
                else
                {
                    outputFolder = Path.Combine(recording.FilePath, "preview");
                    inputFileName = Directory.GetFiles(recording.FilePath)
                        .Where(x => x.EndsWith("_meta.json"))
                        .SingleOrDefault();
                }

                if (inputFileName == null)
                {
                    throw new Exception("Could not find studio meta data.");
                }

                // convert files
                RecordingMetadata metaData = null;

                if (recording.Type == RecordingType.GREEN_SCREEN_RECORDING)
                {
                    metaData = ConvertStudioRecording(inputFileName, outputFolder, true);

                    recording.Preview = true;
                    recording.Duration = metaData.Duration;
                }
                else if (recording.Type == RecordingType.SIMPLE_RECORDING)
                {
                    metaData = ConvertSimpleRecording(inputFileName, outputFolder, true);

                    recording.Preview = true;
                    recording.Duration = metaData.Duration;
                }

                if (metaData != null)
                {
                    _context.RecordingChapters.RemoveRange(recording.Chapters);

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

                    // copy thumbnails
                    var thumbsDirectory = Path.Combine(outputFolder, "..", "thumbs");
                    if (Directory.Exists(thumbsDirectory))
                        Directory.Delete(thumbsDirectory, true);

                    Directory.CreateDirectory(thumbsDirectory);

                    foreach (var thumb in Directory.GetFiles(Path.Combine(outputFolder, "thumbs")))
                    {
                        File.Copy(thumb, Path.Combine(outputFolder, "..", "thumbs", Path.GetFileName(thumb)));
                    }
                }

                await _context.SaveChangesAsync();
                await UpdateLectureRecordingStatus();
            }
        }

        private RecordingMetadata ConvertStudioRecording(string inputFileName, string outputFolder, bool preview)
        {
            // identify file paths
            var slideVideoPath = inputFileName.Replace("_meta.json", "_slides.mp4");
            var thVideoPath = inputFileName.Replace("_meta.json", "_talkinghead.mp4");
            var ckFile = inputFileName.Replace("_meta.json", ".ckparams");

            // setup of recording
            var targetDimension = Dimension.Dim720p;
            var recordingStyle = new TKStudioStyle(targetDimension);

            if (File.Exists(ckFile))
            {
                dynamic ckInfo = JsonConvert.DeserializeObject(File.ReadAllText(ckFile));

                if (ckInfo["color"] != null)
                    recordingStyle.ChromaKeyParams.Color = ckInfo["color"];
                else
                {
                    recordingStyle.ChromaKeyParams.Color = chromaKeyParamGuesser.GuessChromaKeyParams(thVideoPath);
                }

                if (ckInfo["similarity"] != null)
                    recordingStyle.ChromaKeyParams.Similarity = ckInfo["similarity"];
                if (ckInfo["blend"] != null)
                    recordingStyle.ChromaKeyParams.Blend = ckInfo["blend"];
            }
            else
            {
                recordingStyle.ChromaKeyParams.Color = chromaKeyParamGuesser.GuessChromaKeyParams(thVideoPath);
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

            return preview ? converter.ConvertPreviewMedia(config) : converter.ConvertMedia(config);
        }

        private RecordingMetadata ConvertSimpleRecording(string inputFileName, string outputFolder, bool preview)
        {
            // identify file paths
            var slideVideoPath = inputFileName.Replace("_meta.json", "_slides.mp4");
            var thVideoPath = inputFileName.Replace("_meta.json", "_talkinghead.mp4");

            // setup of recording
            var targetDimension = Dimension.Dim720p;
            var recordingStyle = new TKSimpleStyle(targetDimension);

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

            return preview ? converter.ConvertPreviewMedia(config) : converter.ConvertMedia(config);
        }

        private async Task UpdateLectureRecordingStatus()
        {
            await hub.Clients.All.SendAsync("StatusChanged", new Message()
            {
                Type = "UPDATE_LECTURE_RECORDING_STATUS"
            });
        }
    }
}
