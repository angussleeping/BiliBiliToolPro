#!/bin/bash
set -e
set -o pipefail

# 容器内应用运行身份，可用 PUID/PGID 覆盖（便于与宿主机挂载目录属主对齐）
PUID=${PUID:-1000}
PGID=${PGID:-1000}

echo "Starting BiliTool container..."

# 以 root 启动时：先修正挂载目录属主，再降权运行，避免应用进程以 root 身份跑在容器里
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

# 已以非 root 启动（如 docker run --user），直接运行
mkdir -p /app/config /app/Logs /app/keys 2>/dev/null || true
echo "Starting application as uid=$(id -u)..."
exec dotnet Ray.BiliBiliTool.Web.dll "$@"
