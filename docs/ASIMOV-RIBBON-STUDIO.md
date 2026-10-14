# ASIMOV Ribbon Studio — experimental v0.1

Derived from fernandreu/office-ribbonx-editor under the original MIT license. Preserve LICENSE and THIRD-PARTY-NOTICES.

## Purpose
Extend the original WPF XML editor with a visual preview of tabs, groups, and direct controls.

## Safety rules
- Keep master unchanged until pull request review.
- Baseline backup: backup/master-2026-10-14-pre-asimov at commit d17a1f569ca3b8e98bdc94c6d02cd06e09235549.
- Develop on feature/asimov-ribbon-preview-v0.1.
- Before each future iteration, create a new backup branch from the current feature HEAD.
- Never force-push backups. Recovery uses new branches from saved commit refs.
- Preview only: XML parser never executes callbacks or modifies the Office file.
- Maintain the original Office document save/reload implementation.

## Verification (Windows and .NET 10 SDK)
dotnet restore OfficeRibbonXEditor.slnx
dotnet build OfficeRibbonXEditor.slnx -c Release --no-restore
dotnet test tests/UnitTests/UnitTests.csproj -c Release --no-build

Final Ribbon rendering must be validated in Office; the WPF preview is approximate.

## Preview UI (first slice)

1. Open a customUI or customUI14 XML part from an Office document.
2. Click Ribbon preview in the XML editor toolbar.
3. The right panel displays tabs, groups, and control labels; callback names are tooltips.
4. Edit XML and click Refresh to update. Close hides the panel.

This is a read-only approximation, not yet a drag-and-drop or XML-writing designer.
