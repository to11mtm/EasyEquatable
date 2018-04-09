using System;
namespace EasyEquatable
{
    public sealed class EquatableItemAttribute : Attribute
    {
        public StringComparison StringComparison { get; set; }
    }
}
