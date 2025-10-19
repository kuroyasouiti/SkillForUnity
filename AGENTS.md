# Repository Guidelines

## Project Structure & Module Organization
Unity assets live under `Assets/`, grouped by feature (scenes in `Assets/Scenes`, scripts in `Assets/Scripts`, shared art in `Assets/Art`). Third-party packages and custom registries are resolved from `Packages/`. Team-wide configuration is committed in `ProjectSettings/`. The `Library/`, `Logs/`, and `Temp/` folders are Unity-generated; do not version or hand-edit them.

## Build, Test, and Development Commands
Open the project via Unity Hub or `Unity.exe -projectPath "%CD%"`. For automated builds use `Unity.exe -batchmode -quit -projectPath "%CD%" -executeMethod BuildPipeline.BuildPlayer`. Run domain reload safe playtests with `Unity.exe -batchmode -quit -projectPath "%CD%" -executeMethod PlayModeSmokeTests.Run`. When iterating on scripts, leverage the Unity editor's play mode and enable `Enter Play Mode Options` for faster cycles.

## Coding Style & Naming Conventions
Author gameplay code in C# with four-space indentation and braces on new lines (Allman style), aligning with Unity's default formatter. Name classes and public members in PascalCase, private fields in `_camelCase`, and serialized fields with a `[SerializeField] private` pattern. Group behaviour scripts per scene subfolder and suffix MonoBehaviours with `Controller`, `Manager`, or `View` to clarify roles. Use `namespace MCP.Gameplay.Feature` to keep scripts discoverable. Run `dotnet format` or the Rider/ReSharper profile before sending changes.

## Testing Guidelines
Prefer Unity Test Framework for both EditMode and PlayMode tests; place tests in `Assets/Tests/EditMode` or `Assets/Tests/PlayMode`. Name test classes `<Feature>Tests` and methods with `[Test]`-annotated PascalCase sentences (e.g., `PlayerTakesDamageWhenHit`). Execute suites via the Test Runner window or `Unity.exe -batchmode -quit -projectPath "%CD%" -runTests -testResults Logs/TestResults.xml`. Target smoke coverage on core gameplay loops and regression cases for reported bugs.

## Commit & Pull Request Guidelines
Write atomic commits using imperative present tense (e.g., `Add enemy spawn throttling`). Keep diffs limited to a single feature or fix, and avoid committing generated artifacts. Pull requests should include: a concise summary, testing notes (`EditMode`, `PlayMode`, or manual steps), linked issue IDs, and relevant screenshots or GIFs for player-facing changes. Request review from the owning discipline (engineering, design, art) before merging.
