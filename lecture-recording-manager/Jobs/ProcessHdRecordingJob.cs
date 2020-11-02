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
    public class ProcessHdRecordingJob
    {
        private readonly DatabaseContext _context;
        private readonly IConfiguration _config;
        private readonly IHubContext<MessageHub> hub;
        private readonly ChromaKeyParamGuesser chromaKeyParamGuesser;
        private readonly MediaConverter converter;

        public ProcessHdRecordingJob(DatabaseContext context,
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

        [AutomaticRetry(Attempts = 1)]
        public async Task Execute(int recordingId)
        {
            // set status
            var recording = await _context.Recordings
                .Include(x => x.Chapters)
                .FirstOrDefaultAsync(x => x.Id == recordingId);

            if (recording == null || recording.FullHdStatus == RecordingStatus.PROCESSING)
            {
                return;
            }

            // get lecture
            var lecture = await _context.Lectures
                .FindAsync(recording.LectureId);

            // start encoding
            if (recording.Type == RecordingType.GREEN_SCREEN_RECORDING || recording.Type == RecordingType.SIMPLE_RECORDING)
            {
                // set status
                recording.FullHdStatus = RecordingStatus.PROCESSING;
                await _context.SaveChangesAsync();
                await UpdateLectureRecordingStatus();

                // specify files
                string outputFolder = Path.Combine(lecture.ConvertedPath, recording.Id.ToString(), "output_1080p");
                string targetName = recording.CustomTargetName != null ? recording.CustomTargetName : "";
                string inputFileName = Directory.GetFiles(recording.FilePath)
                        .Where(x => x.EndsWith(targetName + "_meta.json"))
                        .SingleOrDefault();

                // create output directory
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

                    // reload recording
                    recording = await _context.Recordings
                        .Include(x => x.Chapters)
                        .FirstOrDefaultAsync(x => x.Id == recordingId);

                    if (recording.Status == RecordingStatus.SCHEDULED_PUBLISH || recording.Status == RecordingStatus.PUBLISHED)
                    {
                        recording.FullHdStatus = RecordingStatus.SCHEDULED_PUBLISH;
                    }
                    else
                    {
                        recording.FullHdStatus = RecordingStatus.PROCESSED;
                    }

                    await _context.SaveChangesAsync();
                    await UpdateLectureRecordingStatus();

                    // do we need to publish the hd version?
                    if (recording.FullHdStatus == RecordingStatus.SCHEDULED_PUBLISH)
                    {
                        BackgroundJob.Enqueue<PublishRecordingJob>(x => x.PublishRecording(recordingId));
                    }
                }
                catch (Exception ex)
                {
                    recording = await _context.Recordings
                        .FindAsync(recordingId);

                    recording.FullHdStatus = RecordingStatus.ERROR;
                    recording.StatusText = ex.Message;
                    await _context.SaveChangesAsync();
                    await UpdateLectureRecordingStatus();

                    throw ex;
                }
            }
        }

        private RecordingMetadata ConvertStudioRecording(string inputFileName, string outputFolder, bool preview)
        {
            // identify file paths
            var slideVideoPath = inputFileName.Replace("_meta.json", "_slides.mp4");
            var thVideoPath = inputFileName.Replace("_meta.json", "_talkinghead.mp4");
            var ckFile = inputFileName.Replace("_meta.json", ".ckparams");

            // setup of recording
            var targetDimension = Dimension.Dim1080p;
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
            var targetDimension = Dimension.Dim1080p;
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
