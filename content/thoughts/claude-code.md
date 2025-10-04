# Claude code

CLI agent from Anthropic.

## Installation

Doesn't run on Windows directly, but can be run in WSL. Installation guides on the [Anthropic website](https://docs.anthropic.com/en/docs/claude-code/setup).

## Usage

Go to the directory you want your agent to work in and run: `claude`. It will guide you on signing in. If you want to continue your last session, run `claude -c` or `claude --resume` to be able to pick among earlier sessions.

## Troubleshooting

On Windows+wsl ubuntu I've seen connection issues like:

```text
Unable to connect to Anthropic services
Failed to connect to api.anthropic.com: ETIMEDOUT
```

For me the fix was to avoid ipv6 with:

```bash
sudo sysctl -w net.ipv6.conf.all.disable_ipv6=1
sudo sysctl -w net.ipv6.conf.default.disable_ipv6=1
sudo sysctl -w net.ipv6.conf.lo.disable_ipv6=1
```

## Files

The agent creates a `.claude/settings.local.json` in the project directory that holds information on commands you've allowed claude to run without confirmation. Possibly .gitignore this if other developers might not be comfortable with the same commands. Also it seems to break and reset some times (with updates?).

If you create a `CLAUDE.md` file in the project directory, it will be used as the initial context for the agent. This is useful for providing specific instructions or context about the project. If the readme in the project is good, this CLAUDE.md can just list commands for how to build, run, test, format, lint the project, etc.
