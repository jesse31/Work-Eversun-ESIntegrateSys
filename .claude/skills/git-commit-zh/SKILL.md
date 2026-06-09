---
name: git-commit-zh
description: 讀取 git staged changes 並產生正體中文 (zh-tw) Conventional Commits 格式的 commit message。當使用者要求產生 commit message、撰寫提交訊息、整理 git 變更、或提到「commit」「提交」「git 變更」時觸發此 skill。
---

# Git Commit 正體中文產生器

讀取 git staged changes，分析變更內容，產生符合 Conventional Commits 規範的正體中文 commit message。

## 工作流程

### 1. 讀取 staged changes

```bash
git diff --staged
```

若無 staged changes，提示使用者先執行 `git add`。

同時執行 `git diff --staged --stat` 取得變更檔案摘要，用於判斷 scope。

### 2. 分析變更內容

根據 diff 內容判斷：
- 新增了什麼功能或檔案
- 修改了什麼邏輯或行為
- 刪除了什麼程式碼或檔案
- 變更的影響範圍

### 3. 判斷 type

根據變更性質選擇 Conventional Commits type。完整的 type 說明與範例見 [references/conventional-commits-zh.md](references/conventional-commits-zh.md)。

快速判斷指引：
- 新功能 → `feat`
- 修復問題 → `fix`
- 只改文件/註解 → `docs`
- 格式調整不影響邏輯 → `style`
- 重組程式碼不改行為 → `refactor`
- 效能改善 → `perf`
- 測試相關 → `test`
- 建置/CI 設定 → `build` / `ci`
- 其他雜務 → `chore`

### 4. 推斷 scope

根據變更檔案路徑推斷 scope：
- 單一模組/目錄 → 使用該模組名稱，如 `feat(auth):`
- 跨多個模組 → 省略 scope，如 `refactor: 統一錯誤處理邏輯`
- 常見 scope：`api`、`ui`、`db`、`auth`、`config`、`deps`

### 5. 撰寫 commit message

格式：
```
<type>(<scope>): <subject>
```

規則：
- subject 使用正體中文，簡明扼要
- 使用祈使語氣（「新增」而非「新增了」）
- header 不超過 72 字元，不以句號結尾
- 複雜變更加上 body 說明「為什麼」做這個變更
- 有破壞性變更時在 type 後加 `!` 並在 footer 加 `BREAKING CHANGE:`
- 關聯 issue 用 `Refs: #123` 或 `Closes: #456`

正體中文術語請參考 [references/conventional-commits-zh.md](references/conventional-commits-zh.md) 中的術語對照表。

### 6. 輸出

以 code block 輸出完整的 commit message，方便使用者複製使用。

若變更涵蓋多個不相關的主題，建議使用者拆分為多個 commit，並分別提供各 commit message。
