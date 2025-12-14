/*! Parser: ISO-8601 date - updated 10/26/2014 (v2.18.0) */
/* This parser works with dates in ISO8601 format
 * 2013-02-18T18:18:44+00:00
 * Written by Sean Ellingham :https://github.com/seanellingham
 * See https://github.com/Mottie/tablesorter/issues/247
 */
/*global jQuery: false */
(function($) {
	'use strict';

    var fedDate = /^(([0]?[1-9]|[1-3][0-9])\/([0-2]?[0-9]|3[0-1])\/([1-2]\d{3})) (20|21|22|23|[0-1]?\d{1}):([0-5]?\d{1})$/;

	$.tablesorter.addParser({
		id : 'fedDate',
		is : function(s) {
            return s ? s.match(fedDate) : false;
		},
		format : function(s) {
            var result = s ? s.match(fedDate) : s;
			if (result) {
				var date = new Date();
                if (result[2]) { date.setDate(result[2]); }
                if (result[3]) { date.setMonth(result[3] - 1); }
                if (result[4]) { date.setFullYear(result[4]); }
				if (result[5]) { date.setHours(result[5]); }
				if (result[6]) { date.setMinutes(result[6]); }
				return date.getTime();
			}
			return s;
		},
		type : 'numeric'
	});

})(jQuery);
