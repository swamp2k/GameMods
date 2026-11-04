# Session bookmark

## Doing
Completed. No further work requested.

## State
- Rechecked repository root, both instruction files on the PR branch, root/project READMEs, source files (`Plugin.cs`, `MenuConfig.cs`, `.csproj`), recent commits, and open PRs on 2026-11-04. Root `main` still lacks AGENTS.md, CLAUDE.md, NOW.md and HANDOVER.md; PR #1 supplies the first three. Its AGENTS.md and CLAUDE.md have matching blob SHAs and required session rules.
- PR #1 is ready to merge and contains the 1.3.0 metadata correction plus synchronized instructions/bookmark. Code remains unbuilt and untested in VR here.
- Reviewed main at `a9f392303c94913470f1c7dfbfc40e77eb1b8f76`, both README files, all three source/project files, recent commits, and open pull requests on 2026-11-02.
- Neither root AGENTS.md nor CLAUDE.md existed. Added identical files with session-start, bookmark-update, and synchronization rules. No HANDOVER.md, NOW.md, or nested instruction files existed in the inspected tree.
- The repository contains one mod: Subnautica-VRCheatMenu, targeting .NET Framework 4.7.2 with BepInEx 5, SubmersedVR, SteamVR, and local game assemblies. A distributable DLL is tracked under dist/; its correspondence to current source has not been verified.
- All four existing commits are dated 2026-10-06. The latest adds version 1.3.0 toggles and automatic button-file upgrades; earlier commits added 1.2.0 and 1.1.0.
- Fixed the confirmed version mismatch: Plugin.cs BepInPlugin now declares 1.3.0, matching VRCheatMenu.csproj. Verified the source change is limited to the version attribute.
- MenuConfig.Load replaces files whose default-version marker is missing and overwrites the single .old backup. Customizations require manual copying, as documented; recovery across repeated upgrades is a candidate for later review.
- No open PRs existed before this documentation change. No checked-in test suite, CI configuration, contribution guide, or PR template was found; repository rulesets returned an empty list.
- The documentation and version metadata fix are published on `docs/session-bookmark-2026-11-02` in PR #1. The distributed DLL has not been rebuilt.

## Next step
done

## Blocked by
None for the requested work. Build and SteamVR runtime validation remain unverified, and the tracked distribution DLL has not been rebuilt; these are accepted as out of scope.

## Updated
2026-11-04
