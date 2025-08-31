using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using TodoListApp.Models;
using TodoListApp.Services;
using TaskStatus = TodoListApp.Models.TaskStatus;

namespace TodoListApp
{
    public partial class MainWindow : Window
    {
        private readonly TodoService _todoService;
        private ObservableCollection<TodoTask> _currentDisplayTasks = new();
        private ObservableCollection<TodoTask> _unscheduledTasks = new();
        private HashSet<string> _selectedUnscheduled = new();
        private DateTime? _viewDate;
        private bool _isViewingUpcoming = false;
        private Timer? _autoMoveTimer;

        public MainWindow()
        {
            InitializeComponent();
            _todoService = new TodoService();

            // Set up data binding
            TodayTasksListBox.ItemsSource = _currentDisplayTasks;
            UnscheduledTasksListBox.ItemsSource = _unscheduledTasks;

            LoadTasks();
            SetupAutoMoveTimer();
        }

        private void SetupAutoMoveTimer()
        {
            try
            {
                _autoMoveTimer = new Timer(60000); // Check every minute
                _autoMoveTimer.Elapsed += AutoMoveTimer_Elapsed;
                _autoMoveTimer.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Timer setup error: {ex.Message}");
            }
        }

        private void AutoMoveTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    _todoService.AutoMoveScheduledToToday();
                    LoadTasks();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auto-move timer error: {ex.Message}");
            }
        }

        private void LoadTasks()
        {
            try
            {
                _currentDisplayTasks.Clear();
                _unscheduledTasks.Clear();

                if (_viewDate.HasValue)
                {
                    var scheduledTasks = _todoService.Tasks
                        .Where(t => t.Status == TaskStatus.Scheduled &&
                                   t.ScheduledFor?.Date == _viewDate.Value.Date)
                        .ToList();

                    foreach (var task in scheduledTasks)
                        _currentDisplayTasks.Add(task);

                    TodayLabel.Text = $"Date: {_viewDate.Value:yyyy-MM-dd}";
                }
                else
                {
                    var todayTasks = _todoService.Tasks
                        .Where(t => t.Status == TaskStatus.Today)
                        .ToList();

                    foreach (var task in todayTasks)
                        _currentDisplayTasks.Add(task);

                    TodayLabel.Text = "Today";
                }

                var unscheduled = _todoService.Tasks
                    .Where(t => t.Status == TaskStatus.Unscheduled)
                    .ToList();

                foreach (var task in unscheduled)
                    _unscheduledTasks.Add(task);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tasks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddTask();
        }

        private void TaskInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddTask();
            }
        }

        private void AddTask()
        {
            try
            {
                var text = TaskInputTextBox.Text.Trim();
                if (string.IsNullOrEmpty(text)) return;

                var priority = PriorityComboBox.SelectedIndex + 1;
                _todoService.AddTask(text, priority);

                TaskInputTextBox.Clear();
                TaskInputTextBox.Focus();
                LoadTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // REMOVED: TaskCheckBox_Changed method - now handled by two-way binding

        private void UnscheduledCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is CheckBox checkBox)
                {
                    var taskId = checkBox.Tag?.ToString();
                    if (string.IsNullOrEmpty(taskId)) return;

                    if (checkBox.IsChecked == true)
                        _selectedUnscheduled.Add(taskId);
                    else
                        _selectedUnscheduled.Remove(taskId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error selecting task: {ex.Message}");
            }
        }

        private void MoveToTodayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedUnscheduled.Any())
                {
                    _todoService.MoveTasksToToday(_selectedUnscheduled);
                    _selectedUnscheduled.Clear();
                    ClearUnscheduledSelections();
                    LoadTasks();
                }
                else
                {
                    MessageBox.Show("Please select tasks to move to today.", "No Tasks Selected");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error moving tasks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_selectedUnscheduled.Any())
                {
                    MessageBox.Show("Please select tasks to schedule.", "No Tasks Selected");
                    return;
                }

                _isViewingUpcoming = false;
                ShowCalendarModal();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error scheduling tasks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpcomingButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _isViewingUpcoming = true;
                ShowCalendarModal();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing upcoming tasks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowCalendarModal()
        {
            DatePicker.SelectedDate = DateTime.Today;
            CalendarModal.Visibility = Visibility.Visible;
        }

        private void CalendarOkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DatePicker.SelectedDate.HasValue)
                {
                    if (_isViewingUpcoming)
                    {
                        _viewDate = DatePicker.SelectedDate.Value;
                        LoadTasks();
                    }
                    else
                    {
                        _todoService.ScheduleTasks(_selectedUnscheduled, DatePicker.SelectedDate.Value);
                        _selectedUnscheduled.Clear();
                        ClearUnscheduledSelections();
                        LoadTasks();
                    }
                    CalendarModal.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MessageBox.Show("Please select a date!", "No Date Selected");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing calendar selection: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseCalendarButton_Click(object sender, RoutedEventArgs e)
        {
            CalendarModal.Visibility = Visibility.Collapsed;
            if (_isViewingUpcoming)
            {
                _viewDate = null;
                LoadTasks();
            }
        }

        private void MoveToArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewDate.HasValue)
                {
                    _todoService.MoveCompletedTasksToArchive(TaskStatus.Scheduled, _viewDate);
                    _viewDate = null;
                }
                else
                {
                    _todoService.MoveCompletedTasksToArchive(TaskStatus.Today);
                }

                LoadTasks();
                MessageBox.Show("Completed tasks moved to archive successfully!", "Archive Updated");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error archiving tasks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    FilterIndex = 1,
                    FileName = $"todo-archive-{DateTime.Today:yyyy-MM-dd}.csv",
                    DefaultExt = "csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var csv = _todoService.ExportArchiveToCsv();

                    using (var writer = new StreamWriter(saveFileDialog.FileName, false, System.Text.Encoding.UTF8))
                    {
                        writer.Write(csv);
                        writer.Flush();
                    }

                    MessageBox.Show($"Archive exported successfully!\nFile saved to: {saveFileDialog.FileName}", "Export Successful");

                    if (MessageBox.Show("Would you like to open the exported file?", "Open File", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = saveFileDialog.FileName,
                                UseShellExecute = true
                            });
                        }
                        catch (Exception)
                        {
                            MessageBox.Show($"File saved successfully but could not open automatically.\nPlease open the file manually:\n{saveFileDialog.FileName}", "File Saved");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting archive: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearUnscheduledSelections()
        {
            try
            {
                foreach (var item in UnscheduledTasksListBox.Items)
                {
                    var container = UnscheduledTasksListBox.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                    if (container != null)
                    {
                        var checkBox = FindVisualChild<CheckBox>(container);
                        if (checkBox != null)
                            checkBox.IsChecked = false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing selections: {ex.Message}");
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var foundChild = FindVisualChild<T>(child);
                if (foundChild != null)
                    return foundChild;
            }
            return null;
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                _autoMoveTimer?.Stop();
                _autoMoveTimer?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing timer: {ex.Message}");
            }
            finally
            {
                base.OnClosed(e);
            }
        }
    }
}
