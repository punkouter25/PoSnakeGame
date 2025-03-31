// Function to focus an element
window.focusElement = (element) => {
    if (element) {
        element.focus();
    }
};

// Function to detect mobile devices
window.isMobileDevice = () => {
    return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
};

// Error handling for Blazor
window.addEventListener('error', (event) => {
    console.error('Error caught by global handler:', event.error);
    // Log additional information that might help debugging
    console.log('Error details:', {
        message: event.message,
        filename: event.filename,
        lineno: event.lineno,
        colno: event.colno
    });
});

// Make the reload button more effective by forcing a clean reload
document.addEventListener('DOMContentLoaded', () => {
    const reloadLinks = document.querySelectorAll('.reload');
    reloadLinks.forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            window.location.href = window.location.href.split('?')[0] + '?nocache=' + Date.now();
        });
    });
});

// --- Dynamic Arena Cell Size Calculation ---
let resizeTimeout;
let arenaDotNetRef = null;
let currentArenaElement = null;
let currentArenaWidthCells = 0;
let currentArenaHeightCells = 0;

// Calculates the cell size based on the arena's current pixel dimensions
window.calculateCellSize = (arenaElement, arenaWidthInCells, arenaHeightInCells) => {
    if (!arenaElement || !arenaWidthInCells || !arenaHeightInCells) {
        console.warn("Cannot calculate cell size: Missing element or dimensions.");
        return 16; // Default size
    }
    const arenaRect = arenaElement.getBoundingClientRect();
    const cellWidth = arenaRect.width / arenaWidthInCells;
    const cellHeight = arenaRect.height / arenaHeightInCells;
    // Use the smaller dimension to ensure cells fit within the bounds
    return Math.min(cellWidth, cellHeight); 
};

// The actual function called on resize (debounced)
const handleResize = () => {
    if (!arenaDotNetRef || !currentArenaElement || !currentArenaWidthCells || !currentArenaHeightCells) return;
    
    const newCellSize = window.calculateCellSize(currentArenaElement, currentArenaWidthCells, currentArenaHeightCells);
    // console.log("New cell size:", newCellSize); // For debugging
    arenaDotNetRef.invokeMethodAsync('UpdateDynamicCellSize', newCellSize);
};

// Sets up the resize listener and performs initial calculation
window.initArenaResizeListener = (arenaElement, dotNetHelper, arenaWidthCells, arenaHeightCells) => {
    if (!arenaElement || !dotNetHelper) {
        console.error("Failed to initialize arena resize listener: Missing element or DotNet reference.");
        return;
    }
    currentArenaElement = arenaElement;
    arenaDotNetRef = dotNetHelper;
    currentArenaWidthCells = arenaWidthCells;
    currentArenaHeightCells = arenaHeightCells;

    // Initial calculation
    handleResize(); 

    // Add debounced resize listener
    window.addEventListener('resize', () => {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(handleResize, 150); // Debounce resize events (150ms)
    });

    console.log("Arena resize listener initialized.");
};

// Cleans up the resize listener
window.disposeArenaResizeListener = () => {
    window.removeEventListener('resize', handleResize); // Make sure to remove the correct handler reference if debouncing differently
    clearTimeout(resizeTimeout);
    arenaDotNetRef = null; // Don't dispose, Blazor handles it
    currentArenaElement = null;
    console.log("Arena resize listener disposed.");
};


// --- Virtual joystick initialization and handling ---
window.initVirtualJoystick = (joystickElement, thumbElement, dotNetRef) => {
    if (!joystickElement || !thumbElement) return;
    
    let isDragging = false;
    const joystickRect = joystickElement.getBoundingClientRect();
    const centerX = joystickRect.width / 2;
    const centerY = joystickRect.height / 2;
    const maxDistance = joystickRect.width / 3;
    
    const getDirection = (x, y) => {
        const angle = Math.atan2(y, x) * (180 / Math.PI);
        if (angle > -45 && angle <= 45) return "right";
        if (angle > 45 && angle <= 135) return "down";
        if (angle > 135 || angle <= -135) return "left";
        return "up";
    };
    
    const handleStart = (e) => {
        e.preventDefault(); // Prevent page scroll on touch
        isDragging = true;
        handleMove(e);
    };
    
    const handleMove = (e) => {
        if (!isDragging) return;
        e.preventDefault(); // Prevent page scroll on touch
        
        const touch = e.type.startsWith('touch') ? e.touches[0] : e;
        const rect = joystickElement.getBoundingClientRect();
        let x = touch.clientX - rect.left - centerX;
        let y = touch.clientY - rect.top - centerY;
        
        // Calculate distance from center
        const distance = Math.min(Math.sqrt(x * x + y * y), maxDistance);
        const angle = Math.atan2(y, x);
        
        // Update thumb position
        x = Math.cos(angle) * distance;
        y = Math.sin(angle) * distance;
        thumbElement.style.transform = `translate(${x}px, ${y}px)`;
        
        // Only send direction if moved beyond 25% of maxDistance
        if (distance > maxDistance * 0.25) {
            dotNetRef.invokeMethodAsync('OnJoystickMove', getDirection(x, y));
        }
    };
    
    const handleEnd = () => {
        if (!isDragging) return; // Prevent multiple calls if mouseup/touchend fire close together
        isDragging = false;
        thumbElement.style.transform = 'translate(0px, 0px)';
    };
    
    // Add touch event listeners (use passive: false to allow preventDefault)
    joystickElement.addEventListener('touchstart', handleStart, { passive: false });
    joystickElement.addEventListener('touchmove', handleMove, { passive: false });
    joystickElement.addEventListener('touchend', handleEnd);
    joystickElement.addEventListener('touchcancel', handleEnd); // Handle cancellation
    
    // Add mouse event listeners for testing on desktop
    joystickElement.addEventListener('mousedown', handleStart);
    // Listen on document to catch mouse movements/releases outside the joystick element
    document.addEventListener('mousemove', handleMove); 
    document.addEventListener('mouseup', handleEnd);

    // Store references for potential cleanup
    joystickElement._mouseMoveHandler = handleMove;
    joystickElement._mouseUpHandler = handleEnd;
}; 

// Cleanup function for joystick listeners (optional, if needed)
window.disposeVirtualJoystick = (joystickElement) => {
    if (joystickElement) {
        // Remove specific handlers if stored
        if (joystickElement._mouseMoveHandler) {
            document.removeEventListener('mousemove', joystickElement._mouseMoveHandler);
        }
        if (joystickElement._mouseUpHandler) {
            document.removeEventListener('mouseup', joystickElement._mouseUpHandler);
        }
        // Add removal for touch listeners if needed, though Blazor disposal might handle this
    }
};

// --- Sound Effect Playback ---
const audioCache = {}; // Cache Audio objects to avoid re-creating them

window.playSound = (soundFile, volume = 1.0) => {
    try {
        // Construct the full path relative to wwwroot
        const filePath = `sounds/${soundFile}`; // Assuming sounds are in wwwroot/sounds/
        
        let audio = audioCache[filePath];
        if (!audio) {
            audio = new Audio(filePath);
            audio.preload = 'auto'; // Suggest browser to preload
            audioCache[filePath] = audio;
            
            // Optional: Handle loading errors
            audio.onerror = () => {
                console.error(`Error loading sound: ${filePath}`);
                delete audioCache[filePath]; // Remove from cache if loading failed
            };
            // Optional: Log when loaded (for debugging)
            // audio.oncanplaythrough = () => {
            //     console.log(`Sound loaded: ${filePath}`);
            // };
        }

        // Ensure volume is within valid range [0.0, 1.0]
        audio.volume = Math.max(0.0, Math.min(1.0, volume)); 

        // If the sound is already playing, rewind it to play again immediately
        if (!audio.paused) {
            audio.currentTime = 0;
        }
        
        // Play the sound
        const playPromise = audio.play();

        if (playPromise !== undefined) {
            playPromise.catch(error => {
                // Autoplay was prevented, possibly due to browser policy (user interaction needed first)
                console.warn(`Autoplay prevented for ${filePath}: ${error}`);
                // We might need a mechanism to enable audio after first user interaction
            });
        }
    } catch (e) {
        console.error(`Error playing sound ${soundFile}: ${e}`);
    }
};
