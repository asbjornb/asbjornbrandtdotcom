# Interface Uses

## One-to-one misuse

Often in .net one-to-one interfaces are introduced for every class. Arguments are typically:

* They ease tests/mocking
* They decouple
* To assist dependency injection

For tests/mocking just use the original class unless it has dependencies like SQL, HTTP etc. If so create an interface to be able to mock it.
For decoupling - if interface and class are the same and there is no abstraction the coupling is just moved to the interface instead.
For dependency injection just inject the concrete class

## Guidelines (just my own dynamic list)

* Anything that is shared in api's or platform packages should probably use interfaces to allow changes easier without breaking dependents you don't know about
* In general only create interfaces that actually abstract or where multiple implementations exist
* It's easy to switch to an interface down the line if more implementations are needed by:
  * Rename the class to the new interface name with the built in refactoring tool
  * Rename the class back (or to a new name since you've learned that there are two implementations and how they differ) without using the refactoring tool
  * Create the interface - its name is used everywhere

## See also

* Good discussion in this reddit thread: <https://www.reddit.com/r/dotnet/comments/mzxlrc/comment/gw3jo2t/?utm_source=share&utm_medium=web2x&context=3>
* Short and to the point blog post <https://blog.ploeh.dk/2010/12/02/Interfacesarenotabstractions/>
