Notification.requestPermission();

if ('serviceWorker' in navigator) {
    navigator.serviceWorker.register('sw.js').then(function(reg) {
        if(reg.installing) {
            console.log('Service worker installing');
        } else if(reg.waiting) {
            console.log('Service worker installed');
        } else if(reg.active) {
            console.log('Service worker active');
        }

        initialiseState(reg);
    });
} else {
    console.log('Service workers aren\'t supported in this browser.');
}

// Once the service worker is registered set the initial state
function initialiseState(reg) {
    // Are Notifications supported in the service worker?
    if (!(reg.showNotification)) {
        console.log('Notifications aren\'t supported on service workers.');
        useNotifications = false;
    } else {
        useNotifications = true;
    }

    // Check the current Notification permission.
    // If its denied, it's a permanent block until the
    // user changes the permission
    if (Notification.permission === 'denied') {
        console.log('The user has blocked notifications.');
        return;
    }

    // Check if push messaging is supported
    if (!('PushManager' in window)) {
        console.log('Push messaging isn\'t supported.');
        return;
    }

    // We need the service worker registration to check for a subscription
    navigator.serviceWorker.ready.then(function(reg) {
        // Do we already have a push message subscription?
        reg.pushManager.getSubscription()
            .then(function(subscription) {
                if (!subscription) {
                    console.log('Not yet subscribed to Push');
                    return;
                }

                isPushEnabled = true;
                
                console.log(subscription.toJSON());
                var endpoint = subscription.endpoint;
                var key = subscription.getKey('p256dh');
                var auth = subscription.getKey('auth');
                console.log(key);
                updateStatus(endpoint,key,auth,'init');
            })
            .catch(function(err) {
                console.log('Error during getSubscription()', err);
            });
    });
}

function postSubscribeObj(statusType, name, endpoint, key, auth) {
    // Create a new XHR and send an array to the server containing
    // the type of the request, the name of the user subscribing,
    // and the push subscription endpoint + key the server needs
    // to send push messages
    var request = new XMLHttpRequest();

    request.open('POST', 'http://localhost:7000/api/push/');
    request.setRequestHeader('Content-Type', 'application/json');

    var subscribeObj = {
        statusType: statusType,
        name: name,
        endpoint: endpoint,
        key: btoa(String.fromCharCode.apply(null, new Uint8Array(key))),
        auth: btoa(String.fromCharCode.apply(null, new Uint8Array(auth)))
    };
    console.log(subscribeObj);
    request.send(JSON.stringify(subscribeObj));
}

function subscribe() {
    navigator.serviceWorker.ready.then(function(reg) {
        reg.pushManager.subscribe({userVisibleOnly: true})
            .then(function(subscription) {
                // The subscription was successful
                isPushEnabled = true;

                // Update status to subscribe current user on server, and to let
                // other users know this user has subscribed
                var endpoint = subscription.endpoint;
                var key = subscription.getKey('p256dh');
                var auth = subscription.getKey('auth');
                updateStatus(endpoint,key,auth,'subscribe');
            })
            .catch(function(e) {
                if (Notification.permission === 'denied') {
                    // The user denied the notification permission which
                    // means we failed to subscribe and the user will need
                    // to manually change the notification permission to
                    // subscribe to push messages
                    console.log('Permission for Notifications was denied');

                } else {
                    // A problem occurred with the subscription, this can
                    // often be down to an issue or lack of the gcm_sender_id
                    // and / or gcm_user_visible_only
                    console.log('Unable to subscribe to push.', e);
                }
            });
    });
}

function updateStatus(endpoint,key,auth,statusType) {
    console.log("updateStatus, endpoint: " + endpoint);
    console.log("updateStatus, key: " + key);

    // If we are subscribing to push
    if(statusType === 'subscribe' || statusType === 'init') {
        var name = document.getElementById('user_id').value;
        postSubscribeObj(statusType, name, endpoint, key, auth);
    }
}

document.querySelector('.js-push-button').onclick = function () {
    subscribe();
};
