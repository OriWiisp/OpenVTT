using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace OpenVTT
{
    public partial class TeleprompterWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private readonly Stopwatch _clock = new();
        private double _pixelsPerSecond = 140; // default speed
        private bool _isPlaying = false;
        private bool _isFullscreen = false;
        private bool _isBlackout = false;

        public bool Mirror { get; set; }

        public TeleprompterWindow(string text)
        {
            InitializeComponent();
            PromptText.Text = Prettify(text);
            MirrorToggle.IsChecked = Mirror;
            ApplyMirror();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // ~60fps
            _timer.Tick += Timer_Tick;
            _timer.Start();
            _clock.Start();
            UpdateStatus();
        }

        private static string Prettify(string input)
        {
            var lines = input.Replace("
", "
").Split('
');
            var list = new List<string>();
            foreach (var l in lines)
            {
                var s = l.TrimEnd();
                if (s.StartsWith("#")) s = s.TrimStart('#', ' ');
                list.Add(s);
            }
            return string.Join("
", list).Trim();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (!_isPlaying || _pixelsPerSecond <= 0) { _clock.Restart(); return; }

            double dt = _clock.Elapsed.TotalSeconds;
            _clock.Restart();
            double dy = _pixelsPerSecond * dt;
            Scroller.ScrollToVerticalOffset(Scroller.VerticalOffset + dy);
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            Status.Text = $"Offset: {Scroller.VerticalOffset:F0}px · Speed: {_pixelsPerSecond:F0}px/s";
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e) => TogglePlay();
        private void TogglePlay()
        {
            _isPlaying = !_isPlaying;
            PlayPauseBtn.Content = _isPlaying ? "❚❚ Pause" : "▶ Play";
        }
        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            Scroller.ScrollToVerticalOffset(0);
            _isPlaying = false;
            PlayPauseBtn.Content = "▶ Play";
        }
        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        { _pixelsPerSecond = e.NewValue; UpdateStatus(); }
        private void FontSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        { PromptText.FontSize = e.NewValue; }
        private void MirrorToggle_Checked(object sender, RoutedEventArgs e)
        { Mirror = (sender as ToggleButton)!.IsChecked == true; ApplyMirror(); }
        private void ApplyMirror()
        {
            if (PromptText is null) return;
            PromptText.LayoutTransform = Mirror ? new ScaleTransform { ScaleX = -1, ScaleY = 1 } : Transform.Identity;
        }
        private void Fullscreen_Click(object sender, RoutedEventArgs e) => ToggleFullscreen();
        private void ToggleFullscreen()
        {
            _isFullscreen = !_isFullscreen;
            if (_isFullscreen)
            { WindowStyle = WindowStyle.None; WindowState = WindowState.Maximized; Topmost = true; Background = Brushes.Black; }
            else
            { WindowStyle = WindowStyle.SingleBorderWindow; WindowState = WindowState.Normal; Topmost = false; Background = Brushes.Black; }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) { TogglePlay(); e.Handled = true; }
            else if (e.Key == Key.Up) { SpeedSlider.Value = Math.Min(SpeedSlider.Maximum, SpeedSlider.Value + 20); e.Handled = true; }
            else if (e.Key == Key.Down) { SpeedSlider.Value = Math.Max(SpeedSlider.Minimum, SpeedSlider.Value - 20); e.Handled = true; }
            else if (e.Key == Key.F11) { ToggleFullscreen(); e.Handled = true; }
            else if (e.Key == Key.B) { ToggleBlackout(); e.Handled = true; }
            else if (e.Key == Key.Escape)
            { if (_isFullscreen) ToggleFullscreen(); else Close(); e.Handled = true; }
        }
        private void ToggleBlackout()
        {
            _isBlackout = !_isBlackout;
            PromptText.Visibility = _isBlackout ? Visibility.Hidden : Visibility.Visible;
            Background = Brushes.Black;
        }
    }
}
