document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".clickable-row").forEach(row => {
        row.addEventListener("click", function (e) {
            if (!e.target.closest('.action-btns')) {
                // Proceed with row click action
                const href = this.dataset.href;
                window.location.href = href;
            }
        });
    });
});
