using Newtonsoft.Json;
using RecordingProcessor.Studio;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace RecordingProcessor.Utils
{
    public class FFmpegHelper
    {
        public static Process FFmpeg(string args)
        {
            return FFmpeg(args, true);
        }

        public static Process FFmpeg(string args, bool redirectOutput)
        {
            return BuildProcess(CommandForPlatform("ffmpeg"), args, redirectOutput);
        }

        public static String CommandForPlatform(String command)
        {
            string os = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                os = "win";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                os = "unix";

            string architecture = null;
            if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
                architecture = "x86";
            if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                architecture = "x64";
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm)
                architecture = "arm";
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                architecture = "arm64";

            return Path.Combine("resources", "ffmpeg", os, architecture, command);
        }

        public static Process BuildProcess(String command, String args, bool redirectOutput)
        {
            Console.WriteLine("Executing " + command + " " + args);

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = redirectOutput,
                    RedirectStandardError = redirectOutput
                }
            };

            return proc;
        }

        public static TimeSpan GetMediaLength(string path)
        {
            Process p = FFmpeg("-i \"" + path + "\"");
            p.Start();
            p.WaitForExit();

            string stringDuration = "";

            while (!p.StandardError.EndOfStream)
            {
                string line = p.StandardError.ReadLine();
                if (line.Contains("Duration"))
                {
                    var durationSplit = line.Split(',')[0].Split(' ');
                    stringDuration = durationSplit[durationSplit.Length - 1];
                }
            }

            return TimeSpan.Parse(stringDuration);
        }

        public static AudioLevelParameters GetAudioLevels(string path)
        {
            Process p = FFmpeg("-i \"" + path + "\" -af loudnorm=print_format=json:I=-23:LRA=11:tp=-1 -t 60 -f null - ");
            p.Start();

            // To avoid deadlocks, always read the output stream first and then wait.  
            string output = p.StandardError.ReadToEnd();

            p.WaitForExit();

            // read the last rows
            string[] rows = output.Split(Environment.NewLine);

            string outputLines = "";
            bool readLine = false;

            foreach (var line in rows)
            {
                if (line.Contains("{"))
                {
                    readLine = true;
                }

                if (readLine)
                {
                    outputLines += line;
                }
            }

            try
            {
                if (outputLines.Equals(""))
                {
                    return null;
                }

                return JsonConvert.DeserializeObject<AudioLevelParameters>(outputLines);
            }
            catch
            {
                return null;
            }
        }

        private static void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        public static string ExportThumbnail(float timeInSeconds, string clip, string outPath, string id)
        {
            var outFile = id + ".jpg";

            var args = "-y -ss " + timeInSeconds.ToString("0.00000", CultureInfo.InvariantCulture) +
                       " " +
                       "-i \"" + clip + "\" " +
                       "-vframes 1 " +
                       "\"" + Path.Combine(outPath, outFile) + "\"";

            Process p = FFmpeg(args, false);
            p.Start();
            p.WaitForExit();

            return outFile;
        }
    }
}
