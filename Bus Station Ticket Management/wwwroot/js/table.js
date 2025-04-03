document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".clickable-row").forEach(row => {
        row.addEventListener("click", function () {
            window.location = this.dataset.href;
        });
    });
});
