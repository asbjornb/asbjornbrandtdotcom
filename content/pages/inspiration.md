# Inspiration for this site

I've long been fascinated with zettelkasten, personal wikis, gtd and similar but never committed. I initially experimented with [Foam](https://github.com/foambubble/foam) to try it out and collected some simple notes, though I've since moved to a custom approach. Reading more blogposts this one really resonated with me: [Building a digital Garden](https://tomcritchlow.com/2019/02/17/building-digital-garden/) - give it a read and see if you agree.

## A digital garden

Quoting [Maggie Appleton](https://github.com/MaggieAppleton/digital-gardeners):
>A garden is something in between a personal blog and a wiki. It's a collection of evolving notes, essays, and ideas that aren't strictly organised by their publication date. They're inherently exploratory – posts are linked through contextual associations. They aren't refined or complete - posts can be published as half-finished thoughts that will grow and evolve over time. They're less rigid, less performative, and less perfect than the personal "blogs" we're used to encountering on the web.

## Gardens and sites I like

* [notes.busterbenson.com](https://notes.busterbenson.com/) - this entire site is just perfect. Very noteworthy is the [codex](https://notes.busterbenson.com/codex)
* [Andy Matuschak's notes](https://notes.andymatuschak.org/About_these_notes)
* [Maggie Appleton](https://maggieappleton.com/) and her list of [resources for digital gardeners](https://github.com/MaggieAppleton/digital-gardeners)
* [Brendan Schlagel's canon](https://www.brendanschlagel.com/canon/)
* [capwatkins.com](https://capwatkins.com/) - neat simple design
* The [now page](https://nownownow.com/about) idea, which gave me my own [now](/now/) page

## The technical stuff

As many others in the note-taking/static-site/mind-garden space I didn't want to lock myself into some proprietary software. I've built a custom static site generator in C#, but all my notes and website content exists only as markdown files. This means I can take my content elsewhere without much ado if I want to change tools again. More on the setup in [static sites](/notes/static-sites/).

Notes link to each other with `[[wikilinks]]` and each note shows what links back to it. My notes start out private, and I publish them one by one when they are ready enough to share - which for a garden isn't saying much.

## Longer term

There's an early [note graph](/graph/) showing how notes connect, but it needs some love. I'd also love to set up commenting for some parts of this site, but haven't yet settled on a great way to do just that. For now [my github](https://github.com/asbjornb/asbjornbrandtdotcom) is the best interaction point. As is apparent, my design sense is also certainly unrefined (well just crap).
