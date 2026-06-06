## ADDED Requirements

### Requirement: Anti-CSRF Token Validation on POST Actions

The system SHALL validate Anti-CSRF tokens on all POST actions that modify data. All POST methods that change server state SHALL be decorated with [ValidateAntiForgeryToken] attribute and matched with @Html.AntiForgeryToken() in corresponding forms.

#### Scenario: CreateMaintainWork validates CSRF token
- **WHEN** CreateMaintainWork (POST) receives a request without valid CSRF token
- **THEN** the system rejects the request and returns HTTP 403 Forbidden

#### Scenario: MaterialGunForRepair validates CSRF token
- **WHEN** MaterialGunForRepair (POST) receives a request without valid CSRF token
- **THEN** the system rejects the request and returns HTTP 403 Forbidden

#### Scenario: Discard validates CSRF token
- **WHEN** Discard (POST) receives a request without valid CSRF token
- **THEN** the system rejects the request and returns HTTP 403 Forbidden

#### Scenario: MaterialGunRepair validates CSRF token
- **WHEN** MaterialGunRepair (POST) receives a request without valid CSRF token
- **THEN** the system rejects the request and returns HTTP 403 Forbidden

#### Scenario: MaterialGunCreate validates CSRF token
- **WHEN** MaterialGunCreate (POST) receives a request without valid CSRF token
- **THEN** the system rejects the request and returns HTTP 403 Forbidden

### Requirement: Safe Exception Messages

The system SHALL hide technical exception details from user-facing error messages. All exceptions caught in API endpoints and action methods SHALL be logged fully but returned to client with generic messages only.

#### Scenario: GetGunByBarcode hides database errors
- **WHEN** GetGunByBarcode catches a SqlException or EntityException
- **THEN** the system logs the full exception but returns `{ success: false, message: "系統處理發生錯誤，請聯繫管理員" }` to client

#### Scenario: API endpoint hides stack traces
- **WHEN** any API endpoint (CheckMaintainStatus, CheckData, BadDesc, etc.) encounters an error
- **THEN** the system logs full stack trace with Serilog but returns generic error message to prevent information disclosure

#### Scenario: User doesn't see database schema information
- **WHEN** a database schema-related error occurs
- **THEN** the system doesn't return error messages that reveal table names, column names, or connection strings

#### Scenario: User doesn't see file system paths
- **WHEN** a file operation error occurs
- **THEN** the system doesn't return error messages that reveal local file paths or system structure

### Requirement: Null Safety Checks

The system SHALL perform null checks before accessing Session, form data, or database query results. All member accesses SHALL be protected with null-coalescing operators or explicit null guards.

#### Scenario: Session["Member"] is checked before use
- **WHEN** any action method accesses Session["Member"]
- **THEN** the system uses null-coalescing (`??`) or explicit null check to prevent NullReferenceException

#### Scenario: Query results are validated
- **WHEN** SingleOrDefault, FirstOrDefault, or Find returns null
- **THEN** the system checks for null before accessing properties and handles gracefully

#### Scenario: Form data is validated for null
- **WHEN** POST action receives null or empty form parameters
- **THEN** the system validates parameters before processing and returns appropriate error message

### Requirement: Audit Logging for Sensitive Operations

The system SHALL log all modifications to completed repair records (Chk=true) with user authorization reason, timestamp, and detailed change information.

#### Scenario: Authorization reason is logged
- **WHEN** an authorized user updates a completed repair record
- **THEN** the system logs the authorization reason (account whitelist, department authorization) for audit trail

#### Scenario: Unauthorized access attempt is logged
- **WHEN** an unauthorized user attempts to update a completed repair record
- **THEN** the system logs a warning with userId, department, material gun serial, and denial reason

#### Scenario: Change details are logged
- **WHEN** any field change occurs in a repair record
- **THEN** the system logs old value and new value for Classification and MaintenanceResult in JSON format
