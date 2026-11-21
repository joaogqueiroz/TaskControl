// Task list: the search box hides the rows that do not contain the typed text
document.querySelectorAll('[data-search]').forEach(box => {
    const rows = document.querySelectorAll(box.dataset.search + ' tr');
    const count = document.querySelector('[data-count]');
    box.addEventListener('input', () => {
        const text = box.value.trim().toLowerCase();
        let shown = 0;
        rows.forEach(row => {
            const match = row.textContent.toLowerCase().includes(text);
            row.hidden = !match;
            if (match) shown++;
        });
        if (count) count.textContent = shown;
    });
});
