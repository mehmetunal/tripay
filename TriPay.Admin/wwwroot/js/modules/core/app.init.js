/**
 * Application Initialization Module
 * Uygulamanın başlatılmasını ve tüm sayfalarda ortak ayarları yönetir
 */
const AppInit = (function() {
    // Uygulama ayarları
    const settings = {
        baseUrl: '/',
        debug: false,
        dateFormat: 'DD.MM.YYYY',
        timeFormat: 'HH:mm',
        defaultPageSize: 10,
        autoInitTooltips: true,
        autoInitPopovers: true,
        ajaxTimeoutMs: 30000, // 30 sn
        csrfTokenName: '__RequestVerificationToken',
        defaultTheme: 'default'
    };
    
    /**
     * Uygulama ayarlarını yapılandırır
     * @param {object} options - Ayar opsiyonları
     */
    function configure(options) {
        // jQuery varsa extend kullan, yoksa Object.assign kullan
        if (typeof $ !== 'undefined' && $.extend) {
            $.extend(settings, options);
        } else {
            Object.assign(settings, options);
        }
        
        if (settings.debug) {
            console.info('AppInit: Uygulama ayarları yapılandırıldı', settings);
        }
    }
    
    /**
     * Uygulama ayarlarını döndürür
     * @returns {object} - Ayarlar
     */
    function getSettings() {
        return settings;
    }
    
    /**
     * AJAX ayarlarını yapılandırır
     */
    function configureAjax() {
        // jQuery varsa global AJAX ayarlarını yapılandır
        if (typeof $ !== 'undefined' && $.ajaxSetup) {
            // Global AJAX ayarları
            $.ajaxSetup({
                timeout: settings.ajaxTimeoutMs,
                cache: false,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });
            
            // AJAX istek/yanıt filtreleri
            $(document).ajaxStart(function() {
                // AJAX istek başladı
            });
            
            $(document).ajaxStop(function() {
                // Tüm AJAX istekler tamamlandı
            });
            
            $(document).ajaxError(function(event, jqXHR, ajaxSettings, thrownError) {
                if (settings.debug) {
                    console.error('AppInit: AJAX hatası', {
                        url: ajaxSettings.url,
                        status: jqXHR.status,
                        statusText: jqXHR.statusText,
                        responseText: jqXHR.responseText,
                        error: thrownError
                    });
                }
                
                // Global hata yönetimi
                if (jqXHR.status === 401) {
                    if (typeof UIService !== 'undefined' && UIService.notification) {
                        UIService.notification.error('Oturum süreniz doldu, lütfen tekrar giriş yapın.');
                    }
                    setTimeout(function() {
                        window.location.href = `${settings.baseUrl}Account/Login`;
                    }, 2000);
                } else if (jqXHR.status === 403) {
                    if (typeof UIService !== 'undefined' && UIService.notification) {
                        UIService.notification.error('Bu işlem için yetkiniz bulunmamaktadır.');
                    }
                } else if (jqXHR.status === 404) {
                    if (typeof UIService !== 'undefined' && UIService.notification) {
                        UIService.notification.error('İstenen kaynak bulunamadı.');
                    }
                } else if (jqXHR.status === 500) {
                    if (typeof UIService !== 'undefined' && UIService.notification) {
                        UIService.notification.error('Sunucu hatası oluştu, lütfen daha sonra tekrar deneyin.');
                    }
                }
            });
        }
        // jQuery yoksa, ApiService kullanılıyor (zaten hata yönetimi var)
    }
    
    /**
     * Bootstrap bileşenlerini başlatır
     */
    function initBootstrapComponents() {
        // Tooltips
        if (settings.autoInitTooltips && typeof UIService !== 'undefined' && UIService.initTooltips) {
            UIService.initTooltips();
        }
        
        // Popovers
        if (settings.autoInitPopovers) {
            var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
            popoverTriggerList.map(function(popoverTriggerEl) {
                if (window.bootstrap && window.bootstrap.Popover) {
                    return new bootstrap.Popover(popoverTriggerEl);
                }
            });
        }
    }
    
    /**
     * Uygulamayı başlatır
     * @param {object} options - Başlatma opsiyonları
     */
    function init(options = {}) {
        // Ayarları yapılandır
        configure(options);
        
        // AJAX ayarlarını yapılandır
        configureAjax();
        
        // Bootstrap bileşenlerini başlat
        initBootstrapComponents();
        
        if (settings.debug) {
            console.info('AppInit: Uygulama başlatıldı');
        }
    }
    
    // Public API
    return {
        init: init,
        configure: configure,
        getSettings: getSettings
    };
})();

// Otomatik başlatma
(function() {
    function initApp() {
        // DOM yüklendiğinde uygulamayı başlat
        AppInit.init({
            debug: false
        });
    }
    
    // DOM yüklendiğinde başlat
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initApp);
    } else {
        // DOM zaten yüklü
        initApp();
    }
})();

// Küresel namespace'e ekleme
window.AppInit = window.AppInit || AppInit;

