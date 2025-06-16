# Static sites

Static site generator is definitely the way to go for a modern blog - though interactivity such as comments then require workarounds.

Static site generators work by just delivering html, js, css files directly. This means no easy contact to a server so more difficult to support stuff like contact forms etc. However it also means very fast page loads, no database/security concerns, no updates on the site, very cheap if not free hosting.

* For cool features that require a server just build that in a separate app and host it elsewhere?
* Blazor can be hosted in azure - can it somehow then be embedded at the same domain? Should be doable with pointing subdomains there?

## Options

To see and compare many options check [jamstack.org/generators](]https://jamstack.org/generators/)

* [[11ty]] - currently preferred. Very simple and can separate all site code from all content so that content is at the root.
* Jekyll
* Hugo

Also note that it might actually not be too bad to build a static site generator directly in e.g. python by importing jinja2 and markdown2.
