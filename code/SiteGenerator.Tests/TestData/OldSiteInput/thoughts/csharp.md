# C\#

## Design rules of thumb (these actually go for any object oriented language - maybe should be moved to design patterns or similar?)

* Single responsibility principle
  * If your class does two different things, split it
  * **If you have to scroll to see your whole class, split it**
  * See <https://www.youtube.com/watch?v=5RwhyZnVRS8>
* Open Closed principle
  * Modify existing code without changing it.
  * Open for extension, closed for changes
  * Generally hide everything behind interfaces
  * See <https://www.youtube.com/watch?v=VFlk43QGEgc>
* Liskov substitution principle
  * Any place a class can be used any subclass should be usable in it's place without throwing exceptions the class wouldn't have.
  * See <https://www.youtube.com/watch?v=-3UXq2krhyw>
* Interface segregation principle
  * Instead of using a large interface and implementing methods that are not meaningful split into smaller interfaces and implement the union.
* Dependency inversion principle
  * Any class depending on other classes should do so through interfaces.
  * Dependencies should be supplied in constructor and managed with dependency injection?
  * **If you ever see a "new" in your code then you have tight coupling**

Spells out SOLID
See also: <https://www.youtube.com/watch?v=-9b8NRqjUFM> top 10 c# best practices.

Practical programmer has some simpler design principles:

* DRY - don't repeat yourself
* Easier to change
* Law of demeter for functions: An objects's method should call only methods from itself, from parameters passed in and from component objects.
* <https://blog.codinghorror.com/a-pragmatic-quick-reference/> pragmatic reference list.

## Clean code

<https://www.youtube.com/watch?v=7EmboKQH8lM>

* Inside a function every line should operate in the same level of abstraction that is exactly one level below the name.
* Take as long to clean the code as it took to write it (well take as long as it needs but...).
* Functions should be small. Smaller than that! The function should do one thing. A function does one thing if you cannot meaningfully extract another function from it!
* Large functions with variables are probably actually classes. You see this when extracting functions.
* Indent level of a function should not be greater than one or two (no nested structures in small functions).
* If statements are if(function()){function()}else{function()}
* Functions should at best have 0->max 3 arguments. If you pass more than 3 things that are cohesive they should probably be an object.
* Don't pass booleans into a function - just split function in two and call the correct one.
* Don't use output arguments (where you input something just to manipulate it for later reference).
* Don't use switch statements! Because you have to use them everywhere - not dry. Use polymorphism instead.
* Always close in same method you open so you don't get side effects.
* A function that returns void has side effect, a function that returns a value should never have side effects. It's a convention (query and command).
* A function with try catch finally should have nothing else. Just try(someErrorableFunction())catch. And no nested try blocks!
* Generally don't comment unless necessary or at least don't check it in! Worst is commented out code.
* Naming:
  * Variables - the smaller the scope the smaller the name. For a single line scope a single letter is ok. For a global variable use multiple words.
  * Functions - opposite. The larger the scope the more generic/abstract the function is so can be small like "Open". In smaller scope it's more specific like OpenFilestreamAndHandleErrors.
* **Don't ship shit!**
* Agile means it should be deployable at the end of every sprint. Even if sprint is one day. Some companies have 1 day sprints - unit tests in the morning then code to make them not fail.
* Tests allow cleaning the code so it will always get better instead of rotting.
* Do pair programming. It's more efficient than code then review. Just .5-1.5 hours 3 times a week.
* Pert(?) estimating: always 3 numbers. Best case, nominal case, worst case. Worst case(sick colleagues to cover for, production breakdowns to fix, personal illness). 1 week, 3 weeks, 8 weeks for example.
* **Test driven development**: One line of tests, then one line of code. Three laws:

        First Law:      You may not write production code until
                        you have written a *failing* unit test.
        
        Second Law:     You may not write more of a unit test than is
                        sufficient to fail, and not compiling is failing.
        
        Third Law:      You may not write more production code than
                        is sufficient to pass the current failing test.

  * See <https://www.youtube.com/watch?v=58jGpV2Cg50&t=2628s> for example test driven development.
* If statements and while loops should have 1 line.
* View models in MVC only contains strings? See <https://www.youtube.com/watch?v=sn0aFEMVTpA&t=5421s>
* Architecture is to defer decisions until they're necessary. Good architecture maximizes the number of decisions NOT made
* Plugin architecture is to point all dependencies in one direction. Defends f.e. business rules from gui changes if the UI is a plugin. Same - let the database be a plugin so it can be swap-able for another db or for flat files.

## Other notable watches

* Check Youtube for DevTernity 2017: Ian Cooper Tdd Where Did it all go wrong?
* Maybe check those videos: <https://martinfowler.com/articles/is-tdd-dead/>
* Only book to ever read on TDD: Test-Driven Development By Example by Kent Beck
* Check Martin Fowlers: Refactoring Improving the Design of existing code
* The trigger for writing a test:
  * Should NOT be adding a new method to a class (that binds the application structure in place)
  * Should be implementing a requirement!
* Should only test any surface area of the application (that are accessed from outside)
* Don't write tests for classes - classes are not behavior to be tested. Test the system. Otherwise it gets impossible to refactor without having to rewrite all tests.
* "Unit" in "unit test" is the test. The test should run in isolation from other tests.
* Red-green-refactor:
  * Red: Write a little test that doesn't work
  * Green: Make the test work quickly, committing whatever sins necessary in the process (even copy code from stackoverflow)
  * Refactor: Eliminate all of the duplication created when getting the test to work
* Refactoring more generally:
  * You can't refactor a public method on a public class since refactoring may not change external behavior.
* <https://nakabonne.dev/posts/depth-of-module/> Depth of module and referenced book seem to be a better approach to public api's.

## Mixed best practices

Stuff I read that resonated below.

* Avoid primitive obsession
  * e.g. string Currency is usually a smell - create a type for it possibly with a private constructor and a TryParse method to ensure validity.
* Avoid anemic domain models
  * Methods acting on outside objects might be a good giveaway. Put relevant code with the data itself. That is the essence of OO programming.
* Parse don't validate
* Use composition over inheritance. Difficult, but when it is difficult it is often a sign that the used abstractions are not right.
* Don't throw exceptions but return error-state.
  * See <https://www.joelonsoftware.com/2003/10/13/13/>
* Don't rewrite large codebases, just refactor one thing at a time.
  * See <https://www.joelonsoftware.com/2000/04/06/things-you-should-never-do-part-i/>
* Law of demeter

The overall guiding principle seems to be that code should be understandable/verifiable in the near context from naming, typing etc. So exceptions are bad because you lose ability to see whether this code actually ensures cleaning up or if an exception can skip the cleanup step:

````csharp
doSomething();
cleanup();
````

same goes for primitive obsession. Whenever you see a `string CurrencyCode` in code you will wonder whether it's empty, whether it's 3 characters etc. To know if it has been validated you need to F12 all the way back through countless methods. If you see a `CurrencyCode tradeCurrency` you can F12 the CurrencyCode type and see that it can't be created from empty strings. Same issue with extension methods.

## Effective Engineer by Edmond Lau

Leverage = impact / time. Maximize this by:

* Invest in education to produce better stuff per time spent
* Tools is probably the best investment - anything you can build to save manual steps or speed up automatic steps radically speeds up iterations. Even speeding up or removing future bugs help a lot.
* Spend 10% of time validating ideas before actually implementing them
* Always ask if a simpler version will do? Reduce code complexity, systems complexity, organizational code

## Analysers wanted

* Law of demeter
* Unused code
* Namespace doesn't match folder structure
