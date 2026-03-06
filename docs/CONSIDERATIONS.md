# Considerations — Imeritas.Agent.DadJokes.CLI

Design decisions, trade-offs, and forward-looking notes.

---

## 1. Architecture Decisions

(to be populated during planning and implementation)

---

## 2. Key Design Principles

1. **CLAUDE.md is the contract.** It defines what the agent knows about the solution.
2. **agent.yml is the boundary.** Allowed paths, build commands, and context paths are declared upfront.
3. **Issues are units of work, not units of code.** Describe capabilities, not files.
4. **Gates are cheap, rework is expensive.** Review plans before implementation.
5. **Context flows forward.** Each completed issue enriches context for subsequent issues.
6. **Build and test are non-negotiable.** Every issue must leave the build green.
