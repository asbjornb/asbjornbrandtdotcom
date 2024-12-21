# Make

Make is a common way to standardize the commands used in a software repository. It can be helpful to have a standard way to build, test, run and deploy software across different repositories with e.g. different languages, package managers, testing frameworks, formatting tools, etc.

## Makefile

A Makefile is a file that contains the commands to build, test, run and deploy software. It is typically named `Makefile` or `makefile`.

Example of a Makefile with phony targets:

```makefile
PHONY=test all package build clean

test:
    poetry run pytest
    poetry run mypy -p some.module

format:
    poetry run ruff format

clean:
    poetry lock --no-update
    poetry install

build: format clean
    poetry build
```

`PHONY` targets are used to define commands that are not associated with a file. Without `PHONY` targets, Make will assume that the target is a file and will not run the command if the file exists.

With make installed, you can run `make test` to run the `test` target.

## Limitations & Alternatives

A common limitation of Make is that you can't easily pass arguments to the commands. One way to work around this is to have make call a shell script that prompts the user for the arguments.

A recent alternative to make is [Just](https://github.com/casey/just) which is more of a command runner than a build system and as such has some niceties over Make like build arguments, simple `just -l` to list all commands, no need for PHONY etc.
