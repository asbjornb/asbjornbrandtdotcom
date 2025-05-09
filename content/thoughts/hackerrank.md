# Hackerrank

C# tips

* `System.Console.Error.WriteLine` works to write debug messages like System.Console.Error.WriteLine(System.Text.Json.JsonSerializer.Serialize(person));
* Formatting decimals is easiest with `string.Format("{0:0.00}", 256.583); // "256.58"`

Generic corner cases:

* Integer overflow (consider (Int64))
* 0, Int.MaxValue, Int.MinValue, 1, -1
