#!/bin/bash
set -e
set -o pipefail

# Runtime identity inside the container; override with PUID/PGID to align with the
# owner of host-mounted directories
PUID=${PUID:-1000}
PGID=${PGID:-1000}

echo "Starting BiliTool container..."
mkdir -p /app/config

# Started as root: fix ownership of mounted dirs, then drop privileges so the app
# process does not run as root inside the container
if [ "$(id -u)" = "0" ]; then
    if ! getent group "$PGID" >/dev/null; then
        groupadd -g "$PGID" bilitool
    fi
    if ! getent passwd "$PUID" >/dev/null; then
        useradd -u "$PUID" -g "$PGID" -M -d /app -s /sbin/nologin bilitool
    fi

    mkdir -p /app/config /app/Logs /app/keys
    chown -R "$PUID:$PGID" /app/config /app/Logs /app/keys

    echo "Running maintenance scripts..."

    # 3.3.0 need migrate db file location to /app/config
    if [ -f "/app/BiliBiliTool.db" ]; then
        echo "[3.3.0] Migrate db file location to /app/config"
        mv /app/BiliBiliTool.db /app/config/BiliBiliTool.db
        chown "$PUID:$PGID" /app/config/BiliBiliTool.db
    fi

    echo "Starting application as uid=$PUID gid=$PGID..."
    exec gosu "$PUID:$PGID" dotnet Ray.BiliBiliTool.Web.dll "$@"
fi

# Already non-root (e.g. docker run --user): run directly
mkdir -p /app/config /app/Logs /app/keys 2>/dev/null || true
echo "Running maintenance scripts..."

# 3.3.0 need migrate db file location to /app/config
if [ -f "/app/BiliBiliTool.db" ]; then
    echo "[3.3.0] Migrate db file location to /app/config"
    mv /app/BiliBiliTool.db /app/config/BiliBiliTool.db
fi

echo "Starting application as uid=$(id -u)..."
exec dotnet Ray.BiliBiliTool.Web.dll "$@"
