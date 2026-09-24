/* TubeMail Gorilla web edition - interactions ported from the MAUI Unlocked app
   (CollapsibleCard tap gesture, Send page token chips/template load/rotation,
    Settings switch auto-save + shortcode selection, busy states). */

(function () {
    'use strict';

    /* ------------------------------------------- CollapsibleCard header tap */
    document.querySelectorAll('.card-header').forEach(function (header) {
        header.addEventListener('click', function () {
            var card = header.closest('.card');
            if (card) card.classList.toggle('is-collapsed');
        });
    });

    /* ------------------------------------------------------ Send: token chips */
    // MAUI tracks the last focused compose field (Subject vs Body) and inserts
    // the tapped [token] there, at the caret.
    var subjectInput = document.getElementById('subjectInput');
    var bodyInput = document.getElementById('bodyInput');
    var lastFocused = null;
    if (subjectInput) subjectInput.addEventListener('focus', function () { lastFocused = subjectInput; });
    if (bodyInput) bodyInput.addEventListener('focus', function () { lastFocused = bodyInput; });

    document.querySelectorAll('.token-chip').forEach(function (chip) {
        chip.addEventListener('click', function () {
            var token = chip.getAttribute('data-token') || chip.textContent.trim();
            var target = lastFocused || bodyInput || subjectInput;
            if (!target) return;
            var value = target.value || '';
            var start = (target === lastFocused && typeof target.selectionStart === 'number')
                ? target.selectionStart : value.length;
            target.value = value.slice(0, start) + token + ' ' + value.slice(start);
            target.focus();
            var caret = start + token.length + 1;
            try { target.setSelectionRange(caret, caret); } catch (e) { }
        });
    });

    /* --------------------------------------------- Send: template picker load */
    var templateSelect = document.getElementById('templateSelect');
    var templateStatus = document.getElementById('templateStatus');
    if (templateSelect) {
        templateSelect.addEventListener('change', function () {
            var opt = templateSelect.options[templateSelect.selectedIndex];
            if (!opt || !opt.value) {
                if (templateStatus) templateStatus.textContent = '';
                return;
            }
            if (subjectInput && opt.getAttribute('data-subject')) subjectInput.value = opt.getAttribute('data-subject');
            if (bodyInput && opt.getAttribute('data-body')) bodyInput.value = opt.getAttribute('data-body');
            if (templateStatus) {
                templateStatus.textContent = 'Loaded "' + opt.textContent.trim() + '" into the form. Tweak it if you like, then hit Send Campaign.';
            }
        });
    }

    /* ------------------------------------- Send: rotation visibility + autosave */
    var rotationSwitch = document.getElementById('messageRotationSwitch');
    var composeSection = document.getElementById('composeSection');
    var variationsSection = document.getElementById('variationsSection');

    function refreshRotationUi() {
        var on = rotationSwitch && rotationSwitch.checked;
        if (composeSection) composeSection.classList.toggle('hidden', on);
        if (variationsSection) variationsSection.classList.toggle('hidden', !on);
    }
    if (rotationSwitch) {
        rotationSwitch.addEventListener('change', refreshRotationUi);
        refreshRotationUi();
    }

    // Auto-post settings as soon as a switch/select changes (MAUI Toggled handlers).
    var saveOpts = document.getElementById('saveOptsBtn');
    document.querySelectorAll('[data-autosave]').forEach(function (control) {
        control.addEventListener('change', function () {
            if (saveOpts) saveOpts.click();
        });
    });

    /* ---------------------------------------- Send: variation editor open/close */
    var addVariationBtn = document.getElementById('addVariationBtn');
    var cancelVariationBtn = document.getElementById('cancelVariationBtn');
    var variationEditor = document.getElementById('variationEditor');
    if (addVariationBtn && variationEditor) {
        addVariationBtn.addEventListener('click', function (e) {
            e.preventDefault();
            variationEditor.classList.remove('hidden');
            var vs = document.getElementById('varSubjectInput');
            if (vs) vs.focus();
        });
    }
    if (cancelVariationBtn && variationEditor) {
        cancelVariationBtn.addEventListener('click', function (e) {
            e.preventDefault();
            variationEditor.classList.add('hidden');
        });
    }

    /* ------------------------------------------- Settings: shortcode selection */
    document.querySelectorAll('.list-row.selectable').forEach(function (row) {
        row.addEventListener('click', function (e) {
            if (e.target.closest('form') || e.target.closest('button')) return;
            document.querySelectorAll('.list-row.selectable.selected')
                .forEach(function (r) { r.classList.remove('selected'); });
            row.classList.add('selected');
            var hidden = document.getElementById('selectedParameterId');
            if (hidden) hidden.value = row.getAttribute('data-id') || '';
        });
    });

    /* ------------------------------------------------------- busy-on-submit */
    document.querySelectorAll('form.js-busy').forEach(function (form) {
        form.addEventListener('submit', function () {
            form.querySelectorAll('button[type="submit"]').forEach(function (btn) { btn.disabled = true; });
            var busy = form.querySelector('[data-busy-target]');
            if (busy) busy.insertAdjacentHTML('beforeend', ' <span class="spinner"></span>');
        });
    });

    /* ---------------------------------------------------- confirm-on-submit */
    document.querySelectorAll('form.js-confirm').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            var message = form.getAttribute('data-confirm') || 'Are you sure?';
            if (!window.confirm(message)) e.preventDefault();
        });
    });

    /* ------------------------------- contacts: debounce live search (GET submit) */
    var searchInput = document.getElementById('contactSearch');
    var filterForm = document.getElementById('contactFilterForm');
    var searchTimer = null;
    if (searchInput && filterForm) {
        searchInput.addEventListener('input', function () {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(function () {
                var pos = searchInput.selectionStart;
                filterForm.submit();
                try { searchInput.setSelectionRange(pos, pos); } catch (e) { }
            }, 350);
        });
    }
    var sortSelect = document.getElementById('contactSort');
    if (sortSelect && filterForm) {
        sortSelect.addEventListener('change', function () { filterForm.submit(); });
    }
})();
