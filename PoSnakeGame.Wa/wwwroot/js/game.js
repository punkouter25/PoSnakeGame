// Function to focus an element
window.focusElement = (element) => {
    element.focus();
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

// Virtual joystick initialization and handling
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
        isDragging = true;
        handleMove(e);
    };
    
    const handleMove = (e) => {
        if (!isDragging) return;
        
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
        isDragging = false;
        thumbElement.style.transform = 'translate(0px, 0px)';
    };
    
    // Add touch event listeners
    joystickElement.addEventListener('touchstart', handleStart);
    joystickElement.addEventListener('touchmove', handleMove);
    joystickElement.addEventListener('touchend', handleEnd);
    
    // Add mouse event listeners for testing on desktop
    joystickElement.addEventListener('mousedown', handleStart);
    document.addEventListener('mousemove', handleMove);
    document.addEventListener('mouseup', handleEnd);
}; 