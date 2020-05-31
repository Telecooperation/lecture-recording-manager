using System;

namespace RecordingProcessor.Utils
{
    public class StringUtils
    {
        public static string GetSemester(DateTime date)
        {
            if (date.Month >= 4 && date.Month < 10)
            {
                return "Summer term " + date.Year;
            }
            else
            {
                if (date.Month < 10)
                {
                    return "Winter term " + (date.Year - 1) + " / " + date.Year;
                }
                else
                {
                    return "Winter term " + date.Year + " / " + (date.Year + 1);
                }
            }
        }

        public static string GetCleanTitleFromFileName(string fileName)
        {
            if (fileName == null)
            {
                return null;
            }

            string name = fileName.Replace(".mp4", "").Replace(".trec", "").Replace("_meta.json", "");
            name = name.Replace("-", " ").Replace("_", " ").Replace("New", "").Replace("new", "").Replace("  ", " ");

            string[] splitName = name.Split(' ');

            string result = "";
            for (int i = 0; i < splitName.Length; i++)
            {
                var part = splitName[i];

                if (part.ToLower().StartsWith("ws") || part.ToLower().StartsWith("ss"))
                {
                    // ignore
                }
                else
                {
                    result += part + " ";
                }
            }

            return result.Trim();
        }
    }
}
