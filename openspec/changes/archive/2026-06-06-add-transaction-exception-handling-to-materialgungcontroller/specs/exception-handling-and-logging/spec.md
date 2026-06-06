## ADDED Requirements

### Requirement: Unified Exception Handling with Serilog Logging

The system SHALL catch all exceptions in critical business operations and log them using Serilog with structured context (userId, department, material gun serial number, operation type). User-facing messages SHALL NOT expose technical details.

#### Scenario: SaveChanges exception is caught and logged
- **WHEN** db.SaveChanges() throws an exception (e.g., constraint violation, connection failure)
- **THEN** the system catches the exception, logs it with Serilog including context fields, and returns a generic user message

#### Scenario: API call exception is caught and logged
- **WHEN** an external API call or LINQ query throws an exception
- **THEN** the system catches the exception, logs it with structured context, and returns a generic error message to the user

#### Scenario: Null reference exception is prevented
- **WHEN** accessing Session["Member"] or other potentially null values
- **THEN** the system performs null checking and logs the issue before attempting access

#### Scenario: Exception context includes operation metadata
- **WHEN** any critical operation fails (CreateMaintainWork, MaterialGunForRepair, MaterialGunRepair)
- **THEN** the logged exception includes UserId, UDeptNo, MaterialGunSno, operation name, and timestamp using Serilog ForContext

### Requirement: GetGunByBarcode Exception Safety

The system SHALL catch exceptions in GetGunByBarcode and return a generic error message without exposing internal system details.

#### Scenario: Database query error is masked
- **WHEN** GetGunByBarcode encounters a database connection error or query failure
- **THEN** the system catches the exception, logs it with Serilog, and returns `{ success: false, message: "系統處理發生錯誤，請聯繫管理員" }`

#### Scenario: Invalid barcode handling
- **WHEN** user provides an invalid barcode that causes parsing or validation error
- **THEN** the system catches the exception, logs it, and returns a user-friendly message without technical details

#### Scenario: Null data handling
- **WHEN** query results return null or unexpected data structures
- **THEN** the system safely handles null values and logs the anomaly without throwing unhandled exceptions

### Requirement: Session Access Safety

The system SHALL safely handle Session object access and prevent NullReferenceException.

#### Scenario: Safe Session["Member"] access in CreateMaintainWork
- **WHEN** CreateMaintainWork method executes
- **THEN** the system checks if Session["Member"] exists before accessing properties, returns redirect to login if null

#### Scenario: Safe Session access in MaterialGunRepair
- **WHEN** MaterialGunRepair (POST) method accesses Session member data
- **THEN** the system validates Session["Member"] is not null before extracting userId and department

#### Scenario: Session data fallback
- **WHEN** Session["Member"] is null or corrupted
- **THEN** the system logs a warning and redirects to login page instead of crashing

### Requirement: Comprehensive Action Method Exception Handling

The system SHALL wrap all critical POST actions (CreateMaintainWork, MaterialGunForRepair, MaterialGunRepair, Discard, MaterialGunCreate) with try-catch blocks.

#### Scenario: CreateMaintainWork exception caught
- **WHEN** any error occurs during CreateMaintainWork processing
- **THEN** the system catches it, logs with context, and returns appropriate response (redirect or error message)

#### Scenario: MaterialGunRepair exception caught
- **WHEN** any error occurs during repair data submission
- **THEN** the system catches, logs, and returns JSON error response with generic message

#### Scenario: Discard operation exception caught
- **WHEN** database error occurs during Discard or ManagerCheck
- **THEN** the system catches, logs, and redirects to info view with generic error message
