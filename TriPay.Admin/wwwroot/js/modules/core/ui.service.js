/**
 * UI Service Module
 * Genel UI işlemlerini içeren servis modülü
 * Tüm modüller tarafından kullanılabilecek ortak UI işlevleri
 * Hexagon projesinden alınan yapı + Trimango Tailwind tasarımı
 */
var UIService = UIService || (function() {
    'use strict';
    
    // UI bildirimlerinin tüm katmanların üzerinde görünmesi için global z-index
    const UI_OVERLAY_Z_INDEX = 2147483647;
    
    // ============================================================================
    // TOOLTIP VE POPOVER
    // ============================================================================
    
    /**
     * Tooltip'leri başlatır
     * @param {string} selector - Tooltip seçicisi
     */
    function initTooltips(selector) {
        selector = selector || '[data-bs-toggle="tooltip"]';
        if (typeof bootstrap === 'undefined' || !bootstrap.Tooltip) {
            return;
        }
        
        var tooltipTriggerList = [].slice.call(document.querySelectorAll(selector));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            var existingTooltip = bootstrap.Tooltip.getInstance(tooltipTriggerEl);
            if (existingTooltip) {
                existingTooltip.dispose();
            }
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
    
    /**
     * Popover'ları başlatır
     * @param {string} selector - Popover seçicisi
     */
    function initPopovers(selector) {
        selector = selector || '[data-bs-toggle="popover"]';
        if (typeof bootstrap === 'undefined' || !bootstrap.Popover) {
            return;
        }
        
        var popoverTriggerList = [].slice.call(document.querySelectorAll(selector));
        popoverTriggerList.map(function (popoverTriggerEl) {
            var existingPopover = bootstrap.Popover.getInstance(popoverTriggerEl);
            if (existingPopover) {
                existingPopover.dispose();
            }
            return new bootstrap.Popover(popoverTriggerEl);
        });
    }
    
    // ============================================================================
    // TOAST BİLDİRİMLERİ (toastr kullanıyor)
    // ============================================================================
    
    const toast = {
        /**
         * Toast bildirimi gösterir
         * @param {string} message - Bildirim mesajı 
         * @param {string} type - Bildirim tipi (success, error, warning, info)
         * @param {number} duration - Bildirim süresi (ms)
         * @param {object} options - Ek ayarlar
         */
        show: function(message, type = 'info', duration = 3000, options = {}) {
            // Eğer toastr yüklüyse kullan
            if (typeof toastr !== 'undefined') {
                const toastrOptions = typeof $ !== 'undefined' && $.extend ? $.extend({
                    closeButton: true,
                    progressBar: true,
                    positionClass: 'toast-top-right',
                    timeOut: duration
                }, options) : Object.assign({
                    closeButton: true,
                    progressBar: true,
                    positionClass: 'toast-top-right',
                    timeOut: duration
                }, options);
                
                toastr.options = toastrOptions;
                toastr[type](message);

                // Toast container'ı her zaman en üstte tut
                if (typeof document !== 'undefined') {
                    var toastContainer = document.getElementById('toast-container');
                    if (toastContainer) {
                        toastContainer.style.zIndex = String(UI_OVERLAY_Z_INDEX);
                    }
                }
            } else {
                // Fallback - notification kullan
                if (typeof notification !== 'undefined' && notification[type]) {
                    notification[type](message);
                }
            }
        },
        
        success: function(message, duration = 3000, options = {}) {
            this.show(message, 'success', duration, options);
        },
        
        error: function(message, duration = 3000, options = {}) {
            this.show(message, 'error', duration, options);
        },
        
        warning: function(message, duration = 3000, options = {}) {
            this.show(message, 'warning', duration, options);
        },
        
        info: function(message, duration = 3000, options = {}) {
            this.show(message, 'info', duration, options);
        }
    };
    
    // ============================================================================
    // NOTIFICATION BİLDİRİMLERİ (Tailwind öncelikli, SweetAlert fallback)
    // ============================================================================
    
    const notification = {
        /**
         * HTML escape helper
         * @private
         */
        _escapeHtml: function(text) {
            if (!text) return '';
            var map = {
                '&': '&amp;',
                '<': '&lt;',
                '>': '&gt;',
                '"': '&quot;',
                "'": '&#039;'
            };
            return String(text).replace(/[&<>"']/g, function(m) {
                return map[m];
            });
        },
        
        /**
         * Tailwind CSS uyumlu notification gösterir (ana tasarım)
         * @private
         */
        _showTailwindNotification: function(message, type, title) {
            if (!message) {
                return;
            }
            
            // ✅ Tailwind CSS uyumlu notification tasarımı
            var bgColor = type === 'success' ? 'bg-green-50 dark:bg-green-900/20 border-green-200 dark:border-green-800 text-green-800 dark:text-green-200' :
                          type === 'error' ? 'bg-red-50 dark:bg-red-900/20 border-red-200 dark:border-red-800 text-red-800 dark:text-red-200' :
                          type === 'warning' ? 'bg-yellow-50 dark:bg-yellow-900/20 border-yellow-200 dark:border-yellow-800 text-yellow-800 dark:text-yellow-200' :
                          'bg-blue-50 dark:bg-blue-900/20 border-blue-200 dark:border-blue-800 text-blue-800 dark:text-blue-200';
            
            var icon = type === 'success' ? 'check_circle' :
                       type === 'error' ? 'error' :
                       type === 'warning' ? 'warning' :
                       'info';
            
            // ✅ Notification container oluştur (eğer yoksa)
            var notificationContainer = document.getElementById('notification-container');
            if (!notificationContainer) {
                notificationContainer = document.createElement('div');
                notificationContainer.id = 'notification-container';
                notificationContainer.style.cssText = 'position: fixed; top: 20px; right: 20px; z-index: ' + UI_OVERLAY_Z_INDEX + '; max-width: 400px; width: 100%; pointer-events: none;';
                document.body.appendChild(notificationContainer);
            }
            
            // ✅ Notification element oluştur
            var notification = document.createElement('div');
            notification.className = 'notification-item rounded-lg border px-4 py-3 mb-3 shadow-lg transform transition-all duration-300 ease-in-out opacity-0 translate-x-full pointer-events-auto ' + bgColor;
            notification.setAttribute('role', 'alert');
            
            var titleHtml = title ? '<div class="font-semibold text-sm mb-1">' + this._escapeHtml(title) + '</div>' : '';
            var messageHtml = '<div class="text-sm">' + this._escapeHtml(message) + '</div>';
            
            notification.innerHTML = 
                '<div class="flex items-start gap-3">' +
                    '<span class="material-symbols-outlined text-base flex-shrink-0 mt-0.5">' + icon + '</span>' +
                    '<div class="flex-1 min-w-0">' +
                        titleHtml +
                        messageHtml +
                    '</div>' +
                    '<button type="button" class="notification-close flex-shrink-0 text-current opacity-70 hover:opacity-100 transition-opacity" aria-label="Close">' +
                        '<span class="material-symbols-outlined text-base">close</span>' +
                    '</button>' +
                '</div>';
            
            notificationContainer.appendChild(notification);
            
            // ✅ Animasyon ile göster
            setTimeout(function() {
                notification.style.opacity = '1';
                notification.style.transform = 'translateX(0)';
            }, 10);
            
            // ✅ Close butonu event listener
            var self = this;
            var closeBtn = notification.querySelector('.notification-close');
            if (closeBtn) {
                closeBtn.addEventListener('click', function() {
                    hideNotification();
                });
            }
            
            // ✅ Otomatik kapanma
            var autoHideTimeout = setTimeout(function() {
                hideNotification();
            }, 5000);
            
            // ✅ Hide fonksiyonu
            function hideNotification() {
                if (autoHideTimeout) {
                    clearTimeout(autoHideTimeout);
                }
                notification.style.opacity = '0';
                notification.style.transform = 'translateX(100%)';
                setTimeout(function() {
                    if (notification.parentNode) {
                        notification.parentNode.removeChild(notification);
                    }
                    // ✅ Eğer container boşsa, container'ı da kaldır
                    if (notificationContainer && notificationContainer.children.length === 0) {
                        if (notificationContainer.parentNode) {
                            notificationContainer.parentNode.removeChild(notificationContainer);
                        }
                    }
                }, 300);
            }
        },
        
        /**
         * Bildirim gösterir
         * @param {object} options - SweetAlert opsiyonları
         * @returns {Promise} - Promise nesnesi
         */
        show: function(options = {}) {
            // ✅ Öncelik: Tailwind notification (bizim tasarımımız)
            if (!options.forceSwal) {
                this._showTailwindNotification(
                    options.text || options.html || '', 
                    options.icon || 'info', 
                    options.title
                );
                return Promise.resolve();
            }
            
            // Fallback: SweetAlert (eğer options içinde forceSwal varsa)
            if (typeof Swal !== 'undefined') {
                return Swal.fire(options);
            } else {
                // SweetAlert yoksa Tailwind kullan
                this._showTailwindNotification(
                    options.text || options.html || '', 
                    options.icon || 'info', 
                    options.title
                );
                return Promise.resolve();
            }
        },
        
        /**
         * Başarı bildirimi gösterir
         * @param {string} message - Bildirim mesajı
         * @param {function} callback - İşlem sonrası çağrılacak fonksiyon
         * @param {object} options - Ek opsiyonlar
         */
        success: function(message, callback, options = {}) {
            // ✅ Öncelik: Tailwind notification (bizim tasarımımız)
            this._showTailwindNotification(message, 'success', 'Başarılı!');
            
            if (callback) {
                setTimeout(function() {
                    callback();
                }, 100);
            }
            
            // Fallback: SweetAlert (eğer options içinde forceSwal varsa)
            if (options.forceSwal && typeof Swal !== 'undefined') {
                const alertOptions = typeof $ !== 'undefined' && $.extend ? $.extend({
                    icon: 'success',
                    title: 'Başarılı!',
                    text: message,
                    confirmButtonText: 'Tamam'
                }, options) : Object.assign({
                    icon: 'success',
                    title: 'Başarılı!',
                    text: message,
                    confirmButtonText: 'Tamam'
                }, options);
                
                Swal.fire(alertOptions).then((result) => {
                    if (result.isConfirmed && callback) {
                        callback(result);
                    }
                });
            }
        },
        
        /**
         * Hata bildirimi gösterir
         * @param {object|string} error - Hata nesnesi veya mesajı
         * @param {object} options - Ek opsiyonlar
         */
        error: function(error, options = {}) {
            console.error('Hata oluştu:', error);
            
            // Hata mesajını parse et
            let errorMessage = 'Bir hata oluştu';
            if (typeof error === 'string') {
                errorMessage = error;
            } else if (error && error.responseJSON && error.responseJSON.message) {
                errorMessage += ': ' + error.responseJSON.message;
            } else if (error && error.statusText) {
                errorMessage += ': ' + error.statusText;
            }
            
            // ✅ Öncelik: Tailwind notification (bizim tasarımımız)
            this._showTailwindNotification(errorMessage, 'error', 'Hata!');
            
            // Fallback: SweetAlert (eğer options içinde forceSwal varsa)
            if (options.forceSwal && typeof Swal !== 'undefined') {
                const alertOptions = typeof $ !== 'undefined' && $.extend ? $.extend({
                    icon: 'error',
                    title: 'Hata!',
                    text: errorMessage,
                    confirmButtonText: 'Tamam'
                }, options) : Object.assign({
                    icon: 'error',
                    title: 'Hata!',
                    text: errorMessage,
                    confirmButtonText: 'Tamam'
                }, options);
                
                Swal.fire(alertOptions);
            }
        },
        
        /**
         * Bilgi bildirimi gösterir
         * @param {string} message - Bildirim mesajı
         * @param {object} options - Ek opsiyonlar
         */
        info: function(message, options = {}) {
            // ✅ Öncelik: Tailwind notification (bizim tasarımımız)
            this._showTailwindNotification(message, 'info', 'Bilgi');
            
            // Fallback: SweetAlert (eğer options içinde forceSwal varsa)
            if (options.forceSwal && typeof Swal !== 'undefined') {
                const alertOptions = typeof $ !== 'undefined' && $.extend ? $.extend({
                    icon: 'info',
                    title: 'Bilgi',
                    text: message,
                    confirmButtonText: 'Tamam'
                }, options) : Object.assign({
                    icon: 'info',
                    title: 'Bilgi',
                    text: message,
                    confirmButtonText: 'Tamam'
                }, options);
                
                Swal.fire(alertOptions);
            }
        },
        
        /**
         * Uyarı bildirimi gösterir
         * @param {string} message - Bildirim mesajı
         * @param {object} options - Ek opsiyonlar
         */
        warning: function(message, options = {}) {
            // ✅ Öncelik: Tailwind notification (bizim tasarımımız)
            this._showTailwindNotification(message, 'warning', 'Uyarı');
            
            // Fallback: SweetAlert (eğer options içinde forceSwal varsa)
            if (options.forceSwal && typeof Swal !== 'undefined') {
                const alertOptions = typeof $ !== 'undefined' && $.extend ? $.extend({
                    icon: 'warning',
                    title: 'Uyarı',
                    text: message,
                    confirmButtonText: 'Tamam'
                }, options) : Object.assign({
                    icon: 'warning',
                    title: 'Uyarı',
                    text: message,
                    confirmButtonText: 'Tamam'
                }, options);
                
                Swal.fire(alertOptions);
            }
        },
        
        /**
         * Onay bildirimi gösterir (SweetAlert kullanır - dialog için uygun)
         * @param {string} title - Bildirim başlığı
         * @param {string} text - Bildirim metni
         * @param {function} confirmCallback - Onay sonrası çağrılacak fonksiyon
         * @param {function} cancelCallback - İptal sonrası çağrılacak fonksiyon
         * @param {object} options - Ek opsiyonlar
         */
        confirm: function(title, text, confirmCallback, cancelCallback, options = {}) {
            if (typeof Swal !== 'undefined') {
                const alertOptions = typeof $ !== 'undefined' && $.extend ? $.extend({
                    title: title,
                    text: text,
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#3085d6',
                    cancelButtonColor: '#d33',
                    confirmButtonText: 'Evet',
                    cancelButtonText: 'İptal'
                }, options) : Object.assign({
                    title: title,
                    text: text,
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#3085d6',
                    cancelButtonColor: '#d33',
                    confirmButtonText: 'Evet',
                    cancelButtonText: 'İptal'
                }, options);
                
                Swal.fire(alertOptions).then((result) => {
                    if (result.isConfirmed) {
                        if (confirmCallback) confirmCallback(result);
                    } else if (cancelCallback) {
                        cancelCallback(result);
                    }
                });
            } else {
                // Fallback - window.confirm kullan
                if (window.confirm(title + '\n\n' + text)) {
                    if (confirmCallback) confirmCallback();
                } else if (cancelCallback) {
                    cancelCallback();
                }
            }
        },
        
        /**
         * Yükleme bildirimi gösterir
         * @param {string} message - Gösterilecek mesaj
         * @param {object} options - Ek opsiyonlar
         * @returns {Promise} - Promise nesnesi
         */
        loading: function(message = 'İşlem yapılıyor...', options = {}) {
            if (typeof Swal !== 'undefined') {
                const alertOptions = typeof $ !== 'undefined' && $.extend ? $.extend({
                    title: message,
                    didOpen: () => {
                        Swal.showLoading();
                    },
                    allowOutsideClick: false,
                    allowEscapeKey: false,
                    showConfirmButton: false
                }, options) : Object.assign({
                    title: message,
                    didOpen: () => {
                        Swal.showLoading();
                    },
                    allowOutsideClick: false,
                    allowEscapeKey: false,
                    showConfirmButton: false
                }, options);
                
                return Swal.fire(alertOptions);
            } else {
                // Fallback - loader kullan
                if (typeof loader !== 'undefined' && loader.show) {
                    loader.show('global', message);
                }
                return Promise.resolve();
            }
        },
        
        /**
         * Yükleme bildirimini gizler
         */
        hideLoading: function() {
            if (typeof Swal !== 'undefined') {
                Swal.close();
            } else {
                // Fallback - loader kullan
                if (typeof loader !== 'undefined' && loader.hide) {
                    loader.hide('global');
                }
            }
        }
    };
    
    // ============================================================================
    // LOADER (YÜKLEME BİLDİRİMLERİ)
    // ============================================================================
    
    const loader = {
        /**
         * Yükleme bildirimi gösterir
         * @param {string|HTMLElement|jQuery} container - Gösterilecek container. Belirtilmezse global loader gösterilir.
         * @param {string} message - Yükleme mesajı (global loader için)
         */
        show: function(container, message) {
            if (!container || container === 'global') {
                // Global loader göster
                const loaderContainer = document.getElementById('globalLoaderContainer');
                
                if (loaderContainer) {
                    if (message) {
                        const textElement = loaderContainer.querySelector('.global-loader-text');
                        if (textElement) {
                            textElement.textContent = message;
                        }
                    }
                    loaderContainer.classList.add('active');
                } else {
                    // Global loader container yoksa oluştur
                    var globalLoader = document.getElementById('global-loader');
                    if (!globalLoader) {
                        globalLoader = document.createElement('div');
                        globalLoader.id = 'global-loader';
                        globalLoader.className = 'loader-overlay';
                        globalLoader.style.cssText = 'position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 9999;';
                        globalLoader.innerHTML = '<div class="spinner-border text-white" role="status"><span class="visually-hidden">' + (message || 'Yükleniyor...') + '</span></div>';
                        document.body.appendChild(globalLoader);
                    } else {
                        globalLoader.style.display = 'flex';
                    }
                }
            } else {
                // Belirli container için içine loader ekle
                const targetContainer = typeof container === 'string' ? 
                    (typeof $ !== 'undefined' ? $(container) : document.querySelector(container)) : 
                    container;
                
                if (!targetContainer) return;
                
                const $container = typeof $ !== 'undefined' && targetContainer.jquery ? targetContainer : 
                                  typeof $ !== 'undefined' ? $(targetContainer) : 
                                  { length: targetContainer ? 1 : 0, find: function() { return { length: 0 }; }, css: function() {}, append: function() {} };
                
                if ($container.length || targetContainer) {
                    const existingLoader = typeof $container.find === 'function' ? 
                        $container.find('.container-loader') : 
                        (targetContainer.querySelector ? targetContainer.querySelector('.container-loader') : null);
                    
                    if (!existingLoader || (typeof existingLoader.length !== 'undefined' && existingLoader.length === 0)) {
                        const loaderHTML = `
                            <div class="container-loader" style="position: absolute; top: 0; left: 0; width: 100%; height: 100%; 
                                background-color: rgba(255, 255, 255, 0.7); display: flex; justify-content: center; 
                                align-items: center; z-index: 1000;">
                                <div class="spinner-border text-primary" role="status">
                                    <span class="visually-hidden">Yükleniyor...</span>
                                </div>
                            </div>
                        `;
                        
                        // Container position kontrolü
                        if (typeof $container.css === 'function') {
                            const position = $container.css('position');
                            if (position !== 'relative' && position !== 'absolute' && position !== 'fixed') {
                                $container.css('position', 'relative');
                            }
                            $container.append(loaderHTML);
                        } else if (targetContainer) {
                            const position = window.getComputedStyle(targetContainer).position;
                            if (position !== 'relative' && position !== 'absolute' && position !== 'fixed') {
                                targetContainer.style.position = 'relative';
                            }
                            targetContainer.insertAdjacentHTML('beforeend', loaderHTML);
                        }
                    } else {
                        if (typeof existingLoader.show === 'function') {
                            existingLoader.show();
                        } else if (existingLoader.style) {
                            existingLoader.style.display = 'flex';
                        }
                    }
                }
            }
        },
        
        /**
         * Yükleme bildirimini gizler
         * @param {string|HTMLElement|jQuery} container - Gizlenecek container. Belirtilmezse global loader gizlenir.
         */
        hide: function(container) {
            if (!container || container === 'global') {
                // Global loader gizle
                const loaderContainer = document.getElementById('globalLoaderContainer');
                if (loaderContainer) {
                    loaderContainer.classList.remove('active');
                } else {
                    var globalLoader = document.getElementById('global-loader');
                    if (globalLoader) {
                        globalLoader.style.display = 'none';
                    }
                }
            } else {
                // Belirli container için loader gizle
                const targetContainer = typeof container === 'string' ? 
                    (typeof $ !== 'undefined' ? $(container) : document.querySelector(container)) : 
                    container;
                
                if (!targetContainer) return;
                
                const $container = typeof $ !== 'undefined' && targetContainer.jquery ? targetContainer : 
                                  typeof $ !== 'undefined' ? $(targetContainer) : 
                                  { length: targetContainer ? 1 : 0, find: function() { return { length: 0, hide: function() {}, remove: function() {} }; } };
                
                if ($container.length || targetContainer) {
                    const loaderEl = typeof $container.find === 'function' ? 
                        $container.find('.container-loader') : 
                        (targetContainer.querySelector ? targetContainer.querySelector('.container-loader') : null);
                    
                    if (loaderEl) {
                        if (typeof loaderEl.hide === 'function') {
                            loaderEl.hide();
                        } else if (typeof loaderEl.remove === 'function') {
                            loaderEl.remove();
                        } else if (loaderEl.style) {
                            loaderEl.style.display = 'none';
                        } else if (typeof $container.find === 'function') {
                            $container.find('.container-loader').remove();
                        } else if (targetContainer.querySelector) {
                            const el = targetContainer.querySelector('.container-loader');
                            if (el) el.remove();
                        }
                    }
                }
            }
        },
        
        /**
         * Loader'ın aktif olup olmadığını kontrol eder
         * @param {string|HTMLElement|jQuery} container - Kontrol edilecek container
         * @returns {boolean} - Loader'ın aktif olup olmadığı
         */
        isActive: function(container) {
            if (!container || container === 'global') {
                const loaderContainer = document.getElementById('globalLoaderContainer');
                if (loaderContainer) {
                    return loaderContainer.classList.contains('active');
                }
                const globalLoader = document.getElementById('global-loader');
                return globalLoader ? globalLoader.style.display !== 'none' : false;
            } else {
                const targetContainer = typeof container === 'string' ? 
                    (typeof $ !== 'undefined' ? $(container) : document.querySelector(container)) : 
                    container;
                
                if (targetContainer) {
                    const loaderEl = typeof targetContainer.find === 'function' ? 
                        targetContainer.find('.container-loader') : 
                        (targetContainer.querySelector ? targetContainer.querySelector('.container-loader') : null);
                    
                    if (loaderEl) {
                        if (typeof loaderEl.is === 'function') {
                            return loaderEl.is(':visible');
                        } else if (loaderEl.style) {
                            return loaderEl.style.display !== 'none';
                        }
                    }
                }
                return false;
            }
        },
        
        /**
         * Yükleme bildirimini gösterir ve belirli bir süre sonra gizler
         * @param {string|HTMLElement|jQuery} container - Gösterilecek container
         * @param {number} duration - Gösterim süresi (ms)
         * @param {string} message - Yükleme mesajı
         */
        showTemporary: function(container, duration = 2000, message) {
            this.show(container, message);
            setTimeout(() => {
                this.hide(container);
            }, duration);
        }
    };
    
    // ============================================================================
    // MODAL İŞLEMLERİ
    // ============================================================================
    
    const modal = {
        /**
         * HTML escape helper
         * @private
         */
        _escapeHtml: function(text) {
            if (!text) return '';
            var map = {
                '&': '&amp;',
                '<': '&lt;',
                '>': '&gt;',
                '"': '&quot;',
                "'": '&#039;'
            };
            return String(text).replace(/[&<>"']/g, function(m) {
                return map[m];
            });
        },
        
        /**
         * Tooltip'leri başlatır
         * @param {string} selector - Tooltip seçicisi
         */
        initTooltips: function(selector = '[data-bs-toggle="tooltip"]') {
            initTooltips(selector);
        },
        
        /**
         * Modal içeriğini yükler ve gösterir
         * @param {string} selector - Modal container seçicisi
         * @param {string} content - Modal içeriği
         * @param {string} modalId - Modal element ID'si
         * @param {object} options - Ek ayarlar
         */
        loadAndShow: function(selector, content, modalId, options = {}) {
            // Bootstrap 5 öncelikli
            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                // Önceki modal arkaplanlarını temizle
                const existingBackdrops = document.querySelectorAll('.modal-backdrop');
                existingBackdrops.forEach(function(backdrop) {
                    backdrop.remove();
                });
                document.body.classList.remove('modal-open');
                document.body.style.paddingRight = '';
                
                const container = typeof selector === 'string' ? document.querySelector(selector) : selector;
                if (container) {
                    container.innerHTML = content;
                }
                
                const modalEl = document.getElementById(modalId);
                if (modalEl) {
                    const bsModal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
                    
                    modalEl.addEventListener('hidden.bs.modal', function() {
                        document.body.classList.remove('modal-open');
                        document.body.style.paddingRight = '';
                        const backdrops = document.querySelectorAll('.modal-backdrop');
                        backdrops.forEach(function(backdrop) {
                            backdrop.remove();
                        });
                        if (options.onHidden) {
                            options.onHidden();
                        }
                    }, { once: true });
                    
                    if (options.onShown) {
                        modalEl.addEventListener('shown.bs.modal', options.onShown, { once: true });
                    }
                    
                    bsModal.show();
                    this.initTooltips();
                }
            } else if (typeof $ !== 'undefined') {
                // jQuery fallback
                $('.modal-backdrop').remove();
                $('body').removeClass('modal-open').css('padding-right', '');
                
                $(selector).html(content);
                const modalEl = $(`#${modalId}`);
                
                modalEl.on('hidden.bs.modal', function () {
                    $('body').attr('tabindex', '-1').focus().removeAttr('tabindex');
                    $('.modal-backdrop').remove();
                    $('body').removeClass('modal-open').css('padding-right', '');
                    if (options.onHidden) {
                        options.onHidden();
                    }
                });
                
                if (options.onShown) {
                    modalEl.on('shown.bs.modal', options.onShown);
                }
                
                modalEl.modal('show');
                this.initTooltips();
            } else {
                // Vanilla JS fallback
                const container = typeof selector === 'string' ? document.querySelector(selector) : selector;
                if (container) {
                    container.innerHTML = content;
                }
                
                const modalEl = document.getElementById(modalId);
                if (modalEl) {
                    modalEl.style.display = 'flex';
                    modalEl.classList.add('flex');
                    document.body.style.overflow = 'hidden';
                    this.initTooltips();
                }
            }
        },
        
        /**
         * Modal'ı kapatır
         * @param {string} modalId - Modal element ID'si
         */
        close: function(modalId) {
            // Bootstrap 5 öncelikli
            const modalEl = document.getElementById(modalId);
            if (modalEl && typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                const bsModal = bootstrap.Modal.getInstance(modalEl);
                if (bsModal) {
                    bsModal.hide();
                }
            } else if (typeof $ !== 'undefined') {
                // jQuery fallback
                $(document.activeElement).trigger('blur');
                $(`#${modalId}`).modal('hide');
                $('.modal-backdrop').remove();
                $('body').removeClass('modal-open').css('padding-right', '');
            } else {
                // Vanilla JS fallback
                if (modalEl) {
                    modalEl.style.display = 'none';
                    modalEl.classList.remove('flex');
                    document.body.style.overflow = '';
                }
            }
        },
        
        /**
         * Modal oluşturur
         * @param {string} modalId - Modal element ID'si
         * @param {string} title - Modal başlığı
         * @param {string} content - Modal içeriği
         * @param {object} options - Ek ayarlar
         */
        create: function(modalId, title, content, options = {}) {
            const modalOptions = typeof $ !== 'undefined' && $.extend ? $.extend({
                size: 'modal-lg',
                footer: true,
                closeButton: true,
                confirmButton: true,
                confirmButtonText: 'Kaydet',
                cancelButton: true,
                cancelButtonText: 'İptal',
                confirmCallback: null,
                cancelCallback: null
            }, options) : Object.assign({
                size: 'modal-lg',
                footer: true,
                closeButton: true,
                confirmButton: true,
                confirmButtonText: 'Kaydet',
                cancelButton: true,
                cancelButtonText: 'İptal',
                confirmCallback: null,
                cancelCallback: null
            }, options);
            
            // Tailwind CSS uyumlu modal HTML
            const sizeClass = modalOptions.size === 'modal-sm' ? 'max-w-md' : 
                            modalOptions.size === 'modal-lg' ? 'max-w-3xl' : 
                            modalOptions.size === 'modal-xl' ? 'max-w-5xl' : 'max-w-2xl';
            
            // Backdrop
            let modalHtml = `
                <div class="fixed inset-0 z-50 flex items-center justify-center px-4 py-8 bg-black/50 backdrop-blur-sm transition-opacity duration-300 opacity-0" 
                     id="${modalId}-backdrop" 
                     style="display: none;">
                    <div class="relative w-full ${sizeClass} transform transition-all duration-300 scale-95 opacity-0" 
                         id="${modalId}-dialog"
                         role="dialog" 
                         aria-modal="true" 
                         aria-labelledby="${modalId}Label">
                        <div class="relative rounded-2xl bg-white dark:bg-gray-900 shadow-2xl overflow-hidden">
                            <!-- Header -->
                            <div class="flex items-center justify-between border-b border-gray-200 dark:border-gray-700 px-6 py-4 ${modalOptions.headerBackground ? '' : 'bg-gray-50 dark:bg-gray-800/50'}" ${modalOptions.headerBackground ? `style="background: ${modalOptions.headerBackground};"` : ''}>
                                <h2 class="text-lg font-semibold text-text-light dark:text-text-dark" id="${modalId}Label">
                                    ${this._escapeHtml(title)}
                                </h2>
                                ${modalOptions.closeButton ? `
                                    <button type="button" 
                                            class="flex size-9 items-center justify-center rounded-full bg-primary/10 text-primary hover:bg-primary/20 focus:outline-none focus-visible:ring-2 focus-visible:ring-primary transition-colors" 
                                            data-bs-dismiss="modal" 
                                            aria-label="Close"
                                            id="${modalId}-close">
                                        <span class="material-symbols-outlined text-lg">close</span>
                                    </button>
                                ` : ''}
                            </div>
                            
                            <!-- Body -->
                            <div class="px-6 py-5 max-h-[70vh] overflow-y-auto">
                                ${content}
                            </div>
            `;
            
            if (modalOptions.footer) {
                modalHtml += `
                            <!-- Footer -->
                            <div class="flex items-center justify-end gap-3 border-t border-gray-200 dark:border-gray-700 px-6 py-4 bg-gray-50 dark:bg-gray-800/50">
                                ${modalOptions.cancelButton ? `
                                    <button type="button" 
                                            class="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-600 focus:outline-none focus-visible:ring-2 focus-visible:ring-primary transition-colors" 
                                            data-bs-dismiss="modal"
                                            id="${modalId}-cancel">
                                        ${this._escapeHtml(modalOptions.cancelButtonText)}
                                    </button>
                                ` : ''}
                                ${modalOptions.confirmButton ? `
                                    <button type="button" 
                                            class="px-4 py-2 text-sm font-medium text-white bg-primary hover:bg-primary/90 focus:outline-none focus-visible:ring-2 focus-visible:ring-primary rounded-lg transition-colors" 
                                            id="${modalId}-confirm">
                                        ${this._escapeHtml(modalOptions.confirmButtonText)}
                                    </button>
                                ` : ''}
                            </div>
                `;
            }
            
            modalHtml += `
                        </div>
                    </div>
                </div>
            `;
            
            // Bootstrap 5 öncelikli (vanilla JS)
            // jQuery modal plugin kontrolü: Eğer jQuery varsa ama modal plugin yoksa, Bootstrap 5 kullan
            var hasJQueryModal = typeof $ !== 'undefined' && typeof $.fn !== 'undefined' && typeof $.fn.modal === 'function';
            var hasBootstrap5 = typeof bootstrap !== 'undefined' && bootstrap.Modal;
            
            if (hasBootstrap5 || !hasJQueryModal) {
                // Tailwind CSS modal (Bootstrap 5 API'si kullanılabilir ama Tailwind tasarımı)
                document.body.insertAdjacentHTML('beforeend', modalHtml);
                const backdropEl = document.getElementById(`${modalId}-backdrop`);
                const dialogEl = document.getElementById(`${modalId}-dialog`);
                const closeBtn = document.getElementById(`${modalId}-close`);
                const cancelBtn = document.getElementById(`${modalId}-cancel`);
                
                if (!backdropEl || !dialogEl) {
                    console.error('Modal element oluşturulamadı:', modalId);
                    return null;
                }
                
                // Modal instance oluştur
                var modalInstance = {
                    element: backdropEl,
                    backdrop: backdropEl,
                    dialog: dialogEl,
                    _isConfirmed: false, // Confirm butonuna basıldı mı?
                    show: function() {
                        // Backdrop göster
                        backdropEl.style.display = 'flex';
                        // Animasyon için requestAnimationFrame
                        requestAnimationFrame(function() {
                            backdropEl.style.opacity = '1';
                            dialogEl.style.transform = 'scale(1)';
                            dialogEl.style.opacity = '1';
                        });
                        // Body scroll'u engelle
                        document.body.style.overflow = 'hidden';
                    },
                    close: function(skipCancelCallback) {
                        // Animasyon ile kapat
                        backdropEl.style.opacity = '0';
                        dialogEl.style.transform = 'scale(0.95)';
                        dialogEl.style.opacity = '0';
                        // Animasyon bitince DOM'dan kaldır
                        var self = this;
                        setTimeout(function() {
                            backdropEl.style.display = 'none';
                            document.body.style.overflow = '';
                            backdropEl.remove();
                            // cancelCallback sadece kullanıcı iptal ettiğinde çağrılır (button, backdrop, ESC)
                            // confirmCallback'te skipCancelCallback = true olarak çağrılır
                            if (!skipCancelCallback && !self._isConfirmed && modalOptions.cancelCallback) {
                                modalOptions.cancelCallback();
                            }
                        }, 300);
                    },
                    updateContent: function(newContent, newOptions = {}) {
                        const bodyEl = dialogEl.querySelector('.px-6.py-5');
                        if (bodyEl) {
                            bodyEl.innerHTML = newContent;
                        }
                        if (newOptions.initTooltips !== false) {
                            UIService.modal.initTooltips();
                        }
                    }
                };
                
                // Close button event
                if (closeBtn) {
                    closeBtn.addEventListener('click', function() {
                        // skipCancelCallback = false (default) - cancelCallback çağrılacak
                        modalInstance.close(false);
                    });
                }
                
                // Cancel button event
                if (cancelBtn) {
                    cancelBtn.addEventListener('click', function() {
                        // skipCancelCallback = false (default) - cancelCallback çağrılacak
                        modalInstance.close(false);
                    });
                }
                
                // Confirm button event
                if (modalOptions.confirmCallback) {
                    const confirmBtn = document.getElementById(`${modalId}-confirm`);
                    if (confirmBtn) {
                        confirmBtn.addEventListener('click', function() {
                            modalInstance._isConfirmed = true; // Confirm edildiğini işaretle
                            modalOptions.confirmCallback(this);
                            // skipCancelCallback = true - cancelCallback çağrılmayacak
                            modalInstance.close(true);
                        });
                    }
                }
                
                // Backdrop click to close
                backdropEl.addEventListener('click', function(e) {
                    if (e.target === backdropEl) {
                        // skipCancelCallback = false (default) - cancelCallback çağrılacak
                        modalInstance.close(false);
                    }
                });
                
                // ESC key to close
                var escHandler = function(e) {
                    if (e.key === 'Escape' && backdropEl.style.display !== 'none') {
                        // skipCancelCallback = false (default) - cancelCallback çağrılacak
                        modalInstance.close(false);
                        document.removeEventListener('keydown', escHandler);
                    }
                };
                document.addEventListener('keydown', escHandler);
                
                // Bootstrap 5 API'si ile entegrasyon (opsiyonel)
                if (hasBootstrap5) {
                    // Bootstrap 5 event'lerini dinle (backward compatibility)
                    backdropEl.addEventListener('hidden.bs.modal', function() {
                        modalInstance.close();
                    }, { once: true });
                }
                
                return modalInstance;
            } else if (hasJQueryModal) {
                // jQuery fallback (Bootstrap 4 için - sadece modal plugin varsa)
                $('body').append(modalHtml);
                const modal = $(`#${modalId}`);
                
                if (modalOptions.confirmCallback) {
                    $(`#${modalId}-confirm`).on('click', function() {
                        modalOptions.confirmCallback(this);
                    });
                }
                
                modal.on('hidden.bs.modal', function() {
                    if (modalOptions.cancelCallback) {
                        modalOptions.cancelCallback();
                    }
                    $(this).remove();
                });
                
                return {
                    element: modal,
                    show: function() {
                        modal.modal('show');
                    },
                    close: function() {
                        modal.modal('hide');
                    },
                    updateContent: function(newContent, newOptions = {}) {
                        UIService.modal.updateContent(modalId, newContent, newOptions);
                    }
                };
            } else {
                // Vanilla JS fallback
                document.body.insertAdjacentHTML('beforeend', modalHtml);
                const modalEl = document.getElementById(modalId);
                
                if (modalOptions.confirmCallback) {
                    const confirmBtn = document.getElementById(`${modalId}-confirm`);
                    if (confirmBtn) {
                        confirmBtn.addEventListener('click', function() {
                            modalOptions.confirmCallback(this);
                        });
                    }
                }
                
                if (modalEl && typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                    const bsModal = new bootstrap.Modal(modalEl);
                    
                    modalEl.addEventListener('hidden.bs.modal', function() {
                        if (modalOptions.cancelCallback) {
                            modalOptions.cancelCallback();
                        }
                        modalEl.remove();
                    });
                    
                    return {
                        element: modalEl,
                        show: function() {
                            bsModal.show();
                        },
                        close: function() {
                            bsModal.hide();
                        },
                        updateContent: function(newContent, newOptions = {}) {
                            UIService.modal.updateContent(modalId, newContent, newOptions);
                        }
                    };
                }
            }
        },
        
        /**
         * ID'ye göre modalı bulur ve modal nesnesini döndürür
         * @param {string} modalId - Modal element ID'si
         * @returns {object|null} - Modal nesnesi veya null
         */
        find: function(modalId) {
            // Tailwind CSS modal yapısı: backdrop ve dialog ayrı elementler
            const backdropEl = document.getElementById(`${modalId}-backdrop`);
            const dialogEl = document.getElementById(`${modalId}-dialog`);
            
            // Eğer Tailwind yapısı varsa (backdrop ve dialog ayrı)
            if (backdropEl && dialogEl) {
                return {
                    element: backdropEl,
                    backdrop: backdropEl,
                    dialog: dialogEl,
                    show: function() {
                        backdropEl.style.display = 'flex';
                        requestAnimationFrame(function() {
                            backdropEl.style.opacity = '1';
                            dialogEl.style.transform = 'scale(1)';
                            dialogEl.style.opacity = '1';
                        });
                        document.body.style.overflow = 'hidden';
                    },
                    close: function() {
                        backdropEl.style.opacity = '0';
                        dialogEl.style.transform = 'scale(0.95)';
                        dialogEl.style.opacity = '0';
                        setTimeout(function() {
                            backdropEl.style.display = 'none';
                            document.body.style.overflow = '';
                        }, 300);
                    },
                    updateContent: function(newContent, newOptions = {}) {
                        const bodyEl = dialogEl.querySelector('.px-6.py-5');
                        if (bodyEl) {
                            bodyEl.innerHTML = newContent;
                        }
                        if (newOptions.initTooltips !== false) {
                            UIService.modal.initTooltips();
                        }
                    }
                };
            }
            
            // Eski Bootstrap yapısı için fallback
            const modalEl = document.getElementById(modalId);
            if (!modalEl) {
                console.warn(`Modal bulunamadı: ${modalId}`);
                return null;
            }
            
            // jQuery modal plugin kontrolü
            var hasJQueryModal = typeof $ !== 'undefined' && typeof $.fn !== 'undefined' && typeof $.fn.modal === 'function';
            var hasBootstrap5 = typeof bootstrap !== 'undefined' && bootstrap.Modal;
            
            // Bootstrap 5 öncelikli
            if (hasBootstrap5 || !hasJQueryModal) {
                if (hasBootstrap5) {
                    const bsModal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
                    return {
                        element: modalEl,
                        show: function() {
                            bsModal.show();
                        },
                        close: function() {
                            bsModal.hide();
                        },
                        updateContent: function(newContent, newOptions = {}) {
                            UIService.modal.updateContent(modalId, newContent, newOptions);
                        }
                    };
                } else {
                    // Bootstrap 5 yok, vanilla JS fallback
                    return {
                        element: modalEl,
                        show: function() {
                            modalEl.style.display = 'flex';
                            modalEl.classList.add('flex', 'show');
                            document.body.style.overflow = 'hidden';
                        },
                        close: function() {
                            modalEl.style.display = 'none';
                            modalEl.classList.remove('flex', 'show');
                            document.body.style.overflow = '';
                        },
                        updateContent: function(newContent, newOptions = {}) {
                            UIService.modal.updateContent(modalId, newContent, newOptions);
                        }
                    };
                }
            } else if (hasJQueryModal) {
                // jQuery fallback (Bootstrap 4 için - sadece modal plugin varsa)
                const modal = $(`#${modalId}`);
                return {
                    element: modal,
                    show: function() {
                        modal.modal('show');
                    },
                    close: function() {
                        modal.modal('hide');
                    },
                    updateContent: function(newContent, newOptions = {}) {
                        UIService.modal.updateContent(modalId, newContent, newOptions);
                    }
                };
            }
            
            return null;
        },
        
        /**
         * Mevcut modalın içeriğini günceller
         * @param {string} modalId - Modal element ID'si
         * @param {string} content - Yeni modal içeriği
         * @param {object} options - Ek ayarlar
         */
        updateContent: function(modalId, content, options = {}) {
            const modalOptions = typeof $ !== 'undefined' && $.extend ? $.extend({
                updateTitle: false,
                title: '',
                initTooltips: true
            }, options) : Object.assign({
                updateTitle: false,
                title: '',
                initTooltips: true
            }, options);

            const modalEl = document.getElementById(modalId);
            if (!modalEl) {
                console.warn(`Modal bulunamadı: ${modalId}`);
                return;
            }
            
            const modalBody = modalEl.querySelector('.modal-body');
            if (modalBody) {
                modalBody.innerHTML = content;

                if (modalOptions.updateTitle && modalOptions.title) {
                    const titleEl = modalEl.querySelector('.modal-title');
                    if (titleEl) {
                        titleEl.textContent = modalOptions.title;
                    }
                }

                if (modalOptions.initTooltips) {
                    this.initTooltips();
                }
            }
        },
        
        /**
         * Onay modalı gösterir
         * @param {string} title - Modal başlığı
         * @param {string} message - Modal mesajı
         * @param {function} confirmCallback - Onay sonrası çağrılacak fonksiyon
         * @param {function} cancelCallback - İptal sonrası çağrılacak fonksiyon
         * @param {object} options - Ek ayarlar
         */
        confirm: function(title, message, confirmCallback, cancelCallback, options = {}) {
            const modalId = 'confirm-modal-' + new Date().getTime();
            
            const modalInstance = this.create(modalId, title, `<p class="text-text-light dark:text-text-dark">${this._escapeHtml(message)}</p>`, {
                size: 'modal-sm',
                confirmButtonText: 'Evet',
                cancelButtonText: 'İptal',
                confirmCallback: function() {
                    // confirmCallback çağrılır (modal kapatma işlemi confirm button handler'ında yapılacak)
                    if (confirmCallback) {
                        confirmCallback();
                    }
                },
                cancelCallback: cancelCallback,
                ...options
            });
            
            if (modalInstance) {
                modalInstance.show();
            }
        },
        
        /**
         * Hata modalı gösterir (sadece OK butonu)
         * @param {string} title - Modal başlığı
         * @param {string} message - Modal mesajı
         * @param {function} okCallback - OK butonuna tıklandığında çağrılacak fonksiyon
         * @param {object} options - Ek ayarlar
         */
        error: function(title, message, okCallback, options = {}) {
            const modalId = 'error-modal-' + new Date().getTime();
           
            const modalContent = `
                <div class="text-center">
                    <p class="text-text-light dark:text-text-dark">${this._escapeHtml(message)}</p>
                </div>
            `;
            
            const modalInstance = this.create(modalId, title || 'Hata', modalContent, {
                size: 'modal-sm',
                confirmButton: true,
                confirmButtonText: options.okButtonText || 'Tamam',
                cancelButton: false, // Cancel butonu yok
                closeButton: true,
                headerBackground: '#ff00007a', // Kırmızı yarı saydam arka plan
                confirmCallback: function() {
                    // OK callback çağrılır
                    if (okCallback) {
                        okCallback();
                    }
                },
                ...options
            });
            
            if (modalInstance) {
                modalInstance.show();
            }
        },
        
        /**
         * Modal'ı gösterir (basit)
         * @param {string} modalId - Modal ID'si
         */
        show: function(modalId) {
            // Tailwind CSS modal yapısı için
            const backdropEl = document.getElementById(`${modalId}-backdrop`);
            const dialogEl = document.getElementById(`${modalId}-dialog`);
            
            if (backdropEl && dialogEl) {
                backdropEl.style.display = 'flex';
                requestAnimationFrame(function() {
                    backdropEl.style.opacity = '1';
                    dialogEl.style.transform = 'scale(1)';
                    dialogEl.style.opacity = '1';
                });
                document.body.style.overflow = 'hidden';
                return;
            }
            
            // Eski Bootstrap yapısı için fallback
            const modalEl = document.getElementById(modalId);
            if (!modalEl) {
                return;
            }
            
            // Bootstrap 5 öncelikli
            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                let bsModal = bootstrap.Modal.getInstance(modalEl);
                if (!bsModal) {
                    bsModal = new bootstrap.Modal(modalEl);
                }
                bsModal.show();
            } else if (typeof $ !== 'undefined') {
                // jQuery fallback
                $(modalEl).modal('show');
            } else {
                // Vanilla JS fallback
                modalEl.style.display = 'flex';
                modalEl.classList.add('flex');
                document.body.style.overflow = 'hidden';
            }
        },
        
        /**
         * Modal'ı gizler (basit)
         * @param {string} modalId - Modal ID'si
         */
        hide: function(modalId) {
            // Tailwind CSS modal yapısı için
            const backdropEl = document.getElementById(`${modalId}-backdrop`);
            const dialogEl = document.getElementById(`${modalId}-dialog`);
            
            if (backdropEl && dialogEl) {
                backdropEl.style.opacity = '0';
                dialogEl.style.transform = 'scale(0.95)';
                dialogEl.style.opacity = '0';
                setTimeout(function() {
                    backdropEl.style.display = 'none';
                    document.body.style.overflow = '';
                }, 300);
                return;
            }
            
            // Eski Bootstrap yapısı için fallback
            const modalEl = document.getElementById(modalId);
            if (!modalEl) {
                return;
            }
            
            // Bootstrap 5 öncelikli
            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                const bsModal = bootstrap.Modal.getInstance(modalEl);
                if (bsModal) {
                    bsModal.hide();
                }
            } else if (typeof $ !== 'undefined') {
                // jQuery fallback
                $(modalEl).modal('hide');
            } else {
                // Vanilla JS fallback
                modalEl.style.display = 'none';
                modalEl.classList.remove('flex');
                document.body.style.overflow = '';
            }
        }
    };
    
    // ============================================================================
    // FORM İŞLEMLERİ
    // ============================================================================
    
    const form = {
        /**
         * Form inputlarını temizler
         * @param {string} formSelector - Form seçicisi
         */
        reset: function(formSelector) {
            const form = typeof $ !== 'undefined' ? $(formSelector) : document.querySelector(formSelector);
            
            if (typeof $ !== 'undefined' && form.length) {
                form[0].reset();
                form.find('.field-validation-error').html('');
                form.find('.input-validation-error').removeClass('input-validation-error');
            } else if (form) {
                form.reset();
                const errorElements = form.querySelectorAll('.field-validation-error');
                errorElements.forEach(function(el) {
                    el.innerHTML = '';
                });
                const inputErrors = form.querySelectorAll('.input-validation-error');
                inputErrors.forEach(function(el) {
                    el.classList.remove('input-validation-error');
                });
            }
        },
        
        /**
         * Form validasyonunu yapar
         * @param {string} formSelector - Form seçicisi
         * @returns {boolean} - Validasyon sonucu
         */
        validate: function(formSelector) {
            const form = typeof $ !== 'undefined' ? $(formSelector) : document.querySelector(formSelector);
            
            if ((typeof $ !== 'undefined' && form.length === 0) || !form) {
                console.warn(`Form bulunamadı: ${formSelector}`);
                return false;
            }
            
            const formEl = typeof $ !== 'undefined' ? form[0] : form;
            
            if (formEl.checkValidity && typeof formEl.checkValidity === 'function') {
                if (formEl.checkValidity() === false) {
                    formEl.reportValidity();
                    return false;
                }
            }
            
            return true;
        },
        
        /**
         * Form verisini JSON olarak alır
         * @param {string} formSelector - Form seçicisi
         * @returns {object} - Form verisi
         */
        getFormData: function(formSelector) {
            const form = typeof $ !== 'undefined' ? $(formSelector) : document.querySelector(formSelector);
            
            if ((typeof $ !== 'undefined' && form.length === 0) || !form) {
                console.warn(`Form bulunamadı: ${formSelector}`);
                return {};
            }
            
            if (typeof $ !== 'undefined') {
                const formArray = form.serializeArray();
                const formData = {};
                
                $.each(formArray, function() {
                    if (formData[this.name]) {
                        if (!formData[this.name].push) {
                            formData[this.name] = [formData[this.name]];
                        }
                        formData[this.name].push(this.value || '');
                    } else {
                        formData[this.name] = this.value || '';
                    }
                });
                
                return formData;
            } else {
                const formData = {};
                const formEl = form;
                const inputs = formEl.querySelectorAll('input, select, textarea');
                
                inputs.forEach(function(input) {
                    if (input.name) {
                        if (input.type === 'checkbox') {
                            formData[input.name] = input.checked;
                        } else if (input.type === 'radio') {
                            if (input.checked) {
                                formData[input.name] = input.value;
                            }
                        } else {
                            formData[input.name] = input.value;
                        }
                    }
                });
                
                return formData;
            }
        }
    };
    
    // ============================================================================
    // TABLO İŞLEMLERİ
    // ============================================================================
    
    const table = {
        /**
         * Tablo satırlarını filtreler
         * @param {string} tableSelector - Tablo seçicisi
         * @param {string} query - Arama sorgusu
         * @param {Array} columns - Aranacak sütunlar
         */
        filter: function(tableSelector, query, columns = null) {
            const table = typeof $ !== 'undefined' ? $(tableSelector) : document.querySelector(tableSelector);
            
            if ((typeof $ !== 'undefined' && table.length === 0) || !table) {
                console.warn(`Tablo bulunamadı: ${tableSelector}`);
                return;
            }
            
            if (typeof $ !== 'undefined') {
                const rows = table.find('tbody tr');
                
                if (query === '') {
                    rows.show();
                    return;
                }
                
                const regex = new RegExp(query, 'i');
                
                rows.each(function() {
                    const row = $(this);
                    let shouldShow = false;
                    
                    if (columns) {
                        columns.forEach(function(colIndex) {
                            const cell = row.find(`td:eq(${colIndex})`);
                            if (regex.test(cell.text())) {
                                shouldShow = true;
                            }
                        });
                    } else {
                        const text = row.text();
                        shouldShow = regex.test(text);
                    }
                    
                    row.toggle(shouldShow);
                });
            } else {
                const tbody = table.querySelector('tbody');
                if (!tbody) return;
                
                const rows = tbody.querySelectorAll('tr');
                const regex = new RegExp(query || '', 'i');
                
                rows.forEach(function(row) {
                    let shouldShow = false;
                    
                    if (columns) {
                        columns.forEach(function(colIndex) {
                            const cells = row.querySelectorAll('td');
                            if (cells[colIndex] && regex.test(cells[colIndex].textContent)) {
                                shouldShow = true;
                            }
                        });
                    } else {
                        shouldShow = regex.test(row.textContent);
                    }
                    
                    row.style.display = shouldShow ? '' : 'none';
                });
            }
        },
        
        /**
         * Tablo satırlarını sıralar
         * @param {string} tableSelector - Tablo seçicisi
         * @param {number} colIndex - Sütun indeksi
         * @param {boolean} asc - Sıralama yönü (artan/azalan)
         */
        sort: function(tableSelector, colIndex, asc = true) {
            const table = typeof $ !== 'undefined' ? $(tableSelector) : document.querySelector(tableSelector);
            
            if ((typeof $ !== 'undefined' && table.length === 0) || !table) {
                console.warn(`Tablo bulunamadı: ${tableSelector}`);
                return;
            }
            
            if (typeof $ !== 'undefined') {
                const tbody = table.find('tbody');
                const rows = tbody.find('tr').toArray();
                
                rows.sort(function(a, b) {
                    const aText = $(a).find(`td:eq(${colIndex})`).text().trim();
                    const bText = $(b).find(`td:eq(${colIndex})`).text().trim();
                    
                    if (asc) {
                        return aText.localeCompare(bText);
                    } else {
                        return bText.localeCompare(aText);
                    }
                });
                
                tbody.append(rows);
            } else {
                const tbody = table.querySelector('tbody');
                if (!tbody) return;
                
                const rows = Array.from(tbody.querySelectorAll('tr'));
                
                rows.sort(function(a, b) {
                    const aCells = a.querySelectorAll('td');
                    const bCells = b.querySelectorAll('td');
                    const aText = aCells[colIndex] ? aCells[colIndex].textContent.trim() : '';
                    const bText = bCells[colIndex] ? bCells[colIndex].textContent.trim() : '';
                    
                    if (asc) {
                        return aText.localeCompare(bText);
                    } else {
                        return bText.localeCompare(aText);
                    }
                });
                
                rows.forEach(function(row) {
                    tbody.appendChild(row);
                });
            }
        }
    };
    
    // ============================================================================
    // PUBLIC API
    // ============================================================================
    
    return {
        initTooltips: initTooltips,
        initPopovers: initPopovers,
        toast: toast,
        notification: notification,
        loader: loader,
        modal: modal,
        form: form,
        table: table
    };
})();

// Küresel namespace'e ekleme
window.UIService = window.UIService || UIService;
