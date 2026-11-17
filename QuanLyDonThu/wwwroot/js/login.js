const togglePassword = document.getElementById('togglePassword');

if (togglePassword) {
    togglePassword.addEventListener('click', function () {
        const passwordInput = document.getElementById('Password');
        const icon = document.getElementById('eyeIcon');
        const isPassword = passwordInput.type === 'password';

        passwordInput.type = isPassword ? 'text' : 'password';
        icon.classList.toggle('bi-eye-slash', !isPassword);
        icon.classList.toggle('bi-eye', isPassword);
        this.setAttribute('aria-label', isPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu');
    });
}
