using ClassSched.Models;
using ClassSched.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;

namespace ClassSched.ViewModels;

public partial class CalendarViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private ObservableCollection<ClassSchedule> _selectedDayClasses = new();

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty]
    private string _selectedDateDisplay = DateTime.Today.ToString("dddd, MMMM dd, yyyy");

    [ObservableProperty]
    private int _currentMonth = DateTime.Today.Month;

    [ObservableProperty]
    private int _currentYear = DateTime.Today.Year;

    [ObservableProperty]
    private string _monthYearDisplay = DateTime.Today.ToString("MMMM yyyy");

    [ObservableProperty]
    private ObservableCollection<CalendarDay> _calendarDays = new();

    private List<ClassSchedule> _allClasses = new();

    public CalendarViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task LoadDataAsync()
    {
        _allClasses = await _databaseService.GetAllClassesAsync();
        GenerateCalendar();
        LoadClassesForSelectedDate();
    }

    [RelayCommand]
    public async Task SelectDateAsync(CalendarDay day)
    {
        if (day == null) return;

        // Update previous selection
        var previousDay = CalendarDays.FirstOrDefault(d => d.Date == SelectedDate);
        if (previousDay != null)
        {
            previousDay.IsSelected = false;
        }

        // Update new selection
        SelectedDate = day.Date;
        day.IsSelected = true;
        SelectedDateDisplay = SelectedDate.ToString("dddd, MMMM dd, yyyy");
        
        LoadClassesForSelectedDate();

        // Show dialog with schedule details
        await ShowScheduleDialogAsync(day);
    }

    private async Task ShowScheduleDialogAsync(CalendarDay day)
    {
        var dayOfWeek = day.Date.DayOfWeek;
        var classesForDay = _allClasses
            .Where(c => c.DayOfWeek == dayOfWeek)
            .OrderBy(c => c.StartTime)
            .ToList();

        var dateString = day.Date.ToString("dddd, MMMM dd, yyyy");

        if (classesForDay.Count == 0)
        {
            await Shell.Current.DisplayAlert(
                dateString,
                "No schedule for this date.",
                "OK");
        }
        else
        {
            var scheduleDetails = string.Join("\n\n", classesForDay.Select(c => 
                $"📚 {c.SubjectName}\n" +
                $"🕐 {c.TimeDisplay}\n" +
                $"📍 {c.Room}" +
                (string.IsNullOrEmpty(c.Instructor) ? "" : $"\n👤 {c.Instructor}")
            ));

            await Shell.Current.DisplayAlert(
                $"Schedule for {dateString}",
                scheduleDetails,
                "OK");
        }
    }

    [RelayCommand]
    public void PreviousMonth()
    {
        CurrentMonth--;
        if (CurrentMonth < 1)
        {
            CurrentMonth = 12;
            CurrentYear--;
        }
        MonthYearDisplay = new DateTime(CurrentYear, CurrentMonth, 1).ToString("MMMM yyyy");
        GenerateCalendar();
    }

    [RelayCommand]
    public void NextMonth()
    {
        CurrentMonth++;
        if (CurrentMonth > 12)
        {
            CurrentMonth = 1;
            CurrentYear++;
        }
        MonthYearDisplay = new DateTime(CurrentYear, CurrentMonth, 1).ToString("MMMM yyyy");
        GenerateCalendar();
    }

    [RelayCommand]
    public async Task GoToTodayAsync()
    {
        CurrentMonth = DateTime.Today.Month;
        CurrentYear = DateTime.Today.Year;
        MonthYearDisplay = new DateTime(CurrentYear, CurrentMonth, 1).ToString("MMMM yyyy");
        
        SelectedDate = DateTime.Today;
        SelectedDateDisplay = SelectedDate.ToString("dddd, MMMM dd, yyyy");
        
        await LoadDataAsync();
    }

    [RelayCommand]
    public async Task CloseAsync()
    {
        // Pop the Calendar page from the navigation stack
        await Shell.Current.Navigation.PopAsync();
    }

    private void GenerateCalendar()
    {
        var days = new List<CalendarDay>();
        var firstDayOfMonth = new DateTime(CurrentYear, CurrentMonth, 1);
        var daysInMonth = DateTime.DaysInMonth(CurrentYear, CurrentMonth);
        
        // Get the day of week for the first day (0 = Sunday, 1 = Monday, etc.)
        var firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
        
        // Adjust for Monday as first day of week (Mon=0, Tue=1, ..., Sun=6)
        var startOffset = firstDayOfWeek == 0 ? 6 : firstDayOfWeek - 1;

        // Add empty days for padding
        for (int i = 0; i < startOffset; i++)
        {
            days.Add(new CalendarDay { IsEmpty = true, Column = i, Row = 0 });
        }

        // Add actual days
        int currentRow = 0;
        int currentCol = startOffset;
        
        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(CurrentYear, CurrentMonth, day);
            var dayOfWeek = date.DayOfWeek;
            
            // Check if there are classes on this day
            var hasClasses = _allClasses.Any(c => c.DayOfWeek == dayOfWeek);
            var classCount = _allClasses.Count(c => c.DayOfWeek == dayOfWeek);
            
            days.Add(new CalendarDay
            {
                Date = date,
                DayNumber = day,
                IsToday = date.Date == DateTime.Today.Date,
                IsSelected = date.Date == SelectedDate.Date,
                HasClasses = hasClasses,
                ClassCount = classCount,
                DayOfWeek = dayOfWeek,
                Column = currentCol,
                Row = currentRow
            });
            
            // Move to next position
            currentCol++;
            if (currentCol > 6)
            {
                currentCol = 0;
                currentRow++;
            }
        }

        CalendarDays = new ObservableCollection<CalendarDay>(days);
    }

    private void LoadClassesForSelectedDate()
    {
        var dayOfWeek = SelectedDate.DayOfWeek;
        var classes = _allClasses
            .Where(c => c.DayOfWeek == dayOfWeek)
            .OrderBy(c => c.StartTime)
            .ToList();
        
        SelectedDayClasses = new ObservableCollection<ClassSchedule>(classes);
    }
}

public partial class CalendarDay : ObservableObject
{
    [ObservableProperty]
    private DateTime _date;

    [ObservableProperty]
    private int _dayNumber;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private bool _isToday;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _hasClasses;

    [ObservableProperty]
    private int _classCount;

    [ObservableProperty]
    private DayOfWeek _dayOfWeek;

    [ObservableProperty]
    private int _column;

    [ObservableProperty]
    private int _row;

    public Color BackgroundColor => IsSelected ? Color.FromArgb("#512BD4") : 
                                     IsToday ? Color.FromArgb("#E8E0F0") : 
                                     Colors.Transparent;
    
    public Color TextColor => IsSelected ? Colors.White : 
                              IsToday ? Color.FromArgb("#512BD4") : 
                              Colors.White;
    
    public Color IndicatorColor => IsSelected ? Colors.White : Color.FromArgb("#512BD4");
}
