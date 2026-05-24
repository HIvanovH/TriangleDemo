using System.Numerics;

namespace TriangleDemo.Rendering
{
    internal static class TriangleGeometry
    {
        public static float[] ComputeNdcVertices(double a, double b, double c)
        {
            var (vA, vB, vC) = PlaceVertices(a, b, c);

            var centroid = (vA + vB + vC) / 3f;
            vA -= centroid;
            vB -= centroid;
            vC -= centroid;

            float maxRadius = MathF.Max(vA.Length(), MathF.Max(vB.Length(), vC.Length()));
            float scale = 0.9f / maxRadius;

            return
            [
                vA.X * scale, vA.Y * scale, 0f,
                vB.X * scale, vB.Y * scale, 0f,
                vC.X * scale, vC.Y * scale, 0f,
            ];
        }

        private static (Vector2 a, Vector2 b, Vector2 c) PlaceVertices(double a, double b, double c)
        {
            double angleA = Math.Acos((b * b + c * c - a * a) / (2 * b * c));

            return (
                Vector2.Zero,
                new Vector2((float)c, 0f),
                new Vector2((float)(b * Math.Cos(angleA)), (float)(b * Math.Sin(angleA)))
            );
        }
    }
}
