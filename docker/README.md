# Docker 使用说明
<!-- TOC depthFrom:2 -->

- [1. 前期工作](#1-前期工作)
- [2. 方式一：一键脚本(推荐)](#2-方式一一键脚本推荐)
- [3. 方式二：手动 Docker Compose](#3-方式二手动-docker-compose)
    - [3.1. 启动](#31-启动)
    - [3.2. 其他命令参考](#32-其他命令参考)
- [4. 方式三：手动Docker指令](#4-方式三手动docker指令)
    - [4.1. Docker启动](#41-docker启动)
    - [4.2. 其他指令参考](#42-其他指令参考)
    - [4.3. 使用Watchtower更新容器](#43-使用watchtower更新容器)
- [5. 登录](#5-登录)
- [6. 添加 Bili 账号](#6-添加-bili-账号)
- [7. 自己构建镜像（非必须）](#7-自己构建镜像非必须)
- [8. 其他](#8-其他)

<!-- /TOC -->

## 1. 前期工作

```
apt-get update
apt-get install curl
```

## 2. 方式一：一键脚本(推荐)

```
bash <(curl -sSL https://raw.githubusercontent.com/angussleeping/BiliBiliToolPro/main/docker/install.sh)
```

## 3. 方式二：手动 Docker Compose

### 3.1. 启动

```
# 创建目录
mkdir bili_tool_web && cd bili_tool_web

# 下载
wget https://raw.githubusercontent.com/angussleeping/BiliBiliToolPro/main/docker/sample/docker-compose.yml
mkdir -p config
cd ./config
wget https://raw.githubusercontent.com/angussleeping/BiliBiliToolPro/main/docker/sample/config/cookies.json
cd ..

# 启动
docker compose up -d

# 查看启动日志
docker logs -f bili_tool_web
```

最终文件结构如下：

```
bili_tool_web
├── Logs
├── config
├──── cookies.json
└── docker-compose.yml
```

### 3.2. 其他命令参考

```
# 启动 docker-compose
docker compose up -d

# 停止 docker-compose
docker compose stop

# 查看实时日志
docker logs -f bili_tool_web

# 进入容器
docker exec -it bili_tool_web /bin/bash

# 手动更新容器
docker compose pull && docker compose up -d
```

## 4. 方式三：手动Docker指令

### 4.1. Docker启动

```
# 创建目录
mkdir bili_tool_web && cd bili_tool_web

# 生成并运行容器
docker pull ghcr.io/angussleeping/bili_tool_web
docker run -d --name="bili_tool_web" \
    -p 22330:8080 \
    -e TZ=Asia/Shanghai \
    -v ./Logs:/app/Logs \
    -v ./config:/app/config \
    ghcr.io/angussleeping/bili_tool_web

# 查看实时日志
docker logs -f bili_tool_web
```

其中，`cookie`需要替换为自己真实的cookie字符串

### 4.2. 其他指令参考

```
# 启动容器
docker start bili_tool_web

# 停止容器
docker stop bili_tool_web

# 重启容器
docker restart bili_tool_web

# 删除容器
docker rm bili_tool_web

# 进入容器
docker exec -it bili_tool_web /bin/bash
```

### 4.3. 使用Watchtower更新容器
```
docker run --rm \
    -v /var/run/docker.sock:/var/run/docker.sock \
    containrrr/watchtower \
    --run-once --cleanup \
    bili_tool_web
```

## 5. 登录

- 默认用户：`admin`
- 默认密码：`BiliTool@2233`

首次登陆后，请到`Admin`页面修改密码。

也可以在部署时通过环境变量 `Security__DefaultAdminPassword` 指定初始密码（仅在首次初始化数据库时生效）。

连续登录失败 5 次后，登录会被暂时锁定 5 分钟（按来源 IP 统计）。

## 6. 安全加固（公网部署建议）

### 6.1. 使用非 root 运行

容器启动脚本会在以 root 启动时先修正 `/app/config`、`/app/Logs` 的属主，再以普通用户身份运行应用（默认 `1000:1000`）。如需与宿主机目录属主对齐，设置：

```
PUID=1001
PGID=1001
```

### 6.2. 反向代理 + HTTPS

默认只提供 HTTP。公网部署建议在前面挂 Nginx/Caddy/Traefik 做 HTTPS，并打开以下两个开关：

```
Security__UseForwardedHeaders=true   # 识别真实协议与客户端 IP（限流按真实 IP 统计）
Security__RequireHttps=true          # 登录 Cookie 仅通过 HTTPS 传输
```

说明：

- `Security__RequireHttps=true` 后，必须已存在可用的 HTTPS 入口，否则无法登录；
- 开启 `UseForwardedHeaders` 会信任所有代理转发的 `X-Forwarded-For` / `X-Forwarded-Proto`，请确保容器端口 22330 不直接暴露到公网。

### 6.3. 定时任务随机延时

镜像内默认开启：每个定时任务被触发后，先随机延时 `1 ~ 300` 秒再执行，避免每天都在同一秒准点请求。可通过环境变量调整上限（单位秒），设为 `0` 即关闭：

```
Security__RandomSleepMaxSec=300
```

该延时只作用于定时触发，页面上的“立即运行”、扫码登录和测试任务不会延时。完整说明见 [配置说明](../docs/configuration.md) 的 `3.2.2. 随机睡眠的最大时长`。

## 7. 添加 Bili 账号

扫码进行账号添加。

![trigger](../docs/imgs/web-trigger-login.png)

![login](../docs/imgs/docker-login.png)

## 8. 自己构建镜像（非必须）

本仓库通过 `publish-image.yml` 构建并推送镜像，默认地址：

- GitHub: [ghcr.io/angussleeping/bili_tool_web](https://github.com/angussleeping/BiliBiliToolPro/pkgs/container/bili_tool_web)

如果有需要（大部分都不需要），可以使用源码自己构建镜像，如下：

在有项目的Dockerfile的目录运行

`docker build -t TARGET_NAME .`

`TARGET_NAME`为镜像名称和版本，可以自己起个名字

构建并推送到自己的镜像仓库后，可以通过环境变量指定一键脚本使用的镜像：

```
BILI_IMAGE=ghcr.io/<你的用户名>/bili_tool_web \
bash <(curl -sSL https://raw.githubusercontent.com/angussleeping/BiliBiliToolPro/main/docker/install.sh)
```

## 9. 其他

代码编译和发布环境: mcr.microsoft.com/dotnet/sdk:8.0

代码运行环境: mcr.microsoft.com/dotnet/aspnet:8.0

如果下载`github`资源有问题，可以尝试添加加速器。
