# FSM-CSharp 项目配置

## 项目概述
这是一个有限状态机 (FSM) 的 C# 项目，项目旨在提供高效、灵活的状态转换管理，适合嵌入到复杂的行为驱动设计中。以下为详细配置信息。

## 语言规范
- Task 描述和注释说明：中文
- 所有代码（变量名、函数名、类名、docstring）：英文
- Git commit message：英文，遵循 Conventional Commits
- CLI 用户面向的输出信息：英文（面向国际用户）或中文（仅内部使用）
- 文件名和路径：仅使用 ASCII 字符

## Claude Code 配置
当前项目已集成 Claude Code 并实现以下配置：
- **配置文件路径：** `./claude_config/claude-config.json`
- **主要语言：** C#
- **边界测试策略：** 
  - 输入边界检测（`input_edge_cases`）
  - 状态转换边界（`state_transitions`）
  - 异常行为处理（`unexpected_behaviors`）
- **输出目录：**
  - 日志文件：`./logs/`
  - 错误报告：`./reports/errors/`

## 自动生成的内容
- 测试用例（覆盖路径：`src/` 和 `fsm/`）
- 自动化文档（输出至：`./docs/generated/`）。