![BiliTool banner](docs/imgs/2233.png)

<div align="center">

# BiliTool

自动化执行一个或多个 Bilibili 账号的日常任务。

[![release](https://img.shields.io/github/v/release/angussleeping/BiliBiliToolPro?label=release)](https://github.com/angussleeping/BiliBiliToolPro/releases)
[![ghcr](https://img.shields.io/badge/ghcr.io-bili__tool__web-blue?logo=docker&logoColor=white)](https://github.com/angussleeping/BiliBiliToolPro/pkgs/container/bili_tool_web)

</div>

BiliTool 通过 Bilibili 的接口定时执行各类任务。你可以使用 Web 面板管理账号、配置、定时计划和执行日志，也可以用 Console 程序配合任意调度器运行。

支持的功能包括：扫码登录与 Cookie 更新、多账号、每日经验任务、直播间与漫画相关活动、大会员权益、B 币券充电、批量取关，以及可选的消息推送。每个任务都可以独立配置；实际可用性取决于你的账号状态和 Bilibili 当前的服务情况。

> [!IMPORTANT]
> 本项目仅供学习与技术测试使用。运行前请先了解各任务的行为与配置，合理使用，并遵守 Bilibili 的相关条款。请勿在 issue 或公开配置中泄露账号 Cookie 等敏感信息。

> 本仓库是 [RayWangQvQ/BiliBiliToolPro](https://github.com/RayWangQvQ/BiliBiliToolPro) 的 fork。与上游的差异：
>
> - 只保留三种部署方式：自动化面板（青龙/白虎/呆呆）、Docker、本地 Release 包
> - Docker 镜像只推送 GHCR：`ghcr.io/angussleeping/bili_tool_web`
> - 安全加固：登录失败限流与登录表单 antiforgery 校验、Swagger 生产默认关闭、HTTPS 重定向可配置、可配置受信反代 IP、登录 Cookie 显式安全标志
> - 容器内以 `PUID`/`PGID` 非 root 运行，登录签名密钥持久化到 `/app/keys`（重建容器不掉登录态）
> - 恢复 Web/Docker 定时任务的随机延时（`Security:RandomSleepMaxSec`，默认 300 秒，仅定时触发生效）

<p align="center">
  <img src="docs/imgs/web-index.png" alt="Web 面板首页" width="800">
  <br>
  <img src="docs/imgs/web-schedules.png" alt="Web 面板定时计划" width="800">
  <br>
  <img src="docs/imgs/web-schedules-log.png" alt="Web 面板执行日志" width="800">
  <br>
  <img src="docs/imgs/web-configs.png" alt="Web 面板配置页" width="800">
</p>

## 快速开始

任选一种部署方式，启动应用后先运行 **Login（登录）** 任务，扫码添加 Bilibili 账号，然后按需配置任务和定时计划。

| 环境 | 教程 |
| --- | --- |
| 容器 | [Docker](platforms/docker/README.md) |
| 自动化面板 | [青龙](platforms/qinglong/README.md) · [白虎](platforms/baihu/README.md) · [呆呆](platforms/daidai/README.md) |
| 本地或服务器 | [下载 Release 包直接运行](docs/runInLocal.md) |

配置项、环境变量和各任务的详细选项见 [配置说明](docs/configuration.md)；遇到问题先查 [常见问题](docs/questions.md)。

## 任务列表

**任务**是可独立运行的最小单元。部分任务包含多个子活动，只启用你需要的即可。

| 任务 | Console 代码 | 作用 | 建议频率 |
| --- | --- | --- | --- |
| 扫码登录 | `Login` | 添加账号或刷新过期 Cookie | 需要时手动运行 |
| 每日任务 | `Daily` | 登录、观看、分享、投币等每日经验 | 每天一次 |
| 天选时刻抽奖 | `LiveLottery` | 参与符合条件的直播间抽奖（部分需关注主播） | 每天最多四次 |
| 批量取关 | `UnfollowBatched` | 批量取关指定分组中的 Up 主 | 需要时手动运行 |
| 大会员积分 | `VipBigPoint` | 完成符合条件的大会员积分任务 | 每天一次 |
| 粉丝勋章 | `LiveFansMedal` | 执行直播间粉丝勋章相关活动 | 每天一次 |
| 漫画 | `Manga` | 漫画签到与阅读 | 每天一次 |
| 大会员漫画权益 | `MangaPrivilege` | 领取符合条件的漫画权益 | 每天一次 |
| 银瓜子换硬币 | `Silver2Coin` | 将直播银瓜子兑换为硬币 | 每天一次 |
| B 币券充电 | `Charge` | 使用大会员 B 币券为指定 Up 主充电 | 每天一次 |
| 大会员权益 | `VipPrivilege` | 领取符合条件的大会员权益 | 每天一次 |
| Cookie 检测 | `Test` | 检测 Cookie 是否可用 | 需要时手动运行 |

任务的参与资格、奖励和具体时间由 Bilibili 控制，可能随时变化。开关、Cron 表达式、Up 主 ID 等详见 [配置说明](docs/configuration.md)。

## 多账号

再次运行 **Login（登录）** 任务即可添加另一个 Bilibili 账号。Cookie 的存储位置取决于运行方式：

- **Web 面板**：账号在面板中管理，存储于 `config/BiliBiliTool.db`。
- **青龙**：Cookie 以环境变量形式保存，如 `Ray_BiliBiliCookies__0`、`Ray_BiliBiliCookies__1`，以此类推。
- **Console 及其他非 Web 方式**：默认使用本地文件 `cookies.json`：

```json
{
  "BiliBiliCookies": [
    "cookie1",
    "cookie2"
  ]
}
```

**面板管理员**账号与执行任务的 Bilibili 账号是相互独立的。管理员凭据和 Bilibili Cookie 都应作为敏感信息妥善保管。

## 消息推送

可选的执行结果推送支持 Telegram、PushPlus、企业微信、钉钉、Microsoft Teams、Server 酱、酷推以及自定义 API。实际送达取决于服务商和你的配置，设置方法见 [推送配置](docs/configuration.md)。

<p align="center">
  <img src="docs/imgs/push-tg.png" alt="Telegram 推送示例" width="300">
</p>

## 帮助与发布

反馈问题前，请先更新到最新版本，并搜索 [常见问题](docs/questions.md)、[配置说明](docs/configuration.md)、[issues](https://github.com/angussleeping/BiliBiliToolPro/issues) 和 [discussions](https://github.com/angussleeping/BiliBiliToolPro/discussions)。可复现的 bug 或功能请求请开 issue；提问和想法请用 discussions。切勿发布 Cookie、令牌或密码。

已发布的版本见 [releases](https://github.com/angussleeping/BiliBiliToolPro/releases)，进行中的工作见 [issue 列表](https://github.com/angussleeping/BiliBiliToolPro/issues)。合并到 `main` 会产出 alpha 预览镜像，稳定版镜像与 Release 包由发版流程单独发布。

## 致谢与参考

- [Bilibili](https://www.bilibili.com/)
- [SocialSisterYi/bilibili-API-collect](https://github.com/SocialSisterYi/bilibili-API-collect)
- [JunzhouLiu/BILIBILI-HELPER](https://github.com/JunzhouLiu/BILIBILI-HELPER)



