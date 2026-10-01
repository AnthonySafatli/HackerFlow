# HackerFlow

HackerFlow is an app I built to automate how I apply for jobs.

My usual flow is to duplicate a base cover letter, then customize it with AI using my resume and the job description.

This app automates that process and keeps all my jobs, descriptions, links, and files in one place.

It also supports multiple base resumes and cover letters, so I can create more tailored versions for each job.

## Features

### Job Tracker

A table of all the jobs I'm interested in. It holds the important info for each job and tracks its status as I move through the application process.

Each application has its own resume and cover letter attached, so I can copy and store them per job.

I can also build custom AI prompts by injecting the resume, cover letter, and job description into a premade prompt. This automates that step too.

### Resumes

This holds my base resumes. Each resume has a name. To update one, I just upload the new version with the same name and it gets added as a new version. All versions are kept in the app.

### Cover Letters

Cover letters work the same way as resumes.

### Prompts

I can save my prompts for AI cover letter generation. They can include placeholders like `{{resume}}`, `{{coverLetter}}`, or `{{jobDescription}}`, which get filled in automatically.

## Installation and Usage

I use Linux as my daily driver, so this setup is for Linux. It should be easy to set up on other systems too.

First, clone the project and make sure `dotnet-sdk` and `aspnet-runtime` are installed.

Next, add this script to `~/.local/bin/hackerflow`

```bash
#!/bin/bash
set -euo pipefail

SRC="$HOME/source/HackerFlow"          # <- your source folder (contains the .csproj)
APP="$HOME/apps/hackerflow"
STATE="$HOME/.local/state/hackerflow"
PIDFILE="$STATE/pid"
LOG="$STATE/hackerflow.log"
URL="http://127.0.0.1:5000"

mkdir -p "$APP" "$STATE"

running() { [[ -f $PIDFILE ]] && kill -0 "$(cat "$PIDFILE")" 2>/dev/null; }

build() {
  dotnet publish "$SRC" -c Release -r linux-x64 --self-contained false -o "$APP"
}

start() {
  if running; then
    echo "Already running (pid $(cat "$PIDFILE"))"
  else
    cd "$APP"
    ASPNETCORE_URLS="$URL" ASPNETCORE_ENVIRONMENT=Production \
      setsid nohup dotnet HackerFlow.dll >"$LOG" 2>&1 &
    echo $! >"$PIDFILE"
    for _ in {1..30}; do
      curl -s -o /dev/null "$URL" && break
      sleep 0.5
    done
    echo "Started (pid $(cat "$PIDFILE"))"
  fi
  [[ "${1:-}" == "--no-browser" ]] || xdg-open "$URL" >/dev/null 2>&1 || true
}

stop() {
  if running; then
    kill "$(cat "$PIDFILE")"
    rm -f "$PIDFILE"
    echo "Stopped"
  else
    rm -f "$PIDFILE"
    echo "Not running"
  fi
}

case "${1:-start}" in
  start)     start ;;
  stop)      stop ;;
  restart)   stop; sleep 1; start ;;
  status)    running && echo "Running (pid $(cat "$PIDFILE"))" || echo "Stopped" ;;
  logs)      tail -f "$LOG" ;;
  install)   build; echo "Installed. Run: hackerflow" ;;
  update)
    was_running=false; running && was_running=true
    stop; build
    $was_running && start --no-browser
    echo "Updated" ;;
  uninstall) stop; rm -rf "$APP" "$STATE"; echo "Removed (source and ~/.local/bin/hackerflow untouched)" ;;
  *) echo "Usage: hackerflow {start|stop|restart|status|logs|install|update|uninstall}" ;;
esac
```

Make the file executable with

```bash
chmod +x ~/.local/bin/hackerflow
```

That's it, HackerFlow is now installed on your Linux machine.

### Commands

Here are the commands the script gives you

```bash
hackerflow install     # first-time publish
hackerflow             # start + open browser (same as `hackerflow start`)
hackerflow stop
hackerflow restart
hackerflow status
hackerflow logs        # Ctrl+C to exit
hackerflow update      # stop, rebuild, restart if it was running
hackerflow uninstall
```

## Uninstalling

To remove HackerFlow, run

```bash
hackerflow uninstall
```

This stops the app and deletes the install folder (`~/apps/hackerflow`) and the state folder (`~/.local/state/hackerflow`).

**Heads up:** the SQLite database (`hackerflow.db`) lives in the install folder, so uninstalling deletes all your jobs and their info. If you want to keep it, copy it somewhere safe first

```bash
cp ~/apps/hackerflow/hackerflow.db ~/hackerflow-backup.db
```

Your resumes and cover letters are stored separately (by default in `~/Documents/HackerFlowDocs`), so they are not touched. If you want to remove those too, delete that folder yourself.

The script and source code are also left alone. To fully clean up, remove them too

```bash
rm ~/.local/bin/hackerflow
rm -rf ~/source/HackerFlow
```
