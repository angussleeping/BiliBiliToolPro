## 4.2.6
- 修复[vip-big-point]：receive/v2 补齐 App 端请求字段，解决 6007000 无法领取的问题
- 文档：README 增加发布徽章与 GHCR 镜像徽章
- 维护：回写 CHANGELOG（4.2.5）(#4)
## 4.2.5
- 修复：修复定时任务随机延时未生效并加固登录/反代/镜像构建
- 文档：README 移除贡献与支持章节
- 文档：README 翻译为中文并同步 fork 状态
## 4.2.4
- 基于上游最新代码重建 fork：只保留三种部署方式、镜像仅推 GHCR、同步上游全部版本 tag
- Web 安全加固：登录失败限流、Swagger 生产默认关闭、HTTPS 重定向可配置、DataProtection 密钥持久化到 /app/keys、容器内非 root 运行
## 4.2.3
- 新增[天选抽奖]：按粉丝牌 504/1432 预筛天选，直播列表改用 API v3 (#1092)
- 修复：view 接口改用 WBI 签名，解决 -412（request was banned）(#1239)
- 修复[发布]：去掉 body_path，避免 release notes 重复列出同一批 PR (#1235)
- 依赖：coverlet.collector 升级 6.0.4 → 10.1.0 (#1199)
- 维护：bunit 1.x 迁移到 2.x API (#1126)
- 依赖：Swashbuckle.AspNetCore 升级 8.1.4 → 10.2.3 (#1200)
- 依赖：xunit.runner.visualstudio 升级 3.1.5 → 4.0.0 (#1201)
- 维护[CI]：归档腾讯云 SCF 部署工作流 (#1236)
- 维护：回写 CHANGELOG（4.2.2）(#1233)
## 4.2.2
- 修复：Cookie 检查因超时竞态误报「登录失败」(#1231)
- 测试：IApiApi 测试合并进 ApiApiTests (#1230)
- 维护：回写 CHANGELOG（4.2.1）(#1229)
## 4.2.1
- 新增[Web]：优化账号登录与今日任务页 (#1226)
- 新增[Web]：关于页新增赞助区块 (#1224)
- 修复[青龙]：加固安装与更新流程 (#1216)
- 测试：新增 IGaiaApi 功能测试，修正 LiveApiTests 命名空间 (#1223)
- 维护[CI]：恢复 GitHub 自动生成的 release notes (#1222)
- 维护：回写 CHANGELOG（4.2.0）(#1221)
## 4.2.0
- 新增：登记 Web 设备指纹，解决分享报 -403 (#1214)
- 新增[每日任务]：投币支持配置账号等级阈值 (#1207)
- 新增：改进任务配置、粉丝牌名单与后台通知 (#1202)
- 新增[Web]：重做管理员账号页，改用户名与改密码拆成两个操作 (#1187)
- 修复：账号探测时容忍格式异常的 Cookie 条目 (#1197)
- 修复：风控拒绝时跳过分享与投币的前置检查 (#1215)
- 修复：修复青龙多 SDK 版本输出导致的 .NET 环境误判 (#1210)
- 修复：修正升级预估的取整和硬币保留值计算 (#1211)
- 修复[Web]：启动 Quartz 前先恢复已保存的任务开关 (#1204)
- 修复[Web]：今日任务状态尊重大会员权益开关 (#1203)
- 修复：独立部署的登录页与标题焦点问题 (#1191)
- 修复：未授权登录链接与本地 SDK 配置 (#1189)
- 重构：Cookie 过期与任务失败提醒统一走通知管道 (#1219)
- 维护：删除未使用的 moderate-comments 工作流 (#1218)
- 测试：统一测试套件与断言风格 (#1194)
- 文档：README 与部署指南改写为英文 (#1193)
- 维护：Web 面板不再依赖 cookies.json (#1192)
- 新增[Web]：刷新面板视觉设计 (#1190)
- 依赖：softprops/action-gh-release 升级 2 → 3 (#1163)
- 重构[CI]：合并 PR 检查并明确 Actions 命名 (#1188)
- 维护：回写 CHANGELOG（4.1.1）(#1186)
## 4.1.1
- 修复[Web]：发布 Blazor 框架脚本，面板不再失去交互 (#1184)
- 修复[Web]：版本号固定在侧栏底部 (#1183)
- 修复[Web]：窄视口下侧栏不再遮挡内容 (#1182)
- 维护：回写 CHANGELOG（4.1.0）(#1181)
## 4.1.0
- 新增[Web]：全站 MudBlazor 化、首页 UI 重做与暗色模式 (#1179)
- 新增[Web]：今日任务页 UI 重做，并补上侧边栏缺失的图标 (#1178)
- 新增[Web]：面板展示应用版本 (#1176)
- 修复：changelog job 不再尝试 auto-merge，改为开到 PR 等维护者合并（#1159） (#1175)
- 依赖：Scrutor 升级 6.1.0 → 7.0.0 (#1169)
- 维护[CI]：镜像构建改为宿主机架构交叉编译，不再用 QEMU 模拟 arm64 (#1177)
- 维护：回写 CHANGELOG（4.0.8）(#1174)
## 4.0.8
- 修复：changelog 回写改走 auto-merge PR 流，适配 main 的 ruleset 保护（#1159） (#1170)
- 维护[#1153]：发布策略重构——CI 托管版本号与单主干 alpha/stable 通道 (#1160)
- 重构：删除 platforms 的 dev 先行版变体脚本（#1159） (#1172)
- 依赖：docker/setup-qemu-action 升级 3 → 4 (#1165)
- 依赖：docker/setup-buildx-action 升级 3 → 4 (#1164)
- 依赖：actions/checkout 升级 2 → 7 (#1162)
- 文档：version-next.sh 头注登记 CI-skip 标记导致 alpha 断号的坑 (#1171)
- 依赖：actions/setup-dotnet 升级 4 → 6 (#1161)
## 4.0.7
- 修复[#1144]：Web 面板启动失败改为以非 0 退出码结束（此前恒为 0），Docker / 青龙等编排层不再把「启动崩溃」当成「正常退出」；退出码由运行时决定、不保证跨平台一致（Linux 134，Windows `0xE0434352`）
- 修复[#1147][#1149]：补齐 #1138 迁移到 `platforms/` 后仍遗留的旧路径（两轮）：呆呆/白虎的 `copyshfile.sh` 钩子与白虎任务脚本定位仓库根少一级、青龙 `bili_task_tryFix.sh` 找不到 dotnet 安装脚本、docker/podman 构建脚本的上下文多退一级，另有 5 个平台 README 共 30 处 `../docs/` 断链
## 4.0.6
- **不兼容变更[#1138]**: 部署平台目录统一迁移至 `platforms/` 下（`docker`、`podman`、`qinglong`、`baihu`、`daidai`、`helm`、`tencentScf`、`gitHubActions`、`krew`）；按 `raw.githubusercontent.com/.../main/<旧路径>` 直接拉取脚本或示例文件的存量部署会 404，请改用 `platforms/<平台>/...`
- 修复[#1140]：修复 Web 面板启动时因模型与迁移不一致（`PendingModelChangesWarning`）而直接退出的问题；首次启动会自动为 `QRTZ_TRIGGERS`/`QRTZ_FIRED_TRIGGERS` 补列并创建 `QRTZ_PAUSED_JOB_GRPS` 表
- 修复[#1133]：触发器进入 ERROR 状态时面板立刻刷新，补齐 Quartz 4 的 `OnTriggerInError`/`OnTriggersInError` 回调，不再要等下一轮轮询才发现
- 维护[#1133]：升级到 Quartz.NET 4.1.1；作业存储的 System.Text.Json 实现 4.x 已并入 `Quartz` 核心，故删去独立的 `Quartz.Serialization.SystemTextJson` 引用
- 维护[#1125][#1131]：依赖批量升级，含 MudBlazor 8.6.0→9.10.0、AppAny.Quartz.EntityFrameworkCore.Migrations.SQLite 0.6.0→0.6.1、Serilog、QRCoder、CronExpressionDescriptor、Ray.Infrastructure、bunit、xunit 等
- 维护[#1139][#1142]：统一仓库文本文件换行符为 LF（`.bat`/`.cmd` 保留 CRLF），`.gitattributes` 只留兜底与例外声明，`.editorconfig` 去掉与 CSharpier 互相拉扯的 `insert_final_newline = false`；预览镜像触发路径收窄为仅工作流或配置文件变更
## 4.0.5
- 新增[#1106]：Web 新增「今日任务」页面：逐账号列出每个任务今天该不该做、做了没有，并提供单项/整账号/全部补做
- 新增[#1106]：漏做的任务可自动补做，间隔与记录保留天数由 `AutoRecoverConfig` 配置（默认每 2 小时检查一次，文档见 `docs/configuration.md`）
- 新增[#1106]：Web 界面汉化（导航、首页、账号页、计划任务、日志/历史对话框、登录、修改密码、错误页）
- 修复[#1106]：账号页与今日任务页首屏不再同步请求 B 站，改为本地数据先渲染、B 站状态随后并发补齐，并加 60 秒缓存与单账号超时
- 修复[#1106]：账号页 Cookie 只显示截断值，不再把完整 Cookie 写进页面 DOM
- 修复[#1106]：WBI 签名判定误用 `w_rid`（该字段为空时被 Refit 从查询串丢弃，导致签名从未生效），改用必然存在的 `wts` 判定
- 修复[#1106]：今日任务的到点计算改为按传入时刻的时区求值，不再依赖宿主机时区（原先在 UTC 机器上判为「本日无需执行」）
- 重构：GitHub Actions
- 维护：新增 Dependabot（github-actions + nuget，目标分支 develop）
## 4.0.2
- 升级到dotnet10
- 修复[#1104]：WebApiClientCore 迁移到 Refit 后，参数首字母变为大写导致调用失败，现统一还原，并补充回归测试
- 新增：响应解析失败时输出可定位的诊断日志，Cookie 等凭据一律掩码，避免进入日志与推送
- 新增[#1087]：适配呆呆面板（Daidai Panel）
- 修复：Bili Account 页面的增/改/删/排序写配置时，修正之前误用的环境变量式键名（`BiliBiliCookies__N`）
- 修复：Bili Account 页面的保存不再静默失败，成功/失败均给出 Snackbar 提示
## 4.0.1
- 新增：Bili账号管理页面
- 重构：Web
- 维护：整合 Agent Dtos
- 维护：整合 Agent interfaces
- 维护：WebApiClientCore迁移为Refit
- 重构：AppService
- 维护：升级bruno到v3
- 新增：bruno实现APP的sign签名算法和Web的WBI签名算法
- 重构：由 GSD 完成重构
- 新增[#1078]：适配白虎面板
## 3.8.2
- 修复[#1026]: 更新文档
## 3.8.1
- 修复[#1005]: 更新文档
## 3.8.0
- 新增: 使推送的更版本号信息更简洁
- 修复[#998]: 修复企业微信 App 推送缺少 access_token 问题
- 修复[#996]: 修复 Server 酱推送标题不能为空的问题
- 修复[#669]: 企业微信默认消息类型从 markdown 改为 text
## 3.7.0
- 修复[#989]: 修复钉钉推送标题不能为空的问题
## 3.6.0
- 新增[#961]: Web 项目新增推送功能
- 维护[#961]: 升级 Serilog Pkg
- 维护: 使用中心化包管理
## 3.5.0
- 新增[#924]: 新增 Sqlite 配置源
- 新增[#924]: 新增在线配置页
- 新增[#924]: 根据任务拆分配置
- 新增[#924]: 实现开启、关闭任务功能
- 新增[#924]: 实现修改 Cron 定时时间功能
- 新增: 定时任务页改为默认50条
- 文档: 更新配置文档
## 3.4.0
- 新增: 优化登录失败时的提示信息
- 文档: 更新推送的文档说明
## 3.3.0
- 新增[#935]: Web 新增登录功能
- 新增[#935]: Web 新增修改密码功能
- 文档[#935]: 更新文档
- 维护: 更新开源协议为 GNU GPLv3
- 重构: 拆分原本的 Daily 任务
- 文档: 更新文档
- 维护: 升级 csharpier
- 维护: 变更默认数据库文件位置到 /app/config 下
## 3.2.0
- 修复: 修复大会员大积分签到任务
- 修复: 修复大会员大积分的签到和浏览追番频道任务
- 新增[#901]: 实现大会员大积分的浏览影视频道页任务
- 新增[#921]: 新增大会员大积分的观看剧集 bruno 信息
- 维护: 鉴权不再兼容老版本青龙（老版本需要手动添加 bili cookie）
- 修复: 修复 warnings
- 维护: 移除无用的using
- 修复: 修复 VerifyPR CI/CD 流水线
- 文档: README 添加 Trending 信息
## 3.1.0
- 新增[#842]: 对接青龙新的 OpenAPI，实现青龙版 Bili 登录后自动存储 Cookie
- 新增[#842]: 兼容老版本青龙的文件鉴权方式
- 文档[#842]: 新增新版青龙添加鉴权的说明文档
- 文档[#820]: 更新文档解决配置文件边缘场景下的刷新问题
- 修复[#863]: 修复青龙尝试修复异常任务
- 修复[#879]: 移除文档内过期的加速器地址
- 新增: 开启 Nullable 特性，在编译阶段检查潜在的 NullReferenceException 问题
- 维护: 临时取消 Null warning
- 修复: Bruno 脚本错误
- 修复: 尝试修复发布包时 CI/CD 丢失 change log 问题
## 3.0.0
- 新增[#884]: 上线 bili_tool_web
- 修复[#875]: 青龙检测 dotnet 版本只需要大于等于 8.0
- 修复[#876]: 升级 VipBigPoint 接口，解决风控
- 修复[#881]: 升级 LiveLottery 接口，解决风控
## 2.2.2
- 重构：代码重构
- 维护：接入 Husky.Net 与 CSharpier
## 2.2.1
- 修复[#847]：dotnet 8 下 DefaultRequestHeaders 不能为 null 或空
- 修复[#849]：临时关闭 PublishTrimmed
## 2.2.0
- 维护：从 dotnet 6.0 升级到 dotnet 8.0
- 维护：引入 Bruno 记录接口
- 修复[#824]：青龙保存环境变量失败时打印 Cookie
- 修复[#648]：青龙里设置 DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1，规避偶发的 Couldn't find a valid ICU package
## 2.1.3
- 重构：代码重构
- 修复[#791]：修复VipBigPoint任务异常导致终止的问题
## 2.1.2
- 维护：增强 CI/CD 脚本
- 修复[#728]：兼容青龙历史版本
## 2.1.1
- 新增：启动即监听 Ctrl+C
- 修复：修复青龙读取 cron 报错
## 2.1.0
- 新增[#691]: 重构并优化基于qinglong的部署方式，尝试解决偶发的安装失败的问题
- 新增[#670]: 新增针对App的AppUserAgent配置项，用于解决大会员大积分异常问题
- 修复: 修复CICD发布脚本错误
## 2.0.5
- 修复[#260]: 再次尝试修复大会员大积分“账号风险”异常
## 2.0.4
- 修复: 尝试修复大会员大积分“账号风险”异常
- 新增：为agent api创建集成测试
## 2.0.3
- 新增[#641]：实现浏览会员购页面与观看正片内容功能
- 新增[#685]：部分修复大积分功能
- 修复：更新过于老旧的UserAgent
- 修复：更新排行榜api
## 2.0.2
- 新增[#617]：增加专栏投币功能与领取大会员经验的功能
## 2.0.1
- 文档[#539]：更新文档
- 修复[#557]：修复直播接口权限不足问题
## 2.0.0
- 新增[#513]：将基础组件和抽象迁移到nuget包中
- 修复[#543]：修复部分Api调用报“访问权限不足”
## 1.0.3
- 修复[#486]：发布 zip 打包报错
## 1.0.2
- 修复[#484]：读取字典配置报错
- 合并 PR #472：Telegram 推送支持反向代理地址
- 合并 PR #483：更新 entrypoint 的登录字段
## 1.0.1
- 修复[#463]：不再信任用户提交的 ck 配置
- 新增[#460]：发版时发布单文件版本
- 维护：Actions 发版改用新脚本
- 新增[#473]：配置里没有目标任务时提示用户输入
## 1.0.0
- 新增：启用异步
- 修复[#344]：支持 `Ctrl + C` 触发退出事件
- 修复[#451]：重建 Cookie 工厂模式，修复投币 bug
- 维护：AOP 从 MethodBoundaryAspect.Fody 换成 Rougamo.Fody，修复异步异常
- 合并 PR #448：修正文案错字
- 修复[#446]：id 类型由 int 改为 long
## 0.4.6
- 修复：ck 列表初始化空值报错
- 新增[#440]：青龙改用 `apk add` 安装 dotnet
## 0.4.5
- 修复[#423]：int 改为 string，避免溢出异常
## 0.4.4
- 修复[#228]：尝试修复分享视频报错
- 维护：默认镜像仓库由 Docker Hub 改为 GitHub
## 0.4.3
- 新增[#419]：提供 Docker 一键安装脚本
- 新增[#396]：镜像发布到 GitHub Packages
## 0.4.2
- 合并 PR #425 #426 #427：Docker 相关增强，感谢 @zclkkk
## 0.4.1
- 合并 PR #418：修复搜索视频接口报错，感谢 @catlair
## 0.4.0
- 合并PR（ #381 #383 ），新增直播间挂机功能，感谢@bakapiano
## 0.3.2
- 修复( #358 )，获取auth时兼容老版青龙文件路径
- 修复( #364 )，兼容青龙异形response数据类型
- 修复( #366 #361 )，修复一些低级bug
- 新增( #359 )，兼容读取不到`$QL_DIR`的情况
## 0.3.1
- 修复( #260 )，在需要的时候encode cookie
- 更新文档
## 0.3.0
- 修复：docker build 报错（hotfix）
- 合并PR（#341），新增krew部署，感谢@chenliu1993
- 合并PR（##348），更新文档，感谢@jexjws
- 合并PR（#350），修改请求header错误的bug，感谢@catlair
- 合并PR（#353），新增python扫码登录的feature（仅针对青龙），感谢@AFUL1991
- 新增（#351）：重构并新增了扫码登录功能，使之适用于各种部署平台
## 0.2.2
- 新增`podman`部署教程
- 合并PR（#264），腾讯云定时任务补充新增的大会员大积分任务，感谢@layui0320
- 合并PR（#262），更新docker的entry.sh，感谢@syrinka
- 合并PR（#308 #312），新增Chart部署，感谢@chenliu1993
- 合并PR（#309）新增lv6后开启白嫖模式的配置（多账号时可以实现不足lv6的继续投币，达到lv6的开始白嫖），感谢@cluom
- 优化青龙安装dotnet的脚本，改为使用官方`dotnet-install.sh`脚本安装（之前测试网络不通，后发现--no-cdn可以）
- 优化青龙的执行脚本，提取公共部分，并且在执行前会尝试安装一次dotnet，会清理一次缓存
## 0.2.1
- 合并PR（#253、#257），更新文档（@layui0320）
- 合并PR（#256），重构docker运行是cron构建方式，并优化读取环境变量的方式（@syrinka）
- 新增(#65)：新增TG推送配置并使用代理功能
- 新增(#240)：新增gotify推送
- 新增(#259)：大会员状态改为枚举类型，当非会员时自动跳过大积分任务
- 更新、优化docker部署文档
## 0.2.0
- 新增大会员大积分任务
## 0.1.2
- 修复`auto-close-pr.yml`分支错误的bug
- 【#107】新增自动检测并关闭长时无状态issues的actions：no-response.yml
- 【#73】【#105】【#108】更新、纠正文档内容
- 维护：HostConfiguration 删除 CommandLine 配置源，推荐只使用环境变量，同时更新青龙 shell 脚本内配置
- 【#169】领取大会员福利任务更改为每日都尝试执行
- 修复：青龙拉库兼容大小写问题
- 【#197】合并PR，新增了阅读漫画功能到每日任务中（@ChanceLuo）
## 0.1.1
- 【#54】优化青龙shell脚本读取仓库目录方式，解决青龙新老版本切换导致出现多个repo目录的bug
- 【#82】【#85】合并外部PR，更新了文档
- 感谢`JetBrain`提供免费的证书支持
## 0.1.0
- 【#62】`codeql-analysis.yml`可以指定检查的文件类型
- 【#61】`publish-image.yml`手动打镜像时支持指定是否打latest的tag
- 【#32】新增企业微信的应用推送，实现微信接受推送消息
- 优化日志格式
## 0.0.9
- 【#47】青龙安装`dotnet`环境，支持arm架构服务器
## 0.0.8
- 【#55】新增日志推送端：`Microsoft Teams`
- 【#27】更新README
## 0.0.7
- 【#44】兼容青龙最新版本（v2.12.0），修复因青龙调整目录结构导致的bug
- 更新`publish-image.yml`，只有`release`时才打`latest tag`，手动运行时不打`latest tag`
## 0.0.6
- 更新docker镜像的构建
- 【#12】新增配置`Notification:IsSingleAccountSingleNotify`，支持开启每个账号单独推送消息
- publish-release.yml新增手动输入tag功能
## 0.0.5
- 优化推送日志，在标题中显示运行的任务名称
- 新增`CodeQL`workflows，用于检测代码
- 新增`Publish image`workflows，用于发布镜像
- 新增`no-toxic-comments.yml`，用于检测评论
- 更新`auto-close-pr.yml`，用于修正PR的目标到`develop`
## 0.0.4
- 【#15】修复`Actions`部署到腾讯云函数时的偶发异常
## 0.0.3
- 【#16】修复银瓜子兑换硬币bug
- 【#18】修改[青龙面板](https://github.com/whyour/qinglong)以`Production`环境运行
- [青龙面板](https://github.com/whyour/qinglong)新增拉取dev先行版功能
## 0.0.2
- 更新文档
- 天选抽奖新增黑名单功能
- 批量取关新增白名单功能
## 0.0.1
- 重启项目
- 支持[青龙面板](https://github.com/whyour/qinglong)部署
