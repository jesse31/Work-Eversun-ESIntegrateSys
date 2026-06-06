## Context

**Current State:**
- MaterialGunController (1072 lines) handles material gun lifecycle: create, maintain, repair, discard operations
- Critical business operations lack transaction protection: ForRepairWork, RepairWork, MaintainWork, DiscardWork
- Exception handling is inconsistent: some methods lack try-catch, others expose technical details
- Anti-CSRF protection is incomplete: only CreateMaintainWork (line 122) has [ValidateAntiForgeryToken], missing from MaterialGunForRepair, Discard, MaterialGunRepair, MaterialGunCreate
- Session access lacks null safety: direct casting without null checks (line 299: `(Session["Member"] as MemberViewModels).fUserId`)
- Service layer methods (RepairGun, MaintainGun, MaterialGunInfo) in separate assemblies

**Constraints:**
- Must maintain backward compatibility with existing view layer and service contracts
- Cannot refactor entire controller structure; surgical changes only
- Must use existing Serilog configuration already in project
- Database operations use Entity Framework Core with ESIntegrateSysEntities context

**Technical Stack:**
- ASP.NET MVC 5 (System.Web.Mvc)
- Entity Framework Core with EDMX model
- Serilog for structured logging
- NPOI for Excel export

## Goals / Non-Goals

**Goals:**
- Protect critical business operations (repair, maintain, discard) with database transactions ensuring atomicity and rollback on failure
- Implement comprehensive exception handling with Serilog structured logging (userId, department, material gun serial, operation context)
- Hide sensitive exception details from API responses while preserving full stack traces in server logs
- Complete Anti-CSRF token validation on all POST methods that modify server state
- Prevent NullReferenceException by adding null safety checks around Session access and query results
- Enable audit trail for sensitive operations (completed repair record modifications) with authorization reason logging

**Non-Goals:**
- Do NOT refactor entire controller structure or business logic
- Do NOT modify existing service layer contracts (RepairGun, MaintainGun, MaterialGunInfo)
- Do NOT change database schema or entity relationships
- Do NOT implement global exception filter (only action-level handling)
- Do NOT add new external dependencies beyond existing Serilog

## Decisions

### Decision 1: Transaction Implementation Strategy
**Choice:** Use `database.BeginTransactionAsync()` at service layer boundary within controller action methods.

**Rationale:**
- Service methods (ForRepairWork, RepairWork, MaintainWork) already perform multiple DB operations
- Wrapping at controller level (before calling service) keeps transaction scope clear and testable
- Async transaction API matches existing async patterns in codebase (reference: logging at line 345 already uses async context)

**Alternatives Considered:**
1. Wrap transactions in service layer methods — would require service layer refactor (non-goal)
2. Use Entity Framework TransactionScope — less explicit, harder to reason about scope
3. Retry logic with exponential backoff — added complexity without addressing root atomicity issue

**Implementation Pattern:**
```csharp
try
{
    using (var transaction = db.Database.BeginTransaction())
    {
        // Existing service call
        bool success = maintainGun.MaintainWork(itemgun, uId);
        transaction.Commit();
    }
}
catch (Exception ex)
{
    // Exception handling (see Decision 2)
}
```

### Decision 2: Exception Handling and Logging Strategy
**Choice:** Try-catch at each POST action method with Serilog.ForContext() for structured context.

**Rationale:**
- Each action method has distinct context (userId, department, operation type)
- Serilog ForContext allows attaching userId, UDeptNo, MaterialGunSno, operation metadata to all log entries
- Keeps exception handling logic close to business context
- Existing Serilog configuration supports structured logging (reference: line 345 uses ForContext)

**Implementation Pattern:**
```csharp
try
{
    var member = Session["Member"] as MemberViewModels;
    if (member == null) return RedirectToAction("Login", "Home");
    
    using (var transaction = db.Database.BeginTransaction())
    {
        // Business logic
        transaction.Commit();
    }
}
catch (Exception ex)
{
    Log.ForContext("UserId", userId)
       .ForContext("UDeptNo", userDept)
       .ForContext("MaterialGunSno", itemgun)
       .ForContext("Operation", "MaterialGunForRepair")
       .Error(ex, "Operation failed");
    
    TempData["message"] = "系統處理發生錯誤，請聯繫管理員";
    return RedirectToAction(...);
}
```

**Error Message Strategy:**
- All user-facing error messages (TempData, Json responses) shall be generic: "系統處理發生錯誤，請聯繫管理員"
- Full exception stack trace and technical details logged to Serilog only
- Reference: GetGunByBarcode (line 515) currently returns `ex.Message` — will be fixed to generic message

### Decision 3: API Exception Handling (GetGunByBarcode, dropdown endpoints)
**Choice:** Wrap JSON API endpoints in try-catch, return generic error messages, suppress stack traces.

**Rationale:**
- API responses are consumed by JavaScript frontend, no need to expose internal errors
- Serilog logging preserves debugging information server-side
- Prevents information disclosure attacks

**Implementation:**
```csharp
[HttpPost]
public JsonResult GetGunByBarcode(string barcode)
{
    try
    {
        // Existing validation and query logic
        return Json(new { success = true, data = result });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "GetGunByBarcode failed for barcode: {Barcode}", barcode);
        return Json(new { success = false, message = "系統處理發生錯誤，請聯繫管理員", data = null });
    }
}
```

### Decision 4: Anti-CSRF Token Validation
**Choice:** Add [ValidateAntiForgeryToken] attribute to all POST methods that modify data.

**Rationale:**
- ASP.NET MVC built-in anti-CSRF mechanism, minimal code change required
- Already partially implemented (CreateMaintainWork has it, line 121)
- No performance impact, standard security practice

**Methods to Protect:**
- CreateMaintainWork (already has it ✓)
- MaterialGunForRepair (POST, line 293)
- MaterialGunRepair (POST, line 373)
- Discard (POST, line 625)
- MaterialGunCreate (POST, line 693)

**Implementation:** Single-line addition per method:
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult MethodName(...) { ... }
```

### Decision 5: Session Null Safety Pattern
**Choice:** Extract to local variable with null check before use.

**Rationale:**
- Explicit null check prevents NullReferenceException
- Improves readability compared to nested null-coalescing
- Enables early return, reducing nested logic

**Pattern:**
```csharp
var member = Session["Member"] as MemberViewModels;
if (member == null)
{
    Log.Warning("Session["Member"] is null - user not authenticated");
    return RedirectToAction("Login", "Home");
}
string userId = member.fUserId;
string userDept = member.UDeptNo;
```

**Application Points:**
- CreateMaintainWork (line 128)
- MaterialGunForRepair (line 299)
- MaterialGunRepair (line 375)
- MaterialGunCreate (line 695)
- Discard (line 627)

### Decision 6: Database Query Result Null Safety
**Choice:** Explicit null checks after Find, FirstOrDefault, SingleOrDefault operations.

**Rationale:**
- Query operations may return null; accessing properties without checks causes NullReferenceException
- Improves robustness in edge cases

**Application Points:**
- GetGunByBarcode: gunInfo null check (line 467), repairRecord null check (line 479)
- MaterialGunRepair: repairRecord null check (line 338)
- Discard: SingleOrDefault result check (line 612)

## Risks / Trade-offs

| Risk | Mitigation |
|------|-----------|
| **Transaction deadlocks** on high concurrency | Begin with short transaction scopes (test with load); if issues arise, escalate to database index tuning. Current lock mechanism at line 132-153 for duplicate submission prevention remains unchanged. |
| **Serilog performance overhead** from structured logging | Serilog is already in use; ForContext adds minimal overhead. Recommend: monitor log volume in production; adjust log levels if needed. |
| **Backward compatibility with service layer** | Service methods (RepairGun, MaintainGun) remain unchanged. Transaction wrapping happens in controller only. |
| **Generic error messages may reduce debuggability** | Mitigated by comprehensive Serilog logging with structured context. DevOps can monitor logs for patterns. |
| **Missing Anti-CSRF on JsonResult endpoints** | JsonResult endpoints (CheckMaintainStatus, CheckData, BadDesc) are typically called via AJAX with token in headers. Verify front-end sends token; if not, may require separate configuration. Current approach: focus on form-based POST methods first. |
| **Session["Member"] cast ambiguity** | Fixed by explicit null check and early return pattern (Decision 5). |

## Migration Plan

### Phase 1: Foundational Changes (Low Risk)
1. Add null safety checks for Session["Member"] in all action methods
2. Add generic error handling to JSON API endpoints (GetGunByBarcode, BadDesc, Classification, etc.)
3. Add [ValidateAntiForgeryToken] attributes to all POST methods
4. Update Serilog logging configuration to include new context fields

**Verification:** Unit tests for null safety; manual testing of API endpoints with invalid input.

### Phase 2: Transaction Protection (Medium Risk)
1. Wrap ForRepairWork in database transaction (MaterialGunForRepair POST)
2. Wrap RepairWork in database transaction (MaterialGunRepair POST)
3. Wrap MaintainWork in database transaction (CreateMaintainWork POST)
4. Wrap DiscardWork in database transaction (Discard POST, ManagerCheck)
5. Wrap MaterialGunCreate in database transaction (MaterialGunCreate POST)

**Verification:** Integration tests with forced database errors; verify rollback behavior.

### Phase 3: Comprehensive Logging (Low Risk)
1. Add try-catch blocks to all modified action methods
2. Implement structured Serilog logging with ForContext for each operation
3. Verify error logs contain sufficient context for debugging

**Verification:** Production log analysis; trace error scenarios.

### Rollback Strategy
- All changes are additive (new try-catch, new attributes, new context)
- Can be reverted by removing try-catch blocks and [ValidateAntiForgeryToken] attributes
- Database transactions are scoped to single requests; no persistent changes to schema or stored procedures
- Recommend: feature flag for exception message suppression if urgent rollback needed

## Open Questions

1. **Should JsonResult endpoints (CheckMaintainStatus, CheckData, BadDesc) require Anti-CSRF token?** 
   - Current: AJAX endpoints typically protect via header-based token. Verify front-end implementation before mandating.
   - Recommendation: Phase 2 effort if required.

2. **What is the desired log retention policy for Serilog?**
   - Impacts how verbose we can be with structured logging.
   - Recommendation: Align with DevOps/infrastructure team.

3. **Should transaction rollback log entry include "before" and "after" state of affected records?**
   - Current decision: Log at operation level (e.g., "ForRepairWork failed"). More granular audit trail could be Phase 2 enhancement.

4. **What about concurrent requests from same user on same material gun?**
   - Current duplicate submission lock (line 132-153) handles 5-second window. Transaction rollback may need to coordinate with this.
   - Recommendation: Review lock behavior during testing.
