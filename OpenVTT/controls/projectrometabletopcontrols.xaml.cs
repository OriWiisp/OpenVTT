sing System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace OpenVTT.Controls
{
    public partial class Tabletop3DControl : UserControl
    {
        private Point _last;
        private double _yaw = 30, _pitch = 25, _radius = 35;

        public Tabletop3DControl()
        {
            InitializeComponent();
            BuildGroundGrid();
            UpdateCamera();
        }

        private void BuildGroundGrid()
        {
            // Create a tiled grid bitmap brush
            int cell = 64; int tiles = 8; int size = cell*tiles;
            var wb = new WriteableBitmap(size, size, 96, 96, PixelFormats.Pbgra32, null);
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(245,240,220)), null, new Rect(0,0,size,size));
                var pen = new Pen(new SolidColorBrush(Color.FromRgb(170,150,120)), 1);
                for (int i=0;i<=tiles;i++)
                {
                    double x = i*cell; dc.DrawLine(pen, new Point(x,0), new Point(x,size));
                    double y = i*cell; dc.DrawLine(pen, new Point(0,y), new Point(size,y));
                }
            }
            wb.Render(dv);
            var brush = new ImageBrush(wb) { TileMode = TileMode.Tile, ViewportUnits = BrushMappingMode.Absolute, Viewport = new Rect(0,0,cell,cell) };

            var mesh = new MeshGeometry3D();
            double extent = cell*tiles/2.0; // centered plane
            mesh.Positions = new Point3DCollection(new []{
                new Point3D(-extent,0,-extent), new Point3D(extent,0,-extent), new Point3D(extent,0,extent), new Point3D(-extent,0,extent)
            });
            mesh.TriangleIndices = new Int32Collection(new []{0,1,2, 0,2,3});
            mesh.TextureCoordinates = new PointCollection(new []{
                new System.Windows.Point(0,0), new System.Windows.Point(tiles,0), new System.Windows.Point(tiles,tiles), new System.Windows.Point(0,tiles)
            });

            var material = new DiffuseMaterial(brush);
            var model = new GeometryModel3D(mesh, material);
            model.BackMaterial = material;
            Ground.Content = model;
        }

        private void UpdateCamera()
        {
            double radYaw = _yaw * Math.PI/180.0; double radPitch = _pitch * Math.PI/180.0;
            double x = _radius * Math.Cos(radPitch) * Math.Sin(radYaw);
            double y = _radius * Math.Sin(radPitch);
            double z = _radius * Math.Cos(radPitch) * Math.Cos(radYaw);
            Cam.Position = new Point3D(x, y, z);
            Cam.LookDirection = new Vector3D(-x, -y, -z);
        }

        private void View_MouseDown(object sender, MouseButtonEventArgs e) { _last = e.GetPosition(this); CaptureMouse(); }
        private void View_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsMouseCaptured) return;
            var p = e.GetPosition(this);
            var dx = p.X - _last.X; var dy = p.Y - _last.Y;
            _yaw += dx*0.2; _pitch = Math.Clamp(_pitch - dy*0.2, 5, 80);
            _last = p; UpdateCamera();
        }
        private void View_MouseWheel(object sender, MouseWheelEventArgs e)
        { _radius = Math.Clamp(_radius - e.Delta*0.01, 8, 120); UpdateCamera(); }

        private void Zoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        { _radius = e.NewValue; UpdateCamera(); }

        private void ResetCam_Click(object sender, RoutedEventArgs e)
        { _yaw = 30; _pitch = 25; _radius = 35; UpdateCamera(); }

        private void AddBoxToken_Click(object sender, RoutedEventArgs e)
        {
            var mesh = MeshBox(2,2,2);
            var mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(122,27,29)));
            var model = new GeometryModel3D(mesh, mat);
            model.Transform = new TranslateTransform3D(0,1,0);
            Tokens.Children.Add(new ModelVisual3D { Content = model });
        }

        private MeshGeometry3D MeshBox(double sx, double sy, double sz)
        {
            double x = sx/2, y = sy/2, z = sz/2;
            var p = new Point3DCollection{
                new(-x,-y,-z), new(x,-y,-z), new(x,y,-z), new(-x,y,-z),
                new(-x,-y,z),  new(x,-y,z),  new(x,y,z),  new(-x,y,z)
            };
            int[] idx = { 0,1,2, 0,2,3,  1,5,6, 1,6,2,  5,4,7, 5,7,6,  4,0,3, 4,3,7,  3,2,6, 3,6,7,  4,5,1, 4,1,0 };
            return new MeshGeometry3D { Positions = p, TriangleIndices = new Int32Collection(idx) };
        }
    }
}
