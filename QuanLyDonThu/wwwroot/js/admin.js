(() => {
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(element => {
        bootstrap.Tooltip.getOrCreateInstance(element);
    });

    document.querySelectorAll('form[data-confirm-native]').forEach(form => {
        form.addEventListener('submit', event => {
            if (!window.confirm(form.dataset.confirmNative)) event.preventDefault();
        });
    });

    document.querySelectorAll('[data-auto-dismiss]').forEach(alertElement => {
        window.setTimeout(() => {
            if (!alertElement.isConnected) return;
            const alert = bootstrap.Alert.getOrCreateInstance(alertElement);
            alert.close();
        }, 5000);
    });

    document.querySelectorAll('[data-bulk-list]').forEach(list => {
        const checkboxes = [...list.querySelectorAll('[data-row-select]')];
        const selectAll = list.querySelector('[data-select-all]');
        const toolbar = list.querySelector('[data-bulk-toolbar]');
        const selectedCount = list.querySelector('[data-selected-count]');
        const updateSelection = () => {
            const selected = checkboxes.filter(checkbox => checkbox.checked);
            if (toolbar) toolbar.classList.toggle('d-none', selected.length === 0);
            if (selectedCount) selectedCount.textContent = String(selected.length);
            if (selectAll) {
                selectAll.checked = checkboxes.length > 0 && selected.length === checkboxes.length;
                selectAll.indeterminate = selected.length > 0 && selected.length < checkboxes.length;
            }
        };

        selectAll?.addEventListener('change', () => {
            checkboxes.forEach(checkbox => { checkbox.checked = selectAll.checked; });
            updateSelection();
        });
        checkboxes.forEach(checkbox => checkbox.addEventListener('change', updateSelection));
        updateSelection();
    });
})();
