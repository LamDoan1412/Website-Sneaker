// wwwroot/js/admin.js

document.addEventListener('DOMContentLoaded', function () {

    // ── Sidebar toggle (desktop) ──────────────────────
    const toggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('adminSidebar');
    const main = document.getElementById('adminMain');

    toggle?.addEventListener('click', () => {
        sidebar.classList.toggle('collapsed');
        main.classList.toggle('expanded');

        // Mobile
        sidebar.classList.toggle('mobile-open');
    });

    // ── Click ngoài sidebar để đóng (mobile) ─────────
    document.addEventListener('click', function (e) {
        if (window.innerWidth <= 992) {
            if (!sidebar.contains(e.target) && !toggle.contains(e.target)) {
                sidebar.classList.remove('mobile-open');
            }
        }
    });

    // ── Active link ───────────────────────────────────
    const currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll('.sidebar-link').forEach(link => {
        const href = link.getAttribute('href');
        if (href && currentPath.includes(href.toLowerCase()) && href !== '/') {
            link.classList.add('active');
        }
    });

    // ── Auto-dismiss alerts ───────────────────────────
    document.querySelectorAll('.alert').forEach(alert => {
        setTimeout(() => {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            bsAlert?.close();
        }, 4000);
    });

    // ── Confirm delete ────────────────────────────────
    document.querySelectorAll('[data-confirm]').forEach(btn => {
        btn.addEventListener('click', function (e) {
            const msg = this.getAttribute('data-confirm') || 'Bạn có chắc chắn?';
            if (!confirm(msg)) e.preventDefault();
        });
    });

    // ── Image preview khi upload ──────────────────────
    document.querySelectorAll('input[type=file][data-preview]').forEach(input => {
        input.addEventListener('change', function () {
            const previewId = this.getAttribute('data-preview');
            const preview = document.getElementById(previewId);
            if (preview && this.files[0]) {
                const reader = new FileReader();
                reader.onload = e => {
                    preview.src = e.target.result;
                    preview.style.display = 'block';
                };
                reader.readAsDataURL(this.files[0]);
            }
        });
    });

});