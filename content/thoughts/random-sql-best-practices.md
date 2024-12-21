# Random sql best practices

Ordinary:

* Use datetime2 datatype over datetime. More precision, lower footprint.
* Avoid varchar(max)/text/xml etc. when possible varchar(4000) is much better than max. varchar(64) better of course if it fits - or smaller even.
* Almost always use temp tables (`#Table`) instead of table variables (`@Table`). Cardinality estimation is wack with table variables even in newer versions.
* Take care of invalidating varchar indexes when calling sql from application code - if strings are supplied in unicode they translate to nvarchar and can't use the index on some collations.

Minor ones:

* Avoid merge due to issues
