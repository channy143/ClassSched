using SQLite;
using System;

namespace ClassSched.Models;

[Table("ClassSchedules")]
public class ClassSchedule
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string SubjectName { get; set; } = string.Empty;

    [NotNull]
    public string Room { get; set; } = string.Empty;

    public string? Instructor { get; set; }

    [NotNull]
    public DayOfWeek DayOfWeek { get; set; }

    [NotNull]
    public TimeSpan StartTime { get; set; }

    [NotNull]
    public TimeSpan EndTime { get; set; }

    public string ColorCode { get; set; } = "#512BD4";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public string TimeDisplay => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";

    public string DayDisplay => DayOfWeek.ToString();

    public bool IsToday => DayOfWeek == DateTime.Today.DayOfWeek;
}
