# AutoTest

AutoTest 是一个基于 WinForms + C# .NET + Playwright 的自动化测试桌面工具，用于对目标网站执行测试并输出可追溯结果。

## 当前进度

- WinForms 主界面：`URL`、`Username`、`Password`、用例 Excel 路径
- Playwright smoke：`Run Smoke` 打开目标 URL
- 用例步骤执行：`Import` 后勾选用例，点 `Run Cases` 按 `Steps` 列逐步执行
- 失败取证：截图 + 可选视频（`Enable video on failure`）
- 本地结果：`artifacts/results.jsonl`（含 smoke 与用例运行记录）
- 本地报告：每次 `Run Cases` 结束后自动生成 HTML；也可用 `Export Report` 另存

## 技术栈

- WinForms (.NET 9)
- C#
- Microsoft.Playwright
- ClosedXML（Excel）

## 运行方式

1. 进入解决方案目录：`d:\Cursor\AutoTest\AutoTest`
2. 编译运行：`dotnet run --project "AutoTest/AutoTest.csproj"`
3. 首次运行前安装浏览器（PowerShell 7）：`pwsh "AutoTest/bin/Debug/net9.0-windows/playwright.ps1" install`  
   若未安装 `pwsh`，可用 Windows PowerShell 执行同路径下的 `playwright.ps1`。

## 操作流程

1. 填写 `Test URL`（默认 `https://pci.moa.com.au`）
2. `Case File` 指向 `TestCases.xlsx`，点 `Import`
3. 在列表中**勾选**要跑的用例，点 `Run Cases`
4. 运行结束后会提示报告路径（默认在 `artifacts/reports/report_*.html`）
5. 需要另存报告时点 `Export Report`
6. 需要录制脚本时点 `Record Case`（会启动 Playwright codegen，目标地址使用当前 `Test URL`）
7. 在 codegen 里复制脚本后，点 `Save .cs Case` 保存到 `TestCasesCs` 目录
8. 点 `Run Recorded .cs` 可执行 `TestCasesCs` 下所有已保存的 `.cs` 用例（Excel 功能不变）

## TestCases.xlsx 列名（英文）

| 列名 | 说明 |
|------|------|
| `CaseName` | 用例名称 |
| `Description` | 描述 |
| `Steps` | 步骤（见下文 DSL，一行一步） |
| `ExpectedResult` | 预期结果（写入报告） |
| `ActualResult` | 导入时的占位；执行后会用页面标题或错误信息覆盖到结果与报告 |
| `ExecutionStatus` | 导入展示用 |
| `ExecutionTime` | 导入展示用 |
| `Executor` | 导入展示用 |
| `ExecutionResult` | 导入展示用 |

## Steps 列语法（每行一条）

格式：`动词|参数1|参数2...`，参数中可使用占位符：`{url}`、`{username}`、`{password}`。  
以 `#` 开头的行视为注释。若 `Steps` 为空，则等价于只执行 `goto|{url}`。

**Excel 多行**：同一单元格里换行请用 **Alt+Enter**；若写成一行，只会被当成一条非法步骤。

**长 `href` 选择器**：`a[href*="https://..."]` 里未加引号的值含 `:`，在 CSS 里容易无效。可改为 `a[href*="remotelogin.aspx"]` 或 `a[href*="/Web/login/remotelogin.aspx"]`（按页面实际 `href` 调整）。失败时异常会提示 **Step N failed:** 及该行原文。

- `goto|<url>` / `navigate|<url>`：打开 URL（等待 `load` 事件，不用 `networkidle`，避免站点长期有后台请求时一直超时）  
- `click|<selector>`：点击（若匹配多个节点，会选 **第一个可见的**，跳过隐藏菜单里的重复链接）  
- `dblclick|<selector>`：双击（同 `click`，优先可见匹配）  
- `click_force|<selector>`：对 **第一个 DOM 匹配** 强制点击（不等待可见；用于特殊场景）  
- `scroll_into_view|<selector>`：将 **第一个可见匹配** 滚入视口  
- `fill|<selector>|<value>`：填充（多个匹配时取 **第一个可见**；value 中若含 `|`，从第三段起会拼回）  
- `press|<Key>`：按键，如 `Enter`  
- `wait|<ms>`：固定等待毫秒  
- `wait_visible|<selector>`：等待 **至少一个匹配节点变为可见**（同样跳过仅隐藏匹配）  
- `expect_text|<substring>`：断言 body 含文本  
- `expect_title_contains|<substring>`：断言标题包含子串  
- `await ...`：可直接粘贴 Playwright C# 录制语句（每行一条），运行时自动转换为上述步骤并执行  
- `raw_playwright|<base64>`：兼容模式；把 Playwright C# 录制脚本（base64）自动转换为上述步骤并执行  
  - 支持：`GotoAsync`、`GetByRole(...).ClickAsync/FillAsync`、`GetByLabel(...).ClickAsync/FillAsync`、`Keyboard.PressAsync`、`WaitForTimeoutAsync`  
  - `GotoAsync("https://login.microsoftonline.com/...")` 会被自动忽略，避免写死 OAuth 跳转地址  

示例（单元格内多行）：

```text
goto|{url}
wait_visible|input[name="username"]
fill|input[name="username"]|{username}
fill|input[name="password"]|{password}
click|button[type="submit"]
expect_title_contains|Dashboard
```

`raw_playwright` 示例：

```text
raw_playwright|YXdhaXQgcGFnZS5Hb3RvQXN5bmMoImh0dHBzOi8vcGNpLm1vYS5jb20uYXUvIik7CmF3YWl0IHBhZ2UuR2V0QnlSb2xlKEFyaWFSb2xlLkxpbmssIG5ldygpIHsgTmFtZSA9ICJMT0dJTiIgfSkuQ2xpY2tBc3luYygpOwphd2FpdCBwYWdlLkdldEJ5Um9sZShBcmlhUm9sZS5UZXh0Ym94LCBuZXcoKSB7IE5hbWUgPSAiV29yayBlbWFpbCIgfSkuRmlsbEFzeW5jKCJ7dXNlcm5hbWV9Iik7CmF3YWl0IHBhZ2UuR2V0QnlSb2xlKEFyaWFSb2xlLkJ1dHRvbiwgbmV3KCkgeyBOYW1lID0gIlNpZ24gaW4iIH0pLkNsaWNrQXN5bmMoKTs=
```

直接粘贴录制语句示例：

```text
await page.GotoAsync("https://moa.com.au/?returnurl=https%3A%2F%2Fpci.moa.com.au");
await page.GetByRole(AriaRole.Link, new() { Name = "LOGIN" }).ClickAsync();
```

## Run log（第二个列表）

- **Clear log**：清空界面上的运行记录，并删除本地 `results.jsonl` 历史
- **列头排序**：点击列标题切换升序/降序；再次点击同一列反转顺序
- **复制**：先单击某一列格子选中行并记录列，再 **Ctrl+C** 复制该格内容；右键 **Copy cell** / **Copy row**（整行 Tab 分隔，便于粘贴 Excel）
- **Reports folder**：打开 `artifacts/reports` 目录（若不存在会自动创建）

## 输出目录

- `artifacts/<runId>/`：单次 smoke 证据
- `artifacts/batch_<时间戳>/<用例名>/`：用例运行证据（失败时含 `failure.png`、`video/`）
- `artifacts/reports/report_*.html`：自动生成的 HTML 报告
- `artifacts/results.jsonl`：运行历史（JSON 每行一条）
- `TestCasesCs/*.cs`：录制并保存的代码用例文件

## 下一步（可选）

- 更丰富的步骤类型（下拉、断言 URL 等）
- 报告内嵌截图缩略图
