# 料槍維修部門授權 - Design

## Context

**當前狀況：**
- 料槍維修模組已實現帳號級白名單機制（MaterialGunConstants.cs 定義 RepairEditWhitelist HashSet），允許特定帳號（如 02898）編輯已完成維修記錄（Chk=1）
- MaterialGunController 的 IsRepairWhitelisted() 方法目前只檢查帳號
- 系統登入時已從資料庫讀取使用者的部門代碼（UDeptNo），存儲在 MemberViewModels 中
- 系統已存在通用的部門授權過濾器（DepartmentAuthorizationAttribute）用於其他模組（ManpowerAllocationController、ManpowerAuthorizationController）

**使用者流程：**
1. 使用者進入 `/MaterialGun/MaterialGunRepairView`，看到維修清單（分已完成/未完成）
2. 未完成 (Chk=0) 的料槍：所有非 V、FR 角色皆可點擊「料槍維修」進行編輯
3. 已完成 (Chk=1) 的料槍：目前只有帳號在白名單中的人員可編輯，現在要擴展為支援部門授權

**制約條件：**
- 部門授權清單暫時硬編碼在常數中（與帳號白名單平行），無需建表
- 使用 OR 邏輯：帳號在白名單 **或** 部門在授權清單中，即允許編輯
- 前端無需改動（不傳遞部門信息）
- 稽審日誌應記錄授權原因與部門代碼

## Goals / Non-Goals

**Goals：**
- 擴展現有白名單機制支援部門級授權（IT 部門）
- 使用 OR 組合邏輯，帳號白名單和部門授權任一符合即可
- 在稽審日誌中清楚記錄授權原因（帳號 vs 部門）與部門代碼
- 後端驗證強化，同時檢查帳號與部門代碼
- 實作簡潔，為未來完整權限系統預留接口

**Non-Goals：**
- 不建置永久性授權資料表
- 不修改現有角色系統 (ROLE_ID)
- 不改變未完成維修 (Chk=0) 的邏輯
- 不修改前端 View 和 JavaScript（ViewBag.IsWhitelisted 仍為布林值）
- 不實現細粒度授權（如分級編輯權限）

## Decisions

### Decision 1：授權邏輯組合方式 - OR 邏輯

**選擇：** 帳號白名單 **或** 部門授權任一符合即允許

```
IsRepairWhitelisted(userId, userDept) 
  = userId in RepairEditWhitelistUsers 
    OR userDept in RepairEditWhitelistDepts
```

**理由：**
- 簡潔直觀，符合業務意圖
- 帳號白名單和部門授權是獨立的授權維度
- 易於理解與維護

**替代方案考慮：**
- ❌ AND 邏輯：過於嚴格，實際上就是強制限制帳號必須在特定部門
- ❌ 分層邏輯（不同權限等級）：複雜度高，超出當前需求

### Decision 2：部門代碼管理 - 硬編碼常數

**選擇：** 在 `MaterialGunConstants.cs` 新增 `RepairEditWhitelistDepts` HashSet，包含授權部門代碼

```csharp
public static readonly HashSet<string> RepairEditWhitelistDepts = 
    new HashSet<string> { "IT" };
```

**理由：**
- 與帳號白名單平行的結構，保持一致性
- 部門代碼相對穩定，無需動態管理
- 便於版本管理與 Git 追蹤

**替代方案考慮：**
- ❌ 資料庫表：成本高，未來重構需遷移；系統已明確說明暫不建表
- ❌ Web.config：難以類型安全
- ✅ Constants.cs：簡潔、強型態、易於管理

### Decision 3：授權檢查方法簽名 - 支援原因返回

**選擇：** IsRepairWhitelisted() 改為接受 (userId, userDept)，使用元組返回 (bool, string reason)，或新增帶原因的版本

```csharp
// 簡單版
private bool IsRepairWhitelisted(string userId, string userDept)
{
    return MaterialGunConstants.RepairEditWhitelistUsers.Contains(userId)
        || (!string.IsNullOrWhiteSpace(userDept) && 
            MaterialGunConstants.RepairEditWhitelistDepts.Contains(userDept));
}

// 帶原因的版本（便於日誌）
private (bool isWhitelisted, string reason) IsRepairWhitelistedWithReason(string userId, string userDept)
{
    if (MaterialGunConstants.RepairEditWhitelistUsers.Contains(userId))
        return (true, "Account whitelist");
    
    if (!string.IsNullOrWhiteSpace(userDept) && 
        MaterialGunConstants.RepairEditWhitelistDepts.Contains(userDept))
        return (true, "IT department");
    
    return (false, "Not whitelisted");
}
```

**理由：**
- 帶原因的版本便於稽審日誌精確記錄授權來源
- 兩個版本都提供增強了檢查邏輯的完整性

**替代方案考慮：**
- ❌ 只返回布林值：無法區分授權原因，稽審追蹤粗糙
- ✅ 帶原因的版本：最大化稽審價值

### Decision 4：稽審日誌記錄 - 記錄原因與部門代碼

**選擇：** 成功/失敗時都記錄 Serilog，包含 UDeptNo、授權原因、判定邏輯

**成功情況（Information）：**
```
User {UserId} (Dept: {UDeptNo}) authorized to edit repair {Sno} - Reason: {Reason}
Reason 可能是："Account whitelist" 或 "IT department"
```

**失敗情況（Warning）：**
```
User {UserId} (Dept: {UDeptNo}) denied access to repair record {Sno}
```

**理由：**
- 記錄授權原因，便於事後追蹤
- 包含部門代碼，便於組織級別的合規檢查
- 區分成功/失敗便於快速查詢

**替代方案考慮：**
- ❌ 不記錄部門代碼：無法完整追蹤授權信息
- ❌ 不區分原因：難以區分是帳號授權還是部門授權
- ✅ 完整記錄原因與部門：最大化稽審價值

### Decision 5：前端無需改動

**選擇：** ViewBag.IsWhitelisted 仍為簡單布林值，不傳遞部門信息

**理由：**
- 簡化前端邏輯
- 後端完整負責授權決策
- 防止前端繞過授權檢查

**不需要傳遞部門是因為：**
- 前端無法信任使用者提交的部門信息
- 授權決策應由後端唯一負責
- 簡化 JavaScript 邏輯

## Risks / Trade-offs

| 風險 | 影響 | 緩解方案 |
|-----|------|--------|
| **部門代碼拼寫錯誤** | IT 部門授權失效 | 使用常數，避免字串拼寫；可考慮加入測試驗證部門代碼是否存在於系統 |
| **使用者部門代碼為 null** | 無法授權 | IsRepairWhitelisted 應明確處理 null 情況（使用 string.IsNullOrWhiteSpace 檢查） |
| **帳號既在白名單又在 IT 部門** | 重複授權 | 日誌中應優先記錄帳號白名單原因（檢查順序），無功能問題 |
| **未來需要其他部門授權** | 需修改常數代碼 | 常數設計已預留擴展空間，只需在 HashSet 中新增部門代碼 |
| **使用者在 GET 時授權，POST 時權限被移除** | 資料不一致 | 後端 POST 應再次驗證白名單（已實現），防止權限變更後繞過 |

## Migration Plan

**部署步驟：**
1. 修改 MaterialGunConstants.cs：新增 RepairEditWhitelistDepts
2. 修改 MaterialGunController.IsRepairWhitelisted()：擴展簽名、加入部門檢查邏輯
3. 修改 MaterialGunRepairView()：取得 userDept，傳遞給白名單檢查
4. 修改 MaterialGunRepair() POST：傳遞 userDept，改進稽審日誌
5. 編譯驗證：無編譯錯誤
6. 功能測試：白名單帳號、IT 部門使用者、非授權使用者的各種場景

**回滾方案：**
- 回滾 MaterialGunConstants.cs：移除 RepairEditWhitelistDepts
- 回滾 MaterialGunController：恢復舊的 IsRepairWhitelisted(userId) 簽名
- 影響：IT 部門人員將無法編輯已完成維修記錄（回歸舊邏輯）
- 無資料遷移風險（常數改動）

## Open Questions

1. **未來是否會支援其他部門？** 
   - 如支援，部門代碼的統一編碼標準是什麼？
   - 建議：與組織部門編碼保持一致

2. **IT 部門內是否有進一步的細分授權？**
   - 如「IT-QA 可編輯，但 IT-Dev 不可編輯」
   - 當前不支援，可作為未來擴展

3. **Serilog 輸出目標確認？**
   - 當前假設系統已配置 Serilog（文件/ApplicationInsights）
   - 需驗證日誌是否正確輸出
