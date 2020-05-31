using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
                    //CreateNoWindow = true
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
