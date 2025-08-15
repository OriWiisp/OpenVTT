using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using OpenVTT.Models;

namespace OpenVTT.Controls
{
    public partial class Tabletop2DControl : UserControl
    {
        private double _grid = 48;
        private readonly List<UIElement> _tokens = new();

        public Tabletop2DControl()
        {
            InitializeComponent();
            Board.Tag = MakeGridBrush(_grid);
            Board.MouseLeftButtonDown += Board_MouseLeftButtonDown;
        }

        private Brush MakeGridBrush(double cell)
        {
            var db = new DrawingBrush
            {
                TileMode = TileMode.Tile,
                ViewportUnits = BrushMappingMode.Absolute,
                Viewport = new Rect(0,0,cell,cell),
                Drawing = new GeometryDrawing
                {
                    Brush = new SolidColorBrush(Color.FromRgb(250,245,220)),
                    Pen = new Pen(new SolidColorBrush(Color.FromRgb(180,160,120)), 1)
                }
            };
            var g = new GeometryGroup();
            g.Children.Add(new LineGeometry(new Point(0,0), new Point(cell,0)));
            g.Children.Add(new LineGeometry(new Point(0,0), new Point(0,cell)));
            (db.Drawing as GeometryDrawing)!.Geometry = g;
            return db;
        }

        private void AddToken_Click(object sender, RoutedEventArgs e)
        {
            var token = MakeToken("PC");
            Canvas.SetLeft(token, 3*_grid);
            Canvas.SetTop(token, 3*_grid);
            Board.Children.Add(token);
            _tokens.Add(token);
        }

        private Border MakeToken(string label)
        {
            var circle = new Border
            {
                Width = _grid*0.9, Height = _grid*0.9,
                Background = new SolidColorBrush(Color.FromRgb(122, 27, 29)),
                BorderBrush = Brushes.Black, BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(_grid),
                Child = new TextBlock { Text = label, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Bold }
            };
            circle.MouseLeftButtonDown += (s, e) => { circle.CaptureMouse(); _dragStart = e.GetPosition(Board); _tokenStart = new Point(Canvas.GetLeft(circle), Canvas.GetTop(circle)); };
            circle.MouseMove += (s, e) => { if (circle.IsMouseCaptured) DragToken(circle, e.GetPosition(Board)); };
            circle.MouseLeftButtonUp += (s, e) => { circle.ReleaseMouseCapture(); SnapToken(circle); };
            return circle;
        }

        private Point _dragStart; private Point _tokenStart;
        private void DragToken(FrameworkElement el, Point p)
        {
            var dx = p.X - _dragStart.X; var dy = p.Y - _dragStart.Y;
            Canvas.SetLeft(el, _tokenStart.X + dx);
            Canvas.SetTop(el, _tokenStart.Y + dy);
        }
        private void SnapToken(FrameworkElement el)
        {
            double x = Canvas.GetLeft(el); double y = Canvas.GetTop(el);
            x = Math.Round(x/_grid)*_grid + (_grid-el.Width)/2; y = Math.Round(y/_grid)*_grid + (_grid-el.Height)/2;
            Canvas.SetLeft(el, x); Canvas.SetTop(el, y);
        }

        private void ClearTokens_Click(object sender, RoutedEventArgs e)
        {
            foreach (var t in _tokens) Board.Children.Remove(t);
            _tokens.Clear();
        }

        private void GridSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _grid = e.NewValue;
            Board.Tag = MakeGridBrush(_grid);
            foreach (var t in _tokens) if (t is FrameworkElement fe) { fe.Width = _grid*0.9; fe.Height = _grid*0.9; }
        }

        private void Zoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ZoomTx.ScaleX = Zoom.Value; ZoomTx.ScaleY = Zoom.Value;
        }

        private void Board_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
        }

        private void OpenMap_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog { Filter = "OpenVTT Map|*.ovttmap|JSON|*.json|All files|*.*" };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    var json = File.ReadAllText(ofd.FileName);
                    var map = JsonSerializer.Deserialize<MapFile>(json);
                    if (map != null) ApplyMapToBackground(map);
                }
                catch (Exception ex)
                { MessageBox.Show($"Error opening map: {ex.Message}"); }
            }
        }

        private void ApplyMapToBackground(MapFile map)
        {
            // Render simple tiles to a VisualBrush and use it as background
            double cell = _grid;
            int w = map.Width, h = map.Height;
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(250,245,220)), null, new Rect(0,0,w*cell,h*cell));
                for (int y=0;y<h;y++) for (int x=0;x<w;x++)
                {
                    var r = new Rect(x*cell, y*cell, cell, cell);
                    var tile = (MapTile)map.Tiles[y*w + x];
                    Brush b = tile switch
                    {
                        MapTile.Floor => new SolidColorBrush(Color.FromRgb(230, 220, 190)),
                        MapTile.Wall  => new SolidColorBrush(Color.FromRgb(120, 120, 120)),
                        MapTile.Door  => new SolidColorBrush(Color.FromRgb(160, 110, 60)),
                        MapTile.Water => new SolidColorBrush(Color.FromRgb(90, 150, 210)),
                        MapTile.Trap  => new SolidColorBrush(Color.FromRgb(170, 40, 40)),
                        _ => Brushes.Transparent
                    };
                    if (b != Brushes.Transparent) dc.DrawRectangle(b, new Pen(Brushes.Black, 0.5), r);
                    // grid lines
                    dc.DrawLine(new Pen(new SolidColorBrush(Color.FromRgb(180,160,120)), 0.5), new Point(r.Left, r.Top), new Point(r.Right, r.Top));
                    dc.DrawLine(new Pen(new SolidColorBrush(Color.FromRgb(180,160,120)), 0.5), new Point(r.Left, r.Top), new Point(r.Left, r.Bottom));
                }
            }
            var vb = new VisualBrush(dv) { TileMode = TileMode.None, ViewboxUnits = BrushMappingMode.Absolute, Viewbox = new Rect(0,0,w*cell,h*cell), Stretch = Stretch.None };
            Board.Width = w*cell; Board.Height = h*cell;
            Board.Background = vb;
        }

        // Simple serializable map file
        private record MapFile(int Width, int Height, int[] Tiles);
    }
}
