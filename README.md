# automated_permutations.cs

C# snippet showing a proof of concept on generating automated permutations of a class using a customizable attribute. Each property of the class can be assigned an subclass of the attribute representing the range of values to be permutated. This can be used to quickly generate a range of permutations to be used with automated testing.

## usage
```c#
public class Foo {
    public enum Enum { a, b, c };
    [permutation_attribute_range(start = 1, count = 1)] public int a;
    [permutation_attribute_range(start = 1, count = 3)] public int b;
    [permutation_attribute_range(start = 1, count = 5)] public int c;
    [permutation_attribute_enum(type = typeof(Enum))]   public Enum e;
}

static void Main () {
    foreach (Foo permutation in get_permutations<Foo>()) {
        run_automated_test(permutation);
    }
}
```
