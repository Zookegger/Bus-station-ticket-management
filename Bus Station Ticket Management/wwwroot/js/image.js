

export function previewImage(input) {
    const preview = document.getElementById('avatarPreview');
    const defaultAvatarWrapper = document.getElementById('defaultAvatarWrapper');

    if (input.files && input.files[0]) {

        const reader = new FileReader();
        reader.onload = function (e) {
            preview.src = e.target.result;
            preview.style.display = 'block';
            if (defaultAvatarWrapper) {
                defaultAvatarWrapper.style.display = 'none';
            }
        };
        reader.readAsDataURL(input.files[0]);
    }
}