# Venture

Venture is an app I built to automate how I apply for jobs.

My usual flow is to duplicate a base cover letter, then customize it with AI using my resume and the job description.

This app automates that process and keeps all my jobs, descriptions, links, and files in one place.

It also supports multiple base resumes and cover letters, so I can create more tailored versions for each job.

## Features

### Job Tracker

A table of all the jobs I'm interested in. It holds the important info for each job and tracks its status as I move through the application process.

Each application has its own resume and cover letter attached, so I can copy and store them per job.

I can also build custom AI prompts by injecting the resume, cover letter, and job description into a premade prompt. This automates that step too.

### Networking

This has two panes, the Connections pane and the Companies pane. It with the Job Tracker to keep track of all your connections and companys

You can organize your connections, and see what companies you have been applying to or want to apply to, setting properties for each to help with your networking endeavors.

### Resumes

This holds my base resumes. Each resume has a name. To update one, I just upload the new version with the same name and it gets added as a new version. All versions are kept in the app.

### Cover Letters

Cover letters work the same way as resumes.

### Prompts

I can save my prompts for AI cover letter generation. They can include placeholders like `{{resume}}`, `{{coverLetter}}`, or `{{jobDescription}}`, which get filled in automatically.

## Installation and Usage

I use Linux as my daily driver, so this setup is for Linux. It should be easy to set up on other systems too.

First, clone the project and make sure `dotnet-sdk` and `aspnet-runtime` are installed.

Next, add this script to `~/.local/bin/venture`

```bash
#!/bin/bash
set -euo pipefail

SRC="$HOME/source/Venture"          # <- your source folder (contains the .csproj)
APP="$HOME/apps/venture"
STATE="$HOME/.local/state/venture"
PIDFILE="$STATE/pid"
LOG="$STATE/venture.log"
URL="http://127.0.0.1:4999"

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
      setsid nohup dotnet Venture.dll >"$LOG" 2>&1 &
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
  install)   build; echo "Installed. Run: venture" ;;
  update)
    was_running=false; running && was_running=true
    stop; build
    $was_running && start --no-browser
    echo "Updated" ;;
  uninstall) stop; rm -rf "$APP" "$STATE"; echo "Removed (source and ~/.local/bin/venture untouched)" ;;
  *) echo "Usage: venture {start|stop|restart|status|logs|install|update|uninstall}" ;;
esac
```

Make the file executable with

```bash
chmod +x ~/.local/bin/venture
```

That's it, Venture is now installed on your Linux machine.

### Commands

Here are the commands the script gives you

```bash
venture install     # first-time publish
venture             # start + open browser (same as `venture start`)
venture stop
venture restart
venture status
venture logs        # Ctrl+C to exit
venture update      # stop, rebuild, restart if it was running
venture uninstall
```

## Uninstalling

To remove Venture, run

```bash
venture uninstall
```

This stops the app and deletes the install folder (`~/apps/venture`) and the state folder (`~/.local/state/venture`).

**Heads up:** the SQLite database (`venture.db`) lives in the install folder, so uninstalling deletes all your jobs and their info. If you want to keep it, copy it somewhere safe first

```bash
cp ~/apps/venture/venture.db ~/venture-backup.db
```

Your resumes and cover letters are stored separately (by default in `~/Documents/VentureDocs`), so they are not touched. If you want to remove those too, delete that folder yourself.

The script and source code are also left alone. To fully clean up, remove them too

```bash
rm ~/.local/bin/venture
rm -rf ~/source/venture
```
