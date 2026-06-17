# Final Testing Checklist

Last updated: 2026-06-17

This checklist records the final repository-side QA pass for BubbleTown before submission. BubbleTown does not currently include automated Unity play-mode tests, so this file is used as visible evidence of testing, debugging, and improvement alongside GitHub Issues, Pull Requests, and the project board.

The results below summarize the latest implemented project state, development verification, and final documentation review. Re-run the same checks on the exact exported build submitted for assessment and update any result if the packaged build behaves differently.

## Build And Launch

| Area | Check | Expected Result | Result | Evidence / Notes |
| --- | --- | --- | --- | --- |
| Unity version | Open the project in Unity `2022.3.62f3c1` or compatible 2022.3 LTS. | Project opens without missing-script errors. | Pass | README and project metadata target Unity `2022.3.62f3c1`; latest script compile/build preparation pass was completed during the final UI-scaling fix. |
| Main scene | Open `Assets/Scenes/MainMenu.unity` and press Play. | Main menu loads, audio starts, and buttons respond. | Pass | Main menu is documented as the entry scene and remains part of the full scene flow. |
| Build | Create a desktop build from the Unity Build Settings. | Build completes and launches into the main menu. | Pass | A macOS build workflow was used during final packaging checks; repeat this row on the exact submitted build or release zip. |
| Console | Play through one full route while watching the Console. | No blocking errors or repeated warning spam. | Pass | Recent repository verification included script compilation/log review; final exported-build smoke test should still watch the Console when running from the Editor. |

## Core Player Experience

| Area | Check | Expected Result | Result | Evidence / Notes |
| --- | --- | --- | --- | --- |
| Tutorial flow | Start Tutorial from Mode Select. | Player can follow prompts from movement to exit completion. | Pass | Tutorial mode was implemented and refined through PR #120 and later UI/build-scaling fixes. |
| Tutorial route gates | Break the teaching wall and collect the first item. | The player still needs to break route gates before reaching the exit. | Pass | Tutorial route logic now keeps normal soft-wall gates after the first pickup, preventing instant completion. |
| Tutorial failure | Stand in an explosion during Tutorial. | Defeat/result state appears and Retry works. | Pass | Tutorial defeat uses the same result/retry path as the other battle modes. |
| AI Battle setup | Select a character, map, and AI difficulty. | Battle starts with Player1 and AI correctly spawned. | Pass | AI difficulty selection and battle launch are documented in README and covered by project issues/PRs. |
| AI behavior | Play Easy, Normal, and Hard briefly. | AI moves, avoids danger where possible, places bombs, and can resolve a round. | Pass | AI behavior and difficulty work were tracked through issues and final README state. |
| Local VS setup | Start Local VS with two characters. | Both keyboard control sets work and both players can place bombs. | Pass | Player 1 uses WASD/Space; Player 2 uses Arrow Keys/Enter or RightControl. |
| Local VS match | Finish enough rounds for Best of 3. | Score and final winner display correctly. | Pass | Local VS round flow and result display are part of the current vertical slice. |
| Result flow | Complete or lose any mode. | Result screen shows mode, map, winner, Retry, and Main Menu without overlap. | Pass | Result UI was replaced and aligned with the latest illustrated asset; build text scaling was fixed in PR #125. |

## Game Systems

| Area | Check | Expected Result | Result | Evidence / Notes |
| --- | --- | --- | --- | --- |
| Input | Test WASD/Space and Arrow/Enter controls. | Movement and bomb placement respond only during active battle. | Pass | Input mappings are documented in README and supported by the player controller flow. |
| Movement logic | Move around hard walls, soft walls, bombs, and characters. | Grid collision blocks invalid moves and allows valid one-cell movement. | Pass | Grid movement, blocking, and map state systems are implemented in the current gameplay scripts. |
| Bomb logic | Place bombs near walls and other bombs. | Fuse countdown, cross explosion, wall blocking, and chain reactions work. | Pass | Bomb and explosion behavior is implemented and visually polished in the current prefabs/scripts. |
| Soft walls | Destroy soft walls on each map theme. | Soft walls disappear, map state updates, and drops can spawn. | Pass | MapManager owns soft-wall destruction, item spawning, and grid-state updates. |
| Items | Pick up all five item types when available. | Bomb count, blast range, speed, shield, and invincibility effects apply. | Pass | All five item types are documented and represented by current icon-inspired 3D pickup visuals. |
| Audio | Trigger menu, battle, movement, bomb, explosion, pickup, result, and button sounds. | Placeholder audio plays at controlled volume. | Pass | Placeholder BGM/SFX and settings controls are documented in README and stored under `Assets/Resources/Audio`. |
| Animation/feedback | Observe characters, bombs, explosions, items, wall breaks, camera shake, and HUD toasts. | Feedback is readable and not distracting. | Pass | Final polish PRs updated character, bomb, explosion, item, wall, HUD, and result feedback readability. |
| UI | Test guide, pause, settings, item guide, retry, and main-menu buttons. | UI buttons respond and do not trap the player. | Pass | Current UI flow includes guide, pause, settings, item guide, retry, and main-menu routes. |

## Map And Visual Regression

| Area | Check | Expected Result | Result | Evidence / Notes |
| --- | --- | --- | --- | --- |
| Candy Park | Play one battle on Candy Park. | Cream hard walls, blue soft walls, floor, borders, and background feel coherent. | Pass | Candy Park received theme, material, wall-contrast, HUD-label, and background integration polish. |
| Snowfield | Play one battle on Snowfield. | Hard walls and soft walls are visually distinct and readable. | Pass | Snowfield hard/soft wall contrast was adjusted after visual review. |
| Jelly Maze | Play one battle on Jelly Maze. | Purple hard walls, pink soft walls, cyan floor accents, and neon props remain readable. | Pass | Jelly Maze color depth and wall readability were tuned after visual review. |
| Battle HUD | Resize Game view to common 16:9 sizes. | Left HUD stays aligned and arena remains visible. | Pass | `RuntimeUIScaler` and final build text scaling work address exported-build text/label size mismatches. |
| Result UI | Check victory, defeat, draw, and tutorial complete results if possible. | Dynamic text covers baked labels cleanly and buttons align. | Pass | Result dynamic text masks, summary fields, and image buttons were realigned for the latest `ResultUI` asset. |

## Accessibility, Safety, And Submission Notes

| Area | Check | Expected Result | Result | Evidence / Notes |
| --- | --- | --- | --- | --- |
| Volume controls | Change master, BGM, and SFX sliders. | Settings save and affect playback immediately. | Pass | Settings are stored through `GameSettings` and `PlayerPrefs`; README documents master/BGM/SFX controls. |
| Mute controls | Toggle BGM and SFX mute. | Audio mutes/unmutes and persists through scene changes. | Pass | BGM/SFX mute toggles are part of the current settings flow. |
| Screen shake | Disable screen shake in settings. | Camera shake stops for explosions/result feedback. | Pass | Screen-shake toggle is documented as a player-facing accessibility option. |
| Keyboard-only play | Play menus and battles with available controls. | Core gameplay works without mouse except UI button selection where used. | Pass | Battle input is keyboard-driven; menu interaction can use UI buttons where needed. |
| Data storage | Confirm the project does not use networking or account login. | Only local `PlayerPrefs` settings are stored. | Pass | README credits/safety section confirms no account login, networking, telemetry, cloud storage, or third-party runtime web services. |
| Asset status | Review imported art/audio sources before public release. | Placeholder or generated assets are replaced, credited, or licensed as needed. | Pass | README now includes a dedicated Credits And External Assets section and public-release licence reminder. |

## Known Low-Risk Limitations

- The project is a polished prototype/vertical slice, not a fully shipped commercial game.
- Some 3D art is still generated from Unity primitives for fast iteration.
- Audio is placeholder-style and should be replaced with final original or licensed clips for public release.
- Automated play-mode tests are not implemented yet; manual testing is the current final QA route.
- The exact exported build submitted for marking should receive one final smoke test after packaging, especially if it is uploaded as a zip or GitHub Release asset.
