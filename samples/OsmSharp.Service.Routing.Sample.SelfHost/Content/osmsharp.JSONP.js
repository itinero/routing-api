﻿// osmsharp JSONP call wrapper 
// [wrapper for JSONP calls with DOM cleaning, fencing, timout handling]

osmsharp = {
    DEFAULT: {
        JSONP_TIMEOUT: 100000
    }
};

osmsharp.JSONP = {

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
		if (osmsharp.JSONP.fences[id] == true)
			return false;
		osmsharp.JSONP.fences[id] = true;

		// wrap timeout function
		osmsharp.JSONP.timeouts[id] = function (response) {
			try {
				timeout_function(response, parameters);
			} finally {
				osmsharp.JSONP.callbacks[id] = osmsharp.JSONP.late;				// clean functions
				osmsharp.JSONP.timeouts[id] = osmsharp.JSONP.empty;
				osmsharp.JSONP.fences[id] = undefined;						// clean fence
			}
		};

		// wrap callback function
		osmsharp.JSONP.callbacks[id] = function (response) {
			clearTimeout(osmsharp.JSONP.timers[id]);						// clear timeout timer
			osmsharp.JSONP.timers[id] = undefined;

			try {
				callback_function(response, parameters);				// actual wrapped callback 
			} finally {
				osmsharp.JSONP.callbacks[id] = osmsharp.JSONP.empty;			// clean functions
				osmsharp.JSONP.timeouts[id] = osmsharp.JSONP.late;
				osmsharp.JSONP.fences[id] = undefined;						// clean fence
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
		script.src = source.replace(/%jsonp/, "osmsharp.JSONP.callbacks." + id);
		document.head.appendChild(script);

		// start timeout timer
		osmsharp.JSONP.timers[id] = setTimeout(osmsharp.JSONP.timeouts[id], timeout);
		return true;
	},

	clear: function(id) {
		clearTimeout(osmsharp.JSONP.timers[id]);					// clear timeout timer
		osmsharp.JSONP.callbacks[id] = osmsharp.JSONP.empty;			// clean functions
		osmsharp.JSONP.timeouts[id] = osmsharp.JSONP.empty;
		osmsharp.JSONP.fences[id] = undefined;						// clean fence

		// clean DOM
		var jsonp = document.getElementById('jsonp_'+id);
		if(jsonp)
			jsonp.parentNode.removeChild(jsonp);		
	},

	// reset all data
	reset: function() {
		osmsharp.JSONP.fences = {};
		osmsharp.JSONP.callbacks = {};
		osmsharp.JSONP.timeouts = {};
		osmsharp.JSONP.timers = {};
	}
};