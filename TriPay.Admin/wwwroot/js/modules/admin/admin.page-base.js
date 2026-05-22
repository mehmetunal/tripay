/**
 * Admin sayfa modülleri için ortak DOM yardımcıları.
 */
(function () {
    'use strict';

    function getContentRoot() {
        var pageRoot = document.getElementById('admin-page-root');
        if (!pageRoot) {
            return null;
        }
        return pageRoot.querySelector('[data-admin-content]') || pageRoot;
    }

    function bindValidationSummaryClear() {
        var root = getContentRoot();
        if (!root) {
            return;
        }

        root.querySelectorAll('form[data-admin-ajax="json"]').forEach(function (form) {
            if (form.getAttribute('data-admin-validation-bound') === '1') {
                return;
            }
            form.setAttribute('data-admin-validation-bound', '1');

            function clearSummary() {
                var summary = form.querySelector('[data-admin-validation-summary]');
                if (summary && !summary.classList.contains('hidden')) {
                    summary.innerHTML = '';
                    summary.classList.add('hidden');
                }
            }

            form.addEventListener('input', clearSummary);
            form.addEventListener('change', clearSummary);
        });
    }

    function focusFirstField(selector) {
        var root = getContentRoot();
        if (!root) {
            return;
        }
        var el = root.querySelector(
            selector || 'form input:not([type=hidden]):not([disabled]), form select:not([disabled]), form textarea:not([disabled])'
        );
        if (el && typeof el.focus === 'function') {
            el.focus();
        }
    }

    function bindAutoSubmitHtmlForm(options) {
        options = options || {};
        var root = getContentRoot();
        if (!root) {
            return;
        }

        var form = root.querySelector(options.formSelector || 'form[data-admin-ajax="html"]');
        if (!form || form.getAttribute('data-auto-submit-bound') === '1') {
            return;
        }
        form.setAttribute('data-auto-submit-bound', '1');

        var triggers = form.querySelectorAll(options.triggerSelector || 'select, input[type="checkbox"]');
        triggers.forEach(function (el) {
            el.addEventListener('change', function () {
                if (typeof form.requestSubmit === 'function') {
                    form.requestSubmit();
                } else {
                    form.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
                }
            });
        });
    }

    function bindRolePanelAccessGuard() {
        var root = getContentRoot();
        if (!root) {
            return;
        }

        var form = root.querySelector('form[data-admin-ajax="json"]');
        if (!form || form.getAttribute('data-panel-access-guard') === '1') {
            return;
        }
        form.setAttribute('data-panel-access-guard', '1');

        form.addEventListener('submit', function (e) {
            var panelChecked = false;
            form.querySelectorAll('label').forEach(function (label) {
                var codeInput = label.querySelector('input[type="hidden"][name*=".Code"]');
                var checkbox = label.querySelector('input[type="checkbox"]');
                if (codeInput && checkbox && codeInput.value === 'panel.access' && checkbox.checked) {
                    panelChecked = true;
                }
            });

            if (!panelChecked) {
                e.preventDefault();
                e.stopImmediatePropagation();
                if (typeof UIService !== 'undefined' && UIService.notification) {
                    UIService.notification.info('En az "Panele giriş" izni seçilmelidir.');
                }
            }
        }, true);
    }

    function bindPayloadCopyOnClick() {
        var root = getContentRoot();
        if (!root) {
            return;
        }

        var pre = root.querySelector('pre');
        if (!pre || pre.getAttribute('data-payload-copy-bound') === '1') {
            return;
        }
        pre.setAttribute('data-payload-copy-bound', '1');
        pre.setAttribute('title', 'Kopyalamak için tıklayın');
        pre.classList.add('cursor-pointer');

        pre.addEventListener('click', function () {
            var text = pre.textContent || '';
            if (!text) {
                return;
            }
            if (navigator.clipboard && navigator.clipboard.writeText) {
                navigator.clipboard.writeText(text).then(function () {
                    UIService.notification.success('Payload panoya kopyalandı.');
                }).catch(function () {
                    UIService.notification.error('Kopyalama başarısız.');
                });
            }
        });
    }

    function bindSettingKeyPreview() {
        var root = getContentRoot();
        if (!root) {
            return;
        }

        var keyInput = root.querySelector('input[name="SettingKey"]');
        if (!keyInput || keyInput.getAttribute('data-setting-key-bound') === '1') {
            return;
        }
        keyInput.setAttribute('data-setting-key-bound', '1');
        keyInput.addEventListener('blur', function () {
            keyInput.value = (keyInput.value || '').trim();
        });
    }

    window.AdminPageBase = {
        getContentRoot: getContentRoot,
        bindValidationSummaryClear: bindValidationSummaryClear,
        focusFirstField: focusFirstField,
        bindAutoSubmitHtmlForm: bindAutoSubmitHtmlForm,
        bindRolePanelAccessGuard: bindRolePanelAccessGuard,
        bindPayloadCopyOnClick: bindPayloadCopyOnClick,
        bindSettingKeyPreview: bindSettingKeyPreview
    };
})();
