using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using OpenVTT.Models;
using OpenVTT.Services;

namespace OpenVTT
{
    public partial class MainWindow : Window
    {
        private string _currentPath = string.Empty;
        private List<Scene> _scenes = new();

        public MainWindow()
        {
            InitializeComponent();
            FontSlider.Value = 16;
            UpdateScenes();
            ScriptBox.Focus();
            this.KeyDown += MainWindow_KeyDown;
            ScriptBox.Text = ScriptBox.Text.Length == 0 ? SampleScript() : ScriptBox.Text;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                StartTeleprompter();
                e.Handled = true;
            }
            else if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                SaveAs();
                e.Handled = true;
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            _currentPath = string.Empty;
            ScriptBox.Text = SampleScript();
            UpdateScenes();
            StatusText.Text = "New script created.";
        }

        private string SampleScript() =>
            "# Session 1: Road to Phandolin

Welcome to OpenVTT! Use # or ## for scenes.

## Scene: Roadside Ambush

Read this aloud...

## Scene: Town Gate

Read this next...";

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog { Filter = "Text & Markdown|*.txt;*.md|All files|*.*" };
            if (ofd.ShowDialog() == true)
            {
                _currentPath = ofd.FileName;
                ScriptBox.Text = File.ReadAllText(_currentPath, Encoding.UTF8);
                UpdateScenes();
                StatusText.Text = $"Opened {_currentPath}";
            }
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e) => SaveAs();

        private void SaveAs()
        {
            var sfd = new SaveFileDialog { Filter = "Text|*.txt|Markdown|*.md", FileName = string.IsNullOrWhiteSpace(_currentPath) ? "script.md" : Path.GetFileName(_currentPath) };
            if (sfd.ShowDialog() == true)
            {
                _currentPath = sfd.FileName;
                File.WriteAllText(_currentPath, ScriptBox.Text, Encoding.UTF8);
                StatusText.Text = $"Saved to {_currentPath}";
            }
        }

        private void FontSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ScriptBox is null) return;
            ScriptBox.FontSize = e.NewValue;
        }

        private void ScriptBox_TextChanged(object sender, TextChangedEventArgs e) => UpdateScenes();

        private void UpdateScenes()
        {
            _scenes = ScriptParser.ParseScenes(ScriptBox.Text);
            ScenesList.ItemsSource = _scenes;
            if (_scenes.Count > 0 && ScenesList.SelectedIndex < 0)
                ScenesList.SelectedIndex = 0;
        }

        private void ScenesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ScenesList.SelectedItem is Scene s)
            {
                ScriptBox.Focus();
                ScriptBox.CaretIndex = Math.Clamp(s.StartCharIndex, 0, ScriptBox.Text.Length);
                ScriptBox.ScrollToLine(GetLineIndexFromCharIndex(ScriptBox, s.StartCharIndex));
                StatusText.Text = $"Jumped to scene: {s.Title}";
            }
        }

        private static int GetLineIndexFromCharIndex(TextBox tb, int charIndex)
        {
            charIndex = Math.Clamp(charIndex, 0, tb.Text.Length);
            int line = tb.GetLineIndexFromCharacterIndex(charIndex);
            return Math.Max(0, line);
        }

        private void StartTeleprompter_Click(object sender, RoutedEventArgs e) => StartTeleprompter();

        private void StartTeleprompter()
        {
            if (_scenes.Count == 0) return;
            Scene current = ScenesList.SelectedItem as Scene ?? _scenes.First();
            Scene? next = null;
            int idx = _scenes.IndexOf(current);
            if (idx >= 0 && idx < _scenes.Count - 1) next = _scenes[idx + 1];

            string text = OpenVTT.Services.ScriptParser.GetSceneText(ScriptBox.Text, current, next);
            var tp = new TeleprompterWindow(text)
            {
                Owner = this,
                Mirror = MirrorToggle.IsChecked == true
            };
            tp.Show();
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FindNext();
                e.Handled = true;
            }
        }

        private void FindNext_Click(object sender, RoutedEventArgs e) => FindNext();

        private void FindNext()
        {
            string query = SearchBox.Text;
            if (string.IsNullOrWhiteSpace(query)) return;

            int start = Math.Max(ScriptBox.CaretIndex + 1, 0);
            int pos = ScriptBox.Text.IndexOf(query, start, StringComparison.OrdinalIgnoreCase);
            if (pos < 0) pos = ScriptBox.Text.IndexOf(query, 0, StringComparison.OrdinalIgnoreCase);
            if (pos >= 0)
            {
                ScriptBox.Focus();
                ScriptBox.CaretIndex = pos;
                ScriptBox.Select(pos, query.Length);
                ScriptBox.ScrollToLine(GetLineIndexFromCharIndex(ScriptBox, pos));
                StatusText.Text = $"Found at {pos}";
            }
            else
            {
                StatusText.Text = "No matches";
            }
        }
    }
}
