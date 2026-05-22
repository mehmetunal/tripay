/**
 * TriPay Admin — ortak AJAX kabuk (Trimango ModuleFactory + ApiService).
 * Tüm formlar ve sayfa içi linkler AJAX ile çalışır.
 */
(function () {
    'use strict';

    var PAGE_ROOT_ID = 'admin-page-root';
    var CONTENT_SELECTOR = '[data-admin-content]';

    var AdminShellModule = ModuleFactory.createBaseModule({
        name: 'AdminShellModule',

        init: function () {
            this._bindAjaxForms();
            this._bindConfirmActions();
            this._initAutoDismiss();
            this._syncActiveNavLink();
            AdminPageRouter.init();
        },

        initEventListeners: function () {
            this._bindAjaxLinks();
            this._bindHistoryNavigation();
        },

        _initAutoDismiss: function () {
            document.querySelectorAll('[data-auto-dismiss]').forEach(function (el) {
                window.setTimeout(function () {
                    el.classList.add('opacity-0', 'transition-opacity', 'duration-300');
                    window.setTimeout(function () {
                        el.remove();
                    }, 300);
                }, 5000);
            });
        },

        _getPageRoot: function () {
            return document.getElementById(PAGE_ROOT_ID);
        },

        _getContentTarget: function () {
            var root = this._getPageRoot();
            if (!root) {
                return null;
            }
            return root.querySelector(CONTENT_SELECTOR) || root;
        },

        _bindAjaxForms: function () {
            var self = this;
            document.querySelectorAll('form[data-admin-ajax]').forEach(function (form) {
                if (form.getAttribute('data-admin-ajax-bound') === '1') {
                    return;
                }
                form.setAttribute('data-admin-ajax-bound', '1');
                form.addEventListener('submit', function (e) {
                    e.preventDefault();
                    var mode = form.getAttribute('data-admin-ajax') || 'json';
                    if (mode === 'html') {
                        self._submitHtmlForm(form);
                    } else {
                        self._submitJsonForm(form);
                    }
                });
            });
        },

        _bindAjaxLinks: function () {
            if (this._ajaxLinksBound) {
                return;
            }
            this._ajaxLinksBound = true;

            var self = this;
            document.addEventListener('click', function (e) {
                var link = e.target.closest('[data-admin-ajax-link]');
                if (!link || link.hasAttribute('data-admin-no-ajax')) {
                    return;
                }
                if (e.defaultPrevented || e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) {
                    return;
                }
                var href = link.getAttribute('href');
                if (!href || href.indexOf('#') === 0) {
                    return;
                }
                e.preventDefault();
                self.loadPage(href);
            });
        },

        _bindConfirmActions: function () {
            var self = this;
            document.querySelectorAll('form[data-admin-confirm]').forEach(function (form) {
                if (form.getAttribute('data-admin-confirm-bound') === '1') {
                    return;
                }
                form.setAttribute('data-admin-confirm-bound', '1');
                var msg = form.getAttribute('data-admin-confirm') || 'Devam etmek istiyor musunuz?';
                form.addEventListener('submit', function (e) {
                    if (form.getAttribute('data-admin-confirmed') === '1') {
                        form.removeAttribute('data-admin-confirmed');
                        return;
                    }
                    e.preventDefault();
                    e.stopImmediatePropagation();
                    UIService.modal.confirm('Onay', msg, function () {
                        form.setAttribute('data-admin-confirmed', '1');
                        if (typeof form.requestSubmit === 'function') {
                            form.requestSubmit();
                        } else {
                            form.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
                        }
                    });
                }, true);
            });
        },

        _submitHtmlForm: function (form) {
            var self = this;
            var $form = $(form);
            var url = $form.attr('action') || window.location.href;
            var method = ($form.attr('method') || 'GET').toUpperCase();
            var target = self._getContentTarget();

            if (target) {
                UIService.loader.show(target, 'Yükleniyor...');
            }

            $.ajax({
                url: url,
                type: method,
                data: $form.serialize(),
                headers: self._ajaxHeaders(),
                success: function (html) {
                    if (target) {
                        UIService.loader.hide(target);
                        target.innerHTML = html;
                        self._afterContentSwap();
                    }
                },
                error: function (xhr) {
                    if (target) {
                        UIService.loader.hide(target);
                    }
                    UIService.notification.error(xhr);
                }
            });
        },

        _submitJsonForm: function (form) {
            var self = this;
            if (!UIService.form.validate(form)) {
                return;
            }

            UIService.loader.show('global', 'Kaydediliyor...');

            ApiService.submitForm({
                form: form,
                url: $(form).attr('action'),
                method: $(form).attr('method') || 'POST',
                dataType: 'json',
                success: function (response) {
                    UIService.loader.hide('global');
                    if (response && response.success) {
                        if (response.message) {
                            UIService.notification.success(response.message);
                        }
                        if (response.redirectUrl) {
                            if (response.redirectUrl === window.location.pathname + window.location.search) {
                                self.loadPage(response.redirectUrl);
                            } else {
                                window.location.href = response.redirectUrl;
                            }
                        }
                    } else {
                        self._showJsonErrors(form, response);
                    }
                },
                error: function (xhr) {
                    UIService.loader.hide('global');
                    var data = xhr.responseJSON;
                    if (data && data.errors) {
                        self._showJsonErrors(form, data);
                    } else {
                        UIService.notification.error(xhr);
                    }
                }
            });
        },

        _showJsonErrors: function (form, response) {
            var summary = form.querySelector('[data-admin-validation-summary]');
            if (summary) {
                var messages = [];
                if (response.message) {
                    messages.push(response.message);
                }
                if (response.errors) {
                    Object.keys(response.errors).forEach(function (key) {
                        var arr = response.errors[key];
                        if (arr && arr.length) {
                            arr.forEach(function (m) {
                                messages.push(m);
                            });
                        }
                    });
                }
                summary.innerHTML = messages.map(function (m) {
                    return '<p>' + HelperService.escapeHtml(m) + '</p>';
                }).join('');
                summary.classList.remove('hidden');
            } else if (response.message) {
                UIService.notification.error(response.message);
            }
        },

        _ajaxHeaders: function () {
            return {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': HelperService.getCsrfToken()
            };
        },

        loadPage: function (url, pushState) {
            var self = this;
            if (pushState !== false) {
                history.pushState({ adminAjax: true }, '', url);
            }

            var target = self._getContentTarget();
            if (target) {
                UIService.loader.show(target, 'Yükleniyor...');
            }

            $.ajax({
                url: url,
                type: 'GET',
                headers: self._ajaxHeaders(),
                success: function (html) {
                    if (target) {
                        UIService.loader.hide(target);
                        target.innerHTML = html;
                        self._afterContentSwap();
                    }
                    var pageTitle = self._extractTitle(html);
                    if (pageTitle) {
                        document.title = pageTitle;
                        var titleEl = document.querySelector('[data-admin-page-title]');
                        if (titleEl) {
                            titleEl.textContent = pageTitle.replace(' — TriPay Admin', '');
                        }
                    }
                },
                error: function (xhr) {
                    if (target) {
                        UIService.loader.hide(target);
                    }
                    if (xhr.status === 401) {
                        window.location.href = (AppInit.getSettings().baseUrl || '/') + 'Account/Login';
                        return;
                    }
                    UIService.notification.error(xhr);
                }
            });
        },

        _extractTitle: function (html) {
            var match = html.match(/<h1[^>]*>([^<]+)<\/h1>/i);
            return match ? match[1].trim() + ' — TriPay Admin' : null;
        },

        _syncPageModule: function () {
            var root = this._getPageRoot();
            var target = this._getContentTarget();
            if (!root || !target) {
                return;
            }
            var marker = target.querySelector('[data-admin-module]');
            if (marker) {
                root.setAttribute('data-admin-module', marker.getAttribute('data-admin-module') || '');
            }
            AdminPageRouter.reset();
            AdminPageRouter.init();
        },

        _bindHistoryNavigation: function () {
            if (this._historyBound) {
                return;
            }
            this._historyBound = true;

            var self = this;
            window.addEventListener('popstate', function (e) {
                if (e.state && e.state.adminAjax) {
                    AdminPageRouter.reset();
                    self.loadPage(window.location.href, false);
                }
            });
        },

        _syncActiveNavLink: function () {
            var nav = document.querySelector('[data-admin-nav]');
            if (!nav) {
                return;
            }
            var path = window.location.pathname.toLowerCase();
            nav.querySelectorAll('a[data-admin-ajax-link]').forEach(function (link) {
                var href = (link.getAttribute('href') || '').toLowerCase();
                if (!href) {
                    return;
                }
                var isActive = path === href || (href.length > 1 && path.indexOf(href) === 0);
                link.classList.toggle('bg-slate-100', isActive);
                link.classList.toggle('font-semibold', isActive);
                link.classList.toggle('text-tripay-800', isActive);
            });
        },

        _afterContentSwap: function () {
            this._bindAjaxForms();
            this._bindConfirmActions();
            this._syncActiveNavLink();
            this._syncPageModule();
        }
    });

    /** Sayfa modülü yönlendirici */
    var AdminPageRouter = (function () {
        var registry = {};

        function register(name, module) {
            registry[name] = module;
        }

        function init() {
            var root = document.getElementById(PAGE_ROOT_ID);
            if (!root) {
                return;
            }
            var name = root.getAttribute('data-admin-module');
            if (!name || !registry[name]) {
                return;
            }
            var mod = registry[name];
            if (mod._adminInitialized) {
                return;
            }
            mod._adminInitialized = true;
            mod.init();
        }

        function reset() {
            Object.keys(registry).forEach(function (key) {
                registry[key]._adminInitialized = false;
            });
        }

        return {
            register: register,
            init: init,
            reset: reset
        };
    })();

    window.AdminShellModule = AdminShellModule;
    window.AdminPageRouter = AdminPageRouter;

    $(document).ready(function () {
        AppInit.configure({ baseUrl: '/', debug: false });
        AdminShellModule.init();
        window.setTimeout(function () {
            AdminPageRouter.init();
        }, 0);
    });
})();
