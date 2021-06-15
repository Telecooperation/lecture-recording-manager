using Hangfire;
using LectureRecordingManager.Hubs;
using LectureRecordingManager.Jobs.Configuration;
using LectureRecordingManager.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RecordingProcessor.Model;
using RecordingProcessor.Studio;
using RecordingProcessor.Studio.Styles;
using System;
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

        [AutomaticRetry(Attempts = 1)]
        [Queue("processing-queue")]
        public async Task Execute(ProcessRecordingJobConfiguration configuration)
        {
            // get recording
            var recording = await _context.Recordings
                .Include(x => x.Chapters)
                .Include(x => x.Outputs)
                .FirstOrDefaultAsync(x => x.Id == configuration.RecordingId);

            if (recording == null)
            {
                return;
            }

            // create new recording output
            var recordingOutput = new RecordingOutput()
            {
                RecordingId = configuration.RecordingId,
                DateStarted = DateTime.Now,
                Status = RecordingStatus.PROCESSING,
                JobType = typeof(ProcessRecordingJob).FullName,
                JobConfiguration = JsonConvert.SerializeObject(configuration)
            };
            _context.RecordingOutputs.Add(recordingOutput);
            await _context.SaveChangesAsync();
            await UpdateLectureRecordingStatus();

            // get lecture
            var lecture = await _context.Lectures
                .FindAsync(recording.LectureId);

            // start encoding
            if (recording.Type == RecordingType.GREEN_SCREEN_RECORDING || recording.Type == RecordingType.SIMPLE_RECORDING || recording.Type == RecordingType.ZOOM_RECORDING)
            {
                // file path is file?
                string outputFolder = Path.Combine(lecture.ConvertedPath, recording.Id.ToString(), "output_" + recordingOutput.Id);
                string inputFileName = "";

                if (File.Exists(recording.FilePath))
                {
                    inputFileName = recording.FilePath;
                }
                else if (recording.Type == RecordingType.ZOOM_RECORDING)
                {
                    inputFileName = Directory.GetFiles(recording.FilePath)
                        .Where(x => x.EndsWith(".mp4"))
                        .SingleOrDefault();
                }
                else
                {
                    string targetName = recording.CustomTargetName != null ? recording.CustomTargetName : "";
                    inputFileName = Directory.GetFiles(recording.FilePath)
                            .Where(x => x.EndsWith(targetName + "_meta.json"))
                            .SingleOrDefault();
                }

                // create output directory
                Directory.CreateDirectory(outputFolder);

                if (inputFileName == null)
                {
                    recordingOutput.Status = RecordingStatus.ERROR;
                    recordingOutput.JobError = "Could not find studio meta data.";
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
                        metaData = ConvertStudioRecording(inputFileName, outputFolder, GetDimension(configuration.OutputType));
                    }
                    else if (recording.Type == RecordingType.SIMPLE_RECORDING)
                    {
                        metaData = ConvertSimpleRecording(inputFileName, outputFolder, GetDimension(configuration.OutputType));
                    }
                    else if (recording.Type == RecordingType.ZOOM_RECORDING)
                    {
                        metaData = ConvertZoomRecording(inputFileName, outputFolder);
                    }

                    recordingOutput.Status = RecordingStatus.PROCESSED;
                    recordingOutput.JobError = null;
                    recordingOutput.DateFinished = DateTime.Now;

                    await _context.SaveChangesAsync();
                    await UpdateLectureRecordingStatus();
                }
                catch (Exception ex)
                {
                    recordingOutput.Status = RecordingStatus.ERROR;
                    recordingOutput.JobError = ex.Message;
                    recordingOutput.DateFinished = DateTime.Now;
                    await _context.SaveChangesAsync();

                    await UpdateLectureRecordingStatus();

                    throw ex;
                }
            }
        }

        private Dimension GetDimension(ProcessRecordingOutputType outputType)
        {
            if (outputType == ProcessRecordingOutputType.Default || outputType == ProcessRecordingOutputType.Video_720p)
            {
                return Dimension.Dim720p;
            }
            else if (outputType == ProcessRecordingOutputType.Video_1080P)
            {
                return Dimension.Dim1080p;
            }

            return Dimension.Dim720p;
        }

        private RecordingMetadata ConvertStudioRecording(string inputFileName, string outputFolder, Dimension targetDimension)
        {
            // identify file paths
            var slideVideoPath = inputFileName.Replace("_meta.json", "_slides.mp4");
            var thVideoPath = inputFileName.Replace("_meta.json", "_talkinghead.mp4");
            var ckFile = inputFileName.Replace("_meta.json", ".ckparams");

            // setup of recording
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

            return converter.ConvertMedia(config);
        }

        private RecordingMetadata ConvertSimpleRecording(string inputFileName, string outputFolder, Dimension targetDimension)
        {
            // identify file paths
            var slideVideoPath = inputFileName.Replace("_meta.json", "_slides.mp4");
            var thVideoPath = inputFileName.Replace("_meta.json", "_talkinghead.mp4");

            // setup of recording
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

            if (File.Exists(thVideoPath))
            {
                return converter.ConvertMedia(config);
            }
            else
            {
                return converter.ConvertMediaSingle(config);
            }
        }

        private RecordingMetadata ConvertZoomRecording(string inputFileName, string outputFolder)
        {
            // setup recording
            var config = new ConversionConfiguration()
            {
                SlideVideoPath = inputFileName,
                OutputDirectory = outputFolder,
                ProjectName = Path.GetFileName(inputFileName),
                ExportJson = false
            };

            return converter.ConvertZoomMedia(config);
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
