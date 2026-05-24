namespace TriangleDemo.Models
{
    public static class TriangleFactory
    {
        public static TriangleData? TryCreate(double a, double b, double c, string colorHex)
        {
            if (a >= b + c || b >= a + c || c >= a + b)
                return null;

            return new TriangleData(a, b, c, colorHex);
        }
    }
}
