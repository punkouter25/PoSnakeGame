# Snake Battle Royale

A modern take on the classic Snake game, built with Blazor WebAssembly. Battle against AI-controlled snakes in a fast-paced arena where survival is key!

## Features

- **Multi-Snake Battle Arena**: Compete against AI-controlled snakes with unique personalities and behaviors
- **Time-Limited Matches**: Fast-paced 30-second rounds with increasing difficulty
- **Cross-Platform Controls**: 
  - Desktop: Arrow keys or WASD for movement
  - Mobile: Touch-based virtual joystick
- **High Score System**: Enter your 3-letter initials to immortalize your achievements
- **Azure Integration**: High scores stored in Azure Table Storage
- **Responsive Design**: Optimized for both desktop and mobile play

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Azure Storage Emulator (for local development) or Azure Storage Account
- Visual Studio 2022 or Visual Studio Code

### Local Development Setup

1. Clone the repository
2. Navigate to the project directory
3. Run the Azure Storage Emulator (or configure your Azure Storage connection string)
4. Run the following commands:

```bash
dotnet restore
dotnet build
dotnet run
```

### Project Structure

- **PoSnakeGame.Core**: Core game logic and models
- **PoSnakeGame.Infrastructure**: Azure storage and infrastructure services
- **PoSnakeGame.Wa**: Blazor WebAssembly frontend application
- **PoSnakeGame.Web**: Server-side components and API
- **PoSnakeGame.Tests**: Unit tests

## How to Play

1. Start a new game from the main menu
2. Control your snake using arrow keys/WASD (desktop) or virtual joystick (mobile)
3. Collect food to grow and increase your score
4. Avoid collisions with walls, other snakes, and obstacles
5. Survive until the timer runs out
6. Submit your score if you make it to the leaderboard!

## Development Features

- Blazor WebAssembly for client-side rendering
- Azure Table Storage for high score persistence
- Responsive design with mobile-first approach
- Modular architecture with separation of concerns
- Unit tests for core game logic

## Contributing

Contributions are welcome! Please feel free to submit pull requests.