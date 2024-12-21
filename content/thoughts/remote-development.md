# Remote development

With [[vscode]] there is an option to develop inside a container.

## Pros

* Development is consistent across developer laptops - no more tests that work on one pc due to internationalization settings but not on another
* No more python virtual environment shenanigans

## Cons

* It's a bit slow on both startup and intellisense
* Setup is not difficult, but not completely trivial either - especially with vscode settings, extensions etc.

## How to

For a quick setup to build upon:

* Use command (ctrl+shift+p) `Remote-Containers: Add Development Container Configuration Files...` and follow the dialog to add a suitable `Dovkerfile` and `devcontainer.json` file
* Use command `Remote-Containers: Rebuild and Reopen in container` to run the container and start developing inside
* Commit and then start tweaking to your liking
