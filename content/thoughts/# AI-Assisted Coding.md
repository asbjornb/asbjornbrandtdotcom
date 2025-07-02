# AI-Assisted Coding

Personal experiences and observations on using AI tools for software development.

## Personal Experience: JavaScript Game Development

Made a small JavaScript game in 2 nights while watching TV with my wife, with no prior JavaScript game development experience. In general it was a great experience to be able to churn ideas into code and see the results immediately. 2 things blew my mind:

- Claude Code generated great textures in pure code. I didn't have to fiddle with using tile sheets or image files, not even for buildings.
- I got some typescript errors in github actions that I hadn't seen locally (due to not running the check). Claude helped me fix the errors and add a precommit hook in under 5 minutes to prevent recurrence. This in tools I didn't know well at all.

## My most used workflow

1. Ask Claude Code to do something fairly small
2. Review the code manually
3. Commit it

This approach often lets me complete features in 30 minutes that would have taken 8 hours before.

Sometimes it is fun to go full yolo and not review the code - like when experimenting with new ideas in unfamiliar languages, but there is a higher risk of getting into unresolvable situations that way.

## When AI Gets Stuck

It is often better to completely restart and approach differently

### Example: Game Autosave Feature

**First attempt (failed):** Asked Claude Code to build autosaves directly

It built the feature, but it was buggy. Chasing one bug led to another and even trying to enforce with tests had it stuck.
I discarded all changes (threw away my branch) and took a different approach.

**Second attempt (successful):**

1. First asked it to make save games easier without building save functionality yet
2. It created a central gamestate interface for managing gamestate
3. Integrated the gamestate management
4. Then when I asked for autosaves, it worked right away

### Other Strategies When Stuck

- **Generate tests first** - Can provide better structure
- **Try a different AI model** with the same task
- **Break down the problem** into smaller pieces

## Tool Comparisons

### Claude Code

See also [[Claude code]].

- Great for small, focused tasks
- Excellent at code generation and debugging
- Works well with manual review workflow

### GitHub Copilot (Visual Studio)

- **Still too many errors** to be fully worthwhile
- Crashes or stalls when running terminal commands
- Hopefully this will improve quickly

### OpenAI Codex/ChatGPT

- Nice in principle with container isolation and allowing for parallel tasks
- **Downsides:**
  - Difficult to add proper tools in current iteration (no custom containers yet)
  - Slow back-and-forth through web interface

## Notable Resources & Industry Perspectives

### Blog Posts

**[My AI Skeptic Friends Are All Nuts](https://fly.io)** - Thomas Ptacek · 16 min  
Spicy rebuttal to "LLMs are a fad": agents already handle yak-shaving, hallucinations are mostly solved by tooling, and mediocre generated code still beats hours of tedium.

**[AI Eats the World](https://ben-evans.com)** - Benedict Evans slide deck · ~110 slides / 20 min skim  
Annual big-picture deck mapping how generative AI ripples through every sector, from compute supply chains to creative work. Addresses why "everyone's building but hardly anyone's using" is the real paradox.

**[The Six-Month Recap](https://ghuntley.com)** - Geoffrey Huntley · ~30 min  
Blog-ified keynote chronicling experiments with headless agents, emoji COBOL, and why companies that drag their feet on AI will hemorrhage talent.

**[LLMs Are Kryptonite for Legacy Code (But Don't Let Them Touch It)](https://the.scapegoat.dev)** - scapegoat.dev · 8 min  
Six-step playbook for using models as "software archaeologists" to map, instrument and gradually replace spaghetti systems—without letting them refactor live code.

**[LLMs Bring New Nature of Abstraction](https://martinfowler.com)** - Martin Fowler · 24 Jun 2025 · ~8 min  
Argues Gen-AI represents the biggest leap since assembler to high-level languages: prompt-driven work lifts us up an abstraction level and pushes us sideways into non-determinism. Expect knock-on effects for version control, testing, and what "source of truth" even means.

**[Augmented Coding: Beyond the Vibes](https://tidyfirst.substack.com)** - Kent Beck · 25 Jun 2025 · ~10 min  
Hands-on notes from a real production project pairing with an LLM "genie." Concrete stories about design docs, domain nuance challenges, and evolving "tidy-as-you-code" techniques.

### Videos

**[Software Is Changing (Again)](https://youtube.com)** - Andrej Karpathy · 46 min  
Steps through Software 1.0→2.0→3.0, arguing that natural-language prompts are the new programming language and tracing implications for tooling, infrastructure and organizational charts.

**[Industry Perspectives on AI-Assisted Coding](https://www.youtube.com/watch?v=EO3_qN_Ynsk)**  - Gergely Orosz 25 min
Showcases impressions on AI-assisted coding across the industry (startups, AI vendors, big tech and independent developers).

## Tools & Technologies to Explore

### Code Search & Repository Management

- **Multi-repo search tool** - Build or find an MCP server that enables searching across large multi-repositories. Should index efficiently (merkle trees?) to avoid pure text search. Could generate documentation over time, submit merge requests for missing README files, or maintain internal docs for faster codebase understanding. Essentially a code search tool designed for agent use (potentially human use too).

### Editors & Development Environments

- **Void Editor** - Open source Cursor clone with bring-your-own API key. Check periodically for updates.
- **Amp Code** - Agentic coding tool. Alternative to Claude Code heavily used by Manuel Odendahl (expert at utilizing coding agents, blogs at [the.scapegoat.dev](https://the.scapegoat.dev/) and go-go-golems on [typeshare.co](https://typeshare.co/go-go-golems))

### Code Review & Analysis

- **Greptile** - Code review tool worth investigating

### Building AI products

- **baml** - [baml](https://github.com/BoundaryML/baml) LLM prompts as functions. By the great Samuel Lijin

### Content & Tooling Development

- **Blog development** - Use AI tools to experiment with new blog designs
- **Note-taking tooling** - Build tools for faster note management: quick note opening, LLM-powered search and improvement suggestions, publishing capabilities. Could be web-based, command line, or VS Code extensions. Add simple inbox that is accessible from phone and desktop.

### Following Key Voices

- Set up notifications for Manuel Odendahl blogs
- Set up notifications for Geoffrey Huntley posts
