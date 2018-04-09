EasyEquatable: Make Building Equatable types Easy

EasyEquatable is a library to make it easier to build Equatable types.

This came about by wishing to learn how to build an actual expression rather than just doing reflection tricks on them.



Usage:
```
using EasyEquatable;
using System;
namespace Sample.EasyEquatable
{

    public class Sample : IEasyEquatable<Sample>
    {
        //The StringComparison only means anything for String. It won't impact other types
        [EquatableItem(StringComparison = StringComparison.InvariantCultureIgnoreCase)]
        public String Item1 { get; set; }
        //Default Behavior will be StringComparison.CurrentCulture
        [EquatableItem]
        public String Item2 { get; set; }
        [EquatableItem]
        public double Item3 { get; set; }
        public bool Equals(Sample other)
        {
            
            return this.CompareEasyEquatable(other);
        }
    }
}
```

Features:

  - Reflection is used only to build Expression
  - Expression is Cached as Delegate for re-use
  - Handles Tagged (i.e. Nested) IEquatable items inside IEasyEquatable types.
  - Handles Tagged IEquatable items inside IEasyEquatable types.
  - Comparisons are Thread Safe
    

Stuff you can do (i.e. if you want to submit a PR):

  - Add other Comparison types.
    - Only what's contained in BCL

  - IEquatable is handled in a somewhat hacky way which leads to another level of indirection for the call.

  - Building is not 100% efficient under highly threaded scenarios. It's still thread safe, but can generate extra garbage.
    - Due to the possibility of nested objects, we handle as follows:
      - Check if we have a Delegate.
	  - If we have a delagate, keep going.
	  - If not, build the Expression, then Lock; Check if we have a delegate again, if not, compile and assign. This minimizes memory usage but can still lead to extra pressure on startup. In theory, anyway.

  - We are using .Compile() instead of .CompileToMethod(). This results in a slight performance penalty.
    - Compile with Flag USE_METHODBUILDER if you don't like this. I haven't tested it yet however.
