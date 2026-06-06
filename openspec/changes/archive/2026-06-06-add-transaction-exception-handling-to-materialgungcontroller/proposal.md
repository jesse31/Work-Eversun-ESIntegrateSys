## Why

MaterialGunController 中的關鍵業務操作（送修、維修、新增料槍、保養等）缺乏交易保護和完善的例外處理，導致資料一致性風險、敏感資訊洩露、以及缺少 Anti-CSRF 防護。這些問題直接威脅系統可靠性與安全性。

## What Changes

- **交易保護**：在 ForRepairWork()、RepairWork()、MaterialGunCreate() 等關鍵業務邏輯中添加 Transaction 包裝，確保多步驟操作的原子性。
- **例外處理**：完善 SaveChanges、API 呼叫的例外捕捉、Serilog 日誌記錄，防止未預期的伺服器錯誤。
- **敏感資訊隱藏**：隱藏 GetGunByBarcode() 等 API 的例外訊息，防止資訊洩露。
- **Anti-CSRF 驗證**：補足 MaterialGunForRepair()、Discard() 等 POST 方法的 [ValidateAntiForgeryToken] 標籤。
- **Null 安全檢查**：強化 Session 存取的 Null 檢查，防止 NullReferenceException。

## Capabilities

### New Capabilities

- `transaction-protected-repair-operations`：在 ForRepairWork、RepairWork 等維修操作中實作 Database Transaction，確保異動的原子性與一致性。
- `exception-handling-and-logging`：建立統一的例外處理與 Serilog 日誌機制，捕捉關鍵操作的失敗與異常情況。
- `csrf-protection-and-safe-exceptions`：補足 Anti-CSRF Token 驗證、隱藏敏感例外訊息、強化 Null 安全檢查。

### Modified Capabilities

（無現有規格被修改）

## Impact

- **受影響的控制器方法**：CreateMaintainWork、MaterialGunForRepair、MaterialGunRepair、MaterialGunCreate、Discard、GetGunByBarcode
- **受影響的服務層**：RepairGun.ForRepairWork、RepairGun.RepairWork、MaintainGun.MaintainWork、MaterialGunInfo.DiscardWork
- **受影響的資料表**：ES_MaterialGunRepair、ES_MaintainWork、ES_MaterialGunInfo、ES_MaterialGunDiscard
- **技術棧**：EF Core Database.BeginTransactionAsync、Serilog 日誌、ASP.NET MVC Anti-CSRF Token
- **安全性增強**：資料一致性保障、異常訊息隱藏、CSRF 防護、Null 安全
