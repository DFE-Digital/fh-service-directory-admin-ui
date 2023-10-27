// Set the inactivity timeout to 55 minutes (55 minutes * 60 seconds)
//const inactivityTimeout = 55 * 60 * 1000;
const inactivityTimeout = 1 * 60 * 1000;
let timeoutID;

function checkButtonExits() {
    let button = document.getElementById('hmrc-timeout-keep-signin-btn');

    if (button) {
        addBtnEventListner(button);
    }
}

function addBtnEventListner(button) {

    button.addEventListener('click', function () {
        clearTimeout(timeoutID);
        startInactivityTimer();

        try {
            // Code that may throw an exception

            var result = window.location.href.match(/^[^?]+/)[0];

            let currentUrl = result + "?showTimeoutDialog=false";

            window.location.href = currentUrl;

        } catch (error) {
            alert(error);
        }
    });

}

function showPopup() {

    //showModal()

    //window.location.href = '/LogInTimeout';

    //window.onbeforeunload = function () {

    //    // Set the hidden field value
    //    document.getElementById("ShowTimerOut").value = "true";
    //};

    //alert('reloading!');

    try {
        // Code that may throw an exception

        var result = window.location.href.match(/^[^?]+/)[0];

        let currentUrl = result + "?showTimeoutDialog=true";

        window.location.href = currentUrl;

    } catch (error) {
        alert(error);
    } 

    //// Show the popup div
    //document.getElementById("inactivity-popup").style.display = "block";

    // Handle "Yes" button click
    //document.getElementById("hmrc-timeout-keep-signin-btn").addEventListener("click", function () {
    //    // Reset the inactivity timer
    //    alert('Got Here!');
    //    clearTimeout(timeoutID);
    //    startInactivityTimer();

    //    try {
    //        // Code that may throw an exception

    //        let currentUrl = window.location.href + "?showTimeoutDialog=false";

    //        window.location.href = currentUrl;

    //    } catch (error) {
    //        alert(error);
    //    } 

    //});

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

document.addEventListener('DOMContentLoaded', function () {
    // Your JavaScript code here
    // This code will execute as soon as the DOM is fully parsed, which is typically faster than waiting for all external resources to load.
    checkButtonExits();
});



// Reset the inactivity timer when any user interaction occurs (e.g., mousemove or keydown events)
window.addEventListener("mousemove", function () {
    clearTimeout(timeoutID);
    startInactivityTimer();
});

window.addEventListener("keydown", function () {
    clearTimeout(timeoutID);
    startInactivityTimer();
});

