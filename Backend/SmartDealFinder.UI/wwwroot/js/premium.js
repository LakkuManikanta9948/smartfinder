(() => {
    const root = document.documentElement;
    const themeKey = 'smartcompare-theme';
    const savedTheme = localStorage.getItem(themeKey);
    const preferredTheme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';

    function setTheme(theme) {
        root.dataset.theme = theme;
        localStorage.setItem(themeKey, theme);
        document.querySelectorAll('[data-theme-toggle]').forEach((button) => {
            const dark = theme === 'dark';
            button.setAttribute('aria-label', dark ? 'Switch to light theme' : 'Switch to dark theme');
            button.setAttribute('aria-pressed', String(dark));
            const icon = button.querySelector('[data-theme-icon]');
            if (icon) icon.textContent = dark ? '☼' : '◐';
        });
    }

    setTheme(savedTheme === 'dark' || savedTheme === 'light' ? savedTheme : preferredTheme);
    document.querySelectorAll('[data-theme-toggle]').forEach((button) => {
        button.addEventListener('click', () => setTheme(root.dataset.theme === 'dark' ? 'light' : 'dark'));
    });

    document.querySelectorAll('[data-auto-submit]').forEach((control) => {
        control.addEventListener('change', () => control.form?.requestSubmit());
    });

    document.querySelectorAll('[data-menu-toggle]').forEach((button) => {
        const menu = document.getElementById(button.getAttribute('aria-controls'));
        if (!menu) return;
        button.addEventListener('click', () => {
            const expanded = button.getAttribute('aria-expanded') === 'true';
            button.setAttribute('aria-expanded', String(!expanded));
            menu.classList.toggle('is-open', !expanded);
        });
        menu.querySelectorAll('a').forEach((link) => link.addEventListener('click', () => {
            button.setAttribute('aria-expanded', 'false');
            menu.classList.remove('is-open');
        }));
    });

    const revealItems = document.querySelectorAll('.catalog-section, .product-card, .info-panel, .dashboard-panel, .stat-card, .offer-card, .admin-panel');
    if ('IntersectionObserver' in window && !window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
        const observer = new IntersectionObserver((entries, currentObserver) => {
            entries.forEach((entry) => {
                if (!entry.isIntersecting) return;
                entry.target.classList.add('is-visible');
                currentObserver.unobserve(entry.target);
            });
        }, { threshold: 0.08 });
        revealItems.forEach((item) => {
            item.classList.add('reveal');
            observer.observe(item);
        });
    }

    document.querySelectorAll('[data-product-thumbnail]').forEach((thumbnail) => {
        thumbnail.addEventListener('click', () => {
            const image = document.getElementById('product-primary-image');
            if (!image) return;
            image.src = thumbnail.dataset.imageUrl;
            image.alt = thumbnail.dataset.imageAlt;
            document.querySelectorAll('[data-product-thumbnail]').forEach((item) => item.classList.remove('is-selected'));
            thumbnail.classList.add('is-selected');
        });
    });

    if (window.matchMedia('(pointer: fine)').matches && !window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
        document.querySelectorAll('.product-card').forEach((card) => {
            card.addEventListener('pointermove', (event) => {
                const bounds = card.getBoundingClientRect();
                const x = (event.clientX - bounds.left) / bounds.width;
                const y = (event.clientY - bounds.top) / bounds.height;
                card.style.transform = `perspective(900px) rotateX(${(0.5 - y) * 4}deg) rotateY(${(x - 0.5) * 5}deg) translateY(-4px)`;
            });
            card.addEventListener('pointerleave', () => { card.style.transform = ''; });
        });
    }
})();
