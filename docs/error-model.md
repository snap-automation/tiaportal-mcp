# Error Model and Exception Metadata

This document standardizes how errors are raised in the Siemens portal layer and mapped to MCP responses, and where exception metadata is attached for consistency and observability.

## Principles

- Clear categories
  - Validation: invalid input, missing resources → `PortalErrorCode.InvalidParams` / MCP `InvalidParams`.
  - Invalid state: operation cannot proceed due to project or item state (e.g., inconsistent block/type) → `PortalErrorCode.InvalidState` / MCP `InvalidParams` with guidance.
  - Operation failure: environment/IO/underlying API issues → `PortalErrorCode.ExportFailed` (or similar) / MCP `InternalError` with concise reason.

- Single decoration point
  - Do not attach `Exception.Data` inline at throw sites.
  - Each public portal method (e.g., `ExportBlock`, `ExportType`) attaches standard context keys in a single catch block just before rethrowing, ensuring uniform metadata on all failures:
    - `softwarePath`
    - `blockPath` / `typePath` (as applicable)
    - `exportPath` (as applicable)

- Consistency requirement (TIA Portal)
  - TIA Portal does not export inconsistent blocks/types (`IsConsistent == false`). Single-item exports throw `InvalidState` with a clear message to compile first. Bulk exports skip inconsistent items and report them in a dedicated list.

## Portal Layer Pattern

Within `src/TiaMcpServer/Siemens/Portal.cs` methods:

- Throw lightweight `PortalException` with an appropriate `Code` from locations that detect an error (validation, not-found, invalid state).
- Add context metadata only in the method’s `catch (PortalException)`:
  - `pex.Data["softwarePath"] = ...` etc.
  - Log with structured context.
  - `throw;` to preserve stack.

This ensures consistent context is present even for early-validation failures without duplicating metadata code at multiple sites.

## MCP Mapping

- Map `PortalErrorCode.InvalidParams` and `InvalidState` to MCP `InvalidParams` with user-guidance messages.
- Map `PortalErrorCode.ExportFailed` (and similar) to MCP `InternalError`, include a concise `Reason` from the inner exception, and log full details.
- For `NotFound`, provide suggestions when the input is ambiguous (e.g., single-name block paths).

## Bulk Export Reporting

- Responses for bulk operations include both exported items and a list of inconsistent (skipped) items:
  - `ResponseExportBlocks`: `Items` (exported), `Inconsistent` (skipped)
  - `ResponseExportTypes`: `Items` (exported), `Inconsistent` (skipped)
- `Meta` contains counts for totals, exported, and inconsistent.

