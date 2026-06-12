## 1. 修復 GET 查詢（MemberMaintain.Edit(int fId)）

- [x] 1.1 將 `ES_MemberRole` 的 INNER JOIN 改為 LEFT JOIN（`into roleJoin` + `from b in roleJoin.DefaultIfEmpty()`）
- [x] 1.2 將 `ES_RoleClassification` 的 INNER JOIN 改為 LEFT JOIN（`into roleDescJoin` + `from c in roleDescJoin.DefaultIfEmpty()`）
- [x] 1.3 將 `ES_MemberFunction` 的 INNER JOIN 改為 LEFT JOIN（`into funcJoin` + `from e in funcJoin.DefaultIfEmpty()`）
- [x] 1.4 將 `ES_FunctionItem` 的 INNER JOIN 改為 LEFT JOIN（`into funcItemJoin` + `from d in funcItemJoin.DefaultIfEmpty()`）
- [x] 1.5 修正 select 子句中 `Func = d.FunctionNo` 為 `Func = d != null ? d.FunctionNo : null`

## 2. 修復 POST 儲存（MemberMaintain.Edit(string fUserId, ...)）

- [x] 2.1 在 `member.fName = fName` 前加入 null check：`if (member == null) throw new Exception($"找不到人員：{fUserId}");`
- [x] 2.2 將 `member_Role` 的更新邏輯改為 upsert：null 時 `new ES_MemberRole { USER_ID = fUserId }` 並 `db.ES_MemberRole.Add(member_Role)`
- [x] 2.3 移除原有冗餘的 `member_Role.USER_ID = fUserId` 賦值（已由 upsert 初始化處理）
- [x] 2.4 將 `foreach (var item in fItem)` 改為 `foreach (var item in fItem ?? new List<string>())`

## 3. 驗證

- [x] 3.1 以無 Role 記錄的人員瀏覽 Edit_Member 頁，確認 fUserId、fName 正確顯示（非空）
- [x] 3.2 以無 Function 記錄的人員瀏覽 Edit_Member 頁，確認頁面正常載入、無 checkbox 被預選
- [x] 3.3 提交表單確認儲存成功、導向 MemberMaintain 不出現 500
- [x] 3.4 以有完整資料的人員重複上述操作，確認現行行為不受影響
