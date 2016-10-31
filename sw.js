self.addEventListener('push', function(event) {
    console.log(event);
    if (!(self.Notification && self.Notification.permission === 'granted')) {
        return;
    }

    var data = {};
    if (event.data) {
        data = event.data.json();
    }
    registration.showNotification(data.message);
});