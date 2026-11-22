# Driving the worker agents in OpenCode (reviewer only)

The workers (`grid`, `forms`, `tables`, …) run as sessions in the **OpenCode Desktop** app. The reviewer can read their replies and send them prompts directly, without the user copy-pasting. Everything you send shows up live in the user's Desktop window, because the app and the CLI share one background server.

## Facts (verified 2026-11-21)

- **Use the Desktop's bundled CLI:** `%APPDATA%\ai.opencode.desktop\cli\2.3.2-x64\opencode-cli.exe`.
  - The `opencode` on PATH is an old npm install (1.19.5). It fails with `Database is not empty and has no session table`, because the Desktop app migrated the DB. Don't use it, and don't "fix" the DB.
  - The version folder changes when the Desktop app updates. If the path is missing, look in `%APPDATA%\ai.opencode.desktop\cli\`.
- The Desktop app runs `opencode-cli serve --service` on a random localhost port, with auth. Don't call it with raw HTTP. `opencode-cli api <METHOD> <path>` finds the service and authenticates for you.
- The Desktop app must be running.
- The API spec is at `opencode-cli api GET /openapi.json`. Endpoints used:

| What | Call |
|---|---|
| list sessions (id, title, directory) | `api GET /api/session` |
| which sessions are running | `api GET /api/session/active` returns `{"data":{"<sid>":{"type":"running"}}}`; an absent id means idle |
| messages, newest first | `api GET /api/session/<sid>/message --param limit=60 --param order=desc` |
| send a prompt | `api POST /api/session/<sid>/prompt --data '{"text":"..."}'` |
| pending permission prompts | `api GET /api/permission/request` (answer: `POST /api/session/<sid>/permission/<requestID>/reply`) |
| stop a running worker | `api POST /api/session/<sid>/interrupt` (only when the user asks) |

Message items have a `type`: `user`, `assistant`, `idle`, and others. An assistant message's `content[]` holds `reasoning` and `text` parts. The worker's reply is the `text` parts of every assistant message after the last `user` message.

## The helper: `docs/oc.ps1`

```powershell
pwsh -NoProfile -File docs/oc.ps1 status                          # RUNNING/idle per worker + pending permissions
pwsh -NoProfile -File docs/oc.ps1 last grid                       # the worker's latest reply (text only)
pwsh -NoProfile -File docs/oc.ps1 send grid -File $env:TEMP\p.md  # send a prompt; refuses if the worker is RUNNING
pwsh -NoProfile -File docs/oc.ps1 wait grid                       # poll until idle, then print the reply
```

The agent-to-session map is hard-coded at the top of `oc.ps1`. Update it when the user starts a new session for a worker: find the id with `api GET /api/session`, matching on the title and the `ui-design` directory.

Current map:

| Agent | Session | Desktop title |
|---|---|---|
| grid | `ses_e0238a9d0ffe1NkofzNcUvbWyi` | Grid Input |
| forms | `ses_e023107ceffes9dzt57DNkx3dR` | Forms |
| tables | `ses_e04098a66ffeTvKHynZrtF4SOi` | Data Table |

## Orchestration loop

1. `oc.ps1 status`. Only touch **idle** workers. A RUNNING worker is mid-task: never send to it, and never interrupt it unless the user says so.
2. `oc.ps1 last <agent>`. That's the phase report, and you review it exactly as in `REVIEWER.md`:
   - read the diff
   - capture it yourself, hidden
   - take pixel measurements
3. Write the verdict plus the next-phase prompt (REVIEWER.md templates) to a temp file, then `oc.ps1 send <agent> -File <file>`.
4. Start `oc.ps1 wait <agent>` with `run_in_background`. You're notified when the worker finishes, and then you go back to step 2.
5. After each review, update the status board in `REVIEWER.md`.

## Rules

- **Ask the user first** before anything that's theirs to decide, the same list as in REVIEWER.md:
  - scope changes
  - one agent touching another's files
  - web quirks
  - commits

  Workers' questions about those go to the user. Don't answer them yourself.
- **Send only what you'd have given the user to paste.** Each prompt must stand on its own, because the worker only sees what you send.
- **One prompt per idle turn.** `send` refuses while a worker runs. Don't queue several.
- **Permission prompts:** if a worker sits RUNNING for a long time with no new messages, check `status` for pending permissions and tell the user. Don't approve them yourself.
- **Never** delete, fork, revert or interrupt sessions unless the user asks.
