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

        public bool CheckSize()
        {
            int referenceWidth = this.TopImage?.Width ?? this.LeftImage?.Width ?? this.BottomImage?.Width ?? this.RightImage?.Width ?? -1;
            int referenceHeight = this.TopImage?.Height ?? this.LeftImage?.Height ?? this.BottomImage?.Height ?? this.RightImage?.Height ?? -1;

            var widthOk = (this.TopImage?.Width ?? referenceWidth) == referenceWidth &&
                          (this.LeftImage?.Width ?? referenceWidth) == referenceWidth &&
                          (this.BottomImage?.Width ?? referenceWidth) == referenceWidth &&
                          (this.RightImage?.Width ?? referenceWidth) == referenceWidth;

            var heightOk = (this.TopImage?.Height ?? referenceWidth) == referenceHeight &&
                           (this.LeftImage?.Height ?? referenceWidth) == referenceHeight &&
                           (this.BottomImage?.Height ?? referenceWidth) == referenceHeight &&
                           (this.RightImage?.Height ?? referenceWidth) == referenceHeight;

            return widthOk && heightOk;
        }

        public Image<Rgba32> RightImage { get; }
    }
}