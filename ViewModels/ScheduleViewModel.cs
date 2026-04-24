using ClassSched.Models;
using ClassSched.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ClassSched.ViewModels;

public partial class ScheduleViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private readonly NotificationService _notificationService;

    [ObservableProperty]
    private ObservableCollection<ClassSchedule> _todayClasses = new();

    [ObservableProperty]
    private ObservableCollection<IGrouping<DayOfWeek, ClassSchedule>> _weeklyClasses = new();

    [ObservableProperty]
    private ObservableCollection<ClassSchedule> _allClasses = new();

    [ObservableProperty]
    private ObservableCollection<ClassSchedule> _filteredClasses = new();

    [ObservableProperty]
    private ObservableCollection<Assignment> _upcomingAssignments = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _selectedDay = "Today";

    [ObservableProperty]
    private bool _hasClasses;

    [ObservableProperty]
    private bool _noClassesFound;

    // Statistics Properties
    [ObservableProperty]
    private int _totalClassesThisWeek;

    [ObservableProperty]
    private double _totalHoursThisWeek;

    [ObservableProperty]
    private string _nextClassTimeDisplay = "No upcoming classes";

    [ObservableProperty]
    private string _busiestDayDisplay = "-";

    [ObservableProperty]
    private int _classesTodayCount;

    // Search Properties
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private DayOfWeek? _filterDay;

    public ScheduleViewModel(DatabaseService databaseService, NotificationService notificationService)
    {
        _databaseService = databaseService;
        _notificationService = notificationService;
    }

    [RelayCommand]
    public async Task LoadClassesAsync()
    {
        IsLoading = true;

        try
        {
            var classes = await _databaseService.GetAllClassesAsync();
            AllClasses = new ObservableCollection<ClassSchedule>(classes);

            var today = DateTime.Today.DayOfWeek;
            var todayClasses = classes.Where(c => c.DayOfWeek == today).ToList();
            TodayClasses = new ObservableCollection<ClassSchedule>(todayClasses);

            var grouped = classes.GroupBy(c => c.DayOfWeek).OrderBy(g => g.Key);
            WeeklyClasses = new ObservableCollection<IGrouping<DayOfWeek, ClassSchedule>>(grouped);

            HasClasses = classes.Any();
            NoClassesFound = !HasClasses;

            // Apply search/filter
            ApplySearchAndFilter();

            // Calculate statistics
            CalculateStatistics(classes);

            // Load upcoming assignments
            await LoadUpcomingAssignmentsAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CalculateStatistics(List<ClassSchedule> classes)
    {
        TotalClassesThisWeek = classes.Count;

        TotalHoursThisWeek = classes.Sum(c => (c.EndTime - c.StartTime).TotalHours);

        var today = DateTime.Today.DayOfWeek;
        ClassesTodayCount = classes.Count(c => c.DayOfWeek == today);

        // Find busiest day
        if (classes.Any())
        {
            var dayGroups = classes.GroupBy(c => c.DayOfWeek)
                                   .Select(g => new { Day = g.Key, Count = g.Count() })
                                   .OrderByDescending(g => g.Count)
                                   .First();
            BusiestDayDisplay = dayGroups.Day.ToString();
        }
        else
        {
            BusiestDayDisplay = "-";
        }

        // Find next class
        var now = DateTime.Now;
        var nextClass = classes.Where(c => 
        {
            if (c.DayOfWeek != today) return false;
            var classStart = DateTime.Today.Add(c.StartTime);
            return classStart > now;
        })
        .OrderBy(c => c.StartTime)
        .FirstOrDefault();

        if (nextClass != null)
        {
            var timeUntil = DateTime.Today.Add(nextClass.StartTime) - now;
            if (timeUntil.TotalMinutes < 60)
            {
                NextClassTimeDisplay = $"in {timeUntil.Minutes}m";
            }
            else
            {
                NextClassTimeDisplay = $"in {timeUntil.Hours}h {timeUntil.Minutes}m";
            }
        }
        else
        {
            // Check next days
            for (int i = 1; i <= 7; i++)
            {
                var nextDay = (DayOfWeek)(((int)today + i) % 7);
                var nextDayClasses = classes.Where(c => c.DayOfWeek == nextDay)
                                           .OrderBy(c => c.StartTime)
                                           .FirstOrDefault();
                if (nextDayClasses != null)
                {
                    NextClassTimeDisplay = $"{nextDay}";
                    break;
                }
            }
            if (NextClassTimeDisplay == "No upcoming classes" && classes.Any())
            {
                NextClassTimeDisplay = "Next week";
            }
        }
    }

    private async Task LoadUpcomingAssignmentsAsync()
    {
        var assignments = await _databaseService.GetUpcomingAssignmentsAsync(3);
        UpcomingAssignments = new ObservableCollection<Assignment>(assignments);
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplySearchAndFilter();
    }

    partial void OnFilterDayChanged(DayOfWeek? value)
    {
        ApplySearchAndFilter();
    }

    private void ApplySearchAndFilter()
    {
        var filtered = AllClasses.AsEnumerable();

        // Apply search
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(c =>
                c.SubjectName.ToLower().Contains(search) ||
                c.Room.ToLower().Contains(search) ||
                (c.Instructor?.ToLower().Contains(search) ?? false));
        }

        // Apply day filter
        if (FilterDay.HasValue)
        {
            filtered = filtered.Where(c => c.DayOfWeek == FilterDay.Value);
        }

        FilteredClasses = new ObservableCollection<ClassSchedule>(filtered.ToList());
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadClassesAsync();
    }

    [RelayCommand]
    private async Task AddClassAsync()
    {
        await Shell.Current.GoToAsync("//AddEditClass");
    }

    [RelayCommand]
    private async Task EditClassAsync(ClassSchedule classSchedule)
    {
        var navigationParameter = new Dictionary<string, object>
        {
            { "ClassId", classSchedule.Id }
        };
        await Shell.Current.GoToAsync("//AddEditClass", navigationParameter);
    }

    [RelayCommand]
    private async Task DeleteClassAsync(ClassSchedule classSchedule)
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Delete Class",
            $"Are you sure you want to delete {classSchedule.SubjectName}?",
            "Delete",
            "Cancel");

        if (confirm)
        {
            await _databaseService.DeleteClassAsync(classSchedule);
            _notificationService.CancelNotification(classSchedule.Id);
            await LoadClassesAsync();
        }
    }

    [RelayCommand]
    private async Task GoToSettingsAsync()
    {
        await Shell.Current.GoToAsync("//Settings");
    }

    [RelayCommand]
    private async Task GoToAssignmentsAsync()
    {
        await Shell.Current.GoToAsync("//Assignments");
    }

    [RelayCommand]
    private async Task GoToCalendarAsync()
    {
        await Shell.Current.GoToAsync("Calendar");
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        FilterDay = null;
    }

    [RelayCommand]
    private void FilterDayChanged(DayOfWeek day)
    {
        if (FilterDay == day)
        {
            FilterDay = null; // Toggle off if same day clicked
        }
        else
        {
            FilterDay = day;
        }
    }
}
