# Code that fits in your head

## Takeaways

Use checklists. For example for new projects:

* Create git repo
* Automate unit tests+deployment
* Enable warnings as errors for everything. Also nullability check.

Isolate interaction/surface such as database calls/http requests etc. in slim classes with no business logic.

Encapsulation. Not so much about accessibility but more about guaranteeing objects can't be in invalid states. Using immutables and static constructors/constructers throwing exceptions are ways to accomplish this. Also Take care about primitive obsession - currency is much better than string, quantity is much better than decimal.

Can't fit too much complexity in short term memory. To allow easy understanding of code try to keep to a rule of 7. No more than 7 cyclomatic complexity, no more than 7 variables used inside method (parameters, locals, class fields), 20-30 max lines methods. Leads to fractal design. Each level has max 7 things going on - and each 7 subcomponents again has at most 7 things going on. When decomposing good ways to spot what to split include cohesion (which methods use which fields), validating/parsing, feature envy.

Build any vertical slice first - end to end. Http request to insertion of data in db even without validation/complete model etc.

Reproduce defects as tests. Any bug that makes it to production should result in an added test.

Api design:

* Don't allow illegal states.
* Command/query separation
* Don't use comments when method names suffice, don't use naming when types suffice.
* Use types - avoid stringly api's.

## Appendix

See the appendix (List of practices.) for a shortlist of good practices.
