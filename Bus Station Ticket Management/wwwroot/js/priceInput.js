// Skeleton code for handling price input formatting
/*
<div class="form-group mb-3">
    <label asp-for="Price" class="control-label"></label>
    <!-- Input for smaller screens -->
    <input asp-for="Price" type="tel" pattern="[0-9]*" id="priceInputSmall"
        class="form-control d-md-none d-block" placeholder="..VND" />
    <!-- Input for larger screens -->
    <input asp-for="Price" type="text" id="priceInputLarge" class="form-control d-none d-md-block"
        placeholder="..VND" />
    <span asp-validation-for="Price" class="text-danger"></span>
</div>
*/
document.addEventListener("DOMContentLoaded", function () {
	// Price input for smaller screens
	const priceInputSmall = document.getElementById("priceInputSmall");
	// Price input for larger screens
	const priceInputLarge = document.getElementById("priceInputLarge");

	// Function to handle input formatting
	function formatPriceInput(inputElement) {
		inputElement.addEventListener("input", function (e) {
			let position = this.selectionStart;
			let oldLength = this.value.length;

			// Remove all non-digit characters (e.g., dots)
			let rawValue = this.value.replace(/\D/g, "");

			// Format with dot separators
			let formatted = rawValue.replace(/\B(?=(\d{3})+(?!\d))/g, ".");
			this.value = formatted;

			// Adjust cursor position
			let newLength = this.value.length;
			position += newLength - oldLength;
			this.setSelectionRange(position, position);
		});
	}

	function fillNumber(inputElement) {
		inputElement.addEventListener("input", function (e) {
			if (inputElement === priceInputSmall)
				priceInputLarge.value = priceInputSmall.value;
			else if (inputElement === priceInputLarge)
				priceInputSmall.value = priceInputLarge.value;
		});
	}

	// Call the function for both inputs
	formatPriceInput(priceInputSmall);
	formatPriceInput(priceInputLarge);
	fillNumber(priceInputSmall);
	fillNumber(priceInputLarge);
});