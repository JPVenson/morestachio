using Morestachio.Formatter.Framework;

namespace Morestachio.Tests
{
    public static class NumberFormatter
    {
        [MorestachioFormatter("Multiply", "XXX")]
        public static decimal Multiply(object value, object value2)
        {
            decimal a = 0;
            decimal.TryParse(value.ToString(), out a);
            decimal b = 0;
            decimal.TryParse(value2.ToString(), out b);

            return a * b;
        }
    }
}