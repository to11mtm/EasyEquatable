using EasyEquatable;
using System;

namespace Sample.EasyEquatable
{
    public class Sample2 : IEasyEquatable<Sample2>
    {
        [Comparable(StringComparison = StringComparison.InvariantCultureIgnoreCase)]
        public Sample Item1 { get; set; }

        public bool Equals(Sample2 other)
        {

            return this.CompareEasyEquatable(other);
        }
    }
}
