/**
 * Price Utils Module
 * Para birimi formatlama ve dönüştürme işlemleri için utility modülü
 * Tüm modüller tarafından kullanılabilecek ortak price işlevleri
 * Currency sembolleri ve exchange rate'ler database'den gelir, hard-coded değer yok
 */
var PriceUtils = PriceUtils || (function() {
    'use strict';
    
    // Constants
    var CONSTANTS = {
        DEFAULT_CURRENCY: 'TRY',
        DEFAULT_DECIMAL_PLACES: 2,
        DEFAULT_BASE_CURRENCY: 'TRY'
    };
    
    /**
     * Exchange rate'leri ve currency bilgilerini yükler
     * JSON script tag'lerinden veya parametre olarak alınır
     * @param {object} options - Konfigürasyon seçenekleri
     * @param {object} options.exchangeRates - Exchange rate dictionary (JSON'dan alınır veya parametre)
     * @param {object} options.baseCurrency - Base currency bilgisi (JSON'dan alınır veya parametre)
     * @returns {object} - Exchange rate'ler ve base currency
     */
    function loadCurrencyData(options) {
        options = options || {};
        var exchangeRates = options.exchangeRates || {};
        var baseCurrency = options.baseCurrency || { code: CONSTANTS.DEFAULT_BASE_CURRENCY, symbol: '₺' };
        
        // JSON script tag'lerinden yükle (eğer parametre verilmemişse)
        if (!options.exchangeRates) {
            try {
                var exchangeRatesData = document.getElementById('currency-exchange-rates');
                if (exchangeRatesData) {
                    exchangeRates = JSON.parse(exchangeRatesData.textContent || '{}');
                }
                
                var baseCurrencyData = document.getElementById('base-currency');
                if (baseCurrencyData) {
                    var parsed = JSON.parse(baseCurrencyData.textContent || '{"code":"TRY","symbol":"₺"}');
                    baseCurrency = { 
                        code: parsed.code || CONSTANTS.DEFAULT_BASE_CURRENCY, 
                        symbol: parsed.symbol || '₺' 
                    };
                }
            } catch (e) {
                console.warn('PriceUtils: Exchange rate data parse error:', e.message);
            }
        }
        
        return {
            exchangeRates: exchangeRates,
            baseCurrency: baseCurrency
        };
    }
    
    /**
     * Currency bilgisini alır (exchange rate'lerden)
     * @param {string} currency - Currency kodu (TRY, USD, EUR, vb.)
     * @param {object} exchangeRates - Exchange rate dictionary
     * @returns {object|null} - Currency bilgisi (code, symbol, exchangeRate, decimalPlaces)
     */
    function getCurrencyInfo(currency, exchangeRates) {
        if (!currency || !exchangeRates) {
            return null;
        }
        
        var currencyUpper = currency.toUpperCase();
        var currencyInfo = exchangeRates[currencyUpper];
        
        if (!currencyInfo) {
            // Fallback: currency code'unu direkt kullan
            return {
                code: currencyUpper,
                symbol: currencyUpper,
                exchangeRate: 1.0,
                decimalPlaces: CONSTANTS.DEFAULT_DECIMAL_PLACES
            };
        }
        
        return {
            code: currencyInfo.code || currencyUpper,
            symbol: currencyInfo.symbol || currencyUpper,
            exchangeRate: currencyInfo.exchangeRate || 1.0,
            decimalPlaces: currencyInfo.decimalPlaces !== undefined ? currencyInfo.decimalPlaces : CONSTANTS.DEFAULT_DECIMAL_PLACES
        };
    }
    
    /**
     * Fiyatı bir currency'den başka bir currency'ye dönüştürür
     * ExchangeRate: 1 CURRENCY = X TRY formatında (TRY için ExchangeRate = 1.0)
     * @param {number} price - Kaynak currency cinsinden fiyat
     * @param {string} fromCurrency - Kaynak currency kodu (Property.Currency - örn: EUR)
     * @param {string} toCurrency - Hedef currency kodu
     * @param {object} exchangeRates - Exchange rate dictionary
     * @param {object} baseCurrency - Base currency bilgisi (TRY - sadece fallback için)
     * @returns {number} - Dönüştürülmüş fiyat
     */
    function convertPrice(price, fromCurrency, toCurrency, exchangeRates) {
        if (!price || price <= 0) {
            return 0;
        }
        
        // Aynı currency ise direkt döndür
        if (!fromCurrency || !toCurrency || fromCurrency.toUpperCase() === toCurrency.toUpperCase()) {
            return price;
        }
        
        var fromCurrencyUpper = fromCurrency.toUpperCase();
        var toCurrencyUpper = toCurrency.toUpperCase();
        
        var fromRate = getCurrencyInfo(fromCurrencyUpper, exchangeRates);
        var toRate = getCurrencyInfo(toCurrencyUpper, exchangeRates);
        
        if (!fromRate || !toRate || !fromRate.exchangeRate || !toRate.exchangeRate) {
            return price;
        }
        
        // ⚠️ ÖNEMLİ: fromCurrency zaten Property.Currency (EUR, USD, vb.) - TRY değil!
        // Exchange rate formatı: 1 CURRENCY = X TRY
        // Dönüşüm mantığı:
        // 1. Önce TRY'ye çevir: price * fromRate.exchangeRate
        // 2. Sonra hedef currency'ye çevir: priceInTry / toRate.exchangeRate
        
        // Örnek: 93.75 EUR -> TRY -> USD
        // 1. 93.75 * 32 = 3000 TRY
        // 2. 3000 / 30 = 100 USD
        
        var priceInTry = price * fromRate.exchangeRate;
        var priceInTargetCurrency = priceInTry / toRate.exchangeRate;
        
        return priceInTargetCurrency;
    }
    
    /**
     * Sayıyı Türkçe formatına çevirir (nokta binlik, virgül ondalık)
     * @param {number} value - Formatlanacak değer
     * @param {number} decimalPlaces - Ondalık basamak sayısı
     * @returns {string} - Formatlanmış sayı (örn: "1.234,56")
     */
    function formatNumber(value, decimalPlaces) {
        decimalPlaces = decimalPlaces !== undefined ? decimalPlaces : CONSTANTS.DEFAULT_DECIMAL_PLACES;
        
        var valueStr = parseFloat(value || 0).toFixed(decimalPlaces);
        var parts = valueStr.split('.');
        var integerPart = parts[0];
        var decimalPart = parts[1] || '00'.substring(0, decimalPlaces);
        
        // Binlik ayırıcı ekle (her 3 hanede bir nokta)
        var formattedInteger = '';
        for (var i = integerPart.length - 1, count = 0; i >= 0; i--, count++) {
            if (count > 0 && count % 3 === 0) {
                formattedInteger = '.' + formattedInteger;
            }
            formattedInteger = integerPart[i] + formattedInteger;
        }
        
        // Ondalık kısmı virgül ile birleştir
        return formattedInteger + ',' + decimalPart;
    }
    
    /**
     * Fiyatı formatla (CurrencyExtensions formatına uygun, tamamen dinamik)
     * Currency sembolü ve format bilgileri database'den gelir (exchangeRates dictionary)
     * Hard-coded currency kontrolleri YOK - tüm semboller dinamik
     * @param {number} value - Formatlanacak fiyat
     * @param {string} currency - Currency kodu (TRY, USD, EUR, vb.)
     * @param {object} options - Konfigürasyon seçenekleri
     * @param {object} options.exchangeRates - Exchange rate dictionary (JSON'dan alınır veya parametre)
     * @param {object} options.baseCurrency - Base currency bilgisi (JSON'dan alınır veya parametre)
     * @param {boolean} options.symbolBefore - Sembol başta mı? (varsayılan: false - CurrencyExtensions'a göre belirlenir)
     * @returns {string} - Formatlanmış fiyat (örn: "1.234,56 ₺", "$1.234,56")
     */
    function formatPrice(value, currency, options) {
        options = options || {};
        
        // Currency data'yı yükle
        var currencyData = loadCurrencyData(options);
        var exchangeRates = currencyData.exchangeRates;
        var baseCurrency = currencyData.baseCurrency;
        
        if (!currency) {
            currency = CONSTANTS.DEFAULT_CURRENCY;
        }
        
        var currencyUpper = currency.toUpperCase();
        var currencyInfo = getCurrencyInfo(currencyUpper, exchangeRates);
        
        if (!currencyInfo) {
            // Fallback: basit format
            var fallback = formatNumber(value || 0, CONSTANTS.DEFAULT_DECIMAL_PLACES);
            return fallback + ' ' + currencyUpper;
        }
        
        var symbol = currencyInfo.symbol || currencyUpper;
        var decimalPlaces = currencyInfo.decimalPlaces !== undefined ? currencyInfo.decimalPlaces : CONSTANTS.DEFAULT_DECIMAL_PLACES;
        
        /**
         * Sembol konumunu belirler (tamamen dinamik - hard-coded currency kontrolü YOK)
         * Database'den gelen sembol içeriğine göre belirlenir
         * CurrencyExtensions formatına uygun: $ ve £ gibi özel karakterler genellikle başta gösterilir
         * İleride Currency entity'sine SymbolPosition property'si eklenebilir
         */
        function shouldSymbolBeBefore(symbolValue) {
            // Eğer options'ta belirtilmişse onu kullan
            if (options.symbolBefore !== undefined) {
                return options.symbolBefore;
            }
            
            // Sembol içeriğine göre belirle (dinamik)
            // $ ve £ gibi özel karakterler genellikle başta gösterilir
            // Database'den gelen sembol içeriğine göre otomatik belirlenir
            if (!symbolValue || typeof symbolValue !== 'string') {
                return false;
            }
            
            // Sembol başında $, £ gibi özel karakterler varsa başta göster
            // Bu tamamen dinamik - hard-coded currency kodu yok
            var symbolTrimmed = symbolValue.trim();
            return symbolTrimmed.length > 0 && (
                symbolTrimmed.charAt(0) === '$' || 
                symbolTrimmed.charAt(0) === '£' ||
                symbolTrimmed.charAt(0) === '€'
            );
        }
        
        var symbolBefore = shouldSymbolBeBefore(symbol);
        
        // Zero değeri için format
        if (!value || value <= 0) {
            var zeroDecimal = decimalPlaces > 0 ? ',' + '0'.repeat(decimalPlaces) : '';
            var zeroFormatted = '0' + zeroDecimal;
            
            return symbolBefore ? symbol + zeroFormatted : zeroFormatted + ' ' + symbol;
        }
        
        // Sayıyı formatla
        var formatted = formatNumber(value, decimalPlaces);
        
        // Sembol konumuna göre formatla (tamamen dinamik)
        return symbolBefore ? symbol + formatted : formatted + ' ' + symbol;
    }
    
    /**
     * Fiyatı base currency'den hedef currency'ye dönüştürür ve formatlar
     * @param {number} priceInBase - Base currency (TRY) cinsinden fiyat
     * @param {string} fromCurrency - Kaynak currency kodu
     * @param {string} toCurrency - Hedef currency kodu
     * @param {object} options - Konfigürasyon seçenekleri
     * @returns {string} - Formatlanmış ve dönüştürülmüş fiyat
     */
    function convertAndFormatPrice(priceInBase, fromCurrency, toCurrency, options) {
        options = options || {};
        
        // Currency data'yı yükle
        var currencyData = loadCurrencyData(options);
        var exchangeRates = currencyData.exchangeRates;
        
        // Fiyatı dönüştür (baseCurrency parametresi artık gereksiz - her zaman TRY üzerinden çeviri yapılıyor)
        var convertedPrice = convertPrice(priceInBase, fromCurrency, toCurrency, exchangeRates);
        
        // Formatla ve döndür
        return formatPrice(convertedPrice, toCurrency, options);
    }
    
    // Public API
    return {
        // Constants
        CONSTANTS: CONSTANTS,
        
        // Currency utilities
        loadCurrencyData: loadCurrencyData,
        getCurrencyInfo: getCurrencyInfo,
        convertPrice: convertPrice,
        formatNumber: formatNumber,
        formatPrice: formatPrice,
        convertAndFormatPrice: convertAndFormatPrice
    };
})();

// Küresel namespace'e ekleme
window.PriceUtils = window.PriceUtils || PriceUtils;
