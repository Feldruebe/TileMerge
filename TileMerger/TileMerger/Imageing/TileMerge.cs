using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TileMerger.Imageing
{
    public class TileMerge
    {
        public async static Task<Image<Rgba32>> MergeTiles(Tiles tiles, CancellationToken token)
        {
            Task<Image<Rgba32>> t = Task.Run(() =>
            {
                Image<Rgba32> resultImage = null;

                if(!tiles.CheckSize())
                {
                    return resultImage;
                }

                if (tiles.LeftImage != null && tiles.RightImage != null)
                {
                    // Two textures case.
                    var leftImage = tiles.LeftImage;
                    var rightImage = tiles.RightImage;

                    resultImage = new Image<Rgba32>(leftImage.Width, leftImage.Height);

                    for (int x = 0; x < leftImage.Width; x++)
                    {
                        for (int y = 0; y < leftImage.Height; y++)
                        {
                            token.ThrowIfCancellationRequested();
                            int leftOffset = 0;
                            int rightOffset = 30;
                            int influenceWidth = leftImage.Width - leftOffset - rightOffset;
                            float leftInfluence = 0;
                            if (x - leftOffset < 0)
                            {
                                leftInfluence = 1;
                            }
                            else
                            if (x > leftImage.Width - rightOffset)
                            {
                                leftInfluence = 0;
                            }
                            else
                            {
                                leftInfluence = 1f - (float)(x - leftOffset) / (leftImage.Width - leftOffset - rightOffset);
                            }

                            float rightInfluence = 1 - leftInfluence;

                            Rgba32 leftColor = leftImage[x, y];
                            Rgba32 rightColor = rightImage[x, y];
                            var resultColor = MultiplyColorWithScalar(leftColor, leftInfluence).ToVector4() + MultiplyColorWithScalar(rightColor, rightInfluence).ToVector4();
                            resultImage[x, y] = new Rgba32(resultColor);
                        }
                    }
                }

                return resultImage;
            });

            return await t;
        }

        private static Rgba32 MultiplyColorWithScalar(Rgba32 color, float influence, bool includeA = false)
        {
            var colorVector = Vector4.Multiply(influence, color.ToVector4());
            if (includeA)
            {
                colorVector.W = 255;
            }

            return new Rgba32(colorVector);
        }


    }
}
