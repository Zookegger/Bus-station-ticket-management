document.addEventListener("DOMContentLoaded", function () {
    let password = document.getElementById("Password");
    let confirmPassword = document.getElementById("ConfirmPassword");
    let passwordError = document.getElementById("PasswordError");
    let passwordConfirmError = document.getElementById("PasswordConfirmError");
    let createButton = document.getElementById("submit");
    let specialCharacterRegex = /[!@#$%^&*(),.?":{}|<>]/;

    console.log(createButton);

    function checkPasswordRequirements() {
        if (password.value.length < 8) {
            passwordError.textContent = "Password must be 8 character long";
        } else if (!specialCharacterRegex.test(password.value)) {
            passwordError.textContent = "Password must contain at least one special character (!, @, #, $, %, etc.)";
        } else {
            passwordError.textContent = "";
        }
    }

    function validatePasswords() {
        let valid = true;

        if (password.value !== confirmPassword.value || password.value === "") {
            passwordConfirmError.textContent = "Password do not match";
            valid = false;
        } else {
            passwordConfirmError.textContent = "";
        }
        createButton.disabled = !valid;
    }

    password.addEventListener("input", checkPasswordRequirements);

    password.addEventListener("input", validatePasswords);
    confirmPassword.addEventListener("input", validatePasswords);
});