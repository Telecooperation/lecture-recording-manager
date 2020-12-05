using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RecordingProcessor.Model;
using System.Collections.Generic;
using System.IO;

namespace RecordingProcessor.Metadata
{
    public class MetadataService
    {
        public static Lecture LoadSettings(string lectureName, string semesterName, string courseJsonFilePath)
        {
            // deserialize JSON directly from a file
            if (File.Exists(courseJsonFilePath))
            {
                return JsonConvert.DeserializeObject<Lecture>(File.ReadAllText(courseJsonFilePath));
            }
            else
            {
                return new Lecture()
                {
                    Name = lectureName,
                    Semester = semesterName,
                    Recordings = new List<RecordingMetadata>()
                };
            }
        }

        public static void SaveSettings(Lecture obj, string courseJsonFilePath)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // deserialize JSON directly from a file
            var jsonString = JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
            File.WriteAllText(courseJsonFilePath, jsonString);
        }
    }
}
