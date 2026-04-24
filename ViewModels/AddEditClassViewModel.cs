using ClassSched.Models;
using ClassSched.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClassSched.ViewModels;

[QueryProperty(nameof(ClassId), "ClassId")]
public partial class AddEditClassViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private readonly NotificationService _notificationService;
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    private int _classId;

    [ObservableProperty]
    private string _subjectName = string.Empty;

    [ObservableProperty]
    private string _room = string.Empty;

    [ObservableProperty]
    private string? _instructor;

    [ObservableProperty]
    private DayOfWeek _selectedDay = DayOfWeek.Monday;

    [ObservableProperty]
    private TimeSpan _startTime = new TimeSpan(8, 0, 0);

    [ObservableProperty]
    private TimeSpan _endTime = new TimeSpan(9, 0, 0);

    [ObservableProperty]
    private string _colorCode = "#512BD4";

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _pageTitle = "Add Class";

    public List<DayOfWeek> DaysOfWeek { get; } = new List<DayOfWeek>
    {
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday,
        DayOfWeek.Sunday
    };

    public List<string> ColorOptions { get; } = new List<string>
    {
        "#512BD4",
        "#2D7D46",
        "#E74C3C",
        "#3498DB",
        "#F39C12",
        "#9B59B6",
        "#1ABC9C"
    };

    public AddEditClassViewModel(DatabaseService databaseService, NotificationService notificationService, SettingsService settingsService)
    {
        _databaseService = databaseService;
        _notificationService = notificationService;
        _settingsService = settingsService;
    }

    public async Task LoadClassAsync()
    {
        if (ClassId == 0)
            return;

        var classSchedule = await _databaseService.GetClassAsync(ClassId);
        if (classSchedule == null)
            return;

        SubjectName = classSchedule.SubjectName;
        Room = classSchedule.Room;
        Instructor = classSchedule.Instructor;
        SelectedDay = classSchedule.DayOfWeek;
        StartTime = classSchedule.StartTime;
        EndTime = classSchedule.EndTime;
        ColorCode = classSchedule.ColorCode;
        IsEditing = true;
        PageTitle = "Edit Class";
    }

    [RelayCommand]
    private async Task SaveClassAsync()
    {
        if (string.IsNullOrWhiteSpace(SubjectName))
        {
            await Shell.Current.DisplayAlert("Error", "Please enter a subject name.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(Room))
        {
            await Shell.Current.DisplayAlert("Error", "Please enter a room.", "OK");
            return;
        }

        if (EndTime <= StartTime)
        {
            await Shell.Current.DisplayAlert("Error", "End time must be after start time.", "OK");
            return;
        }

        // Check for schedule conflicts
        var conflictingClass = await _databaseService.GetConflictingClassAsync(
            SelectedDay, StartTime, EndTime, ClassId > 0 ? ClassId : null);
        
        if (conflictingClass != null)
        {
            await Shell.Current.DisplayAlert(
                "Schedule Conflict", 
                $"There is already a class scheduled at this time:\n\n" +
                $"{conflictingClass.SubjectName}\n" +
                $"{conflictingClass.DayOfWeek}: {conflictingClass.TimeDisplay}\n" +
                $"Room: {conflictingClass.Room}\n\n" +
                $"Please choose a different time.", 
                "OK");
            return;
        }

        var classSchedule = new ClassSchedule
        {
            Id = ClassId,
            SubjectName = SubjectName.Trim(),
            Room = Room.Trim(),
            Instructor = Instructor?.Trim(),
            DayOfWeek = SelectedDay,
            StartTime = StartTime,
            EndTime = EndTime,
            ColorCode = ColorCode
        };

        var result = await _databaseService.SaveClassAsync(classSchedule);

        if (result > 0)
        {
            if (ClassId == 0)
            {
                classSchedule.Id = result;
            }

            var reminderMinutes = await _settingsService.GetReminderMinutesAsync();
            var notificationsEnabled = await _settingsService.GetNotificationsEnabledAsync();

            if (notificationsEnabled)
            {
                await _notificationService.ScheduleClassReminderAsync(classSchedule, reminderMinutes);
            }

            await Shell.Current.GoToAsync("//Schedule");
        }
        else
        {
            await Shell.Current.DisplayAlert("Error", "Failed to save class.", "OK");
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("//Schedule");
    }

    partial void OnClassIdChanged(int value)
    {
        if (value > 0)
        {
            _ = LoadClassAsync();
        }
    }
}
