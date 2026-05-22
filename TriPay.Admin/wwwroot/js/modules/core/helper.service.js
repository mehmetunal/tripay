/**
 * Helper Service Module
 * Genel yardımcı fonksiyonları içeren servis modülü
 * Tüm modüller tarafından kullanılabilecek ortak helper işlevleri
 */
var HelperService = HelperService || (function() {
    'use strict';
    
    /**
     * Slug oluşturur (Türkçe karakter desteği ile)
     * @param {string} text - Slug'a dönüştürülecek metin
     * @returns {string} - Slug
     */
    function createSlug(text) {
        if (!text || typeof text !== 'string') {
            return '';
        }
        
        // Türkçe karakterleri değiştir
        var turkishMap = {
            'ç': 'c', 'Ç': 'C',
            'ğ': 'g', 'Ğ': 'G',
            'ı': 'i', 'İ': 'I',
            'ö': 'o', 'Ö': 'O',
            'ş': 's', 'Ş': 'S',
            'ü': 'u', 'Ü': 'U'
        };
        
        var slug = text;
        for (var char in turkishMap) {
            if (turkishMap.hasOwnProperty(char)) {
                slug = slug.replace(new RegExp(char, 'g'), turkishMap[char]);
            }
        }
        
        // Küçük harfe çevir
        slug = slug.toLowerCase();
        
        // Özel karakterleri temizle
        slug = slug.replace(/[^a-z0-9\s-]/g, '');
        
        // Boşlukları tire ile değiştir
        slug = slug.replace(/\s+/g, '-');
        
        // Birden fazla tire'yi tek tire yap
        slug = slug.replace(/-+/g, '-');
        
        // Başta ve sonda tire varsa kaldır
        slug = slug.replace(/^-+|-+$/g, '');
        
        return slug;
    }
    
    /**
     * URL'den slug çıkarır
     * @param {string} url - URL
     * @returns {string} - Slug
     */
    function extractSlugFromUrl(url) {
        if (!url || typeof url !== 'string') {
            return '';
        }
        
        try {
            var urlObj = new URL(url);
            var pathParts = urlObj.pathname.split('/').filter(function(part) {
                return part && part.length > 0;
            });
            
            // Son path segment'ini al
            return pathParts.length > 0 ? pathParts[pathParts.length - 1] : '';
        } catch (e) {
            // URL parse edilemezse pathname'i direkt kullan
            var pathParts = url.split('/').filter(function(part) {
                return part && part.length > 0;
            });
            return pathParts.length > 0 ? pathParts[pathParts.length - 1] : '';
        }
    }
    
    /**
     * Para formatlar
     * @param {number} amount - Tutar
     * @param {string} currency - Para birimi (TRY, USD, EUR)
     * @param {string} culture - Kültür (tr-TR, en-US)
     * @returns {string} - Formatlanmış para
     */
    function formatCurrency(amount, currency, culture) {
        currency = currency || 'TRY';
        culture = culture || 'tr-TR';
        amount = isNaN(amount) ? 0 : amount;
        
        try {
            return new Intl.NumberFormat(culture, {
                style: 'currency',
                currency: currency,
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            }).format(amount);
        } catch (error) {
            // ⚠️ YUVARLAMA YOK: Son 2 küsürat gösterilir
            var symbol = currency === 'TRY' ? '₺' : currency + ' ';
            return symbol + parseFloat(amount).toFixed(2);
        }
    }
    
    /**
     * Tarih formatlar
     * @param {Date|string} date - Tarih
     * @param {string} format - Format (DD.MM.YYYY, YYYY-MM-DD)
     * @returns {string} - Formatlanmış tarih
     */
    function formatDate(date, format) {
        if (!date) {
            return '';
        }
        
        var d = date instanceof Date ? date : new Date(date);
        if (isNaN(d.getTime())) {
            return '';
        }
        
        format = format || 'DD.MM.YYYY';
        
        var day = String(d.getDate()).padStart(2, '0');
        var month = String(d.getMonth() + 1).padStart(2, '0');
        var year = d.getFullYear();
        
        return format
            .replace('DD', day)
            .replace('MM', month)
            .replace('YYYY', year);
    }
    
    /**
     * Tarih aralığındaki gece sayısını hesaplar
     * @param {Date|string} checkIn - Giriş tarihi
     * @param {Date|string} checkOut - Çıkış tarihi
     * @returns {number} - Gece sayısı
     */
    function calculateNights(checkIn, checkOut) {
        if (!checkIn || !checkOut) {
            return 0;
        }
        
        var checkInDate = checkIn instanceof Date ? checkIn : new Date(checkIn);
        var checkOutDate = checkOut instanceof Date ? checkOut : new Date(checkOut);
        
        if (isNaN(checkInDate.getTime()) || isNaN(checkOutDate.getTime())) {
            return 0;
        }
        
        var diffTime = checkOutDate.getTime() - checkInDate.getTime();
        var diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
        
        return Math.max(0, diffDays);
    }
    
    /**
     * URL'den dil kodunu çıkarır
     * @param {string} url - URL (opsiyonel, window.location.pathname kullanılır)
     * @returns {string} - Dil kodu (tr, en, de)
     */
    function getLanguageFromUrl(url) {
        url = url || (typeof window !== 'undefined' ? window.location.pathname : '');
        if (!url || typeof url !== 'string') {
            return 'tr';
        }
        
        var match = url.match(/^\/([a-z]{2})\//);
        return match ? match[1] : 'tr';
    }
    
    /**
     * CSRF token'ı alır
     * @returns {string} - CSRF token
     */
    function getCsrfToken() {
        var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }
    
    /**
     * Query string parametrelerini alır
     * @param {string} name - Parametre adı
     * @returns {string|null} - Parametre değeri
     */
    function getQueryParam(name) {
        if (typeof window === 'undefined' || !window.location) {
            return null;
        }
        
        var urlParams = new URLSearchParams(window.location.search);
        return urlParams.get(name);
    }
    
    /**
     * Query string parametrelerini objeye dönüştürür
     * @returns {object} - Query parametreleri
     */
    function getQueryParams() {
        if (typeof window === 'undefined' || !window.location) {
            return {};
        }
        
        var params = {};
        var urlParams = new URLSearchParams(window.location.search);
        urlParams.forEach(function(value, key) {
            params[key] = value;
        });
        
        return params;
    }
    
    /**
     * URL oluşturur
     * @param {string} baseUrl - Temel URL
     * @param {object} params - Query parametreleri
     * @returns {string} - Tam URL
     */
    function buildUrl(baseUrl, params) {
        if (!baseUrl) {
            return '';
        }
        
        var url = baseUrl;
        if (params && typeof params === 'object') {
            var queryString = Object.keys(params)
                .filter(function(key) {
                    return params[key] !== null && params[key] !== undefined && params[key] !== '';
                })
                .map(function(key) {
                    return encodeURIComponent(key) + '=' + encodeURIComponent(params[key]);
                })
                .join('&');
            
            if (queryString) {
                url += (url.indexOf('?') === -1 ? '?' : '&') + queryString;
            }
        }
        
        return url;
    }
    
    /**
     * String'i HTML escape eder
     * @param {string} text - Metin
     * @returns {string} - Escape edilmiş metin
     */
    function escapeHtml(text) {
        if (!text || typeof text !== 'string') {
            return '';
        }
        
        var map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        
        return text.replace(/[&<>"']/g, function(m) {
            return map[m];
        });
    }
    
    /**
     * HTML'i decode eder
     * @param {string} html - HTML metin
     * @returns {string} - Decode edilmiş metin
     */
    function unescapeHtml(html) {
        if (!html || typeof html !== 'string') {
            return '';
        }
        
        var textarea = document.createElement('textarea');
        textarea.innerHTML = html;
        return textarea.value;
    }
    
    /**
     * Debounce fonksiyonu
     * @param {function} func - Çalıştırılacak fonksiyon
     * @param {number} wait - Bekleme süresi (ms)
     * @returns {function} - Debounced fonksiyon
     */
    function debounce(func, wait) {
        var timeout;
        return function() {
            var context = this;
            var args = arguments;
            clearTimeout(timeout);
            timeout = setTimeout(function() {
                func.apply(context, args);
            }, wait);
        };
    }
    
    /**
     * Throttle fonksiyonu
     * @param {function} func - Çalıştırılacak fonksiyon
     * @param {number} limit - Limit süresi (ms)
     * @returns {function} - Throttled fonksiyon
     */
    function throttle(func, limit) {
        var inThrottle;
        return function() {
            var args = arguments;
            var context = this;
            if (!inThrottle) {
                func.apply(context, args);
                inThrottle = true;
                setTimeout(function() {
                    inThrottle = false;
                }, limit);
            }
        };
    }
    
    /**
     * Deep clone yapar
     * @param {*} obj - Clone edilecek obje
     * @returns {*} - Clone edilmiş obje
     */
    function deepClone(obj) {
        if (obj === null || typeof obj !== 'object') {
            return obj;
        }
        
        if (obj instanceof Date) {
            return new Date(obj.getTime());
        }
        
        if (obj instanceof Array) {
            return obj.map(function(item) {
                return deepClone(item);
            });
        }
        
        if (typeof obj === 'object') {
            var cloned = {};
            for (var key in obj) {
                if (obj.hasOwnProperty(key)) {
                    cloned[key] = deepClone(obj[key]);
                }
            }
            return cloned;
        }
        
        return obj;
    }
    
    /**
     * Objeyi query string'e dönüştürür
     * @param {object} obj - Obje
     * @returns {string} - Query string
     */
    function objectToQueryString(obj) {
        if (!obj || typeof obj !== 'object') {
            return '';
        }
        
        return Object.keys(obj)
            .filter(function(key) {
                return obj[key] !== null && obj[key] !== undefined && obj[key] !== '';
            })
            .map(function(key) {
                return encodeURIComponent(key) + '=' + encodeURIComponent(obj[key]);
            })
            .join('&');
    }
    
    /**
     * Query string'i objeye dönüştürür
     * @param {string} queryString - Query string
     * @returns {object} - Obje
     */
    function queryStringToObject(queryString) {
        if (!queryString || typeof queryString !== 'string') {
            return {};
        }
        
        var params = {};
        if (queryString.startsWith('?')) {
            queryString = queryString.substring(1);
        }
        
        queryString.split('&').forEach(function(param) {
            var parts = param.split('=');
            if (parts.length === 2) {
                params[decodeURIComponent(parts[0])] = decodeURIComponent(parts[1]);
            }
        });
        
        return params;
    }
    
    // Public API
    return {
        // Slug helpers
        createSlug: createSlug,
        extractSlugFromUrl: extractSlugFromUrl,
        
        // Format helpers
        formatCurrency: formatCurrency,
        formatDate: formatDate,
        
        // Date helpers
        calculateNights: calculateNights,
        
        // URL helpers
        getLanguageFromUrl: getLanguageFromUrl,
        getQueryParam: getQueryParam,
        getQueryParams: getQueryParams,
        buildUrl: buildUrl,
        objectToQueryString: objectToQueryString,
        queryStringToObject: queryStringToObject,
        
        // Security helpers
        getCsrfToken: getCsrfToken,
        escapeHtml: escapeHtml,
        unescapeHtml: unescapeHtml,
        
        // Utility helpers
        debounce: debounce,
        throttle: throttle,
        deepClone: deepClone
    };
})();

// Küresel namespace'e ekleme
window.HelperService = window.HelperService || HelperService;
