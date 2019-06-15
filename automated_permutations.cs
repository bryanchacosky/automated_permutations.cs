using System.Reflection;

class automated_permutations {
    // abstract base attribute assigned to properties to be permutated
    public abstract class permutation_attribute : Attribute {
        public abstract IEnumerable get_range ();
    }

    // defines a permutation attribute for an int range
    public class permutation_attribute_range : permutation_attribute {
        public int start { get; set; }
        public int count { get; set; }
        public override IEnumerable get_range () {
            return Enumerable.Range(this.start, this.count);
        }
    }

    // defines a permutation attribute for an enum range
    public class permutation_attribute_enum : permutation_attribute {
        public Type type { get; set; }
        public override IEnumerable get_range () {
            return Enum.GetValues(this.type);
        }
    }

    // returns an enumerable of all permutations of the class type
    public static IEnumerable<T> get_permutations<T> () where T : new() {
        return get_permutations<T>(Enumerable.Empty<permutation_property>(),
            typeof(T).GetFields()
                .Select((property) => {
                        object[] attributes = property.GetCustomAttributes(typeof(permutation_attribute), true);
                        permutation_attribute attribute = attributes.FirstOrDefault() as permutation_attribute;
                        return new { property = property, attribute = attribute };
                    })
                .Where ((it) => it.attribute != null)
                .Select((it) => new permutation_range {
                        property    = it.property,
                        range       = it.attribute.get_range()
                    }));
    }

    // helper class representing a single permutation property
    private class permutation_property {
        public FieldInfo property;
        public object value;
    }

    // helper class representing a permutation range
    private class permutation_range {
        public FieldInfo property;
        public IEnumerable range;
    }

    private static IEnumerable<T> get_permutations<T> (
                IEnumerable<permutation_property>   properties,
                IEnumerable<permutation_range>      ranges)
            where T : new() {
        permutation_range current_range = ranges.FirstOrDefault();
        
        if (current_range == null) {
            // no more ranges to iterate, create permutation
            T instance = new T();
            foreach (permutation_property property in properties) {
                property.property.SetValue(instance, property.value);
            }
            yield return instance;
        } else {
            // iterate into the current property ranges, recurse into permutations
            foreach (object current_value in current_range.range) {
                foreach (T current_result in get_permutations<T>(properties.Concat(
                            new permutation_property {
                                    property = current_range.property,
                                    value = current_value,
                                }.ToEnumerable()),
                        ranges.Skip(1))) {
                    yield return current_result;
                }
            }
        }
    }

    public class Foo {
        public enum Enum { a, b, c };
        
        [permutation_attribute_range(start = 1, count = 1)] public int a;
        [permutation_attribute_range(start = 1, count = 3)] public int b;
        [permutation_attribute_range(start = 1, count = 5)] public int c;
        [permutation_attribute_enum(type = typeof(Enum))]   public Enum e;
        
        public override string ToString () {
            return string.Format(
                "Foo [a={0}; b={1}; c={2}; e={3}]",
                this.a, this.b, this.c, this.e);
        }
    }
    
    static void Main () {
        IEnumerable<Foo> permutations = get_permutations<Foo>();
        Console.WriteLine(string.Join("\n", permutations.Select((p) => p.ToString())
            .ToArray()));
    }
}

public static class LinqExtensions {
    // returns an IEnumerable<T> containing a single object
    public static IEnumerable<T> ToEnumerable<T> (this T instance) {
        yield return instance;
    }
}
