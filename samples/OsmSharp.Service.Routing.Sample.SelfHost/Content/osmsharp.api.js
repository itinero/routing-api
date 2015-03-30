// API MANAGEMENT.
// an interface to the osmsharp-API.

osmsharp.api = {
    // defines different vehicle options.
    Vehicles: {
        Pedestrian: 'pedestrian',
        Bicycle: 'bicycle',
        Car: 'car'
    },

    id: 1,

    // sends a requests for multimodal route.
    multimodal: function (url, vehicles, time, locations, callback, error, context) {
        // increase request id.
        this.id++;

        var requestString = '/multimodal?callback=%jsonp';
        // add vehicle parameter.
        requestString += '&vehicle=' + vehicles[0];
        for (var i = 1; i < vehicles.length; i++) {
            requestString = requestString + '|' + vehicles[i];
        }
        // add time parameter.
        requestString = requestString + '&time=' + time.getFullYear() + osmsharp.Utils.pad(time.getMonth() + 1, 2) + osmsharp.Utils.pad(time.getDate(), 2) + osmsharp.Utils.pad(time.getHours(), 2) + osmsharp.Utils.pad(time.getMinutes(), 2);
        for (var i = 0; i < locations.length; i++) {
            requestString += '&loc=' + locations[i].lat.toFixed(6) + ',' + locations[i].lon.toFixed(6);
        }

        // aad instructions.
        requestString = requestString + '&instructions=true'

        // execute JSONP request.
        var currentId = this.id;
        osmsharp.JSONP.call(
			url + requestString,
			function (response) { // success.
			    $.proxy(callback, context)(response, currentId);
			},
			function (response) { // timeout.
			    $.proxy(error, context)(response, currentId);
			},
			osmsharp.DEFAULT.JSONP_TIMEOUT,
			'route' + this.id,
			{}
		);
    },

    // sends a requests for multimodal route.
    routing: function (url, options, callback, error, context) {
        // increase request id.
        this.id++;

        var requestString = '/routing?callback=%jsonp';

        // add vehicle parameter.
        requestString += '&vehicle=' + options.vehicle;

        // add locations.
        for (var i = 0; i < options.locations.length; i++) {
            requestString += '&loc=' + options.locations[i].lat.toFixed(6) + ',' + options.locations[i].lon.toFixed(6);
        }

        // sort.
        if (options.sort) {
            requestString += '&sort=true';
        }

        // instructions.
        if (options.instructions) {
            requestString += '&instructions=true';
        }

        // execute JSONP request.
        var currentId = this.id;
        osmsharp.JSONP.call(
            url + requestString,
            function (response) { // success.
                $.proxy(callback, context)(response, currentId);
            },
            function (response) { // timeout.
                $.proxy(error, context)(response, currentId);
            },
            osmsharp.DEFAULT.JSONP_TIMEOUT,
            'route' + this.id,
            {}
        );
    },

    // sends a requests for multimodal route.
    alongjustone: function (url, vehicles, locations, callback, error, context) {
        // increase request id.
        this.id++;

        var requestString = '/alongjustone?callback=%jsonp';
        // add vehicle parameter.
        requestString += '&vehicle=' + vehicles[0];
        for (var i = 1; i < vehicles.length; i++) {
            requestString = requestString + '|' + vehicles[i];
        }
        // add time parameter.
        for (var i = 0; i < locations.length; i++) {
            requestString += '&loc=' + locations[i].lat.toFixed(6) + ',' + locations[i].lon.toFixed(6);
        }

        // execute JSONP request.
        var currentId = this.id;
        osmsharp.JSONP.call(
            url + requestString,
            function (response) { // success.
                $.proxy(callback, context)(response, currentId, locations);
            },
            function (response) { // timeout.
                $.proxy(error, context)(response, currentId);
            },
            osmsharp.DEFAULT.JSONP_TIMEOUT,
            'route' + this.id,
            {}
        );
    }
};