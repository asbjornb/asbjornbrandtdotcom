# Exists vs in

Classic blog post by Gail explaining:
<https://sqlinthewild.co.za/index.php/2009/08/17/exists-vs-in/>
IN / EXISTS are comparable in plans and runtime.
The blog further links to comparisons for IN vs. INNER JOIN, NOT EXISTS vs. LEFT JOIN, NOT EXISTS vs. NOT IN.
Summary - performance is not too different so use what semantically makes the most sense. WHERE ... IN ... makes more sense if that is what you're checking.
