# Petapoco tips

* Connection pooling makes DatabaseProvider.GetDatabaseConnection fine.
* Best practice is to create one connection for each query and dispose again. See <https://softwareengineering.stackexchange.com/questions/142065/creating-database-connections-do-it-once-or-for-each-query>
* Query vs. fetch: In petapoco they fire the same sql, but fetch returns a list and query uses yield return: <https://stackoverflow.com/questions/24059871/what-is-the-difference-between-fetch-and-query>
* Async queries (database, not petapoco): <https://stackoverflow.com/questions/63165599/using-async-await-for-database-queries>
