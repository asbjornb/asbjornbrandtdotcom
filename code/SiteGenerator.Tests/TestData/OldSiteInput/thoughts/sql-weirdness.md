# Sql weirdness

## Can use space with zero width and other gimmicks for unreadable code

``` Sql
DECLARE​@​int=5;
WITH​a​AS(SELECT​x​FROM(VALUES(1),(1),(1),(1),(1))x(x))
SELECT​TOP(@+1)IIF(r-@=1,SPACE(@)+'*',SPACE(@-r+1)+REPLICATE('*',r*2-1))
FROM(SELECT​r=ROW_NUMBER()OVER(ORDER​BY​1/0)FROM​a,a​b,a​c,a​d)b
```

* Variable named @ (temp tables can be named # too)
* ORDERBY has 0-width space [https://en.wikipedia.org/wiki/Zero-width_space]
* Other stuff is weirdly working even if SSMS indicates warnings.

## Zalgo script is also allowed

[https://en.wikipedia.org/wiki/Zalgo_text]

![For select and comments](Assets/ZalgoSelect.png)

![Even for sysnames](Assets/ZalgoSql.png)

Also see this link for postgres weirdness which might largely be replicateable in sql:
[Select wat from sql](https://scattered-thoughts.net/writing/select-wat-from-sql/)]
