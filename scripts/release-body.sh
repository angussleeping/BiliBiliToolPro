#!/usr/bin/env bash
# 生成 GitHub Release 正文（中文），供 publish-stable-release.yml 的 body_path 使用。
#
# 为什么不再用 GitHub 的 generate_release_notes：
#   1) 它只输出英文模板（What's Changed / New Contributors），发布页永远是英文；
#   2) 与自定义正文叠加时会把同一批 PR 列两遍（4.2.1/4.2.2 的教训）。
# 正文改由本脚本合成：条目与 CHANGELOG 段落同源（都走 release-notes.sh），
# 因此发布页与 CHANGELOG 永远一致，语言也随提交标题（中文约定见 AGENTS.md）。
#
#   release-body.sh <版本> [--since <tag>] [--from-file <文件>]
#
# 环境变量（括号内为缺省）：
#   VERSION_REPO_DIR   目标仓库（测试 seam）
#   GITHUB_REPOSITORY  拼 compare 链接（缺省从 origin 推导）
#   RELEASE_IMAGE      正文里的镜像名（ghcr.io/angussleeping/bili_tool_web）
set -euo pipefail

script_dir=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
repo_dir=${VERSION_REPO_DIR:-$(cd "$script_dir/.." && pwd)}

die() {
    echo "$1" >&2
    exit 1
}

version=${1:-}
[ -n "$version" ] || die "用法：$0 <版本> [--since <tag>] [--from-file <文件>]"
shift

since=""
from_file=""
while [ $# -gt 0 ]; do
    case "$1" in
    --since)
        since=${2:-}
        shift 2
        ;;
    --from-file)
        from_file=${2:-}
        shift 2
        ;;
    *) die "未知参数: $1" ;;
    esac
done

# 只把非空值透传下去：--since "" 会让生成器回落到"最新稳定 tag"，
# 与"没有比较基准"是两回事（首个版本时 since 为空）。
notes_args=()
if [ -n "$from_file" ]; then
    notes_args=(--from-file "$from_file")
elif [ -n "$since" ]; then
    notes_args=(--since "$since")
fi

# 条目：与 CHANGELOG 同一个生成器（分类排序、机器人标题中文化、空列表拒绝发版）
entries=$(VERSION_REPO_DIR="$repo_dir" bash "$script_dir/release-notes.sh" notes ${notes_args[@]+"${notes_args[@]}"})

slug=${GITHUB_REPOSITORY:-}
if [ -z "$slug" ]; then
    slug=$(git -C "$repo_dir" remote get-url origin 2>/dev/null |
        sed -E 's#(git@|https://)github\.com[:/]##; s#\.git$##' || true)
fi

image=${RELEASE_IMAGE:-ghcr.io/angussleeping/bili_tool_web}

printf '## 更新内容\n\n%s\n\n## 升级方式\n\n```bash\ndocker compose pull && docker compose up -d\n```\n\n镜像：`%s:%s`（amd64 / arm64）\n' \
    "$entries" "$image" "$version"

# 没有比较基准（首个稳定版）或推导不出仓库名时不输出链接，避免半截 URL
if [ -n "$since" ] && [ -n "$slug" ]; then
    printf '\n**完整变更**：[%s...%s](https://github.com/%s/compare/%s...%s)\n' \
        "$since" "$version" "$slug" "$since" "$version"
fi
