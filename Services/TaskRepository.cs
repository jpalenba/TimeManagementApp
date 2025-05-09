using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TimeManagementApp.Models;

namespace TimeManagementApp.Services
{
    public static class TaskRepository
    {
        // path to the JSON file on disk
        private static readonly string FilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tasks.json");

        // in-memory list of all tasks
        public static List<CalendarTask> Tasks { get; private set; }

        static TaskRepository()
        {
            Load(); // initialize Tasks from disk
        }

        // load tasks.json into memory
        public static void Load()
        {
            if (File.Exists(FilePath))
            {
                Tasks = JsonConvert
                    .DeserializeObject<List<CalendarTask>>(File.ReadAllText(FilePath))
                    ?? new List<CalendarTask>();
            }
            else
            {
                Tasks = new List<CalendarTask>();
            }
        }

        // write in-memory tasks back to disk
        public static void Save()
        {
            File.WriteAllText(
                FilePath,
                JsonConvert.SerializeObject(Tasks, Formatting.Indented)
            );
        }

        // add new task or update existing one by day/time
        public static void Upsert(CalendarTask t)
        {
            var existing = Tasks.Find(x => x.Day == t.Day && x.Time == t.Time);
            if (existing != null)
            {
                existing.Title       = t.Title;
                existing.Category    = t.Category;
                existing.IsImportant = t.IsImportant;
                existing.IsUrgent    = t.IsUrgent;
            }
            else
            {
                Tasks.Add(t);
            }
            Save();
        }

        // remove all tasks matching day/time
        public static void Remove(CalendarTask t)
        {
            Tasks.RemoveAll(x => x.Day == t.Day && x.Time == t.Time);
            Save();
        }
    }
}
