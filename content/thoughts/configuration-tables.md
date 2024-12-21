# Configuration Tables

Some ideas for setting up configuration tables for stuff like Ola Hallengren's Maintenance scripts or other scripts. Makes it easier to have the same maintenance and logging jobs across databases but with different settings.

>At my current gig, there are configuration tables on each SQL Server instance for the parameters that Ola’s stored procedures take. These get deployed as part of a database project.
>
>A row for each DB gets inserted into the config tables by an Agent job for with some sane >default values.
>
>Then Agent jobs execute wrapper stored procedures that read the config tables and pass the parameters to Ola’s index defrag and backup etc stored procedures.
>
>In this way we can customize the index defrag and backup behavior for each DB as needed by updating the config tables without ever having to edit any Agent jobs (except in the case of changing the job schedule from the defaults) or modifying Ola’s scripts.

From comments here [How to Configure Ola Hallengren’s Maintenance Scripts](
https://www.brentozar.com/archive/2014/12/tweaking-defaults-ola-hallengrens-maintenance-scripts/)
