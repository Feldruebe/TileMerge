namespace TileMerger.Imageing
{
    using System.Collections.Generic;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;

    using MathNet.Spatial.Euclidean;

    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;

    public class TileMerge
    {
        public async static Task<Image<Rgba32>> MergeTilesAsync(Tiles tiles, CancellationToken token = default(CancellationToken))
        {
            Task<Image<Rgba32>> t = Task.Run(() => MergeTiles(tiles, token), token);
            return await t;
        }

        public static Image<Rgba32> MergeTiles(Tiles tiles, CancellationToken token = default(CancellationToken))
        {
            if (!tiles.CheckSize())
            {
                return null;
            }

            var resultImage = new Image<Rgba32>(tiles.ImagesWidth, tiles.ImagesHeight);

            var mergeCenter = new Point2D(tiles.HorizontalSeperationIndex, tiles.VerticalSeperationIndex);
            var leftMergePoint = new Point2D(0, tiles.VerticalSeperationIndex);
            var rightMergePoint = new Point2D(tiles.ImagesWidth, tiles.VerticalSeperationIndex);
            var topMergePoint = new Point2D(tiles.HorizontalSeperationIndex, tiles.ImagesHeight);
            var bottomtMergePoint = new Point2D(tiles.HorizontalSeperationIndex, 0);
            var leftBottomPoint = new Point2D(0, 0);
            var leftTopPoint = new Point2D(0, tiles.ImagesHeight);
            var rightTopPoint = new Point2D(tiles.ImagesWidth, tiles.ImagesHeight);
            var rightBottomPoint = new Point2D(tiles.ImagesWidth, 0);

            var bottomLines = GetBorderLines(tiles.LeftImage != null, tiles.RightImage != null, tiles.TopImage != null, leftBottomPoint, leftMergePoint, leftTopPoint, mergeCenter, rightTopPoint, rightMergePoint, rightBottomPoint, out var bottomPolygon);
            var leftLines = GetBorderLines(tiles.TopImage != null, tiles.BottomImage != null, tiles.RightImage != null, leftTopPoint, topMergePoint, rightTopPoint, mergeCenter, rightBottomPoint, bottomtMergePoint, leftBottomPoint, out var leftPolygon);
            var topLines = GetBorderLines(tiles.RightImage != null, tiles.LeftImage != null, tiles.BottomImage != null, rightTopPoint, rightMergePoint, rightBottomPoint, mergeCenter, leftBottomPoint, leftMergePoint, leftTopPoint, out var topPolygon);
            var rightLines = GetBorderLines(tiles.BottomImage != null, tiles.TopImage != null, tiles.LeftImage != null, rightBottomPoint, bottomtMergePoint, leftBottomPoint, mergeCenter, leftTopPoint, topMergePoint, rightTopPoint, out var rightPolygon);


            var leftOrigin = new Vector2(0, tiles.VerticalSeperationIndex / 2f);
            var maxLeftDistance = Vector2.Subtract(leftOrigin, new Vector2(tiles.HorizontalSeperationEndIndex, tiles.VerticalSeperationIndex / 2f)).Length();

            for (int x = 0; x < tiles.ImagesWidth; x++)
            {
                for (int yIteration = 0; yIteration < tiles.ImagesHeight; yIteration++)
                {
                    var y = tiles.ImagesHeight - 1 - yIteration;
                    token.ThrowIfCancellationRequested();

                    var currentPoint = new Point2D(x, yIteration);

                    var leftNormalizedMergeInfluence = tiles.LeftImage != null ? NormalizedMergeInfluence(tiles, currentPoint, leftLines, leftPolygon) : 0;
                    var topNormalizedMergeInfluence = tiles.TopImage != null ? NormalizedMergeInfluence(tiles, currentPoint, topLines, topPolygon) : 0;
                    var rightNormalizedMergeInfluence = tiles.RightImage != null ? NormalizedMergeInfluence(tiles, currentPoint, rightLines, rightPolygon) : 0;
                    var bottomNormalizedMergeInfluence = tiles.BottomImage != null ? NormalizedMergeInfluence(tiles, currentPoint, bottomLines, bottomPolygon) : 0;

                    var influenceSum = leftNormalizedMergeInfluence + topNormalizedMergeInfluence + rightNormalizedMergeInfluence + bottomNormalizedMergeInfluence;

                    var overallLeftInfluence = leftNormalizedMergeInfluence / influenceSum;
                    var overallTopInfluence = topNormalizedMergeInfluence / influenceSum;
                    var overallRightInfluence = rightNormalizedMergeInfluence / influenceSum;
                    var overallBottomInfluence = bottomNormalizedMergeInfluence / influenceSum;

                    var leftColorVector = tiles.LeftImage?[x, y].ToVector4() ?? Vector4.Zero;
                    var topColorVector = tiles.TopImage?[x, y].ToVector4() ?? Vector4.Zero;
                    var rightColorVector = tiles.RightImage?[x, y].ToVector4() ?? Vector4.Zero;
                    var bottomColorVector = tiles.BottomImage?[x, y].ToVector4() ?? Vector4.Zero;

                    Vector4 colorVector = (leftColorVector * overallLeftInfluence) + (topColorVector * overallTopInfluence) + (rightColorVector * overallRightInfluence)
                                          + (bottomColorVector * overallBottomInfluence);

                    resultImage[x, y] = new Rgba32(colorVector);
                }
            }

            return resultImage;
        }

        private static float NormalizedMergeInfluence(Tiles tiles, Point2D currentPoint, PolyLine2D lines, Polygon2D polygon)
        {
            double mergeDistance;
            double minDistance = lines.ClosestPointTo(currentPoint).DistanceTo(currentPoint);
            if (polygon.EnclosesPoint(currentPoint))
            {
                mergeDistance = minDistance + tiles.MergeWidth;
            }
            else
            {
                mergeDistance = tiles.MergeWidth - minDistance;
                if (mergeDistance < 0)
                {
                    mergeDistance = 0;
                }
            }

            float normalizedMergeInfluence = (float)mergeDistance / (tiles.MergeWidth * 2f);
            return normalizedMergeInfluence;
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

        private static PolyLine2D GetBorderLines(bool hasLeft, bool hasRight, bool hasOpposite, Point2D leftBottomPoint, Point2D leftMergePoint, Point2D leftTopPoint, Point2D centerMergePoint, Point2D rightTopPoint, Point2D rightMergePoint, Point2D rightBottomPoint, out Polygon2D polygon)
        {
            var result = new List<Point2D>();
            var polygonResult = new List<Point2D>();

            if (hasLeft)
            {
                result.Add(leftBottomPoint);
                polygonResult.Add(leftBottomPoint);
            }
            else
            {
                polygonResult.Add(leftBottomPoint);
            }

            if (!hasLeft && hasOpposite)
            {
                result.Add(leftMergePoint);
                polygonResult.Add(leftMergePoint);
            }

            if (!hasLeft && !hasOpposite)
            {
                result.Add(leftTopPoint);
                polygonResult.Add(leftTopPoint);
            }

            result.Add(centerMergePoint);
            polygonResult.Add(centerMergePoint);

            if (!hasRight && !hasOpposite)
            {
                result.Add(rightTopPoint);
                polygonResult.Add(rightTopPoint);
            }

            if (!hasRight && hasOpposite)
            {
                result.Add(rightMergePoint);
                polygonResult.Add(rightMergePoint);
            }

            if (hasRight)
            {
                result.Add(rightBottomPoint);
                polygonResult.Add(rightBottomPoint);
            }
            else
            {
                polygonResult.Add(rightBottomPoint);
            }

            polygon = new Polygon2D(polygonResult);
            return new PolyLine2D(result);
        }
    }
}
