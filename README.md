![2233](docs/imgs/2233.png)

<div align="center">

<h1 align="center">

BiliTool

</h1>

[![GitHub Stars](https://img.shields.io/github/stars/RayWangQvQ/BiliBiliToolPro?style=flat-square)](https://github.com/RayWangQvQ/BiliBiliToolPro/stargazers)
[![GitHub Forks](https://img.shields.io/github/forks/RayWangQvQ/BiliBiliToolPro?style=flat-square)](https://github.com/RayWangQvQ/BiliBiliToolPro/network)
[![GitHub Issues](https://img.shields.io/github/issues/RayWangQvQ/BiliBiliToolPro?style=flat-square)](https://github.com/RayWangQvQ/BiliBiliToolPro/issues)
[![GitHub Contributors](https://img.shields.io/github/contributors/RayWangQvQ/BiliBiliToolPro?style=flat-square)](https://github.com/RayWangQvQ/BiliBiliToolPro/graphs/contributors)
[![GitHub All Releases](https://img.shields.io/github/downloads/RayWangQvQ/BiliBiliToolPro/total?style=flat-square)](https://github.com/RayWangQvQ/BiliBiliToolPro/releases)
[![GitHub Release (latest SemVer)](https://img.shields.io/github/v/release/RayWangQvQ/BiliBiliToolPro?style=flat-square)](https://github.com/RayWangQvQ/BiliBiliToolPro/releases)
[![GitHub License](https://img.shields.io/github/license/RayWangQvQ/BiliBiliToolPro?style=flat-square)](https://github.com/RayWangQvQ/BiliBiliToolPro/blob/main/LICENSE)

<a href="https://trendshift.io/repositories/3329" target="_blank">
    <img src="https://trendshift.io/api/badge/repositories/3329" alt="RayWangQvQ%2FBiliBiliToolPro | Trendshift" style="width: 250px; height: 55px;" width="250" height="55"/>
</a>

</div>

**BiliTool 是一个自动执行任务的工具，当我们忘记做某项任务时，它会像一个贴心小助手，按照我们预先吩咐它的命令，在指定频率、时间范围内帮助我们完成计划的任务。**

**BiliTool is an automated task execution tool that acts as a helpful assistant, following pre-configured commands to complete planned tasks within specified frequencies and timeframes when we forget to do them.**

> 本项目 fork 自 [RayWangQvQ/BiliBiliToolPro](https://github.com/RayWangQvQ/BiliBiliToolPro)，仅保留三种部署方式并做了安全加固，改动说明见 [1.1.4](#114-本仓库的安全加固与实现方式)。

主要功能如下：

- **扫码登录，自动更新cookie**
- **每日获取满额升级经验（登录、投币、点赞、分享视频）（支持指定up主）**
- **直播间挂机**
- **每天漫画签到**
- **每天直播签到**
- **直播中心银瓜子兑换为硬币**
- **每月领取大会员赠送的 5 张 B 币券和福利（忘记或者不领就浪费了哦）**
- **每月领取大会员漫画福利**
- **月底在 B 币券过期前进行充电（支持指定想要支持的up主，如果没有喜欢的up，也可以为自己充个电啊，做个用爱为自己发电的人~）**
- **直播中心天选时刻自动参与抽奖**
- **批量取关**
- **大会员大积分任务**
- **支持多账号**
- **理论上支持所有远端的日志推送（默认支持推送到Telegram、企业微信、钉钉、PushPlus、Server酱、酷推，另外也支持自定义推送到任意api）**
---
[目录]

<!-- TOC depthFrom:2 -->

- [1. 如何使用](#1-如何使用)
    - [1.1. 部署 BiliTool](#11-部署-bilitool)
        - [1.1.1. 方式一：青龙 / 呆呆等自动化面板](#111-方式一青龙--呆呆等自动化面板)
        - [1.1.2. 方式二：Docker 运行](#112-方式二docker-运行)
        - [1.1.3. 方式三：下载程序包到本地或服务器运行](#113-方式三下载程序包到本地或服务器运行)
        - [1.1.4. 本仓库的安全加固与实现方式](#114-本仓库的安全加固与实现方式)
    - [1.2. 消息推送（可选）](#12-消息推送可选)
- [2. 功能任务说明](#2-功能任务说明)
- [3. 个性化自定义配置](#3-个性化自定义配置)
- [4. 多账号支持](#4-多账号支持)
- [5. 常见问题](#5-常见问题)
- [6. 版本发布及更新](#6-版本发布及更新)
- [7. 成为开源贡献成员](#7-成为开源贡献成员)
    - [7.1. 贡献代码](#71-贡献代码)
    - [7.2. 贡献文档](#72-贡献文档)
- [8. 捐赠支持](#8-捐赠支持)
- [9. 其他](#9-其他)

<!-- /TOC -->

---
**Github 仓库地址：[angussleeping/BiliBiliToolPro](https://github.com/angussleeping/BiliBiliToolPro)**

**注意：**

- **本应用仅用于学习和测试，作者本人并不对其负责，请于运行测试完成后自行删除，请勿滥用！**
- **所有代码都是开源且透明的，任何人均可查看，程序不会保存或滥用任何用户的个人信息**
- **应用内几乎所有功能都开放了配置（如任务开关、日期、id等），详细信息可阅读配置文档**

运行图示：

<p align="center">
    <img src="docs/imgs/web-schedules.png" alt="运行图示" width="800" />
    <br/>
    <img src="docs/imgs/web-schedules-log.png" alt="运行日志" width="800" />
    <br/>
    <img src="docs/imgs/web-configs.png" alt="运行日志" width="800" />
    <br/>
</p>

## 1. 如何使用

BiliTool 实现自动完成任务的原理，是通过调用一系列开放的api实现的。

**要使用 BiliTool，很简单，按照下面教程部署完成，运行后扫码登录即可。**

### 1.1. 部署 BiliTool

支持以下 3 种部署方式，选择任一适合自己的方式即可。

#### 1.1.1. 方式一：青龙 / 呆呆等自动化面板

[>>青龙部署教程](qinglong/README.md)

> 该方式基于仓库内的 shell 脚本运行，青龙面板、呆呆面板等支持 `ql repo` / 拉库执行 shell 脚本的自动化平台均可使用。

#### 1.1.2. 方式二：Docker 运行

[>>Docker 部署说明](docker/README.md)

#### 1.1.3. 方式三：下载程序包到本地或服务器运行

[>>本地部署说明](docs/runInLocal.md)

#### 1.1.4. 本仓库的安全加固与实现方式

本仓库只保留上述三种部署方式，并针对自建部署做了以下加固。以下说明均对应到具体实现位置，便于二次修改。

**1）管理员密码**

- 存储与校验：`src/Ray.BiliBiliTool.Infrastructure/Helpers/PasswordHelper.cs`，采用 PBKDF2-SHA256（16 字节随机盐、100000 次迭代、32 字节哈希），校验使用常量时间比较，避免时序侧信道；
- 初始密码：`src/Ray.BiliBiliTool.Infrastructure.EF/DbInitializer.cs`，首次初始化数据库时读取配置 `Security:DefaultAdminPassword`（容器环境变量为 `Security__DefaultAdminPassword`），未配置则回落到默认密码并输出告警日志，登录后请在 `Admin` 页面修改。

**2）登录限流**

- `src/Ray.BiliBiliTool.Web/Services/LoginThrottle.cs`：基于 `IMemoryCache` 按来源 IP 统计，5 次失败 / 5 分钟窗口内锁定；
- `src/Ray.BiliBiliTool.Web/Controllers/AuthController.cs`：登录前检查、失败计数、登录成功清零；锁定后登录页会提示稍后再试。

**3）Cookie 与 HTTPS**

- Cookie 策略在 `src/Ray.BiliBiliTool.Web/Extensions/ServiceCollectionExtension.cs`：`HttpOnly` + `SameSite=Lax`，默认 `SecurePolicy=SameAsRequest`；
- 设置 `Security:RequireHttps=true` 后改为 `Always`，Cookie 只经 HTTPS 传输（前提是已有可用的 HTTPS 入口，否则无法登录）；
- 反向代理（Nginx/Caddy/Traefik 等）部署时设置 `Security:UseForwardedHeaders=true`，`Program.cs` 会在管道最前端启用 `UseForwardedHeaders`，从而识别真实协议与客户端 IP（登录限流也按真实 IP 统计）；
- 同一开关同时控制 `UseHttpsRedirection`：`RequireHttps=false`（容器默认）时不启用重定向，避免纯 HTTP 部署出现“无法确定 HTTPS 端口”的告警；
- `/swagger` API 文档默认关闭（`Security:EnableSwagger=false`），需要时显式开启；开发环境（Development）自动开启。

**4）容器运行身份**

- `Dockerfile` 安装 `gosu`；`docker/entrypoint.sh` 以 root 启动时按 `PUID`/`PGID`（默认 `1000`）创建运行用户、修正 `/app/config`、`/app/Logs` 属主，再通过 `gosu` 降权运行 `dotnet`，应用进程不再以 root 身份运行。详细说明见 [Docker 部署说明](docker/README.md)。

**5）发布本仓库自己的产物**

- `.github/workflows/publish-image.yml`：构建并推送 `ghcr.io/<仓库所有者>/bili_tool_web`，支持 amd64 / arm64；
- `.github/workflows/publish-release.yml`：打 tag 后生成 `bilibili-tool-pro-v<版本>-<平台>.zip`，供青龙 `BILI_MODE=bilitool` 与本地运行使用；
- `docker/install.sh`：支持 `BILI_REPO`、`BILI_IMAGE` 覆盖，默认指向本仓库与其镜像。

### 1.2. 消息推送（可选）

如果配置了推送，执行成功后，指定的接收端会收到推送消息，推送效果如下所示：

<p align="center">
    <img src="docs/imgs/push-tg.png" alt="Telegram推送图示" width="300">
</p>

目前默认支持**Telegram推送、PushPlus推送、企业微信应用推送、企业微信推送、钉钉推送、Microsoft Teams推送、Server酱推送和酷推QQ推送**（以上顺序即为个人推荐的排序），如果需要推送到其他端，也可以配置为任意的可以接受消息的Api地址，关于如何配置推送请详见下面的**个性化自定义配置**章节。

推送配置见：[configuration](docs/configuration.md)

## 2. 功能任务说明

这里的**任务**是指一组功能的集合，是工具每次运行的最小单位。

任务列表如下：


|    任务名     |      Code       |                                                功能                                                 |      推荐运行频率      |
| :--------: | :-------------: | :-----------------------------------------------------------------------------------------------: | :--------------: |
|    扫码登录    |      Login      |                   使用app扫码登录，用于第一次运行时初始化cookie，或cookie过期时的更新。不同平台会将cookie存储到不同地方                   |       手动         |
|    每日任务    |      Daily      |                             完成每日任务获取满额65点经验（登录、观看视频、分享视频、投币），快速升级Lv6                              |       每天一次       |
|   天选时刻抽奖   |   LiveLottery   |                                  直播中心天选时刻抽奖，大部分抽奖都需要关注主播，介意的不要开启                                  |      每天0-4次      |
|    批量取关    | UnfollowBatched |                                 批量取关指定分组下的所有关注（主要用于清理天选抽奖而产生的关注）                                  |       手动运行       |
|   大会员大积分   |   VipBigPoint   |                                        大会员大积分任务（签到、浏览、观看）                                         |    每天一次，建议凌晨     |
|   直播间挂机    |  LiveFansMedal  |                                               直播间挂机                                               |       每天一次       |
|    漫画任务    |      Manga      |                                              漫画签到、阅读                                              |       每天一次       |
| 领取大会员漫画权益  | MangaPrivilege  |                                            领取大会员的漫画权益                                             |       每天一次       |
|  银瓜子兑换硬币   |   Silver2Coin   |                                             使用银瓜子换取硬币                                             |       每天一次       |
|  免费B币券充电   |     Charge      |                                 大会员每31天可免费领取一张5B币券，可用于给除自己以外的UP充电                                 |       每天一次       |
|  领取大会员福利   |  VipPrivilege   |                                              领取大会员福利                                              |       每天一次       |
|  测试Cookie  |      Test       |                                           测试Cookie是否正常                                            |       手动运行       |


## 3. 个性化自定义配置

[>>点击查看配置说明文档](docs/configuration.md)

## 4. 多账号支持

部署成功后，直接去运行扫码登录任务，扫码成功后，应用会自动更新或添加cookie。

青龙平台会添加环境变量里，Key 为 `Ray_BiliBiliCookies__0`、`Ray_BiliBiliCookies__1`、`Ray_BiliBiliCookies__2`...

其他平台默认会添加到名为cookies.json的账号配置文件中：
```
{
  "BiliBiliCookies": [
    "cookie1",
    "cookie2",
    "...",
  ],
}

```

## 5. 常见问题

[>>点击查看常见问题文档](docs/questions.md)

[Issues（议题）](https://github.com/angussleeping/BiliBiliToolPro/issues)板块可以用来提交**Bug**和**建议**；

[Discussions（讨论）](https://github.com/angussleeping/BiliBiliToolPro/discussions)板块可以用来**提问**和**讨论**。

大部分问题其实都可以在文档、议题和讨论中找到答案。

所以如果你有疑问，

* 请先确认是否可以通过升级到最新版本解决
* 然后搜索文档（特别是配置说明文档和常见问题文档）、议题和讨论，查看是否已有其他人遇到相同问题、是否已有解决方案

如果确认还未解决，可以自己提交 Issue，或发布 Discussions 与大家一起探讨，我会尽快确认并解决。

（关于如何正确的提交Issue，请详见**常见问题文档**）。

## 6. 版本发布及更新

本仓库的发布产物由 GitHub Actions 生成，可自行触发：

- **Release 程序包**：给仓库打版本 tag（与 `common.props` 中的 `Version` 一致）后，`.github/workflows/publish-release.yml` 会自动打包并上传 `bilibili-tool-pro-v<版本>-<平台>.zip`，供青龙 `BILI_MODE=bilitool` 与本地运行使用；
- **Docker 镜像**：`.github/workflows/publish-image.yml` 构建并推送 `ghcr.io/angussleeping/bili_tool_web`（支持 amd64 / arm64）。

升级已部署的实例：

- Docker：`docker compose pull && docker compose up -d`，或使用 Watchtower（见 [Docker 部署说明](docker/README.md)）；
- 青龙 / 呆呆等面板：在面板中执行 `ql repo` 重新拉库，脚本会自动更新程序包；
- 本地 / 服务器运行：重新下载对应平台的最新 Release 包并覆盖。

## 7. 成为开源贡献成员

### 7.1. 贡献代码

如果你有好的想法，欢迎向仓库贡献你的代码，贡献步骤：

* 搜索查看 Issue，确定是否已有人提过同类问题
* 对于不确定的主题，为避免code结束后PR不被接受，可以先新建 Issue，描述问题或建议，讨论清楚后再动手编码
* 如果确认自己可以解决，请 Fork 仓库后，在**main 分支**进行编码开发，完成后**提交 PR 到 main 分支**

我会尽快进行代码审核，测试成功后会合并入 main 主分支，提前感谢您的贡献。

### 7.2. 贡献文档

文档部分由于我个人精力有限（写文档比写代码累多了），所以有些地方写的很简略，甚至有遗漏和错别字，不能贡献代码的朋友也欢迎来一起维护文档，欢迎 PR 来纠正我，一样都算是对开源做贡献了。

## 8. 捐赠支持

个人维护开源不易

如果觉得我写的程序对你小有帮助

或者，就是单纯的想集资给我买瓶霸王增发液

那么下面的赞赏码可以扫一扫啦

（赞赏时记得留下【昵称】和【留言】~ 另外我发现很多留言想要进群或者加好友的，一定一定要记得留下微信号哈，微信赞赏页面是看不到微信号的）

**☟☟☟ 扫码自动赞赏 1 元：☟☟☟**

![赞赏码](docs/imgs/donate.jpg)

> 项目中的优先支持的UP主的配置项，默认是作者的 UpId （只是作为了 JSON 配置文件的默认值，代码是干净的），需要更改的话，直接修改相应配置即可（secrets或环境变量等各种方式都行）。
当然，不改的话，也算是另一种捐赠支持作者的方式啦。

感谢支持~

## 9. 其他

`API`参考：

- [www.bilibili.com](https://www.bilibili.com/)

- [SocialSisterYi/bilibili-API-collect](https://github.com/SocialSisterYi/bilibili-API-collect)

- [JunzhouLiu/BILIBILI-HELPER](https://github.com/JunzhouLiu/BILIBILI-HELPER)

❤️Thanks to `JetBrains` for the free certificate support:

<p align="center">
    <img src="https://resources.jetbrains.com/storage/products/company/brand/logos/ReSharper.svg" alt="ReSharper logo" width="200">
</p>

❤️Thanks to [YxVM](https://yxvm.com/aff.php?aff=668) & [NodeSeekDev](https://github.com/NodeSeekDev/NodeSupport) for sponsoring the server for testing support:

<p align="center">
    <a href="https://yxvm.com/aff.php?aff=668">
        <img src="docs/imgs/node-support.png" alt="YxVm logo" width="200">
    </a>
</p>

❤️Thanks to [DartNode](https://dartnode.com?aff=FriskyGopher833) for sponsoring the server for testing support:

[![Powered by DartNode](https://dartnode.com/branding/DN-Open-Source-sm.png)](https://dartnode.com "Powered by DartNode - Free VPS for Open Source")

❤️Thank you for your star to this project:

[![Star History Chart](https://api.star-history.com/svg?repos=RayWangQvQ/BiliBiliToolPro&type=Date)](https://www.star-history.com/#RayWangQvQ/BiliBiliToolPro&Date)
