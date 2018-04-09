using EasyEquatable;
using System;
namespace Sample.EasyEquatable
{

    public class Sample : IEasyEquatable<Sample>
    {
        //The StringComparison only means anything for String. It won't impact other types
        [Comparable(StringComparison = StringComparison.InvariantCultureIgnoreCase)]
        public String Item1 { get; set; }
        //Default Behavior will be StringComparison.CurrentCulture
        [Comparable]
        public String Item2 { get; set; }
        [Comparable]
        public double Item3 { get; set; }
        public bool Equals(Sample other)
        {
            
            return this.CompareEasyEquatable(other);
        }
    }
}
