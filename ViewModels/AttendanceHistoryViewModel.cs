using ClassSched.Models;
using ClassSched.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ClassSched.ViewModels;

public partial class AttendanceHistoryViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private ObservableCollection<AttendanceRecordDisplay> _attendanceRecords = new();

    [ObservableProperty]
    private ObservableCollection<ClassSchedule> _classes = new();

    [ObservableProperty]
    private ClassSchedule? _selectedClass;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Now.AddDays(-30);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Now;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasRecords;

    // Statistics
    [ObservableProperty]
    private int _totalAttended;

    [ObservableProperty]
    private int _totalMissed;

    [ObservableProperty]
    private int _totalExcused;

    [ObservableProperty]
    private int _totalLate;

    [ObservableProperty]
    private int _totalRecords;

    [ObservableProperty]
    private double _attendanceRate;

    public AttendanceHistoryViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;

        try
        {
            // Load all classes for filter dropdown
            var classes = await _databaseService.GetAllClassesAsync();
            Classes = new ObservableCollection<ClassSchedule>(classes);

            await LoadAttendanceRecordsAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadAttendanceRecordsAsync()
    {
        IsLoading = true;

        try
        {
            List<ClassAttendance> records;

            if (SelectedClass != null)
            {
                records = await _databaseService.GetAttendanceForClassAsync(SelectedClass.Id, StartDate, EndDate);
                var stats = await _databaseService.GetAttendanceStatisticsAsync(SelectedClass.Id);
                TotalAttended = stats.Attended;
                TotalMissed = stats.Missed;
                TotalExcused = stats.Excused;
                TotalLate = stats.Late;
                TotalRecords = stats.Total;
                AttendanceRate = stats.Rate;
            }
            else
            {
                records = await _databaseService.GetRecentAttendanceAsync(100);
                var stats = await _databaseService.GetOverallAttendanceStatsAsync(StartDate, EndDate);
                TotalAttended = stats.Attended;
                TotalMissed = stats.Missed;
                TotalExcused = stats.Excused;
                TotalLate = stats.Late;
                TotalRecords = stats.Total;
                AttendanceRate = stats.Rate;
            }

            // Get class names for display
            var classDictionary = Classes.ToDictionary(c => c.Id, c => c.SubjectName);

            var displayRecords = records.Select(r => new AttendanceRecordDisplay
            {
                Id = r.Id,
                ClassScheduleId = r.ClassScheduleId,
                ClassName = classDictionary.TryGetValue(r.ClassScheduleId, out var name) ? name : "Unknown Class",
                Date = r.Date,
                Status = r.Status,
                Notes = r.Notes,
                StatusColor = r.StatusColor,
                StatusIcon = r.StatusIcon
            });

            AttendanceRecords = new ObservableCollection<AttendanceRecordDisplay>(displayRecords);
            HasRecords = AttendanceRecords.Any();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task FilterChangedAsync()
    {
        await LoadAttendanceRecordsAsync();
    }

    [RelayCommand]
    private async Task DeleteRecordAsync(AttendanceRecordDisplay record)
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Delete Record",
            $"Delete attendance record for {record.ClassName} on {record.Date:MMM dd, yyyy}?",
            "Delete",
            "Cancel");

        if (confirm)
        {
            await _databaseService.DeleteAttendanceAsync(record.Id);
            await LoadAttendanceRecordsAsync();
        }
    }

    [RelayCommand]
    private async Task ChangeStatusAsync(AttendanceRecordDisplay record)
    {
        var options = new[] { "Attended", "Missed", "Excused", "Late" };
        var selection = await Shell.Current.DisplayActionSheet(
            "Change Status",
            "Cancel",
            null,
            options);

        if (selection != null && selection != "Cancel")
        {
            var newStatus = Enum.Parse<AttendanceStatus>(selection);
            var attendance = new ClassAttendance
            {
                ClassScheduleId = record.ClassScheduleId,
                Date = record.Date,
                Status = newStatus,
                Notes = record.Notes
            };

            await _databaseService.RecordAttendanceAsync(attendance);
            await LoadAttendanceRecordsAsync();
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task SetLast7DaysAsync()
    {
        StartDate = DateTime.Now.AddDays(-7);
        EndDate = DateTime.Now;
        await LoadAttendanceRecordsAsync();
    }

    [RelayCommand]
    private async Task SetLast30DaysAsync()
    {
        StartDate = DateTime.Now.AddDays(-30);
        EndDate = DateTime.Now;
        await LoadAttendanceRecordsAsync();
    }

    [RelayCommand]
    private async Task SetThisMonthAsync()
    {
        var today = DateTime.Now;
        StartDate = new DateTime(today.Year, today.Month, 1);
        EndDate = today;
        await LoadAttendanceRecordsAsync();
    }
}

public class AttendanceRecordDisplay
{
    public int Id { get; set; }
    public int ClassScheduleId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Notes { get; set; }
    public Color StatusColor { get; set; }
    public string StatusIcon { get; set; } = string.Empty;
}
