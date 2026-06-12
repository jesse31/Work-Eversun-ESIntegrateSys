## Why

`EmailController.Woatt()` 中所有 QuoteSchedule 通知信件的連結 Base URL 為 hardcode 舊伺服器位址（`192.168.4.70`），伺服器已遷移至 `192.168.4.133:8004`。透過 Web.config 管理 Base URL，日後更換伺服器無需修改程式碼。

## What Changes

- 在 `Web.config` 的 `<appSettings>` 新增：
  `<add key="QuoteScheduleBaseUrl" value="http://192.168.4.133:8004" />`
- 在 `EmailController.Woatt()` 中，以
  `ConfigurationManager.AppSettings["QuoteScheduleBaseUrl"]`
  取代 5 處 hardcode 的 `http://192.168.4.70`

## Capabilities

### New Capabilities

（無）

### Modified Capabilities

（無，純實作層面的設定值管理調整，無 spec-level 需求變更）

## Impact

- **新增**：`ESIntegrateSys/Web.config` — `<appSettings>` 新增 1 個 key
- **修改**：`ESIntegrateSys/Controllers/EmailController.cs`
  — 第 90、98、107、112、121 行（5 處替換）
- **不影響**：商業邏輯、email 寄送流程、收件人清單、信件主旨
