// File: TaskRepository.cs
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TimeManagementApp.Models;        


namespace TimeManagementApp.Services
{
    public static class TaskRepository
    {
        private static readonly string FilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tasks.json");

        // In‐memory store
        public static List<CalendarTask> Tasks { get; private set; }

        static TaskRepository()
        {
            Load();
        }

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

        public static void Save()
        {
            File.WriteAllText(
                FilePath,
                JsonConvert.SerializeObject(Tasks, Formatting.Indented)
            );
        }

        public static void Upsert(CalendarTask t)
        {
            var existing = Tasks
                .Find(x => x.Day == t.Day && x.Time == t.Time);
            if (existing != null)
                existing.Title = t.Title;
            else
                Tasks.Add(t);
            Save();
        }

        public static void Remove(CalendarTask t)
        {
            Tasks.RemoveAll(x => x.Day == t.Day && x.Time == t.Time);
            Save();
        }
    }
}
