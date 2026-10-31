#!/usr/bin/env bash
# 新发布策略（ADR-0002）的 release notes / CHANGELOG 段落生成器。
# 输入是自上个稳定 tag 以来合并的 PR 标题（squash 提交的标题行），
# 按类型前缀分类排序输出；PR 标题不做强制校验，不合规的原样收录进末尾，绝不中断发版。
#
# 语言：条目语言即提交标题语言，因此主干上的标题约定为中文（见 AGENTS.md），
# 生成的 CHANGELOG 与 GitHub Release 正文才是中文。机器人（Dependabot、
# changelog 回写）的标题是固定英文模板，由 localize 做确定性改写；其余英文
# 标题原样保留——宁可留着英文，也不做不可靠的机器翻译。
#
#   notes [--since <tag>] [--from-file <f>]   -> "- <标题>" 条目列表
#   section <版本> [--since <tag>] [--from-file <f>] -> "## <版本>" + 空行 + 条目
#
# 默认 --since 为最新稳定 tag（version-next.sh latest）；--from-file 是测试 seam。
set -euo pipefail

repo_dir=${VERSION_REPO_DIR:-$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)}

die() {
    echo "$1" >&2
    exit 1
}

# 分类优先级：BREAKING > Feature > Fix > 维护 > 其他。
# 判定前剥掉前导 **（历史 BREAKING 条目用粗体包裹），但输出保留原文。
# 中英双前缀都认：标题约定为中文，历史条目与依赖机器人仍可能是英文。
rank_of() {
    local t=$1
    t=$(sed -E 's/^\*+//; s/^[[:space:]]+//' <<< "$t")
    case "$t" in
    BREAKING* | breaking* | 不兼容* | 破坏*) echo 0 ;;
    Feature* | feature* | feat* | 新增* | 功能*) echo 1 ;;
    Fix* | fix* | 修复*) echo 2 ;;
    维护* | 依赖* | 重构* | 文档* | 测试*) echo 3 ;;
    *) echo 4 ;;
    esac
}

# 机器人标题的确定性中文改写。只覆盖固定模板，不做"猜翻译"：
#   chore(deps): bump <pkg> from <a> to <b>  ->  依赖：<pkg> 升级 <a> → <b>
#   Bump the <group> group with <n> updates  ->  依赖：<group> 分组升级（<n> 项）
#   docs(changelog): <版本>                  ->  维护：回写 CHANGELOG（<版本>）
# 未命中的标题原样返回。
localize() {
    sed -E \
        -e 's/^(chore\(deps(-dev)?\):[[:space:]]*)?[Bb]ump (.+) from ([^[:space:]]+) to ([^[:space:]]+)( \(#[0-9]+\))$/依赖：\3 升级 \4 → \5\6/' \
        -e 's/^(chore\(deps(-dev)?\):[[:space:]]*)?[Bb]ump (.+) from ([^[:space:]]+) to ([^[:space:]]+)$/依赖：\3 升级 \4 → \5/' \
        -e 's/^[Bb]ump the (.+) group with ([0-9]+) updates?( \(#[0-9]+\))?$/依赖：\1 分组升级（\2 项）\3/' \
        -e 's/^docs\(changelog\): (.*) (\(#[0-9]+\))$/维护：回写 CHANGELOG（\1）\2/' \
        -e 's/^docs\(changelog\): (.*)$/维护：回写 CHANGELOG（\1）/'
}

# 稳定排序：按 rank 分类，同类内保持输入（合并）顺序
sort_titles() {
    local i=0 t
    while IFS= read -r t; do
        [ -n "$t" ] || continue
        echo "$(rank_of "$t") $i $t"
        i=$((i + 1))
    done | sort -s -n -k1,1 -k2,2 | cut -d' ' -f3-
}

read_titles() { # <since> <from_file>
    if [ -n "$2" ]; then
        cat "$2"
    elif [ -n "$1" ]; then
        git -C "$repo_dir" log --pretty=%s "$1..HEAD"
    else
        git -C "$repo_dir" log --pretty=%s
    fi
}

latest_stable() {
    bash "$repo_dir/scripts/version-next.sh" latest
}

entries() { # <since> <from_file>：条目列表；空列表报错
    local since=$1 from_file=$2 titles
    titles=$(read_titles "$since" "$from_file")
    [ -n "${titles// /}" ] || die "自 ${since:-仓库起点} 以来没有合并的 PR，拒绝发空版本"
    # 先改写机器人模板再排序，分类才看得见改写后的中文前缀
    titles=$(printf '%s\n' "$titles" | localize)
    sort_titles <<< "$titles" | sed 's/^/- /'
}

cmd=${1:-help}
shift || true
case "$cmd" in
notes | section)
    version=""
    if [ "$cmd" = section ]; then
        version=${1:-}
        [ -n "$version" ] || die "section 需要版本号参数"
        shift
    fi
    since=""
    from_file=""
    while [ $# -gt 0 ]; do
        case "$1" in
        --since)
            since=$2
            shift 2
            ;;
        --from-file)
            from_file=$2
            shift 2
            ;;
        *) die "未知参数: $1" ;;
        esac
    done
    [ -n "$from_file" ] || since=${since:-$(latest_stable)}
    body=$(entries "$since" "$from_file")
    if [ "$cmd" = section ]; then
        # 与仓库现有 CHANGELOG 风格一致：二级标题后直接跟条目，不空行
        printf '## %s\n%s\n' "$version" "$body"
    else
        echo "$body"
    fi
    ;;
*)
    cat <<EOF
用法：$0 {notes|section <版本>} [--since <tag>] [--from-file <文件>]
  notes    输出分类排序后的 "- <标题>" 条目
  section  输出 CHANGELOG 段落（## <版本> + 条目）
  --since     从该 tag 到 HEAD 的提交标题（缺省：最新稳定 tag）
  --from-file 从文件读标题列表（测试 seam，优先级高于 --since）
环境变量 VERSION_REPO_DIR 可指定目标 git 仓库。
EOF
    ;;
esac
