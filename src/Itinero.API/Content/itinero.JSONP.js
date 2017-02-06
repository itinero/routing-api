﻿// itinero JSONP call wrapper 
// [wrapper for JSONP calls with DOM cleaning, fencing, timout handling]

itinero = {
    DEFAULT: {
        JSONP_TIMEOUT: 100000
    }
};

itinero.JSONP = {

	// storage to keep track of unfinished JSONP calls
	fences: {},
	callbacks: {},
	timeouts: {},
	timers: {},


	// default callback routines
	late: function() {},	
	empty: function() {},

	// init JSONP call
	call: function(source, callback_function, timeout_function, timeout, id, parameters) {
		// only one active JSONP call per id
		if (itinero.JSONP.fences[id] == true)
			return false;
		itinero.JSONP.fences[id] = true;

		// wrap timeout function
		itinero.JSONP.timeouts[id] = function (response) {
			try {
				timeout_function(response, parameters);
			} finally {
				itinero.JSONP.callbacks[id] = itinero.JSONP.late;				// clean functions
				itinero.JSONP.timeouts[id] = itinero.JSONP.empty;
				itinero.JSONP.fences[id] = undefined;						// clean fence
			}
		};

		// wrap callback function
		itinero.JSONP.callbacks[id] = function (response) {
			clearTimeout(itinero.JSONP.timers[id]);						// clear timeout timer
			itinero.JSONP.timers[id] = undefined;

			try {
				callback_function(response, parameters);				// actual wrapped callback 
			} finally {
				itinero.JSONP.callbacks[id] = itinero.JSONP.empty;			// clean functions
				itinero.JSONP.timeouts[id] = itinero.JSONP.late;
				itinero.JSONP.fences[id] = undefined;						// clean fence
			}
		};

		// clean DOM
		var jsonp = document.getElementById('jsonp_'+id);
		if(jsonp)
			jsonp.parentNode.removeChild(jsonp);

		// add script to DOM
		var script = document.createElement('script');
		script.type = 'text/javascript';
		script.id = 'jsonp_' + id;
		script.src = source.replace(/%jsonp/, "itinero.JSONP.callbacks." + id);
		document.head.appendChild(script);

		// start timeout timer
		itinero.JSONP.timers[id] = setTimeout(itinero.JSONP.timeouts[id], timeout);
		return true;
	},

	clear: function(id) {
		clearTimeout(itinero.JSONP.timers[id]);					// clear timeout timer
		itinero.JSONP.callbacks[id] = itinero.JSONP.empty;			// clean functions
		itinero.JSONP.timeouts[id] = itinero.JSONP.empty;
		itinero.JSONP.fences[id] = undefined;						// clean fence

		// clean DOM
		var jsonp = document.getElementById('jsonp_'+id);
		if(jsonp)
			jsonp.parentNode.removeChild(jsonp);		
	},

	// reset all data
	reset: function() {
		itinero.JSONP.fences = {};
		itinero.JSONP.callbacks = {};
		itinero.JSONP.timeouts = {};
		itinero.JSONP.timers = {};
	}
};