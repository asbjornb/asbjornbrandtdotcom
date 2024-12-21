# Project Euler tactics

Some generic hints:

* Even if `HashSet<int>`/`Dict<int,int>` should in principle be fast the overhead is high enough that often it's much faster to use `int[]` or `bool[]`
  * For primes and checks rather than `HashSet<int>` with primes and `IsPrime(n) => set.Contains(n)` it's faster with `bool[]` and `IsPrime(n) => primes[n]`
  * For storing say divisors for some numbers it still might be faster to just use `int[]` over dictionary with `Divisors(n) => divisors[n]>-1?divisors[n]:CalculateDivisors(n)`
* Use memoization to short circuit expensive recursive calculations
* It's often cheaper to pre-generate all values than calculate individual.
  * Sieve primes rather than check individually
  * Find NumDivisors or SumOfDivisors across a range by iterating rather than for individual keys
