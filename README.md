# <img src="FlappyBird/Assets/bird-idle.png" width="32" height="28" alt=""> Flappy Bird

A Flappy Bird remake in C# with Windows Forms. Flap through the gaps using Space or the left mouse button. Each pipe pair you pass earns one point. Hitting a pipe or the ground ends the round.

## Technologies

- C# and .NET 10
- Windows Forms and `System.Windows.Forms.Timer`

## Build and run

Requires Windows and the .NET 10 SDK. Open a terminal in the repository root and run:

```powershell
dotnet build FlappyBird.slnx
dotnet run --project FlappyBird/FlappyBird.csproj
```

You can also open `FlappyBird.slnx` in a version of Visual Studio that supports .NET 10.

## Controls and rules

- **Space / Left click:** Start the round with the first flap on the **GET READY** screen. During play, press Space or click the play area to flap again.
- **Esc / Pause button:** Pause or resume the round. The orange button is in the upper-left corner. In the menu, **Resume** continues the round and **New game** returns to the ready screen with the score reset.
- **Sound: On / Off:** Toggle wing, point, and collision effects for the current session from the pause menu.
- **Play again:** After a crash, wait for the short delay and the collision sound to end. Then click **Play again**, click the play area, or press Space. With sound off, the delay is 300 ms.

From the ready screen, the bird starts in the center and slides into its usual flight position as the first pipe enters. Two pipe pairs scroll from right to left. Each gap is 120 game pixels high. Its vertical position and the horizontal spacing between pairs are randomized.

Resize the window from any edge or corner to keep the play area at a 3:4 aspect ratio. The maximize button fits the window to the monitor's work area. Press it again to restore the previous size. The minimum play area is 480 × 640 pixels.

## Design and limitations

`GameState` manages movement, obstacles, collision, and scoring. Windows Forms handles keyboard and mouse input and updates the game on a timer. `ArcadeRenderer` draws the scene and interface. Collision checks use bounding rectangles.

The score resets each round. A new best score is stored in `%LOCALAPPDATA%/FlappyBird/high-score.txt`. If the file cannot be read or saved, the game continues and shows a status message.

The background is drawn in code. The bird, pipes, and ground use embedded PNG assets. The bird icon appears in the window title bar and taskbar.

`GameAudio` manages embedded WAV effects. Point and collision effects use independent playback so they can be heard alongside other sounds.

The sound effects were converted to WAV from archived assets of Flappy Bird for Android v1.3 by Dong Nguyen and .GEARS Studios. [Sound archive](https://github.com/FlappyBirdRestored/flappybird-restored/tree/restored/apktool_source/assets/sounds)

## Project structure

```text
FlappyBird/
├── FlappyBird.slnx             # Solution
├── FlappyBird/
│   ├── FlappyBird.csproj       # WinForms project and embedded resources
│   ├── Program.cs              # Application entry point
│   ├── Form1.cs                # Input, game loop, and menus
│   ├── Form1.Designer.cs       # Form initialization and disposal
│   ├── GameState.cs            # Physics, obstacles, collision, and score
│   ├── ArcadeRenderer.cs       # Game scene and interface rendering
│   ├── ArcadeButton.cs         # Pixel-style buttons
│   ├── PixelAssets.cs          # Loading and drawing sprites
│   ├── PixelHeading.cs         # Pixel-style headings
│   ├── ScoreDigits.cs          # Score digits
│   ├── ResultScreen.cs         # Game Over screen
│   ├── GameAudio.cs            # Sound effects and mute
│   ├── WaveEffect.cs           # Independent WAV playback
│   ├── HighScoreStore.cs       # Best score persistence
│   ├── WindowSizing.cs         # Window size and aspect ratio
│   └── Assets/                 # PNG sprites, WAV effects, and application icon
├── docs/
│   └── screenshots/           # In-game screenshots
└── README.md
```

## Screenshots

**Ready screen**

![Get Ready heading, bird, and instruction to click or press Space](docs/screenshots/start.png)

**Gameplay**

![Bird approaching the pipes with the score and pause button visible](docs/screenshots/gameplay.png)

**Pause menu**

![Pause menu with Resume, New game, and Sound controls](docs/screenshots/pause.png)

**Game Over**

![Game Over screen with the score, best score, and Play again button](docs/screenshots/game-over.png)
