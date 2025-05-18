$(document).ready(function() {
    $('#deleteConfirmationModal').on('show.bs.modal', function () {
        $('.modal-backdrop').remove();
        var backdrop = $('<div class="modal-backdrop fade show"></div>');
        $('main').append(backdrop);
        backdrop.css('z-index', '1040');
    });

    $('#deleteConfirmationModal').on('hidden.bs.modal', function () {
        $('.modal-backdrop').remove();
        $('body').removeClass('modal-open');
    });
});