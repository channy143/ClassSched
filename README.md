# Class Sched - .NET MAUI Class Scheduling App

A simple and user-friendly Class Scheduling App built with .NET MAUI that allows students to manage their class schedules and receive reminders before classes start.

## Features

- **Add Class**: Input subject name, room, instructor, day of week, start & end time
- **Edit Class**: Tap any class to update details
- **Delete Class**: Swipe to delete with confirmation
- **Smart Reminders**: Get notified 10-15 minutes before class starts
- **Weekly Overview**: View all classes organized by day
- **Today's Schedule**: Quick view of today's classes

## Built With

- **.NET MAUI** - Cross-platform framework (Android, iOS, Windows)
- **C#** - Programming language
- **SQLite** - Local database storage
- **Plugin.LocalNotification** - Local notifications
- **MVVM Architecture** - Clean and organized code

## Project Structure

```
ClassSched/
├── Models/
│   └── ClassSchedule.cs          # Data model
├── Services/
│   ├── DatabaseService.cs        # SQLite operations
│   ├── NotificationService.cs    # Local notifications
│   └── SettingsService.cs        # App preferences
├── ViewModels/
│   ├── ScheduleViewModel.cs      # Main schedule logic
│   ├── AddEditClassViewModel.cs  # Add/Edit logic
│   └── SettingsViewModel.cs      # Settings logic
├── Views/
│   ├── SchedulePage.xaml         # Main schedule UI
│   ├── AddEditClassPage.xaml     # Add/Edit form UI
│   └── SettingsPage.xaml         # Settings UI
├── Converters/
│   ├── ColorFromHexConverter.cs  # Color conversion
│   └── StringIsNotNullOrEmptyConverter.cs
└── Resources/
    ├── AppIcon/                  # App icons
    ├── Splash/                   # Splash screen
    ├── Fonts/                    # Custom fonts
    └── Styles/                   # Colors & Styles
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or later with .NET MAUI workload
- Android SDK (for Android development)
- Xcode (for iOS development on Mac)

### Installation

1. Clone the repository or extract the project files
2. Open the solution in Visual Studio 2022
3. Restore NuGet packages
4. Select your target platform (Android, iOS, or Windows)
5. Build and run the application

### NuGet Packages

The following packages are used:

- `sqlite-net-pcl` (1.8.116) - SQLite database
- `Plugin.LocalNotification` (11.1.2) - Local notifications
- `CommunityToolkit.Mvvm` (8.2.2) - MVVM support
- `CommunityToolkit.Maui` (9.0.0) - MAUI toolkit

## Usage

1. **Add a Class**: Tap the "+" button and fill in the details
2. **Edit a Class**: Tap on any class card or swipe left and tap "Edit"
3. **Delete a Class**: Swipe left on a class and tap "Delete"
4. **Set Reminders**: Go to Settings to enable notifications and set reminder time
5. **View Weekly Schedule**: Scroll down to see all classes organized by day

## Architecture

The app follows the **MVVM (Model-View-ViewModel)** pattern:

- **Models**: Define data structures (ClassSchedule)
- **Views**: XAML UI pages
- **ViewModels**: Business logic and data binding
- **Services**: Database and notification handling

## Screens

- **Home / Schedule Page**: Displays daily and weekly class list
- **Add/Edit Page**: Input form for class details
- **Settings Page**: Configure reminder time and notification preferences

## UI Design

- Minimalist layout with card-style class displays
- Soft academic color theme (purple/blue primary)
- Easy navigation with intuitive gestures
- Responsive design for all screen sizes

## Future Improvements

- Calendar view integration
- Dark mode support
- Cloud sync capability
- Export schedule to PDF
- Widget support
- Multiple schedule profiles

## License

This project is open source and available for personal and educational use.

## Support

For issues or feature requests, please refer to the project documentation or create an issue in the repository.
