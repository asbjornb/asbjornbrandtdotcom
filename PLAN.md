# Site plan

Working plan for asbjornbrandt.com. Longer version with the full site review:
[Note capture & triage plan](https://claude.ai/code/artifact/dd48df15-96f5-457b-86d8-51b041cc6eb5).
Once the repo split is done, this file moves to the private `notes` repo.

Last updated 2026-10-17.

## Decisions

- **Split code and content.** This public repo keeps the generator (a public showcase). A new private
  `asbjornb/notes` repo holds all writing: inbox, notes, posts, drafts, and this plan. The notes repo's CI
  builds and deploys the site.
- **No front matter on notes.** `published.txt` in the notes repo lists the notes that go on the site, one
  path per line. Posts and pages always publish.
- **No CV on the site.** Recruiters arrive with the CV already. Add a LinkedIn link and a one-line
  "who I am" for peers arriving from posts or GitHub.
- **Notes: hide now, tend, promote back.** Start `published.txt` with only notes that read as finished.
- **Posts: fewer LinkedIn re-posts, more deep engineering posts** with real numbers from recent work.
- **Capture via Telegram, triage via the Claude Android app.** No home lab.

## Done

- [x] PR #23 (merged): hoarding post paragraphs + typo, `... copy` slug renamed with `_redirects` 301,
      generator copies `content/static/` to output root
- [ ] Check that `https://asbjornbrandt.com/posts/something-feels-different-now%20copy/` redirects

## 1. Homepage and LinkedIn (quick)

- [ ] One identity line: role, domain, stack, place
- [ ] Links to LinkedIn and GitHub (LinkedIn URL: TODO)
- [ ] Show the latest 3 posts on the homepage
- [ ] Typos: "life long learner" -> "lifelong", "Sql" -> "SQL"
- [ ] Refresh Now page (last updated 2025-12-19)

## 2. `published.txt` support in the generator

Done before the split, so notes can be hidden right away.

- [ ] Read `published.txt` from the content root; only render notes listed there
- [ ] If the file is missing, publish everything (keeps tests and old behavior working)
- [ ] `[[wikilinks]]` to unpublished notes render as plain text, not links
- [ ] Backlinks, notes index and knowledge graph only include published notes
- [ ] Generator warning listing links that point at unpublished notes (a to-do list for tending)
- [ ] Tests for all of the above
- [ ] First `published.txt`: only finished-looking notes. Leave out `inbox`, `work-todo`, `work-tools`,
      `work-reading`, and the thin notes listed under step 5
- [ ] Remove "Top of mind" / "Organizational" sections from the notes index (`thoughts/index.md`)

## 3. Repo split

- [ ] Create private `asbjornb/notes`; move `content/` there with history
      (`git filter-repo --subdirectory-filter content`)
- [ ] Move `PLAN.md` into the notes repo
- [ ] Generator: content and output paths from arguments
      (`dotnet run -- --Paths:ContentDirectory=<abs path>` already works via `AddCommandLine`; verify)
- [ ] Notes repo CI: check out this repo, build the generator, generate, deploy to Cloudflare Pages with
      `wrangler pages deploy` (needs `CLOUDFLARE_API_TOKEN`)
- [ ] Repoint the Cloudflare Pages project from "serve committed `site-output/`" to direct uploads
- [ ] This repo: remove `content/` and `site-output/`; keep test fixtures; README explains it is the engine
- [ ] Update homepage "github for this site" link to point at the generator repo
- [ ] Note: old content stays visible in this repo's git history. Fine for what's there today; rewrite history
      only if something sensitive turns up

## 4. Capture and triage

- [ ] Telegram capture bot (BotFather) -> Cloudflare Worker -> commits `inbox/<timestamp>-<slug>.md` to
      the notes repo (secret webhook path, allowlisted chat ID, fetches link titles, replies ✓)
- [ ] PC: bookmarklet + `note "..."` shell function POSTing to the same Worker
- [ ] `/triage` skill in the notes repo: per inbox file, propose append / new note / post seed / discard,
      suggest `[[wikilinks]]`, wait for OK, commit, delete processed files. Proposals only, no auto-publish
- [ ] Try triage from the Claude Android app's Code tab (cloud session)
- [ ] Optional: weekly GitHub Action (`claude-code-action` + `CLAUDE_CODE_OAUTH_TOKEN`) drafts a triage PR,
      Worker pings Telegram with the link

## 5. Notes: tend 20+ over a few days

Each note gets updated and added to `published.txt`, merged into another note, or left unpublished.

- Thinnest (<40 words; merge or drop): cheat-sheets, etl, project-euler, work-reading, mock, tech,
  sql-utility-scripts, podcasts, windows-terminal, readme, tsql-codesmells, fuses, namespaces, dbatools,
  ssms-commandline-usage, meme-templates, mtg-prices, markdown-to-confluence, media-notes,
  logitech-keyboard-mouse, sql-server, fastfood-hobro, chrome-shortcuts, fragmentation, hackerrank,
  row-version, taking-meeting-notes
- Biggest (refresh and keep): csharp, ai-assisted-coding, healthchecks-net, git, ssms-shortcuts, python,
  personal-blog, code-that-fits-in-your-head, being-more-convincing, nuget, docker
- [ ] Fix or remove dead links (~20: gottleben.com, headspringlabs bulk-writer, raindrop, sabin.io,
      red-gate docs, hardkernel, notes.busterbenson.com, thesqlnomad, ...)
- [ ] Decide Danish vs English (backup, career, recipes, Investments)

## 6. Posts: go through recent work

- [ ] Block an hour to list recent work: problems solved, migrations, incidents, performance wins, tools built
- [ ] Pick 2-3 with a story and numbers (before/after, cost, time, scale); check what's OK to share
- [ ] Draft in the notes repo; publish one per month or so
- [ ] Projects page: site generator, visuals.asbjornbrandt.com, anything showable from work

## 7. Site fixes

- [ ] Remaining LinkedIn-style posts merge single line breaks (on-jira, why-i-dont-want-to-manage,
      a-thought-on-pay, voluntary-code-reviews, good-intentions-tangled-systems): add blank lines, or enable
      Markdig `UseSoftlineBreakAsHardlineBreak` for posts only
- [ ] RSS feed, `sitemap.xml`, plain-text `robots.txt` (via `content/static/`), real `404.html`
      (today every unknown URL returns the homepage with 200)
- [ ] PNG `og:image` with absolute URL (SVG doesn't show in LinkedIn/Slack previews)
- [ ] One template: Posts/Now/Inspiration use a different font and column width than Home/post pages
- [ ] Posts list: dates wrap onto two lines
- [ ] `/graph`: orphaned, old nav, graph pane rendered empty in a headless screenshot. Fix or remove
- [ ] Link color contrast; profile image alt text; self-host font (~820 ms render-blocking)
- [ ] GitHub profile: bio, website field, pin the generator + visuals repos, archive 2022 site repos
      (personal-site, ssg, static-site-content)

## 8. Makeover (later)

Lead with writing; distinctive self-hosted type with a real scale; dark mode; non-default accent color;
garden status + "last tended" date per note; keep it fast and JS-free.
