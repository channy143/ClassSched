using ClassSched.Models;
using ClassSched.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ClassSched.ViewModels;

public partial class AssignmentsViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private readonly NotificationService _notificationService;

    [ObservableProperty]
    private ObservableCollection<Assignment> _assignments = new();

    [ObservableProperty]
    private ObservableCollection<ClassSchedule> _classes = new();

    [ObservableProperty]
    private Assignment? _selectedAssignment;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _filterStatus = -1; // -1=All, 0=Pending, 1=Completed

    [ObservableProperty]
    private int _overdueCount;

    [ObservableProperty]
    private int _dueTodayCount;

    [ObservableProperty]
    private int _completedCount;

    public AssignmentsViewModel(DatabaseService databaseService, NotificationService notificationService)
    {
        _databaseService = databaseService;
        _notificationService = notificationService;
    }

    [RelayCommand]
    public async Task LoadAssignmentsAsync()
    {
        IsLoading = true;

        try
        {
            var assignments = await _databaseService.GetAllAssignmentsAsync();
            var filtered = FilterAssignments(assignments);
            Assignments = new ObservableCollection<Assignment>(filtered);

            // Update stats
            OverdueCount = assignments.Count(a => a.IsOverdue);
            DueTodayCount = assignments.Count(a => !a.IsCompleted && a.DueDate.Date == DateTime.Today.Date);
            CompletedCount = assignments.Count(a => a.IsCompleted);

            // Load classes for dropdown
            var classes = await _databaseService.GetAllClassesAsync();
            Classes = new ObservableCollection<ClassSchedule>(classes);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAssignmentsAsync();
    }

    [RelayCommand]
    private async Task AddAssignmentAsync()
    {
        await Shell.Current.GoToAsync("//AddEditAssignment");
    }

    [RelayCommand]
    private async Task EditAssignmentAsync(Assignment assignment)
    {
        var navigationParameter = new Dictionary<string, object>
        {
            { "AssignmentId", assignment.Id }
        };
        await Shell.Current.GoToAsync("//AddEditAssignment", navigationParameter);
    }

    [RelayCommand]
    private async Task DeleteAssignmentAsync(Assignment assignment)
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Delete Assignment",
            $"Are you sure you want to delete '{assignment.Title}'?",
            "Delete",
            "Cancel");

        if (confirm)
        {
            await _databaseService.DeleteAssignmentAsync(assignment);
            _notificationService.CancelNotification(assignment.Id + 10000); // Offset to avoid conflict with class notifications
            await LoadAssignmentsAsync();
        }
    }

    [RelayCommand]
    private async Task ToggleCompleteAsync(Assignment assignment)
    {
        await _databaseService.MarkAssignmentCompleteAsync(assignment.Id, !assignment.IsCompleted);
        
        if (!assignment.IsCompleted)
        {
            // Cancel notification if marking complete
            _notificationService.CancelNotification(assignment.Id + 10000);
        }
        else
        {
            // Re-schedule if marking incomplete
            await ScheduleAssignmentNotificationAsync(assignment);
        }
        
        await LoadAssignmentsAsync();
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("//Schedule");
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = LoadAssignmentsAsync();
    }

    partial void OnFilterStatusChanged(int value)
    {
        _ = LoadAssignmentsAsync();
    }

    private List<Assignment> FilterAssignments(List<Assignment> assignments)
    {
        var filtered = assignments;

        // Filter by search text
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            filtered = filtered.Where(a =>
                a.Title.ToLower().Contains(searchLower) ||
                (a.Description?.ToLower().Contains(searchLower) ?? false)).ToList();
        }

        // Filter by status
        filtered = FilterStatus switch
        {
            0 => filtered.Where(a => !a.IsCompleted).ToList(), // Pending
            1 => filtered.Where(a => a.IsCompleted).ToList(),   // Completed
            _ => filtered // All
        };

        return filtered;
    }

    private async Task ScheduleAssignmentNotificationAsync(Assignment assignment)
    {
        // Schedule notification 24 hours before due date
        var notifyTime = assignment.DueDate.AddHours(-24);
        if (notifyTime > DateTime.Now)
        {
            await _notificationService.ScheduleNotification(
                assignment.Id + 10000,
                "Assignment Due Soon",
                $"'{assignment.Title}' is due {assignment.DueDate:MMM dd}",
                notifyTime);
        }
    }
}
