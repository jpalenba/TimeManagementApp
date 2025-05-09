using System;

namespace TimeManagementApp.Models
{
    /// <summary>
    /// Represents a single scheduled task/event.
    /// </summary>
    public class CalendarTask : IEquatable<CalendarTask>
    {
        /// <summary>
        /// The title/description of the event.
        /// </summary>
        public string Title       { get; set; } = "";

        /// <summary>
        /// Day of week, e.g. "Monday"
        /// </summary>
        public string Day         { get; set; } = "";

        /// <summary>
        /// Time of day for this event.
        /// </summary>
        public TimeSpan Time      { get; set; }

        /// <summary>
        /// Whether the event is marked as important.
        /// </summary>
        public bool   IsImportant { get; set; } = false;  // retained :contentReference[oaicite:0]{index=0}:contentReference[oaicite:1]{index=1}

        /// <summary>
        /// Whether the event is marked as urgent.
        /// </summary>
        public bool   IsUrgent    { get; set; } = false;  // retained :contentReference[oaicite:2]{index=2}:contentReference[oaicite:3]{index=3}

        /// <summary>
        /// New: Category of this event ("Work", "Study", "Personal", "Activity").
        /// </summary>
        public string Category    { get; set; } = "Personal";  // added

        public bool Equals(CalendarTask other)
        {
            if (other is null) return false;
            return Day         == other.Day
                && Time        == other.Time
                && Title       == other.Title
                && IsImportant == other.IsImportant
                && IsUrgent    == other.IsUrgent
                && Category    == other.Category;  // include in equality
        }

        public override bool Equals(object obj) =>
            obj is CalendarTask ct && Equals(ct);

        public override int GetHashCode() =>
            HashCode.Combine(Day, Time, Title, IsImportant, IsUrgent, Category);  // include Category
    }
}
