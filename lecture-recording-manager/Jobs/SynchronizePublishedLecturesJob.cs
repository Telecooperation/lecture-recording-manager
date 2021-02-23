using Hangfire;
using LectureRecordingManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RecordingProcessor.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LectureRecordingManager.Jobs
{
    public class SynchronizePublishedLecturesJob
    {
        private readonly DatabaseContext _context;
        private readonly IConfiguration _config;

        public SynchronizePublishedLecturesJob(DatabaseContext context, IConfiguration config)
        {
            this._context = context;
            this._config = config;
        }

        [Queue("meta-queue")]
        public void SynchronizeLectures()
        {
            var courses = new List<Course>();

            // get all published semesters
            var semesters = _context.Semesters
                .Include(x => x.Lectures)
                .Where(x => x.Published)
                .OrderByDescending(x => x.DateEnd)
                .ToList();

            // get lectures per semester
            foreach (var semester in semesters)
            {
                foreach (var lecture in semester.Lectures)
                {
                    if (!lecture.Publish)
                        continue;

                    var course = new Course()
                    {
                        Id = lecture.ShortHand,
                        Name = lecture.Title,
                        Folder = lecture.PublishPath.Replace(_config["PublishVideoPath"], ""),
                        Current = semester.DateStart < DateTime.Now && semester.DateEnd < DateTime.Now,
                        WeekView = false,
                        PublishMode = false
                    };

                    courses.Add(course);
                }
            }

            // serialize to disk
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            var jsonString = JsonConvert.SerializeObject(courses, settings);
            File.WriteAllText(Path.Combine(_config["PublishVideoPath"], "assets", "courses.json"), jsonString);
        }
    }
}
