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
> - 修复大会员积分「领取日常任务」返回 `6007000 请将客户端更新至最新版本`：`receive/v2`、`complete/v2` 已改成 App 端接口，请求体补上 `build` 等客户端参数后才被受理（上游 [#1195](https://github.com/RayWangQvQ/BiliBiliToolPro/issues/1195)）
> - 界面精简：关于页去掉上游的赞助卡片，项目信息改为居中单卡，赞助二维码图片不再随镜像与 Release 包分发
> - 直播粉丝勋章容错：B 站任务文案改版时降级执行而不是整条失败、心跳被拒后重新进房换参数、单个粉丝牌失败不再拖垮整条任务
> - 数据库自维护：执行历史保留 21 天、运行日志保留 7 天，超期自动清理（此前运行日志只增不减）

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

### 已知限制：大会员积分「观看剧集内容」

`VipBigPoint` 的「日常6：观看剧集」自 **4.2.7** 起默认**跳过**，既不领取也不上报，避免每天产生一次注定失败的请求。其余 7 项日常任务不受影响。

原因是 B 站把完成环节放在 App 客户端专属接口上，中间那步必须携带 App 端风控票据才会下发观看凭证：

| 步骤 | 接口 | 现状 |
| --- | --- | --- |
| 领取 | `POST /pgc/activity/score/task/receive/v2` | 请求体带上 `build` 等客户端参数后正常受理（4.2.6 修复的正是这里）；4.2.7 起不再调用 |
| 开始观看 | `POST /pgc/activity/deliver/material/receive` | 只对携带 `x-bili-ticket`（App 端风控票据）的请求下发 `watch_count_down_cfg`（含 `task_id` / `token` / 600 秒倒计时） |
| 上报完成 | `POST /pgc/activity/deliver/task/complete` | 依赖上一步的 `task_id` / `token`，且只能成功一次 |

即使带上完整 Cookie、`csrf`、`appkey`/`ts`/`sign` 与全套设备头（`buvid` / `fp_local` / `fp_remote` / `x-bili-mid`），第 2 步仍只返回空壳 `{"closeType":"close_win","container":[],"showTime":""}`，因此无法在服务端复现。该票据由 App 按设备指纹与行为向服务端申请，纯 HTTP 请求无法伪造。

**如何拿到这 40 大积分**：在手机 App 的「会员中心 → 赚大积分」中手动领取「观看剧集内容」，任选一集番剧/国创/电影/电视剧/综艺/纪录片，连续观看满 10 分钟即可。由于工具不再代为领取，这一步需要你自己在 App 内点一次。

### 说明：直播粉丝勋章（LiveFansMedal）

`LiveFansMedal` 依次做三件事：给正在直播的粉丝牌直播间发弹幕、点赞、挂机累计观看时长。这三步都依赖 B 站直播间的粉丝牌任务页面，而页面上的任务文案（「发送弹幕」「点赞30次」）属于展示文案，随时可能改版。

自 **4.3.0** 起这部分做了三处容错：

- 文案里的数量解析不出来时按一轮兜底继续执行，并记一条「任务定义解析降级」告警，不再让整条任务在发出第一条弹幕之前就失败；
- 挂机心跳紧跟心跳间隔发出，被 B 站拒绝（`1012002 time check failed`）时重新进房换一组心跳参数，而不是拿已经过期的参数连续硬撞；
- 单个粉丝牌失败（房间刚下播、灯牌任务被灰度等）只记警告，不再让整条任务判为失败；只有全部粉丝牌都失败才算失败。

需要注意的两点：主播不在播时对应的粉丝牌会被跳过，这是预期行为；同一账号同时挂多个直播间仍可能触发 B 站风控，如果经常看到心跳失败，可以把 `HeartBeatNumber`、`LikeNumber` 调小，或减少挂机的粉丝牌数量。

自 **4.3.1** 起，跳过会写明原因：未开播导致点赞、挂机被跳过时，日志里会出现「未开播，跳过点赞/挂机」的 Information 记录，不再静默。这里顺带说明 B 站的规则，避免误解——**未开播的直播间，点赞和观看时长在服务端一律不计亲密度**，这是服务端按房间开播状态判定的，任何客户端都无法绕过；另外亲密度按直播间独立计算，给一个常播的官方直播间（如央视频）挂机，只涨它自己的牌子，帮不到其他主播的粉丝牌。观看亲密度的每日上限是 30（每 5 分钟 6 点，挂满 25 分钟封顶），挂再久也不会更多。

### 说明：升级预估天数（距升级Lv6）

每次登录都会打印一行 `【距升级Lv6】预计 N 天`，自 **4.3.2** 起紧跟一行 `【预估依据】`，把这个数字是怎么来的写清楚：

```text
【距升级Lv6】预计722天
【预估依据】剩余 20280 经验｜首日 35 分/天（基础 15 + 投币 2×10）｜可投币 222 枚（余额 1372 - 保留 1150）｜硬币只够投 222 天，之后回落到 25 分/天
```

预估只做两件事：算出还差多少经验，再按「以后每天能拿多少经验」去除。每天的经验由两部分组成：

- **基础经验**：登录 5 + 观看 5（`IsWatchVideo`）+ 分享 5（`IsShareVideo`），最高 15 分/天；
- **投币经验**：每枚硬币 10 分，每天最多投 `NumberOfCoins` 枚（上限 5）。可用硬币 = `硬币余额 − NumberOfProtectedCoins`，余额低于保留线时要先攒够差额才开始计投币经验。

投币经验会随着硬币耗尽而消失，于是预估曲线有两段：

| 阶段 | 每日经验 | 说明 |
| --- | --- | --- |
| 硬币还没投完 | `15 + NumberOfCoins × 10` | 登录每天仍进账 1 枚硬币，可以当天投掉 |
| 硬币投完之后 | `15 + 10` = 25 | 只剩登录进账的那一枚 |

**所以升级天数会突然变大**：调大 `NumberOfProtectedCoins`（保留硬币）会把可用硬币压到很小，硬币很快耗尽，预估从「每天 35 分」跌到「每天 25 分」，天数随之跳涨。同一账号、剩余 20280 经验、`NumberOfCoins = 2` 时：

| `NumberOfProtectedCoins` | 可用硬币 | 预计天数 |
| --- | --- | --- |
| 0 | 1372 | 580 |
| 1150 | 222 | 723 |
| 1780 | -408（需先攒 408 天） | 975 |

判断方法很简单：看 `【预估依据】` 里的「可投币 X 枚」。X 接近余额说明配置没问题；X 很小或为负，天数偏大就是保留硬币造成的，与经验进度无关。

4.3.2 同时修掉的两个问题：

- **同一天可能打出两个天数**：预估以前用的是进程启动时缓存的配置，Web 面板改配置后要等重启才一致；现在每次调用都取当前配置，同一账号不会再出现一个任务说 722 天、下一个任务说 580 天。
- **停投后仍算投币经验**：达到 `CoinDonationStopLevel`（或 `SaveCoinsWhenLv6`）设定的等级后工具不再投币，预估也不再计投币经验，此时日志显示「已停投硬币」。

需要留意：大会员签到（5 分）、大积分日常（每天约 40 分）等经验不在模型内，所以 `N` 是**保守下限**，实际升级会比它更快。

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

已发布的版本见 [releases](https://github.com/angussleeping/BiliBiliToolPro/releases)，逐版本的变更明细见 [仓库更新日志](CHANGELOG.md)，进行中的工作见 [issue 列表](https://github.com/angussleeping/BiliBiliToolPro/issues)。合并到 `main` 会产出 alpha 预览镜像，稳定版镜像与 Release 包由发版流程单独发布。

## 致谢与参考

- [Bilibili](https://www.bilibili.com/)
- [SocialSisterYi/bilibili-API-collect](https://github.com/SocialSisterYi/bilibili-API-collect)
- [JunzhouLiu/BILIBILI-HELPER](https://github.com/JunzhouLiu/BILIBILI-HELPER)



