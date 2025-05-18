function checkPasswordRequirements(passwordInput, confirmPasswordInput, strengthMeter, requirements, passwordError) {
	if (!passwordInput) {
		console.error("Password input not found");
		return false;
	}

	if (!confirmPasswordInput) {
		console.error("Confirm password input not found"); 
		return false;
	}

	if (!strengthMeter) {
		console.error("Strength meter not found");
		return false;
	}

	if (!requirements) {
		console.error("Requirements not found");
		return false;
	}

	if (!passwordError) {
		console.error("Password error element not found");
		return false;
	}

	passwordInput.addEventListener("input", () => {
		try {
			const password = passwordInput.value;
			let isValid = true;

			const checks = [
				{ test: password.length >= 8, element: requirements.length },
				{ test: /[A-Z]/.test(password), element: requirements.uppercase },
				{ test: /[a-z]/.test(password), element: requirements.lowercase },
				{ test: /[0-9]/.test(password), element: requirements.number },
				{ test: /[!@#$%^&*(),.?":{}|<>]/.test(password), element: requirements.special }
			];

			checks.forEach(({ test, element }) => {
				if (element) {
					updateRequirement(element, test);
					if (!test) isValid = false;
				}
			});

			const strength = checks.filter(c => c.test).length * 20;
			updateStrengthMeter(strengthMeter, strength);

			return isValid;
		} catch (error) {
			console.error("Error checking password requirements:", error);
			return false;
		}
	});
}

function updateRequirement(element, isValid) {
	if (!element) return;

	const icon = element.querySelector("i");

	if (isValid) {
		element.classList.add("valid");
		if (icon) {
			icon.classList.remove("bi-x-circle-fill", "text-danger");
			icon.classList.add("bi-check-circle-fill", "text-success");
		}
	} else {
		element.classList.remove("valid");
		if (icon) {
			icon.classList.remove("bi-check-circle-fill", "text-success");
			icon.classList.add("bi-x-circle-fill", "text-danger");
		}
	}
}

function checkPasswordMatch(passwordInput, confirmPasswordInput, passwordError) {
	if (!passwordInput || !confirmPasswordInput || !passwordError) return;

	const password = passwordInput.value;
	const confirmPassword = confirmPasswordInput.value;

	if (confirmPassword === "") {
		passwordError.textContent = "";
		return false;
	}

	if (password !== confirmPassword) {
		passwordError.textContent = "Passwords do not match";
		return false;
	}

	passwordError.textContent = "";
	return true;
}

function calculateStrength(password) {
	let strength = 0;
	strength += password.length >= 8 ? 20 : 0;
	strength += /[A-Z]/.test(password) ? 20 : 0;
	strength += /[a-z]/.test(password) ? 20 : 0;
	strength += /\d/.test(password) ? 20 : 0;
	strength += /[!@#$%^&*(),.?":{}|<>]/.test(password) ? 20 : 0;
	return strength;
}

function updateStrengthMeter(meter, strength) {
	meter.style.width = strength + "%";
	meter.className = "progress-bar";
	meter.classList.toggle("bg-danger", strength <= 40);
	meter.classList.toggle("bg-warning", strength > 40 && strength <= 80);
	meter.classList.toggle("bg-success", strength > 80);
}

function validatePasswords(passwordInput, confirmPasswordInput, passwordError) {
	try {
		const submitButton = document.getElementById("submit");
		const passwordConfirmError = document.getElementById("PasswordConfirmError");

		const valid = passwordInput.value === confirmPasswordInput.value && passwordInput.value !== "";
		passwordConfirmError.textContent = valid ? "" : "Password do not match";
		submitButton.disabled = !valid;
	} catch (error) {
		console.error("Error validating passwords:", error);
	}
}

function updateSubmitButton(submitButton, passwordInput, confirmPasswordInput, requirements, strengthMeter, passwordError) {
	if (!submitButton) return;

	const passwordValid = checkPasswordRequirements(passwordInput, confirmPasswordInput, strengthMeter, requirements, passwordError);

	passwordInput.addEventListener("input", () => {
		try {
			const password = passwordInput.value;
			updateStrengthMeter(strengthMeter, calculateStrength(password));
            validatePasswords(passwordInput, confirmPasswordInput, passwordError)
		} catch (error) {
			console.error("Error updating strength meter:", error);
		}
	});

	confirmPasswordInput.addEventListener("input", () => {
        try {
            validatePasswords(passwordInput, confirmPasswordInput, passwordError)
        } catch (error) {
            console.error("Error updating strength meter:", error);
        }
	});

	const passwordsMatch = checkPasswordMatch(passwordInput, confirmPasswordInput, passwordError);
	const passwordNotEmpty = passwordInput.value !== "";

    console.log(passwordInput.value);
    console.log(confirmPasswordInput.value);

	console.log(passwordValid, passwordsMatch, passwordNotEmpty);

	submitButton.disabled = !(passwordValid && passwordsMatch && passwordNotEmpty);
}

document.addEventListener("DOMContentLoaded", () => {
	try {
		const passwordInput = document.getElementById("Password");
		const confirmPasswordInput = document.getElementById("ConfirmPassword");
		const submitButton = document.getElementById("submit");
		const passwordError = document.getElementById("PasswordError");
		const togglePassword = document.getElementById("togglePassword");
		const toggleConfirmPassword = document.getElementById("toggleConfirmPassword");
		const strengthMeter = document.getElementById("passwordStrengthMeter");

		const requirements = {
			length: document.getElementById("length-check"),
			uppercase: document.getElementById("uppercase-check"),
			lowercase: document.getElementById("lowercase-check"),
			number: document.getElementById("number-check"),
			special: document.getElementById("special-check")
		};

		if (!passwordInput || !confirmPasswordInput) {
			console.error("Password input elements not found");
			return;
		}

		togglePassword?.addEventListener("click", function () {
			try {
				const type = passwordInput.getAttribute("type") === "password" ? "text" : "password";
				passwordInput.setAttribute("type", type);
				this.querySelector("i").classList.toggle("bi-eye");
				this.querySelector("i").classList.toggle("bi-eye-slash");
			} catch (error) {
				console.error("Error toggling password:", error);
			}
		});

		toggleConfirmPassword?.addEventListener("click", function () {
			try {
				const type = confirmPasswordInput.getAttribute("type") === "password" ? "text" : "password";
				confirmPasswordInput.setAttribute("type", type);
				this.querySelector("i").classList.toggle("bi-eye");
				this.querySelector("i").classList.toggle("bi-eye-slash");
			} catch (error) {
				console.error("Error toggling confirm password:", error);
			}
		});

		passwordInput.addEventListener("input", () =>
			updateSubmitButton(submitButton, passwordInput, confirmPasswordInput, requirements, strengthMeter, passwordError)
        );

		confirmPasswordInput.addEventListener("input", () =>
			updateSubmitButton(submitButton, passwordInput, confirmPasswordInput, requirements, strengthMeter, passwordError)
		);

		updateSubmitButton(submitButton, passwordInput, confirmPasswordInput, requirements, strengthMeter, passwordError);
	} catch (error) {
		console.error("Error initializing password validation:", error);
	}
});
