/**
 * Module Factory
 * CRUD modülleri oluşturmayı kolaylaştıran fabrika sınıfı
 */
var ModuleFactory = ModuleFactory || (function() {
    /**
     * Temel modül özellikleri ve davranışlarını oluşturur
     * @param {object} options - Modül opsiyonları
     * @returns {object} - Temel modül
     */
    function createBaseModule(options) {
        const defaultOptions = {
            name: 'BaseModule',
            debug: window.AppInit && window.AppInit.getSettings ? window.AppInit.getSettings().debug : false,
            events: {}
        };
        
        // jQuery varsa extend kullan, yoksa Object.assign kullan
        let config;
        if (typeof $ !== 'undefined' && $.extend) {
            config = $.extend({}, defaultOptions, options);
        } else {
            config = Object.assign({}, defaultOptions, options);
        }
        
        // Modül nesnesini oluştur
        const module = {
            name: config.name,
            debug: config.debug,
            
            /**
             * Modülü başlatır
             * @param {...any} args - Sınırsız parametre
             */
            init: function(...args) {
                if (this.debug) {
                    console.info(`ModuleFactory: ${this.name} modülü başlatıldı`, args);
                }
                
                // Olay dinleyicilerini kur
                this.initEventListeners();
                
                // Bileşenleri başlat
                this.initComponents();
                
                // Kullanıcı tanımlı init fonksiyonu
                if (typeof config.init === 'function') {
                    config.init.apply(this, args);
                }
            },
            
            /**
             * Olay dinleyicilerini başlatır
             */
            initEventListeners: function() {
                if (typeof config.initEventListeners === 'function') {
                    config.initEventListeners.call(this);
                }
            },
            
            /**
             * Bileşenleri başlatır
             */
            initComponents: function() {
                if (typeof config.initComponents === 'function') {
                    config.initComponents.call(this);
                }
            },
            
            /**
             * Olay ekler
             * @param {string} eventName - Olay adı
             * @param {function} callback - Geri çağırma işlevi
             */
            on: function(eventName, callback) {
                if (!this.events) {
                    this.events = {};
                }
                
                if (!this.events[eventName]) {
                    this.events[eventName] = [];
                }
                
                this.events[eventName].push(callback);
                
            },
            
            /**
             * Olayı tetikler
             * @param {string} eventName - Olay adı
             * @param {object} data - Olay verileri
             */
            trigger: function(eventName, data) {
                if (!this.events || !this.events[eventName]) {
                    return;
                }
                
                const callbacks = this.events[eventName];
                
                for (let i = 0; i < callbacks.length; i++) {
                    callbacks[i].call(this, data);
                }
                
            },
            
            /**
             * Log fonksiyonu - debug veya hata mesajlarını yönetir
             * @param {string} message - Log mesajı
             * @param {*} data - Log verileri (opsiyonel)
             * @param {boolean} isError - Hata log'u mu (opsiyonel)
             */
            log: function(message, data, isError) {
                // Log fonksiyonu - debug modunda console.log kullanılabilir
                // Production'da console.log'lar kaldırıldı
            }
        };
        
        // Olayları ayarla
        module.events = config.events || {};
        
        // Kullanıcı tanımlı özellikler ve yöntemler ekle
        for (const key in config) {
            if (config.hasOwnProperty(key) && !module.hasOwnProperty(key)) {
                module[key] = config[key];
            }
        }
        
        return module;
    }
    
    // Public API
    return {
        createBaseModule: createBaseModule
    };
})();

// Küresel namespace'e ekleme - zaten tanımlanmışsa tekrar tanımlamayı önlemek için
window.ModuleFactory = window.ModuleFactory || ModuleFactory;

