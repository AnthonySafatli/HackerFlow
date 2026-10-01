# HackerFlow

HackerFlow is an app that I created to automate the flow I have for j*b applications

The flow I usually use is duplicating a base cover letter I created, then customizing it using AI based on my resume and the given job description.

This application automates this process, and organizes all the jobs, job descriptions, links, and files for me.

It also allows for multiple base resumes and cover letters, which will help me create more custom versions of each for whatever the job is.

## Features

### Job Tracker

This has a table with any jobs I am interested in. It holds all important info related to the job, and tracks the status of that job as I go through the application process.

Each application also has its own resume and cover letter attached to it. This helps me copy and store each on a per job basis. 

I can also create custom AI prompts by injecting the resume, cover letter, and job description into a premade prompt, automating that process.

### Resumes

This holds my base resumes. Each resume has a name, and to update a resume I can just upload the updated version using the same name to add a new version to HackerFlow. All versions of resumes are stored in the app.

### Cover Letters

The cover letter features are identical to the resume features.

### Prompts

I can store my prompts for AI cover letter generation. These can include placeholders into the prompt where I can dynamically inject the `{{resume}}`, `{{coverLetter}}` or `{{jobDescription}}`

## Installation and Usage

I use Linux as my daily driver, and so this setup is for a Linux machine. I am sure it would be easy to setup on any machine though

First clone the project and ensure `dotnet-sdk` and `aspnet-runtime` are installed.

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

Make sure this file has the correct permissions with

```bash
chmod +x ~/.local/bin/hackerflow
```

Congrats, HackerFlow is now installed on your Linux machine.

### Commands

These commands are available to you using this script

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
