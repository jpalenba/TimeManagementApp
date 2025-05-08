using System;  // for TimeSpan

namespace TimeManagementApp.Models
{
    public class CalendarTask
    {
        public string   Title       { get; set; } = "";
        public string   Day         { get; set; } = "";       // "Monday", etc.
        public TimeSpan Time        { get; set; }             // e.g. 06:00:00
        public bool     IsImportant { get; set; } = false;    // ← NEW
        public bool     IsUrgent    { get; set; } = false;    // ← NEW
    }
}
