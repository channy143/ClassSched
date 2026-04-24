using ClassSched.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClassSched.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _isInitialized = false;

    public async Task InitAsync()
    {
        if (_isInitialized)
            return;

        await _initLock.WaitAsync();
        try
        {
            if (_isInitialized)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "classsched.db3");
            _database = new SQLiteAsyncConnection(databasePath);

            await _database.CreateTableAsync<ClassSchedule>();
            await _database.CreateTableAsync<User>();
            await _database.CreateTableAsync<Assignment>();
            await _database.CreateTableAsync<ClassAttendance>();
            _isInitialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        await InitAsync();
        return _database!;
    }

    public async Task<List<ClassSchedule>> GetAllClassesAsync()
    {
        await InitAsync();
        return await _database!.Table<ClassSchedule>().OrderBy(c => c.DayOfWeek).ThenBy(c => c.StartTime).ToListAsync();
    }

    public async Task<List<ClassSchedule>> GetClassesForDayAsync(DayOfWeek day)
    {
        await InitAsync();
        return await _database!.Table<ClassSchedule>()
            .Where(c => c.DayOfWeek == day)
            .OrderBy(c => c.StartTime)
            .ToListAsync();
    }

    public async Task<List<ClassSchedule>> GetTodayClassesAsync()
    {
        return await GetClassesForDayAsync(DateTime.Today.DayOfWeek);
    }

    public async Task<ClassSchedule?> GetClassAsync(int id)
    {
        await InitAsync();
        return await _database!.Table<ClassSchedule>().Where(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveClassAsync(ClassSchedule classSchedule)
    {
        await InitAsync();
        classSchedule.UpdatedAt = DateTime.Now;

        if (classSchedule.Id != 0)
        {
            return await _database!.UpdateAsync(classSchedule);
        }
        else
        {
            classSchedule.CreatedAt = DateTime.Now;
            return await _database!.InsertAsync(classSchedule);
        }
    }

    public async Task<int> DeleteClassAsync(ClassSchedule classSchedule)
    {
        await InitAsync();
        return await _database!.DeleteAsync(classSchedule);
    }

    public async Task<int> DeleteClassAsync(int id)
    {
        await InitAsync();
        var classSchedule = await GetClassAsync(id);
        if (classSchedule != null)
            return await _database!.DeleteAsync(classSchedule);
        return 0;
    }

    public async Task<List<ClassSchedule>> GetUpcomingClassesAsync(int minutesAhead)
    {
        await InitAsync();
        var now = DateTime.Now;
        var futureTime = now.AddMinutes(minutesAhead);

        var allClasses = await GetAllClassesAsync();
        
        return allClasses.Where(c => 
        {
            if (c.DayOfWeek != now.DayOfWeek)
                return false;

            var classStart = DateTime.Today.Add(c.StartTime);
            return classStart > now && classStart <= futureTime;
        }).ToList();
    }

    // Assignment CRUD Methods
    public async Task<List<Assignment>> GetAllAssignmentsAsync()
    {
        await InitAsync();
        return await _database!.Table<Assignment>()
            .OrderBy(a => a.DueDate)
            .ToListAsync();
    }

    public async Task<List<Assignment>> GetUpcomingAssignmentsAsync(int count = 5)
    {
        await InitAsync();
        return await _database!.Table<Assignment>()
            .Where(a => !a.IsCompleted && a.DueDate >= DateTime.Now)
            .OrderBy(a => a.DueDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Assignment>> GetOverdueAssignmentsAsync()
    {
        await InitAsync();
        return await _database!.Table<Assignment>()
            .Where(a => !a.IsCompleted && a.DueDate < DateTime.Now)
            .OrderBy(a => a.DueDate)
            .ToListAsync();
    }

    public async Task<List<Assignment>> GetAssignmentsForClassAsync(int classScheduleId)
    {
        await InitAsync();
        return await _database!.Table<Assignment>()
            .Where(a => a.ClassScheduleId == classScheduleId)
            .OrderBy(a => a.DueDate)
            .ToListAsync();
    }

    public async Task<Assignment?> GetAssignmentAsync(int id)
    {
        await InitAsync();
        return await _database!.Table<Assignment>().Where(a => a.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveAssignmentAsync(Assignment assignment)
    {
        await InitAsync();
        assignment.UpdatedAt = DateTime.Now;

        if (assignment.Id != 0)
        {
            return await _database!.UpdateAsync(assignment);
        }
        else
        {
            assignment.CreatedAt = DateTime.Now;
            return await _database!.InsertAsync(assignment);
        }
    }

    public async Task<int> DeleteAssignmentAsync(Assignment assignment)
    {
        await InitAsync();
        return await _database!.DeleteAsync(assignment);
    }

    public async Task<int> DeleteAssignmentAsync(int id)
    {
        await InitAsync();
        var assignment = await GetAssignmentAsync(id);
        if (assignment != null)
            return await _database!.DeleteAsync(assignment);
        return 0;
    }

    public async Task<int> MarkAssignmentCompleteAsync(int id, bool isCompleted)
    {
        await InitAsync();
        var assignment = await GetAssignmentAsync(id);
        if (assignment != null)
        {
            assignment.IsCompleted = isCompleted;
            assignment.UpdatedAt = DateTime.Now;
            return await _database!.UpdateAsync(assignment);
        }
        return 0;
    }

    // Check for schedule conflicts (excluding the current class when editing)
    public async Task<ClassSchedule?> GetConflictingClassAsync(DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeClassId = null)
    {
        await InitAsync();
        
        var classesForDay = await GetClassesForDayAsync(dayOfWeek);
        
        return classesForDay.FirstOrDefault(c =>
        {
            // Skip the class being edited
            if (excludeClassId.HasValue && c.Id == excludeClassId.Value)
                return false;
            
            // Check for time overlap
            // Two time ranges overlap if:
            // start1 < end2 AND start2 < end1
            var overlap = startTime < c.EndTime && c.StartTime < endTime;
            
            return overlap;
        });
    }

    // Attendance CRUD Methods
    public async Task<int> RecordAttendanceAsync(ClassAttendance attendance)
    {
        await InitAsync();
        attendance.CreatedAt = DateTime.Now;

        // Check if attendance record already exists for this class and date
        var existing = await _database!.Table<ClassAttendance>()
            .Where(a => a.ClassScheduleId == attendance.ClassScheduleId && a.Date.Date == attendance.Date.Date)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            // Update existing record
            existing.Status = attendance.Status;
            existing.Notes = attendance.Notes;
            existing.CreatedAt = DateTime.Now;
            return await _database.UpdateAsync(existing);
        }
        else
        {
            // Insert new record
            return await _database.InsertAsync(attendance);
        }
    }

    public async Task<ClassAttendance?> GetAttendanceForClassAndDateAsync(int classScheduleId, DateTime date)
    {
        await InitAsync();
        return await _database!.Table<ClassAttendance>()
            .Where(a => a.ClassScheduleId == classScheduleId && a.Date.Date == date.Date)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ClassAttendance>> GetAttendanceForClassAsync(int classScheduleId, DateTime? startDate = null, DateTime? endDate = null)
    {
        await InitAsync();
        var query = _database!.Table<ClassAttendance>()
            .Where(a => a.ClassScheduleId == classScheduleId);

        if (startDate.HasValue)
            query = query.Where(a => a.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Date <= endDate.Value);

        return await query.OrderByDescending(a => a.Date).ToListAsync();
    }

    public async Task<List<ClassAttendance>> GetAttendanceForDateAsync(DateTime date)
    {
        await InitAsync();
        return await _database!.Table<ClassAttendance>()
            .Where(a => a.Date.Date == date.Date)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<(int Attended, int Missed, int Excused, int Late, int Total, double Rate)> GetAttendanceStatisticsAsync(int classScheduleId)
    {
        await InitAsync();
        var records = await _database!.Table<ClassAttendance>()
            .Where(a => a.ClassScheduleId == classScheduleId)
            .ToListAsync();

        var attended = records.Count(a => a.Status == AttendanceStatus.Attended);
        var missed = records.Count(a => a.Status == AttendanceStatus.Missed);
        var excused = records.Count(a => a.Status == AttendanceStatus.Excused);
        var late = records.Count(a => a.Status == AttendanceStatus.Late);
        var total = records.Count;
        var rate = total > 0 ? (double)attended / total * 100 : 0;

        return (attended, missed, excused, late, total, rate);
    }

    public async Task<(int Attended, int Missed, int Excused, int Late, int Total, double Rate)> GetOverallAttendanceStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        await InitAsync();
        var query = _database!.Table<ClassAttendance>();

        List<ClassAttendance> records;
        if (startDate.HasValue && endDate.HasValue)
        {
            records = await query.Where(a => a.Date >= startDate.Value && a.Date <= endDate.Value).ToListAsync();
        }
        else if (startDate.HasValue)
        {
            records = await query.Where(a => a.Date >= startDate.Value).ToListAsync();
        }
        else if (endDate.HasValue)
        {
            records = await query.Where(a => a.Date <= endDate.Value).ToListAsync();
        }
        else
        {
            records = await query.ToListAsync();
        }

        var attended = records.Count(a => a.Status == AttendanceStatus.Attended);
        var missed = records.Count(a => a.Status == AttendanceStatus.Missed);
        var excused = records.Count(a => a.Status == AttendanceStatus.Excused);
        var late = records.Count(a => a.Status == AttendanceStatus.Late);
        var total = records.Count;
        var rate = total > 0 ? (double)attended / total * 100 : 0;

        return (attended, missed, excused, late, total, rate);
    }

    public async Task<int> DeleteAttendanceAsync(int attendanceId)
    {
        await InitAsync();
        var attendance = await _database!.Table<ClassAttendance>().Where(a => a.Id == attendanceId).FirstOrDefaultAsync();
        if (attendance != null)
            return await _database.DeleteAsync(attendance);
        return 0;
    }

    public async Task<int> DeleteAttendanceForClassAndDateAsync(int classScheduleId, DateTime date)
    {
        await InitAsync();
        var attendance = await _database!.Table<ClassAttendance>()
            .Where(a => a.ClassScheduleId == classScheduleId && a.Date.Date == date.Date)
            .FirstOrDefaultAsync();

        if (attendance != null)
            return await _database.DeleteAsync(attendance);
        return 0;
    }

    public async Task<List<ClassAttendance>> GetRecentAttendanceAsync(int count = 50)
    {
        await InitAsync();
        return await _database!.Table<ClassAttendance>()
            .OrderByDescending(a => a.Date)
            .Take(count)
            .ToListAsync();
    }
}
