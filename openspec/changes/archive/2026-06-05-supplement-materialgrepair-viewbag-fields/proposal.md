# Proposal: 補齊後端 ViewBag 欄位傳遞

## Why

編輯已完修的料槍維修記錄時（白名單允許），前端無法顯示三個欄位的舊值：Other（其他原因）、ChangeItemName（更換部品名稱）、ChangeItemNo（更換部品料號）。導致用戶無法看到之前輸入的內容，違反編輯表單的預期行為。

根本原因：後端 `MaterialGunController.MaterialGunRepair(GET)` 方法僅傳送 5 個 ViewBag（Eno、Classic、Results），未傳送這三個欄位的值。

## What Changes

- **後端**：在 `MaterialGunController.MaterialGunRepair(GET)` 方法中，添加三個 ViewBag 賦值：
  - `ViewBag.Other = result.Other`
  - `ViewBag.ChangeItemName = result.ChangeItemName`
  - `ViewBag.ChangeItemNo = result.ChangeItemNo`

- **前端**：在 `MaterialGunRepair.cshtml` 中：
  - 初始化三個 JavaScript 變數，從 ViewBag 取得舊值
  - 綁定三個 input 欄位的初始值（`value="@ViewBag.XXX"`）

## Capabilities

### New Capabilities
（無）

### Modified Capabilities
- `maintenance-result-selection`：補齊 Other 欄位的資料流（後端傳值 → 前端初始化 → 用戶編輯 → POST 回傳）

## Impact

- **後端**：`MaterialGunController.cs`（GET 方法，第 433-437 行附近）
- **前端**：`MaterialGunRepair.cshtml`（初始化部分 + 欄位綁定部分）
- **資料庫**：無改動
- **API**：無改動
- **向後相容**：完全相容，只是補齊遺漏的資料傳遞
