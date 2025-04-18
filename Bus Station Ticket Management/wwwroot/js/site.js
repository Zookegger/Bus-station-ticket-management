// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    $('.route-select').select2({
        theme: 'bootstrap-5', // matches the theme you linked
        width: '100%',
        dropdownAutoWidth: true
    });
    $('.route-select').on('select2:open', function (e) {
        const selectContainer = $(this).next('.select2-container');
        if (selectContainer.length) {
            selectContainer.css('width', '100%');
        }
    });
});