// Set the inactivity timeout to 55 minutes (55 minutes * 60 seconds)
//const inactivityTimeout = 55 * 60 * 1000;
const inactivityTimeout = 1 * 60 * 1000;
let timeoutID;

//function showModal() {
//    $('#modal').show();
//}

//function hideModal() {
//    $('#modal').hide();
//}

function showPopup() {

    //showModal()

    window.location.href = '/LogInTimeout';

    //// Show the popup div
    //document.getElementById("inactivity-popup").style.display = "block";

    // Handle "Yes" button click
    document.getElementById("popup-continue-button").addEventListener("click", function () {
        //// Close the popup
        //document.getElementById("inactivity-popup").style.display = "none";
        // Reset the inactivity timer
        clearTimeout(timeoutID);
        startInactivityTimer();
    });

    //// Handle "No" button click
    //document.getElementById("popup-close-button").addEventListener("click", function () {
    //    // Close the popup
    //    document.getElementById("inactivity-popup").style.display = "none";
    //    // You can perform any other action here, like logging the user out
    //});
}

function startInactivityTimer() {
    // Set a timer to show the popup after the specified inactivity timeout
    timeoutID = setTimeout(showPopup, inactivityTimeout);
}

// Start the inactivity timer when the page loads
startInactivityTimer();

// Reset the inactivity timer when any user interaction occurs (e.g., mousemove or keydown events)
window.addEventListener("mousemove", function () {
    clearTimeout(timeoutID);
    startInactivityTimer();
});

window.addEventListener("keydown", function () {
    clearTimeout(timeoutID);
    startInactivityTimer();
});

