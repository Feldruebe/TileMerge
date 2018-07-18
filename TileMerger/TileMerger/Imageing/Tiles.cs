using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TileMerger.Imageing
{
    public class Tiles
    {
        public Tiles(Image<Rgba32> topImage, Image<Rgba32> leftImage, Image<Rgba32> bottomImage, Image<Rgba32> rightImage)
        {
            this.TopImage = topImage;
            this.LeftImage = leftImage;
            this.BottomImage = bottomImage;
            this.RightImage = rightImage;
        }

        public Image<Rgba32> TopImage { get; }

        public Image<Rgba32> LeftImage { get; }

        public Image<Rgba32> BottomImage { get; }

        public int HorizontalSeperationIndex { get; set; }

        public int MergeWidth { get; set; }

        public int HorizontalSeperationStartIndex => this.HorizontalSeperationIndex - this.MergeWidth;

        public int HorizontalSeperationEndIndex => this.HorizontalSeperationIndex + this.MergeWidth;


        public int VerticalSeperationIndex { get; set; }

        public int VerticalMergeWidth { get; set; }

        public int VerticalSeperationStartIndex => this.VerticalSeperationIndex - this.VerticalMergeWidth;

        public int VerticalSeperationEndIndex => this.VerticalSeperationIndex + this.VerticalMergeWidth;

        public int ImagesWidth => this.TopImage?.Width ?? this.LeftImage?.Width ?? this.BottomImage?.Width ?? this.RightImage?.Width ?? -1;
        public int ImagesHeight => this.TopImage?.Height ?? this.LeftImage?.Height ?? this.BottomImage?.Height ?? this.RightImage?.Height ?? -1;

        public bool CheckSize()
        {
            var widthOk = (this.TopImage?.Width ?? ImagesWidth) == ImagesWidth &&
                          (this.LeftImage?.Width ?? ImagesWidth) == ImagesWidth &&
                          (this.BottomImage?.Width ?? ImagesWidth) == ImagesWidth &&
                          (this.RightImage?.Width ?? ImagesWidth) == ImagesWidth;

            var heightOk = (this.TopImage?.Height ?? ImagesHeight) == ImagesHeight &&
                           (this.LeftImage?.Height ?? ImagesHeight) == ImagesHeight &&
                           (this.BottomImage?.Height ?? ImagesHeight) == ImagesHeight &&
                           (this.RightImage?.Height ?? ImagesHeight) == ImagesHeight;

            return widthOk && heightOk;
        }

        public Image<Rgba32> RightImage { get; }
    }
}