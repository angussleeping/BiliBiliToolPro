#!/usr/bin/env bash
# release-body.sh 的 fixture 测试：验证 Release 正文的结构、语言与边界。
# 只测脚本 CLI 的外部行为（正文文本），不看内部实现。
#
# 本地/CI 通用：bash ./scripts/test-release-body.sh
set -euo pipefail

repo_dir=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
sut="$repo_dir/scripts/release-body.sh"

source "$repo_dir/scripts/test-lib.sh"

titles_file=$(mktemp)

# --- 场景 1：正文骨架（标题、条目、升级方式、镜像、compare 链接）---
cat > "$titles_file" <<'EOF'
修复：某个修复 (#1147)
EOF

expected=$(cat <<'EOF'
## 更新内容

- 修复：某个修复 (#1147)

## 升级方式

```bash
docker compose pull && docker compose up -d
```

镜像：`img:4.0.8`（amd64 / arm64）

**完整变更**：[4.0.7...4.0.8](https://github.com/o/r/compare/4.0.7...4.0.8)
EOF
)

assert_eq "正文骨架：更新内容 + 升级方式 + 镜像 + compare 链接" "$expected" \
    "$(GITHUB_REPOSITORY=o/r RELEASE_IMAGE=img bash "$sut" 4.0.8 --since 4.0.7 --from-file "$titles_file")"

# --- 场景 2：没有比较基准（首个版本）时不输出半截 compare 链接 ---
n=$(GITHUB_REPOSITORY=o/r bash "$sut" 1.0.0 --from-file "$titles_file" | grep -c '完整变更' || true)
assert_eq "since 为空时不输出 compare 链接" "0" "$n"

# --- 场景 3：空标题列表直接失败，不发空版本 ---
: > "$titles_file"
assert_exit_nonzero "空条目列表拒绝生成正文" bash "$sut" 4.0.8 --since 4.0.7 --from-file "$titles_file"
err=$(bash "$sut" 4.0.8 --from-file "$titles_file" 2>&1 >/dev/null || true)
assert_contains "空列表错误信息可读" "没有合并的 PR" "$err"

# --- 场景 4：机器人标题被确定性中文化，不再漏英文模板 ---
cat > "$titles_file" <<'EOF'
chore(deps): bump actions/checkout from 2 to 7 (#1162)
Bump the dotnet-major group with 3 updates (#1170)
docs(changelog): 4.0.7 (#1161)
EOF

body=$(bash "$sut" 4.0.8 --since 4.0.7 --from-file "$titles_file")
assert_contains "chore(deps) bump 改写为「依赖：…升级 a → b」" '依赖：actions/checkout 升级 2 → 7 (#1162)' "$body"
assert_contains "分组升级改写为「依赖：…分组升级（n 项）」" '依赖：dotnet-major 分组升级（3 项） (#1170)' "$body"
assert_contains "changelog 回写改写为「维护：回写 CHANGELOG」" '维护：回写 CHANGELOG（4.0.7）' "$body"

# --- 场景 5：中文前缀参与分类排序（不兼容 > 新增 > 修复 > 维护）---
cat > "$titles_file" <<'EOF'
维护：杂项收尾 (#1147)
修复：修了个 bug (#1145)
新增：加了个功能 (#1146)
不兼容：部署目录迁移 (#1140)
EOF

expected='- 不兼容：部署目录迁移 (#1140)
- 新增：加了个功能 (#1146)
- 修复：修了个 bug (#1145)
- 维护：杂项收尾 (#1147)'

body=$(bash "$sut" 4.0.8 --since 4.0.7 --from-file "$titles_file")
entries=$(sed -n '/^## 更新内容$/,/^## 升级方式$/p' <<< "$body" | sed '1d;$d' | sed '/^$/d')
assert_eq "中文前缀按 不兼容/新增/修复/维护 分类排序" "$expected" "$entries"

# --- 场景 6：镜像名可用环境变量覆盖（不同 fork 复用同一脚本）---
assert_contains "RELEASE_IMAGE 覆盖生效" '`ghcr.io/someone/other:9.9.9`' \
    "$(GITHUB_REPOSITORY=o/r RELEASE_IMAGE=ghcr.io/someone/other bash "$sut" 9.9.9 --from-file "$titles_file")"

# 与其它 fixture 测试同一策略：只在 CI 上删临时文件（本地沙箱对 rm 临时路径可能 SIGTERM）
if [ "${CI:-}" = "true" ]; then rm -f "$titles_file" 2>/dev/null || true; fi

finish
