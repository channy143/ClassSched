using SQLite;
using System;

namespace ClassSched.Models;

[Table("Assignments")]
public class Assignment
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public int ClassScheduleId { get; set; }

    [NotNull]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [NotNull]
    public DateTime DueDate { get; set; }

    public bool IsCompleted { get; set; } = false;

    public int Priority { get; set; } = 1; // 0=Low, 1=Normal, 2=High

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Helper properties for UI binding
    public string PriorityDisplay => Priority switch
    {
        0 => "Low",
        2 => "High",
        _ => "Normal"
    };

    public Color PriorityColor => Priority switch
    {
        0 => Color.FromArgb("#4CAF50"), // Green
        2 => Color.FromArgb("#F44336"), // Red
        _ => Color.FromArgb("#FF9800") // Orange
    };

    public bool IsOverdue => !IsCompleted && DueDate < DateTime.Now;

    public bool IsDueSoon => !IsCompleted && DueDate.Date == DateTime.Today.AddDays(1).Date;

    public string StatusDisplay
    {
        get
        {
            if (IsCompleted) return "Completed";
            if (IsOverdue) return "Overdue";
            if (DueDate.Date == DateTime.Today.Date) return "Due Today";
            if (IsDueSoon) return "Due Tomorrow";
            return $"Due {DueDate:MMM dd}";
        }
    }

    public Color StatusColor
    {
        get
        {
            if (IsCompleted) return Color.FromArgb("#4CAF50"); // Green
            if (IsOverdue) return Color.FromArgb("#F44336"); // Red
            if (DueDate.Date == DateTime.Today.Date) return Color.FromArgb("#FF5722"); // Deep Orange
            return Color.FromArgb("#512BD4"); // Primary
        }
    }
}
