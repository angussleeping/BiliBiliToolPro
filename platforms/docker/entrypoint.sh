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
    # PUID/PGID=0 等于继续以 root 运行，降权形同虚设，明确警告
    if [ "$PUID" = "0" ] || [ "$PGID" = "0" ]; then
        echo "WARNING: PUID/PGID is 0, the application will run as root"
    fi

    # 组/用户名 bilitool 可能已存在但 ID 不同（或 ID 已被其他账号占用），
    # 只在「GID 和组名都空闲」时才创建，避免 set -e 下 groupadd 冲突直接退出
    if ! getent group "$PGID" >/dev/null && ! getent group bilitool >/dev/null; then
        groupadd -g "$PGID" bilitool
    fi
    if ! getent passwd "$PUID" >/dev/null && ! getent passwd bilitool >/dev/null; then
        useradd -u "$PUID" -g "$PGID" -M -d /app -s /sbin/nologin bilitool
    fi

    # 属主已是目标用户就跳过递归 chown，日志量大时省掉每次启动的全量遍历
    for dir in /app/config /app/Logs /app/keys; do
        mkdir -p "$dir"
        if [ "$(stat -c '%u:%g' "$dir")" != "$PUID:$PGID" ]; then
            chown -R "$PUID:$PGID" "$dir"
        fi
    done

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
