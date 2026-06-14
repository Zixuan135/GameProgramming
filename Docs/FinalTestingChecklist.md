# Final Testing Checklist

Last updated: 2026-06-14

This checklist is for the final manual playtest pass before submission. BubbleTown does not currently include automated Unity play-mode tests, so the project should use this file as visible evidence of testing, debugging, and improvement.

Use `Pass`, `Fail`, or `N/A` in the Result column during the final Unity run.

## Build And Launch

| Area | Check | Expected Result | Result |
| --- | --- | --- | --- |
| Unity version | Open the project in Unity `2022.3.62f3c1` or compatible 2022.3 LTS. | Project opens without missing-script errors. | To verify |
| Main scene | Open `Assets/Scenes/MainMenu.unity` and press Play. | Main menu loads, audio starts, and buttons respond. | To verify |
| Build | Create a desktop build from the Unity Build Settings. | Build completes and launches into the main menu. | To verify |
| Console | Play through one full route while watching the Console. | No blocking errors or repeated warning spam. | To verify |

## Core Player Experience

| Area | Check | Expected Result | Result |
| --- | --- | --- | --- |
| Tutorial flow | Start Tutorial from Mode Select. | Player can follow prompts from movement to exit completion. | To verify |
| Tutorial route gates | Break the teaching wall and collect the first item. | The player still needs to break route gates before reaching the exit. | To verify |
| Tutorial failure | Stand in an explosion during Tutorial. | Defeat/result state appears and Retry works. | To verify |
| AI Battle setup | Select a character, map, and AI difficulty. | Battle starts with Player1 and AI correctly spawned. | To verify |
| AI behavior | Play Easy, Normal, and Hard briefly. | AI moves, avoids danger where possible, places bombs, and can resolve a round. | To verify |
| Local VS setup | Start Local VS with two characters. | Both keyboard control sets work and both players can place bombs. | To verify |
| Local VS match | Finish enough rounds for Best of 3. | Score and final winner display correctly. | To verify |
| Result flow | Complete or lose any mode. | Result screen shows mode, map, winner, Retry, and Main Menu without overlap. | To verify |

## Game Systems

| Area | Check | Expected Result | Result |
| --- | --- | --- | --- |
| Input | Test WASD/Space and Arrow/Enter controls. | Movement and bomb placement respond only during active battle. | To verify |
| Movement logic | Move around hard walls, soft walls, bombs, and characters. | Grid collision blocks invalid moves and allows valid one-cell movement. | To verify |
| Bomb logic | Place bombs near walls and other bombs. | Fuse countdown, cross explosion, wall blocking, and chain reactions work. | To verify |
| Soft walls | Destroy soft walls on each map theme. | Soft walls disappear, map state updates, and drops can spawn. | To verify |
| Items | Pick up all five item types when available. | Bomb count, blast range, speed, shield, and invincibility effects apply. | To verify |
| Audio | Trigger menu, battle, movement, bomb, explosion, pickup, result, and button sounds. | Placeholder audio plays at controlled volume. | To verify |
| Animation/feedback | Observe characters, bombs, explosions, items, wall breaks, camera shake, and HUD toasts. | Feedback is readable and not distracting. | To verify |
| UI | Test guide, pause, settings, item guide, retry, and main-menu buttons. | UI buttons respond and do not trap the player. | To verify |

## Map And Visual Regression

| Area | Check | Expected Result | Result |
| --- | --- | --- | --- |
| Candy Park | Play one battle on Candy Park. | Cream hard walls, blue soft walls, floor, borders, and background feel coherent. | To verify |
| Snowfield | Play one battle on Snowfield. | Hard walls and soft walls are visually distinct and readable. | To verify |
| Jelly Maze | Play one battle on Jelly Maze. | Purple hard walls, pink soft walls, cyan floor accents, and neon props remain readable. | To verify |
| Battle HUD | Resize Game view to common 16:9 sizes. | Left HUD stays aligned and arena remains visible. | To verify |
| Result UI | Check victory, defeat, draw, and tutorial complete results if possible. | Dynamic text covers baked labels cleanly and buttons align. | To verify |

## Accessibility, Safety, And Submission Notes

| Area | Check | Expected Result | Result |
| --- | --- | --- | --- |
| Volume controls | Change master, BGM, and SFX sliders. | Settings save and affect playback immediately. | To verify |
| Mute controls | Toggle BGM and SFX mute. | Audio mutes/unmutes and persists through scene changes. | To verify |
| Screen shake | Disable screen shake in settings. | Camera shake stops for explosions/result feedback. | To verify |
| Keyboard-only play | Play menus and battles with available controls. | Core gameplay works without mouse except UI button selection where used. | To verify |
| Data storage | Confirm the project does not use networking or account login. | Only local `PlayerPrefs` settings are stored. | To verify |
| Asset status | Review imported art/audio sources before public release. | Placeholder or generated assets are replaced, credited, or licensed as needed. | To verify |

## Known Low-Risk Limitations

- The project is a polished prototype/vertical slice, not a fully shipped commercial game.
- Some 3D art is still generated from Unity primitives for fast iteration.
- Audio is placeholder-style and should be replaced with final original or licensed clips for public release.
- Automated play-mode tests are not implemented yet; manual testing is the current final QA route.
