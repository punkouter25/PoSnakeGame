# PoSnakeGame Implementation Steps

## High-Level Steps

1. **Set up Project Structure and Azure Development Environment**
   - [x] Create .gitignore file
   - [x] Set up AZURE TABLE STORAGE for  Azure storage 
   - [x] Ensure all projects build correctly
   - [x] Make sure azure table storage connection points to Azure table storage in the cloud
   - [x] Set up local logging

2. **Core Game Logic Implementation**
   - [x] Implement Snake model and movement logic
   - [x] Implement Arena and collision detection
   - [x] Implement PowerUp generation and effects
   - [x] Write unit tests for game logic (All tests passing!)

3. **AI Snake Behavior Implementation**
   - [x] Implement CpuSnakeAI service (Refined base class and helpers)
   - [x] Create different AI personality types (Implemented and refined)
   - [ ] Implement pathfinding algorithms (Skipped complex pathfinding, using heuristics)
   - [x] Write unit tests for AI behavior (Basic tests passing)

4. **Game Engine and Game Service Development**
   - [x] Implement GameEngine for game state management (Handled within GameService)
   - [x] Implement GameService for coordinating game components (Refined and Tested)
   - [x] Create timer and scoring mechanisms (Verified in GameService)
   - [x] Write unit tests for game engine (Added tests for GameService state, timer, score)

5. **User Preferences and Configuration**
   - [x] Implement UserPreferences model
   - [x] Create storage service for user preferences (LocalStorage via Blazored.LocalStorage)
   - [x] Add user customization options (Basic Settings page with color picker)
   - [ ] Write unit tests for preferences system (Skipped due to complexity with LocalStorage)

6. **Blazor WebAssembly UI Development**
   - [ ] Create responsive game board UI
   - [ ] Implement keyboard and touch controls
   - [ ] Add game status and score display
   - [ ] Create main menu and settings screens

7. **High Score System & Backend API Implementation**
   - [ ] Implement HighScore model and storage
   - [ ] Create Azure Functions for score submission and retrieval (Original approach)
   - [x] Create ASP.NET Core Web API project (`PoSnakeGame.Api`) to duplicate/replace Function App logic (New approach)
   - [x] Connect Blazor UI (`PoSnakeGame.Wa`) to use the new Web API instead of Functions/direct service calls
   - [ ] Build high score display UI (UI exists, but now uses API service)
   - [ ] Write unit tests for high score system/API endpoints

8. **Azure Storage Integration**
   - [ ] Implement Azure Table Storage services 
   - [ ] Create data access services
   - [ ] Add error handling and retry logic
   - [ ] Write integration tests for Azure storage

9. **Finalize UI and UX**
   - [ ] Add visual effects and animations
   - [ ] Implement sound effects
   - [ ] Create responsive design for mobile
   - [ ] Conduct usability testing

10. **Deployment and CI/CD**
    - [x] Set up GitHub CI/CD Workflows (`ci.yml`, `posnakegame-staticwebapp.yml`, `deploy-api.yml`)
    - [x] Create Azure resources via CLI (App Service Plan `PoSnakeGameApiPlanWest2`, App Service `posnakegame-api`)
    - [x] Configure GitHub Secret `AZURE_CREDENTIALS` via CLI
    - [x] Update Blazor production config (`appsettings.Production.json`) to use direct API URL (API linking not used due to SWA Free tier)
    - [ ] Create Azure deployment scripts (if needed beyond workflows)
    - [ ] Implement Application Insights monitoring
    - [ ] Complete final testing and deployment

