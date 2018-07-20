namespace TileMerger
{
    using System.IO;
    using System.Numerics;
    using System.Windows;
    using System.Windows.Input;
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

        private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
