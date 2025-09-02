# TODO / Enhancements

Centralized list of actionable improvements gathered from initial repo review. Use this to track, prioritize, and reference across PRs. See file paths in backticks.

## Documentation
- [ ] Add a "CLI Options" section to `README.md` documenting `--tia-major-version <int>` and `--logging <1|2|3>` with defaults and effect (1=stderr, 2=Debug, 3=Event Log). Cross-link to samples.
- [ ] Add a "Build and Run" section to `README.md` showing `dotnet build`, `dotnet run --project src/TiaMcpServer/TiaMcpServer.csproj`, and running compiled `TiaMcpServer.exe`.
- [ ] Add a "Testing" section to `README.md` summarizing prerequisites (TIA Portal V20, `.NET Framework 4.8`, env var `TiaPortalLocation`, Windows group membership "Siemens TIA Openness"), how to run `dotnet test`, and expected limitations if environment is not present. Link to `tests/TiaMcpServer.Test/README.md` and mention manual multi-user session creation.
- [ ] Cross-link the `samples/` directory from `README.md`; reference `samples/vscode/mcp.json` and `samples/claude/claude_desktop_config.json`.
- [ ] Reduce duplication between `gemini.md` and `src/TiaMcpServer/README.md`: consolidate content or keep one as an overview and link to the other.
- [ ] Expand Known Limitations: Document that as of 2025-09-02, importing Ladder (LAD) blocks from SIMATIC SD documents requires the `.s7res` to contain en-US tags for all items; otherwise import may fail.

## CLI / Logging
- [ ] Update `src/TiaMcpServer/CliOptions.cs` `Logging` comment to match current numeric modes (1=stderr, 2=Debug, 3=Event Log) or switch to string values (e.g., "stdio", "debug", "eventlog"). Align parsing and docs accordingly.
- [ ] In `src/TiaMcpServer/Program.cs`, remove hard-coded `options.Logging = 1;` override so CLI-provided logging is honored. Instead, set default only when not provided.
- [ ] Document logging behavior (destinations, filters, minimum levels) in `README.md` or a dedicated `docs/logging.md` and link it.

## Consistency
- [ ] Standardize naming to "TIA Portal" (no hyphen) across all docs and headings; ensure consistent section titles (e.g., clarify "Copilot Chat" vs. "VS Code").
- [ ] Ensure requirements are consistently listed across docs: `.NET Framework 4.8`, `TIA Portal V20`, env var `TiaPortalLocation`, and Windows group membership.

## Changelog
- [ ] Fix typo in `CHANGELOG.md`: "Narketplace" → "Marketplace".

## Tests
- [ ] In `tests/TiaMcpServer.Test/README.md`, double-check instructions for creating `TestSession1.als20` and referencing paths in `Settings.cs`; link this from the main `README.md` Testing section.
- [ ] Consider documenting how to selectively run tests or skip environment-dependent ones (e.g., via MSTest categories) when TIA is unavailable.

## Housekeeping
- [x] Add a "Contributing" link in `README.md` pointing to `agents.md`.
- [ ] Verify that all fenced code blocks in Markdown include language hints per `style.md` and wrap lines for readability.

## MCP Tools Docs (Export/Import)
- [ ] Create per-tool docs under `docs/tools/`:
  - `docs/tools/export-blocks.md`
  - `docs/tools/import-blocks.md`
  Each should include: Overview, Preconditions, Parameters (names/types/defaults), Order of operations (numbered), Error model, Examples (request/response for MCP), Troubleshooting, Performance/limits. Include a Mermaid sequence diagram for call flow.
- [ ] Define a shared error mapping in `docs/error-model.md` (validation → `InvalidParams`, not found → `NotFound`, Openness API → `OpennessError` with native code; guidance for partial vs. overall failure).
- [ ] Add a "Tools" section to `README.md` linking to `docs/tools/` and `docs/error-model.md`; reference `samples/` configs.
- [ ] Add XML documentation comments to export/import methods in `ModelContextProtocol/McpServer.cs` and corresponding Siemens wrappers (e.g., `Siemens/Portal.cs`, `Siemens/Openness.cs`). Cover summary, pre/postconditions, ordered steps, params/returns, exceptions, thread-safety/cancellation, and `<seealso>` links to tool docs.
- [ ] Enable XML documentation file generation in `src/TiaMcpServer/TiaMcpServer.csproj` (set `DocumentationFile` for `net48`) so IDE tooltips and doc generation work.
- [ ] Add usage recipes under `docs/recipes/` (e.g., export only FBs matching `FB_Prod.*`, import with overwrite/skip, preservePath false) with minimal and full payloads and expected responses.
- [ ] Cross-link: from tool docs to relevant tests in `tests/TiaMcpServer.Test` and from code via `<seealso>` to markdown docs; from README to samples and tool docs.
- [ ] Optional: Evaluate DocFX (or similar) to generate API docs from XML comments; if adopted, add a short `docs/README.md` and build instructions.
- [ ] Optional CI: add markdown linting and doc build validation to the pipeline (skippable locally if TIA isn’t installed).

### Version Gating (Export as Documents)
- [ ] Document that `ExportAsDocuments` and `ExportBlocksAsDocuments` require TIA Portal V20+; update prompts and README accordingly.

## Import From Documents (V20+)
- [ ] Add tests for `ImportFromDocuments`: single import happy path, version gating (<20), invalid `importPath`, invalid `fileNameWithoutExtension`.
- [ ] Add tests for `ImportBlocksFromDocuments`: regex filtering on `.s7dcl`, progress notifications, partial failures aggregation, empty directory behavior.
- [ ] Validate enum mapping for `importOption` (Override/None; extend if environment exposes more values).
- [ ] Verify placement into `groupPath` (root vs. nested groups) and behavior when group does not exist.
- [ ] Add docs pages under `docs/tools/` for import-from-documents tools; include file discovery rules (.s7dcl/.s7res), name derivation, and option mapping.
