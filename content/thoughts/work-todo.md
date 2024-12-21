# Work Todo

## Implement

- [ ] ApexSqlCodeFormatter
- [ ] ApexSqlSnippetsAndRepository
- [ ] Quick SSAS cube & power bi demo
- [ ] db mail for jobs
- [ ] Implement delta on fact TradingPL
- [ ] Test sql prompt? Everyone in slack and SQLBI swears to it.
- [ ] Write small script that checks entire solution for classes not named the same as their files. Add other functionality, schedule it and help clean up code.
  - [ ] Ideas:
    - [ ] Analyse for methods that rename interface parameters
    - [ ] Analyse for circular references
    - [ ] Analyse for excessive references

## Research

### Direct use

- [ ] Options for jobs and other server based entities in db-projects. Check [[ssdt]]

### Fun/long term

- [ ] Polybase to read and write parquet - see [docs](https://docs.microsoft.com/en-us/sql/t-sql/statements/create-external-file-format-transact-sql?view=sql-server-ver15)
- [ ] Alternatively try synapse serverless
- [ ] Try clustered columnstore with partitions for ticker/date - performance?
- [ ] Testing TsqlT or other?
- [ ] Setup diagnostics book in Azure Data Studio
- [ ] Test python as alternative to SSIS for ETL. Test speed. Might be much more scalable running from another pc. See also [[etl]].
- [ ] Would it be possible to build a small c#/python script that reads through sp's and checks all insert/update/delete statements and the from clauses that lead there and use that to build a dependency graph? Hard since it has to take into account WITH, temp tables etc. Might be easier to just parse the execution plans that involve insert/update/deletes? Alternatively use sys.dm_sql_referenced_entities to better build dependency graphs.
- [ ] Setup configuration tables in each db for stuff like Ola's scripts: [[configuration-tables]]
- [ ] Link git commit history to individual database objects in doc database via git log parsing.
- [ ] Store job info and other similar in doc
- [ ] Build dependency table in doc instead of inside individual procedures via sys.dm_sql_referenced_entities
- [ ] Check out [machine learning in python](https://www.linkedin.com/learning/sql-server-machine-learning-services-python) - Looks cool
- [ ] Create a code statistics db and report. First do file length distribution in project (files have type, projects are in solutions?), then line length, function length. Check how it goes over time by using different git checkouts. Can then also see total line growth on projects.

## Business ideas

- [ ] Add budget data to Qlik PL-app?

## Hackathon

- Synchronize documentation between markdown at the code and the confluence wiki.
- Check out alternative data formats and dbms'es. FileFormats: Parquet (tried but no adoption), avro, orc, hdf5, Dbms: QuestDB, ClickHouse, InfluxDB, TimeScaleDB, Postgres
