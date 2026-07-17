# AGENTS.md — Instructions for opencode

## Version bump rule
When the user says "push to github" (or any variant like "push", "upload to github", etc.):
1. Read `version.txt` to get the current version
2. Increment the **patch** number (third segment): e.g. `1.7.2` → `1.7.3`
   - If patch is `9`, reset to `0` and increment **minor**: `1.7.9` → `1.8.0`
   - If minor is `9`, reset to `0` and increment **major**: `1.9.9` → `2.0.0`
3. Write the new version to `version.txt`
4. Update version in all these source locations:
   - `Chud.Backend/Console.cs` — `MenuVersion = "X.Y.Z"`
   - `Chud/Plugin.cs` — `[BepInPlugin("chudmenu", "chudmenu", "X.Y.Z")]`
   - `Chud/Plugin.cs` — `public const string Version = "X.Y.Z"`
5. Commit with message `"vX.Y.Z"`
6. Push to GitHub

## Build rule
Always build after making code changes (`MSBuild.exe /restore /m /t:Build /p:Configuration=Release /v:q`).
