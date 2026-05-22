/**
 * API Service Module
 * Genel AJAX işlemlerini içeren servis modülü
 * Tüm modüller tarafından kullanılabilecek ortak API işlevleri
 */
var ApiService = ApiService || (function() {
    // Varsayılan AJAX ayarları
    const defaultAjaxSettings = {
        cache: false,
        headers: {},
        timeout: 120000 // 120 saniye varsayılan timeout
    };

    // Varsayılan HTTP başlıkları
    const defaultHeaders = {};

    /**
     * AJAX istek ayarlarını yapılandırır
     * @param {object} options - AJAX istek opsiyonları
     * @returns {object} - Yapılandırılmış istek opsiyonları
     */
    function configureRequestOptions(options) {
        return $.extend(true, {}, defaultAjaxSettings, options);
    }

    /**
     * AJAX GET isteği gönderir
     * @param {object} params - İstek parametreleri
     * @returns {Promise} - jQuery AJAX promise nesnesi
     */
    function get(params) {
        // Eğer fetch kullanılmak isteniyorsa
        if (params.useFetch === true) {
            return getFetch(params);
        }
        
        const options = configureRequestOptions({
            url: params.url,
            type: 'GET',
            data: params.data || {},
            success: params.success || function() {},
            error: params.error || function() {},
            complete: params.complete || function() {},
            timeout: params.timeout || defaultAjaxSettings.timeout
        });

        return $.ajax(options);
    }
    
    /**
     * Fetch API ile GET isteği gönderir
     * @param {object} params - İstek parametreleri
     * @returns {Promise} - Fetch promise nesnesi
     */
    function getFetch(params) {
        var url = params.url;
        var data = params.data || {};
        
        // Query string oluştur
        if (data && Object.keys(data).length > 0) {
            var queryString = Object.keys(data)
                .map(function(key) {
                    return encodeURIComponent(key) + '=' + encodeURIComponent(data[key]);
                })
                .join('&');
            url += (url.indexOf('?') === -1 ? '?' : '&') + queryString;
        }
        
        var headers = {
            'X-Requested-With': 'XMLHttpRequest'
        };
        
        // CSRF token ekle
        var token = params.token || document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
        if (token) {
            headers['RequestVerificationToken'] = token;
        }
        
        // Custom headers ekle
        if (params.headers) {
            Object.assign(headers, params.headers);
        }
        
        return fetch(url, {
            method: 'GET',
            headers: headers
        })
            .then(function(response) {
                if (!response.ok) {
                    throw new Error('HTTP error! status: ' + response.status);
                }
                return response.json();
            })
            .then(function(result) {
                if (params.success && typeof params.success === 'function') {
                    params.success(result);
                }
                if (params.complete && typeof params.complete === 'function') {
                    params.complete();
                }
                return result;
            })
            .catch(function(error) {
                if (params.error && typeof params.error === 'function') {
                    params.error({ statusText: error.message });
                }
                if (params.complete && typeof params.complete === 'function') {
                    params.complete();
                }
                throw error;
            });
    }

    /**
     * AJAX POST isteği gönderir
     * @param {object} params - İstek parametreleri
     * @returns {Promise} - jQuery AJAX promise nesnesi
     */
    function post(params) {
        // Eğer fetch kullanılmak isteniyorsa
        if (params.useFetch === true) {
            return postFetch(params);
        }
        
        const options = configureRequestOptions({
            url: params.url,
            type: 'POST',
            data: params.data || {},
            success: params.success || function() {},
            error: params.error || function() {},
            complete: params.complete || function() {},
            timeout: params.timeout || defaultAjaxSettings.timeout
        });

        if (params.token) {
            options.headers = {
                ...options.headers,
                'RequestVerificationToken': params.token
            };
        }

        return $.ajax(options);
    }
    
    /**
     * Fetch API ile POST isteği gönderir
     * @param {object} params - İstek parametreleri
     * @returns {Promise} - Fetch promise nesnesi
     */
    function postFetch(params) {
        var url = params.url;
        var data = params.data || {};
        var headers = {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        };
        
        // CSRF token ekle
        var token = params.token || document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
        if (token) {
            headers['RequestVerificationToken'] = token;
        }
        
        // Custom headers ekle
        if (params.headers) {
            Object.assign(headers, params.headers);
        }
        
        var fetchOptions = {
            method: 'POST',
            headers: headers,
            body: JSON.stringify(data)
        };
        
        return fetch(url, fetchOptions)
            .then(function(response) {
                if (!response.ok) {
                    throw new Error('HTTP error! status: ' + response.status);
                }
                return response.json();
            })
            .then(function(result) {
                if (params.success && typeof params.success === 'function') {
                    params.success(result);
                }
                if (params.complete && typeof params.complete === 'function') {
                    params.complete();
                }
                return result;
            })
            .catch(function(error) {
                if (params.error && typeof params.error === 'function') {
                    params.error({ statusText: error.message });
                }
                if (params.complete && typeof params.complete === 'function') {
                    params.complete();
                }
                throw error;
            });
    }

    /**
     * Form verilerini POST isteği ile gönderir
     * @param {object} params - İstek parametreleri
     * @returns {Promise} - jQuery AJAX promise nesnesi
     */
    function submitForm(params) {
        const form = $(params.form);
        
        if (form.length === 0) {
            console.error(`Form bulunamadı`);
            if (typeof params.error === 'function') {
                params.error({ statusText: 'Form bulunamadı' });
            }
            return $.Deferred().reject({ statusText: 'Form bulunamadı' }).promise();
        }
        
        const formData = new FormData(form[0]);
        const token = form.find('input[name="__RequestVerificationToken"]').val();

        const options = configureRequestOptions({
            url: params.url,
            type: params.method || 'POST',
            data: params.processData === false ? formData : form.serialize(),
            processData: params.processData === false ? false : true,
            contentType: params.processData === false ? false : 'application/x-www-form-urlencoded; charset=UTF-8',
            dataType: params.dataType || undefined,
            success: params.success || function() {},
            error: params.error || function() {},
            complete: params.complete || function() {},
            timeout: params.timeout || defaultAjaxSettings.timeout
        });

        if (token) {
            options.headers = {
                ...options.headers,
                'RequestVerificationToken': token
            };
        }

        return $.ajax(options);
    }

    /**
     * AJAX PUT isteği gönderir
     * @param {object} params - İstek parametreleri
     * @returns {Promise} - jQuery AJAX promise nesnesi
     */
    function put(params) {
        const options = configureRequestOptions({
            url: params.url,
            type: 'PUT',
            data: params.data || {},
            success: params.success || function() {},
            error: params.error || function() {},
            complete: params.complete || function() {},
            timeout: params.timeout || defaultAjaxSettings.timeout
        });

        if (params.token) {
            options.headers = {
                ...options.headers,
                'RequestVerificationToken': params.token
            };
        }

        return $.ajax(options);
    }

    /**
     * AJAX DELETE isteği gönderir
     * @param {object} params - İstek parametreleri
     * @returns {Promise} - jQuery AJAX promise nesnesi
     */
    function delete_(params) {
        const options = configureRequestOptions({
            url: params.url,
            type: 'POST',
            data: params.data || {},
            success: params.success || function() {},
            error: params.error || function() {},
            complete: params.complete || function() {},
            timeout: params.timeout || defaultAjaxSettings.timeout
        });

        if (params.token) {
            options.headers = {
                ...options.headers,
                'RequestVerificationToken': params.token
            };
        }

        return $.ajax(options);
    }

    /**
     * Varsayılan HTTP başlıklarını ayarlar
     * @param {object} headers - HTTP başlıkları
     */
    function setDefaultHeaders(headers) {
        $.extend(defaultHeaders, headers);
        defaultAjaxSettings.headers = defaultHeaders;
    }

    /**
     * Tam URL oluşturur
     * @param {string} baseUrl - API temel URL'i
     * @param {string} endpoint - API endpoint'i
     * @returns {string} - Tam URL
     */
    function buildUrl(baseUrl, endpoint) {
        if (!baseUrl.endsWith('/')) {
            baseUrl += '/';
        }
        
        if (endpoint.startsWith('/')) {
            endpoint = endpoint.substring(1);
        }
        
        return baseUrl + endpoint;
    }

    // Public API
    return {
        get: get,
        post: post,
        put: put,
        delete: delete_,
        submitForm: submitForm,
        setDefaultHeaders: setDefaultHeaders,
        buildUrl: buildUrl
    };
})();

// Küresel namespace'e ekleme - zaten tanımlanmışsa tekrar tanımlamayı önlemek için
window.ApiService = window.ApiService || ApiService;

