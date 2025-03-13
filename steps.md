Step 1: Project Setup and Core Structure
Create the Blazor Server project with the necessary component structure. Establish the main layout, navigation flow between pages, and implement the basic UI elements for each screen. Set up the Azure Table Storage connection and create the required tables for high scores and statistics.

Step 2: Game Engine Foundation
Implement the core game loop and timing mechanisms. Create the base Snake entity model and movement logic. Build the arena rendering system with grid-based coordinate mapping. Establish the collision detection framework for boundaries and snake-to-snake interactions.

Step 3: Human Player Controls
Develop keyboard input handling for desktop play (arrow keys and WASD). Create the virtual joystick component for mobile play using JavaScript interop. Implement responsive controls that adjust to the current device and viewport. Test input latency and ensure smooth snake movement response.

Step 4: Food and Power-Up System
Build the food generation algorithm with random but fair distribution. Implement the power-up creation and effect application logic. Create the visual representations for food and power-ups. Establish the consumption detection and snake growth mechanics.

Step 5: CPU Snake AI Personalities
Develop the base AI behavior framework for CPU-controlled snakes. Create seven distinct personality profiles with varying aggression levels, movement patterns, and decision-making algorithms. Implement target selection logic for food seeking and obstacle avoidance. Test AI behaviors for diversity and challenge balance.

Step 6: Game State Management
Implement the 30-second time limit with game speed progression. Create the game initialization with symmetrical starting positions. Build the dying snake transition to static obstacle. Develop score calculation algorithms based on survival time and food consumption. Implement the game over condition detection and transition.

Step 7: High Score System
Create the high score entry interface with three-letter initial input. Implement score submission to Azure Table Storage. Develop the high score retrieval and display functionality for the leaderboard page. Add personal best tracking and highlighting.

Step 8: Statistics Collection and Display
Implement comprehensive gameplay data collection during and after game sessions. Create aggregation functions for calculating statistical metrics. Design and implement visualization components for the Statistics page. Set up automatic statistics updates after each game.

Step 9: Mobile Optimization
Fine-tune the portrait mode layout for optimal mobile experience. Optimize the virtual joystick for various screen sizes. Implement touch gesture fallbacks. Test and adjust UI element sizing and spacing for mobile usability. Ensure responsive adaptation between device types.

Step 10: Polishing and Testing
Add browser-native audio feedback for game events. Refine the retro visual style across all components. Implement performance optimizations for smooth gameplay. Conduct thorough testing across devices and browsers. Add final touches to the user experience flow. Ensure the architecture supports future multiplayer expansion.
