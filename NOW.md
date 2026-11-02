# Session bookmark

## Doing
Repository orientation and publication of aligned root instructions and this bookmark; awaiting user direction before further implementation.

## State
- Reviewed main at `a9f392303c94913470f1c7dfbfc40e77eb1b8f76`, both README files, all three source/project files, recent commits, and open pull requests on 2026-11-02.
- Neither root AGENTS.md nor CLAUDE.md existed. Added identical files with session-start, bookmark-update, and synchronization rules. No HANDOVER.md, NOW.md, or nested instruction files existed in the inspected tree.
- The repository contains one mod: Subnautica-VRCheatMenu, targeting .NET Framework 4.7.2 with BepInEx 5, SubmersedVR, SteamVR, and local game assemblies. A distributable DLL is tracked under dist/; its correspondence to current source has not been verified.
- All four existing commits are dated 2026-10-06. The latest adds version 1.3.0 toggles and automatic button-file upgrades; earlier commits added 1.2.0 and 1.1.0.
- Confirmed version mismatch: VRCheatMenu.csproj declares 1.3.0, but Plugin.cs BepInPlugin declares 1.0.0.
- MenuConfig.Load replaces files whose default-version marker is missing and overwrites the single .old backup. Customizations require manual copying, as documented; recovery across repeated upgrades is a candidate for later review.
- No open PRs existed before this documentation change. No checked-in test suite, CI configuration, contribution guide, or PR template was found; repository rulesets returned an empty list.
- These three documentation files are published on `docs/session-bookmark-2026-11-02` for review. No application code or binary was changed.

## Next step
Await user direction. Proposed first implementation: align the BepInEx plugin version with project version 1.3.0, then build with the required game/mod assemblies and smoke-test plugin loading in Subnautica VR before replacing the distributed DLL.

## Blocked by
- Further implementation awaits the user's direction.
- Build/runtime validation has not been performed. Required local game/mod assemblies and a VR runtime are not supplied in the repository.
- Shell clone failed because the configured proxy was unreachable; GitHub connector access works and is used for inspection/publication.

## Updated
2026-11-02
