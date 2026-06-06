## ADDED Requirements

### Requirement: Database Transaction Protection for Repair Operations

The system SHALL wrap all critical repair and maintenance operations (ForRepairWork, RepairWork, MaterialGunCreate) in a database transaction to ensure atomicity and consistency. If any step fails, all changes SHALL be rolled back automatically.

#### Scenario: ForRepairWork succeeds with transaction
- **WHEN** a user submits repair work creation with valid material gun serial number, bad description, and remarks
- **THEN** the system wraps the operation in a database transaction, creates the repair record, and commits successfully

#### Scenario: ForRepairWork fails and rolls back
- **WHEN** a database error occurs during ForRepairWork execution (e.g., constraint violation)
- **THEN** the system rolls back the entire transaction, no partial records are saved, and user receives an error message

#### Scenario: RepairWork succeeds with transaction
- **WHEN** a user updates repair result, classification, maintenance result, and other repair data
- **THEN** the system wraps the update in a database transaction, updates all affected fields, and commits successfully

#### Scenario: RepairWork fails and rolls back
- **WHEN** an error occurs during RepairWork (e.g., invalid maintenance result code)
- **THEN** the system rolls back all changes, no partial updates occur, and system logs the failure

#### Scenario: MaterialGunCreate succeeds with transaction
- **WHEN** a user creates a new material gun record with serial number, trade, size, and maintenance cycle
- **THEN** the system wraps the creation in a transaction, saves the record, and commits successfully

#### Scenario: MaterialGunCreate fails and rolls back
- **WHEN** database insertion fails (e.g., duplicate serial number)
- **THEN** the system rolls back the transaction, no inconsistent records remain, and user receives clear error message

### Requirement: MaintainWork Transaction Protection

The system SHALL wrap MaintainWork operations in database transactions to ensure all maintenance records are created or updated atomically.

#### Scenario: MaintainWork atomic operation
- **WHEN** a user creates a maintenance work record for a material gun
- **THEN** the system uses BeginTransactionAsync, executes all database operations within the transaction, and commits atomically

#### Scenario: MaintainWork rollback on error
- **WHEN** an error occurs during MaintainWork execution
- **THEN** all database changes are rolled back and no partial maintenance records persist

### Requirement: DiscardWork Transaction Protection

The system SHALL wrap discard and discard confirmation operations in database transactions.

#### Scenario: DiscardWork succeeds atomically
- **WHEN** a user marks a material gun as discarded with discard reason and remarks
- **THEN** the system executes all discard-related updates within a single transaction and commits successfully

#### Scenario: DiscardWork fails and reverts
- **WHEN** database error occurs during DiscardWork
- **THEN** the system rolls back all discard-related changes and maintains data consistency
