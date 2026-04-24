using SQLite;
using System;

namespace ClassSched.Models;

public enum AttendanceStatus
{
    Attended,
    Missed,
    Excused,
    Late
}

[Table("ClassAttendances")]
public class ClassAttendance
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull, Indexed]
    public int ClassScheduleId { get; set; }

    [NotNull]
    public DateTime Date { get; set; }

    [NotNull]
    public AttendanceStatus Status { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Helper properties for UI
    public string StatusDisplay => Status.ToString();

    public Color StatusColor => Status switch
    {
        AttendanceStatus.Attended => Color.FromArgb("#4CAF50"), // Green
        AttendanceStatus.Missed => Color.FromArgb("#F44336"), // Red
        AttendanceStatus.Excused => Color.FromArgb("#FF9800"), // Orange
        AttendanceStatus.Late => Color.FromArgb("#FFC107"), // Yellow
        _ => Color.FromArgb("#9E9E9E") // Gray
    };

    public string StatusIcon => Status switch
    {
        AttendanceStatus.Attended => "✓",
        AttendanceStatus.Missed => "✕",
        AttendanceStatus.Excused => "⊘",
        AttendanceStatus.Late => "⚠",
        _ => "?"
    };
}
