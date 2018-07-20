using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using TileMerger.Imageing;

namespace TileMerger.ViewModel
{
    using System.Net.Cache;
    using System.Reactive;
    using System.Reactive.Linq;

    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private BitmapImage topTileImage;
        private BitmapImage leftTileImage;
        private BitmapImage bottomTileImage;
        private BitmapImage rightTileImage;
        private BitmapImage mergedTileImage;


        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Task mergeTask;

        private int mergeWidth = 10;
        private int horizontalSeperationIndex = 25;
        private int verticalMergeWidth = 10;
        private int verticalSeperationIndex = 25;

        private int referenzWidth;
        private int halfReferenzWidth;

        private int referenzHeight;
        private int halfReferenzHeight;

        private IObservable<EventPattern<object>> mergeThrottle;

        private event EventHandler OnMergeTriggered;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            this.mergeThrottle = Observable.FromEventPattern(ev => this.OnMergeTriggered += ev, ev => this.OnMergeTriggered -= ev).Throttle(TimeSpan.FromMilliseconds(250));
            this.mergeThrottle.Subscribe(pattern => this.MergeTiles());
        }

        public BitmapImage TopTileImage { get => this.topTileImage; set => this.Set(ref this.topTileImage, value); }

        public BitmapImage LeftTileImage { get => this.leftTileImage; set => this.Set(ref this.leftTileImage, value); }

        public BitmapImage BottomTileImage { get => this.bottomTileImage; set => this.Set(ref this.bottomTileImage, value); }

        public BitmapImage RightTileImage { get => this.rightTileImage; set => this.Set(ref this.rightTileImage, value); }

        public BitmapImage MergedTileImage { get => this.mergedTileImage; set => this.Set(ref this.mergedTileImage, value); }

        public int ReferenzWidth
        {
            get => this.referenzWidth;
            set
            {
                this.Set(ref this.referenzWidth, value);
                this.HalfReferenzWidth = value / 2;
            }
        }

        public int HalfReferenzWidth
        {
            get => this.halfReferenzWidth;
            set => this.Set(ref this.halfReferenzWidth, value);
        }

        public int ReferenzHeight
        {
            get => this.referenzHeight;
            set
            {
                this.Set(ref this.referenzHeight, value);
                this.HalfReferenzHeight = value / 2;
            }
        }

        public int HalfReferenzHeight
        {
            get => this.halfReferenzHeight;
            set => this.Set(ref this.halfReferenzHeight, value);
        }

        public int MergeWidth
        {
            get => this.mergeWidth;
            set
            {
                this.Set(ref this.mergeWidth, value);
                this.RaisOnMergeTriggered();
            }
        }

        public int HorizontalSeperationIndex
        {
            get => this.horizontalSeperationIndex;
            set
            {
                this.Set(ref this.horizontalSeperationIndex, value);
                this.RaisOnMergeTriggered();
            }
        }

        public int VerticalMergeWidth
        {
            get => this.verticalMergeWidth;
            set
            {
                this.Set(ref this.verticalMergeWidth, value);
                this.RaisOnMergeTriggered();
            }
        }

        public int VerticalSeperationIndex
        {
            get => this.verticalSeperationIndex;
            set
            {
                this.Set(ref this.verticalSeperationIndex, value);
                this.RaisOnMergeTriggered();
            }
        }

        public RelayCommand<TileType> LoadTileCommand => new RelayCommand<TileType>(this.LoadTile);

        public RelayCommand<TileType> DeleteImageCommand => new RelayCommand<TileType>(this.DeleteTile);

        public RelayCommand MergeTilesCommand => new RelayCommand(this.MergeTiles);

        private void LoadTile(TileType tileType)
        {
            switch (tileType)
            {
                case TileType.Top:
                    this.TopTileImage = this.OpenImage() ?? this.TopTileImage;
                    break;

                case TileType.Left:
                    this.LeftTileImage = this.OpenImage() ?? this.LeftTileImage;
                    break;

                case TileType.Bottom:
                    this.BottomTileImage = this.OpenImage() ?? this.BottomTileImage;
                    break;

                case TileType.Right:
                    this.RightTileImage = this.OpenImage() ?? this.RightTileImage;
                    break;
                default:
                    break;
            }

            this.RaisOnMergeTriggered();
        }

        private void DeleteTile(TileType tileType)
        {
            switch (tileType)
            {
                case TileType.Top:
                    this.TopTileImage = null;
                    break;

                case TileType.Left:
                    this.LeftTileImage = null;
                    break;

                case TileType.Bottom:
                    this.BottomTileImage = null;
                    break;

                case TileType.Right:
                    this.RightTileImage = null;
                    break;
                default:
                    break;
            }

            this.RaisOnMergeTriggered();
        }

        private BitmapImage OpenImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            var result = (bool)openFileDialog.ShowDialog();
            if (result)
            {
                var image = new BitmapImage(new Uri(openFileDialog.FileName));

                if (this.TopTileImage == null && this.LeftTileImage == null && this.BottomTileImage == null && this.RightTileImage == null)
                {
                    this.ReferenzWidth = image.PixelWidth;
                    this.ReferenzHeight = image.PixelHeight;
                    return image;
                }

                if (this.ReferenzWidth == image.PixelWidth && this.ReferenzHeight == image.PixelHeight)
                {
                    return image;
                }

                MessageBox.Show("The size of the images have to be the same!");
            }

            return null;
        }

        private void MergeTiles()
        {
            if (!this.mergeTask?.IsCompleted ?? false)
            {
                this.cancellationTokenSource.Cancel();
            }

            this.cancellationTokenSource = new CancellationTokenSource();
            this.mergeTask = Task.Run(
                () => this.MergeTilesAsync(this.cancellationTokenSource.Token),
                this.cancellationTokenSource.Token);
        }

        private async Task MergeTilesAsync(CancellationToken token)
        {
            Image<Rgba32> top = this.TopTileImage != null ? Image.Load<Rgba32>(this.GetPngFromImageControl(this.TopTileImage)) : null;
            Image<Rgba32> left = this.LeftTileImage != null ? Image.Load<Rgba32>(this.GetPngFromImageControl(this.LeftTileImage)) : null;
            Image<Rgba32> bottom = this.BottomTileImage != null ? Image.Load<Rgba32>(this.GetPngFromImageControl(this.BottomTileImage)) : null;
            Image<Rgba32> right = this.RightTileImage != null ? Image.Load<Rgba32>(this.GetPngFromImageControl(this.RightTileImage)) : null;

            var tiles = new Tiles(top, left, bottom, right)
            {
                MergeWidth = this.MergeWidth,
                HorizontalSeperationIndex = this.HorizontalSeperationIndex,
                VerticalMergeWidth = this.VerticalMergeWidth,
                VerticalSeperationIndex = this.VerticalSeperationIndex
            };
            var mergeTile = await TileMerge.MergeTiles(tiles, token);

            token.ThrowIfCancellationRequested();
            if (mergeTile != null)
            {
                byte[] bytes = null;
                using (MemoryStream stream = new MemoryStream())
                {
                    mergeTile.SaveAsPng(stream);
                    bytes = stream.ToArray();
                }

                token.ThrowIfCancellationRequested();
                this.DispatchToGui(() => this.MergedTileImage = this.BytesToImage(bytes));
            }
            else
            {
                token.ThrowIfCancellationRequested();
                this.DispatchToGui(() => this.MergedTileImage = null);
            }
        }

        public byte[] GetPngFromImageControl(BitmapImage imageC)
        {
            using (var memStream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(imageC));
                encoder.Save(memStream);
                byte[] bytes = memStream.ToArray();
                return bytes;
            }
        }

        private BitmapImage BytesToImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;

            var bitmap = new BitmapImage();
            using (MemoryStream stream = new MemoryStream(imageData))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                return bitmap;
            }
        }

        private void DispatchToGui(Action action)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                action();
            });
        }

        protected virtual void RaisOnMergeTriggered()
        {
            this.OnMergeTriggered?.Invoke(null, null);
        }
    }
}