document.addEventListener('DOMContentLoaded', function() {
    let eventSource = null;
    const startButton = document.getElementById('startButton');
    const stopButton = document.getElementById('stopButton');
    const messagesTextArea = document.getElementById('messages');

    const clearButton = document.getElementById('clearButton');

    clearButton.addEventListener('click', function() {
        messagesTextArea.value = '';
    });

    startButton.addEventListener('click', function() {
        // Disable the start button and enable the stop button
        startButton.disabled = true;
        stopButton.disabled = false;

        const sseUrlElement = document.getElementById('sseUrl');
        const sseUrl = sseUrlElement.value.trim()
        
        console.log("Using SSE URL: ", sseUrl)

        eventSource = new EventSource(sseUrl);

        eventSource.onmessage = function(e) {
            console.log('Received message:', e.data);

            // Append the received message to the text area
            messagesTextArea.value += e.data + '\n';

            // Scroll to the bottom of the text area
            messagesTextArea.scrollTop = messagesTextArea.scrollHeight;
        };

        eventSource.onerror = function(e) {
            console.error('EventSource failed:', e);

            // Display an error message
            messagesTextArea.value += 'Connection error occurred.\n';

            // Close the connection and update button states
            if (eventSource) {
                eventSource.close();
                eventSource = null;
            }
            startButton.disabled = false;
            stopButton.disabled = true;
        };
    });

    stopButton.addEventListener('click', function() {
        // Close the EventSource connection
        if (eventSource) {
            eventSource.close();
            eventSource = null;
        }

        // Update button states
        startButton.disabled = false;
        stopButton.disabled = true;

        // Provide feedback to the user
        messagesTextArea.value += 'SSE connection stopped by user.\n';
        console.log('SSE connection stopped by user.');
    });
});
