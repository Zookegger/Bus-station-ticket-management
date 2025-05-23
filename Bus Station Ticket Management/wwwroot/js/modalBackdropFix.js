$(document).ready(function() {
    // Fix for modal backdrop
    $('#deleteConfirmationModal').on('show.bs.modal', function () {
        let modal = $('#deleteConfirmationModal');

        $('body').append(modal);
    });

    // Clean up when modal is hidden
    $('#deleteConfirmationModal').on('hidden.bs.modal', function () {
        $('.modal-backdrop').remove();
        $('body').removeClass('modal-open');
    });
});