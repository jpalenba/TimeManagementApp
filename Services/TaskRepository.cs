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

        /// <summary>
        /// In‑memory list of all CalendarTask entries.
        /// </summary>
        public static List<CalendarTask> Tasks { get; private set; }

        static TaskRepository()
        {
            Load();
        }

        /// <summary>
        /// Loads the task list from disk (tasks.json) into memory.
        /// </summary>
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

        /// <summary>
        /// Persists the current in‑memory task list back to disk.
        /// </summary>
        public static void Save()
        {
            File.WriteAllText(
                FilePath,
                JsonConvert.SerializeObject(Tasks, Formatting.Indented)
            );
        }

        /// <summary>
        /// Inserts or updates a task.  If a task for the same day/time exists,
        /// updates its Title, Category, IsImportant and IsUrgent flags.
        /// Otherwise adds the new task.
        /// </summary>
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

        /// <summary>
        /// Removes all tasks for the given day/time.
        /// </summary>
        public static void Remove(CalendarTask t)
        {
            Tasks.RemoveAll(x => x.Day == t.Day && x.Time == t.Time);
            Save();
        }
    }
}
