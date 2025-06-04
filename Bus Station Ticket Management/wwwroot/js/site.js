// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
	try {
		$(".route-select").select2({
			theme: "bootstrap-5",
			allowClear: true,
			width: "100%",
			dropdownAutoWidth: true,
		});
		$(".route-select").on("select2:open", function (e) {
			const selectContainer = $(this).next(".select2-container");
			if (selectContainer.length) {
				selectContainer.css("width", "100%");
			}
		});
	} catch (error) {
		console.error("Error initializing Select2:", error);
	}
});

function lockScroll() {
	document.body.style.overflowY = "hidden";
	document.documentElement.style.overflowY = "hidden";
}

function unlockScroll() {
	document.body.style.overflowY = "auto";
	document.documentElement.style.overflowY = "auto";
}
