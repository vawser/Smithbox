# Smithbox 本地化工具脚本

用于维护 `src/Smithbox.Data/Assets/Localization/` 下的 JSON 翻译文件。
所有脚本为纯 Python 3 标准库实现，无需安装第三方依赖。

## 文件说明

| 文件 | 用途 |
|---|---|
| `common.py` | 共享工具模块（路径解析、JSON 读写、占位符提取），不直接运行 |
| `compare.py` | 对比目标语言与英文源，输出覆盖率与缺失/未翻译 key 报告 |
| `sync.py` | 把英文中存在、目标语言中缺失的 key 以 `null` Text 占位形式补齐 |
| `validate.py` | 校验 JSON 合法性、key 唯一性、参数槽一致性、空字符串错误 |

## Text 值语义

C# 加载器（`Localization.cs`）在 `Load()` 时会**跳过 Text 为 null 的条目**，
使 `Get()` 在查询时 `TryGetValue` 失败，自动回退到英文 fallback。
利用这一机制，`sync.py` 用 `null` 标记未翻译的 key：

| Text 值 | 含义 | UI 显示 | 翻译者操作 |
|---|---|---|---|
| `null` | 未处理 | 英文（自动回退） | 翻译成中文，或确认保留英文 |
| `"中文"` | 已翻译 | 中文 | 已完成 |
| `"English"` | 已确认保留英文 | 英文 | 已处理，无需改动 |
| `""`（空字符串） | **错误** | 空白（不回退） | 改成 `null` |

> **重要**：空字符串 `""` 不会触发回退（`TryGetValue` 返回 `true`），
> 会导致 UI 显示空白。`validate.py` 会把空字符串报为 ERROR。
> 未翻译的 key 必须用 `null`，不要用空字符串。

## 运行方式

在 `scripts/localization/` 目录下运行（脚本会自动定位仓库根目录）：

```bash
cd scripts/localization

# 查看中文覆盖率（默认对比 Chinese）
python compare.py

# 指定语言名（Languages.json 里的 Name）或文件夹名
python compare.py Chinese
python compare.py English

# 输出 CSV 报告 + 列出所有未翻译/缺失 key
python compare.py Chinese --csv report.csv --missing

# 把英文中存在、中文里缺失的 key 以 null Text 补到中文文件中（先 dry-run 预览）
python sync.py Chinese --dry-run
python sync.py Chinese

# 同时清理中文里多余的 key（英文中已不存在的）
python sync.py Chinese --remove-extra

# 校验所有语言
python validate.py

# 只校验中文；--strict 把警告也当错误（CI 用）
python validate.py Chinese
python validate.py --strict
```

## 典型翻译流程

1. **同步占位**：`python sync.py Chinese` —— 把所有缺失 key 以 `null` Text 补进中文文件。
2. **翻译**：逐个打开 `Chinese/*.json`，把 `"Text": null` 改成 `"Text": "中文译文"`。
   - 对于应保留英文的 key（如 Vulkan、OpenGL），把 `null` 改成英文原文字符串，
     表示"已确认保留英文"。
3. **校验**：`python validate.py Chinese` —— 检查参数槽、重复 key、空字符串错误。
4. **查看进度**：`python compare.py Chinese` —— 看 Done/Null/Miss 三列，覆盖率提升。
5. 重复 2-4，直到 `validate.py` 零错误、`compare.py` 覆盖率 100%。

## 翻译规范要点

- **保持 Key 不变**，只翻译 `Text`。
- **参数槽与英文完全一致**：`{0}`、`{1}`… 数量必须相同，位置可调整以符合中文语序。
  > 注意：`readme.txt` 里写的 `{1}..{10}` 是文档错误，实现使用 `string.Format`，
  > 实际是 0-based `{0}` 起。以英文源文件中的占位符为准。
- **`_TT` 后缀的 key** 是 tooltip，译成简短说明句，不要译成按钮文案。
- **专有名词保留原文**：Vulkan、OpenGL、ImGui、FMG、MSB、FLVER、Havok 等，
  把 `null` 改成英文原文字符串即可。
- **文件格式**：UTF-8 无 BOM、CRLF 行尾、2 空格缩进、无尾换行。
  `sync.py` 写出的文件已遵循此格式，手动编辑时请保持。

## 退出码

- `validate.py`：有 error（或 `--strict` 下有 warning）时退出码为 1，否则 0。可直接用于 CI。
- `compare.py` / `sync.py`：成功完成返回 0。
