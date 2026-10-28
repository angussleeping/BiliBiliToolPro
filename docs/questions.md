# 常见问题

**[目录]**
<!-- TOC depthFrom:2 -->

- [1. 运行出现异常怎么办？](#1-运行出现异常怎么办)
- [2. 如何提交issue（如何提交Bug或建议）](#2-如何提交issue如何提交bug或建议)
- [3. 我 Fork 之后怎么同步原作者的更新内容？](#3-我-fork-之后怎么同步原作者的更新内容)
    - [3.1. 方法一：使用 Pull App 同步](#31-方法一使用-pull-app-同步)
        - [3.1.1. Pull App 方式一：源作者内容直接覆盖自己内容](#311-pull-app-方式一源作者内容直接覆盖自己内容)
        - [3.1.2. Pull App 方式二：保留自己内容](#312-pull-app-方式二保留自己内容)
    - [3.2. 方法二：删掉自己的仓库再重新 Fork](#32-方法二删掉自己的仓库再重新-fork)
- [4. 本地或服务器如何安装.net环境](#4-本地或服务器如何安装net环境)
- [5. 如何停用运行](#5-如何停用运行)

<!-- /TOC -->

## 1. 运行出现异常怎么办？
第一步：根据异常信息，请先仔细阅读文档（特别是 [常见问题文档](questions.md) 和 [配置说明文档](configuration.md) ），查找相关信息

第二步：如果文档没有找到，请到 [issues](https://github.com/angussleeping/BiliBiliToolPro/issues) 下面查找相关问题，看是否有人其他人也遇到类似问题，并确认issue下是否已经有解决方案

第三步：如果仍没有解决，请将日志输出等级配置为Debug，该等级会输出详细的日志信息，修改后请再次运行，并查看详细的日志信息。如何配置请详见 [配置说明文档](configuration.md)

第四步：拿到详细日志后，如果自己无法根据日志信息确定问题，请将日志信息贴到讨论群里，群里会有大佬及时帮忙解答

第五步：如果根据详细日志信息可以确认是 Bug（缺陷），可以到 [issues](https://github.com/angussleeping/BiliBiliToolPro/issues) 下新建一条 issue 。如何新建issue请见下面的常见问题中的**如何提交issue**，如果是不符合要求的issue，会被关闭，严重的会被删除。

## 2. 如何提交issue（如何提交Bug或建议）
issues 被 GitHub 译为**议题**，用来为开源项目反馈 Bug、提出建议、讨论设计与需求等。

首先先提前感谢所有提交议题的朋友们，你们的反馈和建议会让开源程序优化的越来越好。

但为了使 issues 下面的议题便于维护，便于其他人搜索查找历史议题，避免淹没在一堆无用或重复的 issues 里，请大家自觉遵守下面的提交规范：

Ⅰ. 提交前请先确认自己的议题是否是新的议题（是否在文档中已有说明、是否已经有其他人提过类似的议题），重复议题会被标记为重复并关闭，严重的会被删除

Ⅱ. issue标题请填写完整，语义需清晰，以便在不点击进入详情时，仅根据标题就可以定位到该 issue 所反应的问题

Ⅲ. 如果是提交 bug ，请描述清楚问题，**标明版本号、环境，并贴上详细日志信息（Debug等级的日志信息）**。如果获取Debug等级的日志信息请参见配置说明文档，如果没有日志信息，或日志信息不是Debug等级的日志信息，或在没有日志的情况下描述也不清晰，导致无法复现或无法定位问题，该 issue 会被标记为不清晰，且会被忽略或关闭，严重的会被删除。

## 3. 我 Fork 之后怎么同步原作者的更新内容？
Fork 被 GitHub 译为复刻，相当于拷贝了一份源作者的代码到自己的 Repository （仓库）里，Fork 后，源作者更新自己的代码内容（比如发新的版本），一般情况下 Fork 的项目并不会自动更新源作者的修改。

> 注意：本仓库已移除了自动同步上游的 `repo-sync` 工作流，避免上游更新覆盖本地对部署方式的定制。如需同步，请手动操作并自行解决冲突。

这里提供两种同步更新的方法：

### 3.1. 方法一：使用 Pull App 同步
需要安装 [![](https://prod.download/pull-18h-svg) Pull app](https://github.com/apps/pull) 插件。

安装过程中会让你选择要选择那一种方式;

`All repositories`表示同步已经 frok 的仓库以及未来 fork 的仓库；

`Only select repositories`表示仅选择要自己需要同步的仓库，其他 fork 的仓库不会被同步。

根据自己需求选择，实在不知道怎么选择，就选 `All repositories`。

点击 `install`，完成安装。

![Install Pull App](https://cdn.jsdelivr.net/gh/Ryanjiena/BiliBiliTool.Docs@main/imgs/install_pull_app.png)

Pull App 可以指定是否保留自己已经修改的内容，分为下面两种方式，如果你不知道他们的区别，就请选择方式一；如果你知道他们的区别，并且懂得如何解决 git 冲突，可根据需求自由选择任一方式：

#### 3.1.1. Pull App 方式一：源作者内容直接覆盖自己内容
> 该方式会将源作者的内容直接强制覆盖到自己的仓库中，也就是不会保留自己已经修改过的内容。
步骤如下：

Ⅰ. 确认已安装 [![](https://prod.download/pull-18h-svg) Pull app](https://github.com/apps/pull) 插件。

Ⅱ. 编辑 [pull.yml](https://github.com/angussleeping/BiliBiliToolPro/blob/main/.github/pull.yml) 文件，将第 5 行内容修改为 `mergeMethod: hardreset`，然后保存提交。

（默认就是hardreset，如果未修改过，可以不用再次提交）

完成后，上游代码更新后 pull 插件会自动发起 PR 更新**覆盖**自己仓库的代码！

当然也可以立即手动触发同步：`https://pull.git.ci/process/${owner}/${repo}`

#### 3.1.2. Pull App 方式二：保留自己内容

> 该方式会在上游代码更新后，判断上游更新内容和自己分支代码是否存在冲突，如果有冲突则需要自己手动合并解决（也就是不会直接强制直接覆盖）。如果上游代码更新涉及 workflow 里的文件内容改动，这时也需要自己手动合并解决。

步骤如下：

Ⅰ. 确认已安装 [![](https://prod.download/pull-18h-svg) Pull app](https://github.com/apps/pull) 插件。

Ⅱ. 编辑 [pull.yml](https://github.com/angussleeping/BiliBiliToolPro/blob/main/.github/pull.yml) 文件，将第 5 行内容修改为 `mergeMethod: merge`，然后保存提交。

完成后，上游代码更新后 pull 插件就会自动发起 PR 更新自己分支代码！只是如果存在冲突，需要自己手动去合并解决冲突。

当然也可以立即手动触发同步：`https://pull.git.ci/process/${owner}/${repo}`

### 3.2. 方法二：删掉自己的仓库再重新 Fork
这是最最最保守的方法，删掉后重新Fork会导致之前配置和提交的代码更改全部丢掉，只能重新部署。
所以，请把该方法放到保底的位置，即如果你已经尝试了同步方法都还不能成功，再保底考虑使用该方法。

## 4. 本地或服务器如何安装.net环境

请见[官方文档](https://learn.microsoft.com/zh-cn/dotnet/core/tools/dotnet-install-script)

## 5. 如何停用运行

推荐做法是使用配置关停应用的每日任务；如果彻底不再使用，停掉对应的容器 / 面板定时任务即可。

详情见 [配置说明文档](https://github.com/RayWangQvQ/BiliBiliTool.Docs/blob/main/configuration.md#321-isskipdailytask%E6%98%AF%E5%90%A6%E8%B7%B3%E8%BF%87%E6%89%A7%E8%A1%8C%E4%BB%BB%E5%8A%A1)。

该方法是在应用层面关闭每日任务，程序仍会按计划运行，只是进入程序后不会去执行每日任务，即不会调用任何接口。如果配置了推送，每天仍能收到推送消息。
