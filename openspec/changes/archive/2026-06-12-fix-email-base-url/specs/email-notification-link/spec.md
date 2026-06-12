## ADDED Requirements

### Requirement: Email 通知連結使用設定檔管理的 Base URL

系統 SHALL 從 `Web.config` 的 `AppSettings["QuoteScheduleBaseUrl"]` 讀取 Base URL，
並將其套用至所有 QuoteSchedule 通知信件的連結。

若設定值不存在，系統 SHALL fallback 至 `http://192.168.4.133:8004`，確保服務不中斷。

#### Scenario: 設定值存在時，連結使用設定值

- **WHEN** `Web.config` 中 `QuoteScheduleBaseUrl` 已設定為 `http://192.168.4.133:8004`
- **THEN** 所有 QuoteSchedule 通知信件（報價安排數超過通知、修改報價通知、取消報價通知、IE報價完成通知）的連結 MUST 以 `http://192.168.4.133:8004` 為開頭

#### Scenario: 設定值不存在時，使用 fallback

- **WHEN** `Web.config` 中 `QuoteScheduleBaseUrl` 未設定或為空
- **THEN** 系統 MUST 使用 fallback `http://192.168.4.133:8004` 產生連結，不拋出例外
