using ClassSched.Models;
using Plugin.LocalNotification;
using System;
using System.Threading.Tasks;

namespace ClassSched.Services;

public class NotificationService
{
    public async Task InitializeAsync()
    {
        await LocalNotificationCenter.Current.RequestNotificationPermission();
    }

    public async Task ScheduleClassReminderAsync(ClassSchedule classSchedule, int reminderMinutesBefore)
    {
        if (classSchedule.Id == 0)
            return;

        CancelNotification(classSchedule.Id);

        var notificationId = classSchedule.Id;

        var nextOccurrence = GetNextOccurrence(classSchedule.DayOfWeek, classSchedule.StartTime);
        var notificationTime = nextOccurrence.AddMinutes(-reminderMinutesBefore);

        if (notificationTime <= DateTime.Now)
        {
            nextOccurrence = nextOccurrence.AddDays(7);
            notificationTime = nextOccurrence.AddMinutes(-reminderMinutesBefore);
        }

        var request = new NotificationRequest
        {
            NotificationId = notificationId,
            Title = "Class Reminder",
            Description = $"Your {classSchedule.SubjectName} class will start in {reminderMinutesBefore} minutes at {classSchedule.Room}.",
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = notificationTime,
                RepeatType = NotificationRepeat.Weekly
            },
            CategoryType = NotificationCategoryType.Reminder
        };

        await LocalNotificationCenter.Current.Show(request);
    }

    public async Task ScheduleAllRemindersAsync(ClassSchedule classSchedule, int reminderMinutesBefore)
    {
        if (classSchedule.Id == 0)
            return;

        CancelNotification(classSchedule.Id);

        var notificationId = classSchedule.Id;

        var nextOccurrence = GetNextOccurrence(classSchedule.DayOfWeek, classSchedule.StartTime);
        var notificationTime = nextOccurrence.AddMinutes(-reminderMinutesBefore);

        if (notificationTime <= DateTime.Now)
        {
            nextOccurrence = nextOccurrence.AddDays(7);
            notificationTime = nextOccurrence.AddMinutes(-reminderMinutesBefore);
        }

        var request = new NotificationRequest
        {
            NotificationId = notificationId,
            Title = "Class Reminder",
            Description = $"Your {classSchedule.SubjectName} class will start in {reminderMinutesBefore} minutes at {classSchedule.Room}.",
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = notificationTime,
                RepeatType = NotificationRepeat.Weekly
            },
            CategoryType = NotificationCategoryType.Reminder
        };

        await LocalNotificationCenter.Current.Show(request);
    }

    public void CancelNotification(int classId)
    {
        LocalNotificationCenter.Current.Cancel(classId);
    }

    public void CancelAllNotifications()
    {
        LocalNotificationCenter.Current.CancelAll();
    }

    private static DateTime GetNextOccurrence(DayOfWeek dayOfWeek, TimeSpan time)
    {
        var today = DateTime.Today;
        var daysUntilTarget = ((int)dayOfWeek - (int)today.DayOfWeek + 7) % 7;
        
        if (daysUntilTarget == 0 && DateTime.Now.TimeOfDay > time)
        {
            daysUntilTarget = 7;
        }

        return today.AddDays(daysUntilTarget).Add(time);
    }

    public async Task SendTestNotificationAsync()
    {
        var request = new NotificationRequest
        {
            NotificationId = 9999,
            Title = "Test Notification",
            Description = "Your notifications are working correctly!",
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = DateTime.Now.AddSeconds(2)
            }
        };

        await LocalNotificationCenter.Current.Show(request);
    }

    public async Task ScheduleNotification(int notificationId, string title, string description, DateTime notifyTime)
    {
        if (notifyTime <= DateTime.Now)
            return;

        var request = new NotificationRequest
        {
            NotificationId = notificationId,
            Title = title,
            Description = description,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = notifyTime
            },
            CategoryType = NotificationCategoryType.Reminder
        };

        await LocalNotificationCenter.Current.Show(request);
    }
}
