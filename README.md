# asbjornbrandtdotcom

The home-rolled static site generator behind [asbjornbrandt.com](https://asbjornbrandt.com), a digital garden of
notes, posts and pages written in Markdown.

The site's content lives in a separate private repo. Its CI checks out this repo, runs the generator against the
content and deploys the output to Cloudflare. This repo holds the engine, the HTML templates and a small sample site.

## Features

- **Markdown** to HTML with [Markdig](https://github.com/xoofx/markdig), including pipe tables
- **Templates** with [Handlebars.NET](https://github.com/Handlebars-Net/Handlebars.Net)
- **Wikilinks**: `[[note-name]]` links between notes, with backlinks generated on each note
- **Selective publishing**: `published.txt` lists the notes that go on the site. Links to unpublished notes render
  as plain text, and the generator warns about them
- **Posts** dated by filename, with an index page and an RSS feed
- **Site files**: `sitemap.xml`, a 404 page, and anything in `static/` copied to the output root (e.g. `_redirects`)

## Content layout

```
pages/          Site pages, rendered at the root (index.md, now.md, 404.md, ...)
posts/          Posts, named yyyy-MM-dd-slug.md, rendered under /posts/
thoughts/       Notes (the garden), rendered under /notes/
assets/         CSS, images and other files, copied to /assets/
static/         Files copied as-is to the output root
published.txt   Notes to publish, one path or slug per line. Missing file = publish everything
```

[`sample-content/`](sample-content) is a minimal example.

## Running it

Requires the .NET 10 SDK. [Task](https://taskfile.dev) wraps the common commands:

```sh
task test       # build and run the tests
task generate   # build sample-content/ into site-output/
```

To build other content, pass the paths on the command line (absolute, or relative to the repo root):

```sh
cd code/SiteGenerator
dotnet run -- --Paths:ContentDirectory=/path/to/content --Paths:OutputDirectory=/path/to/output
```

Site metadata (title, base URL, author) is in `code/SiteGenerator/appsettings.json`.

## Project structure

```
code/
  SiteGenerator/         The generator
    Processors/          Pages, notes and posts
    BacklinksProcessing/ Wikilinks and backlinks
    KnowledgeGraph/      Note graph data
    Templates/           Template rendering
  SiteGenerator.Tests/   xUnit tests and test fixtures
  templates/             Handlebars templates
sample-content/          Example content
```
