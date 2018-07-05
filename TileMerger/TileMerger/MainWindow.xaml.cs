namespace TileMerger
{
    using System.IO;
    using System.Numerics;
    using System.Windows;
    using System.Windows.Media;

    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats.Png;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using SixLabors.ImageSharp.Processing.Drawing;
    using SixLabors.ImageSharp.Processing.Drawing.Pens;
    using SixLabors.ImageSharp.Processing.Transforms;
    using SixLabors.Primitives;
    using SixLabors.Shapes;

    using Size = SixLabors.Primitives.Size;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.IO.Directory.CreateDirectory("output");
            var bytes = File.ReadAllBytes("fb.png");
            using (var img = new Image<Rgba32>(100,100))
            {
                img[99, 99] = Rgba32.Black;
                img[98, 99] = Rgba32.Black;
                img[97, 99] = Rgba32.Black;
                img.Mutate(this.Draw);
                this.Save("mutated.png", img);
            }
        }

        private void Draw(IImageProcessingContext<Rgba32> context)
        {
            context.DrawLines(new GraphicsOptions(false), Pens.Solid(Rgba32.Black, 1f), new PointF(0, 0), new PointF(50, 70));
        }

        private void Save(string fileName, Image<Rgba32> image)
        {
            using (var stream = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                image.SaveAsPng(stream, new PngEncoder());
            }

        }
    }
}
