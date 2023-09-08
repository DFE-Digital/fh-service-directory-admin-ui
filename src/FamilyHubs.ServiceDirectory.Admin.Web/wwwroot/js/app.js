// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


(function ($) {
	'use strict';
	function fhgov() {
		this.init = function () {
			restoreConditionalInputs();
		};

		this.showAlert = function (message) {
			alert(message);
		}

		let restoreConditionalInputs = function () {
			$("[data-conditional-active]").click();
		}
	}

	window.fhgov = new fhgov();
}
)(jQuery);

fhgov.init();