# Core Services Dokümantasyonu

Bu dokümantasyon, `trimango` projesindeki core JavaScript servislerinin kullanımını açıklar. Tüm core servisler modüler yapıda tasarlanmıştır ve OOP/SOLID prensiplerine uygundur.

## İçindekiler

1. [ApiService](#apiservice)
2. [UIService](#uiservice)
3. [HelperService](#helperservice)
4. [LocalizationService](#localizationservice)
5. [AppInit](#appinit)
6. [ModuleFactory](#modulefactory)
7. [GTM / dataLayer (TrimangoGtm)](#gtm--datalayer-trimangogtm)

---

## ApiService

**Dosya:** `api.service.js`  
**Amaç:** AJAX isteklerini yönetmek için merkezi servis

### Metodlar

#### `ApiService.get(params)`
GET isteği gönderir.

**Parametreler:**
- `params.url` (string, zorunlu): İstek URL'i
- `params.data` (object, opsiyonel): Query parametreleri
- `params.success` (function, opsiyonel): Başarılı yanıt callback'i
- `params.error` (function, opsiyonel): Hata callback'i
- `params.complete` (function, opsiyonel): İstek tamamlandığında callback
- `params.timeout` (number, opsiyonel): Timeout süresi (ms)
- `params.useFetch` (boolean, opsiyonel): Fetch API kullan (default: false, jQuery AJAX kullanır)
- `params.token` (string, opsiyonel): CSRF token
- `params.headers` (object, opsiyonel): Custom HTTP headers

**Örnek Kullanım:**

```javascript
// jQuery AJAX ile (varsayılan)
ApiService.get({
    url: '/api/properties',
    data: { page: 1, pageSize: 20 },
    success: function(response) {
        console.log('Başarılı:', response);
    },
    error: function(error) {
        console.error('Hata:', error);
    }
});

// Fetch API ile
ApiService.get({
    url: '/api/properties',
    data: { page: 1 },
    useFetch: true,
    success: function(response) {
        console.log('Başarılı:', response);
    },
    error: function(error) {
        UIService.notification.error('Veri yüklenirken hata oluştu');
    }
});
```

**Gerçek Proje Örneği:**

```javascript
// property.index.module.js içinde
function loadPropertiesAjax(form) {
    var params = new URLSearchParams();
    // ... parametreleri hazırla
    
    ApiService.get({
        url: window.propertiesAjaxUrl + '?' + params.toString(),
        useFetch: true,
        success: function(html) {
            container.innerHTML = html;
            // ... DOM güncellemeleri
        },
        error: function(error) {
            UIService.notification.error('Villa listesi yüklenirken hata oluştu');
        }
    });
}
```

---

#### `ApiService.post(params)`
POST isteği gönderir.

**Parametreler:**
- `params.url` (string, zorunlu): İstek URL'i
- `params.data` (object, zorunlu): POST verisi
- `params.success` (function, opsiyonel): Başarılı yanıt callback'i
- `params.error` (function, opsiyonel): Hata callback'i
- `params.complete` (function, opsiyonel): İstek tamamlandığında callback
- `params.timeout` (number, opsiyonel): Timeout süresi (ms)
- `params.useFetch` (boolean, opsiyonel): Fetch API kullan
- `params.token` (string, opsiyonel): CSRF token
- `params.headers` (object, opsiyonel): Custom HTTP headers

**Örnek Kullanım:**

```javascript
// jQuery AJAX ile
ApiService.post({
    url: '/api/reservations',
    data: {
        propertyId: '123',
        checkIn: '2024-01-15',
        checkOut: '2024-01-20'
    },
    success: function(response) {
        if (response.success) {
            UIService.notification.success('Rezervasyon başarıyla oluşturuldu');
        }
    },
    error: function(error) {
        UIService.notification.error('Rezervasyon oluşturulurken hata oluştu');
    }
});

// Fetch API ile
ApiService.post({
    url: '/api/coupons/apply',
    data: { couponCode: 'SUMMER2024' },
    useFetch: true,
    success: function(response) {
        updatePriceSummary(response);
    },
    error: function(error) {
        UIService.notification.error('Kupon kodu uygulanamadı');
    }
});
```

**Gerçek Proje Örneği:**

```javascript
// property.booking.module.js içinde
function calculatePriceFromServer(checkIn, checkOut, adults, children) {
    ApiService.post({
        url: self._config.pricingUrl,
        data: {
            propertyId: self._config.propertyId,
            checkIn: checkIn,
            checkOut: checkOut,
            adultCount: adults,
            childCount: children
        },
        useFetch: true,
        success: function(response) {
            self._nightlyPrice = response.nightlyPrice;
            self._minStay = response.minStay;
            updateSummary();
        },
        error: function(error) {
            UIService.notification.error('Fiyat hesaplanırken hata oluştu');
        }
    });
}
```

---

#### `ApiService.submitForm(params)`
Form verilerini POST isteği ile gönderir.

**Parametreler:**
- `params.form` (string|jQuery|HTMLElement, zorunlu): Form seçicisi veya form elementi
- `params.url` (string, zorunlu): İstek URL'i
- `params.method` (string, opsiyonel): HTTP metodu (default: 'POST')
- `params.processData` (boolean, opsiyonel): FormData işleme (default: true)
- `params.success` (function, opsiyonel): Başarılı yanıt callback'i
- `params.error` (function, opsiyonel): Hata callback'i
- `params.complete` (function, opsiyonel): İstek tamamlandığında callback

**Örnek Kullanım:**

```javascript
// Basit form gönderimi
ApiService.submitForm({
    form: '#review-form',
    url: '/api/reviews',
    success: function(response) {
        if (response.success) {
            UIService.notification.success('Yorumunuz başarıyla gönderildi');
            document.getElementById('review-form').reset();
        }
    },
    error: function(error) {
        UIService.notification.error('Yorum gönderilirken hata oluştu');
    }
});
```

**Gerçek Proje Örneği:**

```javascript
// property.review.module.js içinde
function submitReviewForm() {
    if (self._submitting) {
        return; // Çift gönderimi önle
    }
    
    self._submitting = true;
    
    ApiService.submitForm({
        form: reviewForm,
        url: self._config.submitUrl,
        success: function(response) {
            self._submitting = false;
            
            if (response && response.success) {
                var successMessage = LocalizationService.get("Review.Success", "Yorumunuz başarıyla gönderildi.");
                UIService.notification.success(successMessage);
                reviewForm.reset();
                self.refreshCaptcha();
                setTimeout(function() {
                    window.location.reload();
                }, 3000);
            } else {
                // Hata mesajlarını göster
                displayValidationErrors(response.errors);
            }
        },
        error: function(error) {
            self._submitting = false;
            var errorMessage = LocalizationService.get("Review.Error", "Yorum gönderilirken bir hata oluştu.");
            UIService.notification.error(errorMessage);
            self.refreshCaptcha();
        }
    });
}
```

---

#### `ApiService.put(params)`
PUT isteği gönderir.

**Örnek Kullanım:**

```javascript
ApiService.put({
    url: '/api/users/123',
    data: {
        firstName: 'Ahmet',
        lastName: 'Yılmaz'
    },
    success: function(response) {
        UIService.notification.success('Kullanıcı güncellendi');
    }
});
```

---

#### `ApiService.delete(params)`
DELETE isteği gönderir (POST olarak gönderilir, ASP.NET Core için).

**Örnek Kullanım:**

```javascript
ApiService.delete({
    url: '/api/properties/123',
    success: function(response) {
        UIService.notification.success('Villa silindi');
        // DOM'dan kaldır
    }
});
```

---

#### `ApiService.setDefaultHeaders(headers)`
Varsayılan HTTP başlıklarını ayarlar.

**Örnek Kullanım:**

```javascript
ApiService.setDefaultHeaders({
    'X-Custom-Header': 'value',
    'Authorization': 'Bearer token123'
});
```

---

#### `ApiService.buildUrl(baseUrl, endpoint)`
Tam URL oluşturur.

**Örnek Kullanım:**

```javascript
var fullUrl = ApiService.buildUrl('/api', 'properties');
// Sonuç: '/api/properties'
```

---

## UIService

**Dosya:** `ui.service.js`  
**Amaç:** UI işlemlerini yönetmek için merkezi servis (notifications, modals, loaders, forms, tables)

### Bölümler

#### 1. Notification (Bildirimler)

##### `UIService.notification.success(message, callback, options)`
Başarı bildirimi gösterir (Tailwind CSS tasarımı).

**Parametreler:**
- `message` (string, zorunlu): Bildirim mesajı
- `callback` (function, opsiyonel): İşlem sonrası çağrılacak fonksiyon
- `options` (object, opsiyonel): Ek opsiyonlar
  - `options.forceSwal` (boolean): SweetAlert kullan (default: false, Tailwind kullanır)

**Örnek Kullanım:**

```javascript
// Tailwind notification (varsayılan)
UIService.notification.success('İşlem başarıyla tamamlandı');

// Callback ile
UIService.notification.success('Kayıt başarılı', function() {
    window.location.reload();
});

// SweetAlert ile (zorunlu)
UIService.notification.success('Başarılı', null, { forceSwal: true });
```

**Gerçek Proje Örneği:**

```javascript
// property.review.module.js içinde
if (response && response.success) {
    var successMessage = LocalizationService.get("Review.Success", "Yorumunuz başarıyla gönderildi.");
    UIService.notification.success(successMessage);
    reviewForm.reset();
    self.refreshCaptcha();
}
```

---

##### `UIService.notification.error(error, options)`
Hata bildirimi gösterir.

**Parametreler:**
- `error` (string|object, zorunlu): Hata mesajı veya hata nesnesi
- `options` (object, opsiyonel): Ek opsiyonlar
  - `options.forceSwal` (boolean): SweetAlert kullan

**Örnek Kullanım:**

```javascript
// String mesaj
UIService.notification.error('Bir hata oluştu');

// AJAX error nesnesi
ApiService.get({
    url: '/api/data',
    error: function(error) {
        UIService.notification.error(error);
    }
});

// SweetAlert ile
UIService.notification.error('Kritik hata', { forceSwal: true });
```

**Gerçek Proje Örneği:**

```javascript
// property.review.module.js içinde
error: function(error) {
    self._submitting = false;
    var errorMessage = LocalizationService.get("Review.Error", "Yorum gönderilirken bir hata oluştu.");
    UIService.notification.error(errorMessage);
    self.refreshCaptcha();
}
```

---

##### `UIService.notification.info(message, options)`
Bilgi bildirimi gösterir.

**Örnek Kullanım:**

```javascript
UIService.notification.info('Bilgilendirme mesajı');
```

---

##### `UIService.notification.warning(message, options)`
Uyarı bildirimi gösterir.

**Örnek Kullanım:**

```javascript
UIService.notification.warning('Uyarı mesajı');
```

---

##### `UIService.notification.confirm(title, text, confirmCallback, cancelCallback, options)`
Onay dialog'u gösterir (SweetAlert kullanır).

**Örnek Kullanım:**

```javascript
UIService.notification.confirm(
    'Silme Onayı',
    'Bu kaydı silmek istediğinizden emin misiniz?',
    function() {
        // Onaylandı
        ApiService.delete({
            url: '/api/properties/123',
            success: function() {
                UIService.notification.success('Silindi');
            }
        });
    },
    function() {
        // İptal edildi
        console.log('İptal edildi');
    }
);
```

---

##### `UIService.notification.loading(message, options)`
Yükleme bildirimi gösterir (SweetAlert).

**Örnek Kullanım:**

```javascript
var loadingPromise = UIService.notification.loading('Yükleniyor...');

// İşlem tamamlandığında
UIService.notification.hideLoading();
```

---

#### 2. Loader (Yükleme Göstergeleri)

##### `UIService.loader.show(container, message)`
Yükleme göstergesi gösterir.

**Parametreler:**
- `container` (string|HTMLElement|jQuery|'global', opsiyonel): Container seçicisi veya 'global'
- `message` (string, opsiyonel): Yükleme mesajı (global loader için)

**Örnek Kullanım:**

```javascript
// Global loader
UIService.loader.show('global', 'Yükleniyor...');

// Container'a özel loader
UIService.loader.show('#properties-list', 'Villalar yükleniyor...');

// jQuery selector ile
UIService.loader.show($('#my-container'));
```

**Gerçek Proje Örneği:**

```javascript
// property.index.module.js içinde
function loadPropertiesAjax(form) {
    var container = document.getElementById('properties-list-container');
    
    // Loading state
    container.innerHTML = '<div class="loading">...</div>';
    // veya
    UIService.loader.show('#properties-list-container');
    
    ApiService.get({
        url: url,
        success: function(html) {
            UIService.loader.hide('#properties-list-container');
            container.innerHTML = html;
        }
    });
}
```

---

##### `UIService.loader.hide(container)`
Yükleme göstergesini gizler.

**Örnek Kullanım:**

```javascript
UIService.loader.hide('global');
UIService.loader.hide('#properties-list');
```

---

##### `UIService.loader.isActive(container)`
Loader'ın aktif olup olmadığını kontrol eder.

**Örnek Kullanım:**

```javascript
if (UIService.loader.isActive('global')) {
    console.log('Loader aktif');
}
```

---

##### `UIService.loader.showTemporary(container, duration, message)`
Yükleme göstergesini belirli bir süre gösterir.

**Örnek Kullanım:**

```javascript
UIService.loader.showTemporary('#my-container', 2000, 'Kaydediliyor...');
```

---

#### 3. Modal (Modal Pencereler)

##### `UIService.modal.show(modalId)`
Modal'ı gösterir.

**Örnek Kullanım:**

```javascript
UIService.modal.show('my-modal');
```

---

##### `UIService.modal.hide(modalId)`
Modal'ı gizler.

**Örnek Kullanım:**

```javascript
UIService.modal.hide('my-modal');
```

---

##### `UIService.modal.create(modalId, title, content, options)`
Yeni modal oluşturur.

**Parametreler:**
- `modalId` (string, zorunlu): Modal ID'si
- `title` (string, zorunlu): Modal başlığı
- `content` (string, zorunlu): Modal içeriği (HTML)
- `options` (object, opsiyonel): Modal ayarları
  - `options.size` (string): 'modal-sm', 'modal-lg', 'modal-xl' (default: 'modal-lg')
  - `options.footer` (boolean): Footer göster (default: true)
  - `options.closeButton` (boolean): Kapat butonu (default: true)
  - `options.confirmButton` (boolean): Onay butonu (default: true)
  - `options.confirmButtonText` (string): Onay butonu metni (default: 'Kaydet')
  - `options.cancelButton` (boolean): İptal butonu (default: true)
  - `options.cancelButtonText` (string): İptal butonu metni (default: 'İptal')
  - `options.confirmCallback` (function): Onay callback'i
  - `options.cancelCallback` (function): İptal callback'i

**Örnek Kullanım:**

```javascript
var modal = UIService.modal.create('edit-modal', 'Düzenle', '<p>İçerik</p>', {
    size: 'modal-lg',
    confirmButtonText: 'Kaydet',
    confirmCallback: function() {
        // Kaydet işlemi
        UIService.modal.hide('edit-modal');
    }
});

modal.show();
```

---

##### `UIService.modal.loadAndShow(selector, content, modalId, options)`
Modal içeriğini yükler ve gösterir.

**Örnek Kullanım:**

```javascript
ApiService.get({
    url: '/api/modal-content',
    success: function(html) {
        UIService.modal.loadAndShow('#modal-container', html, 'my-modal', {
            onShown: function() {
                console.log('Modal gösterildi');
            },
            onHidden: function() {
                console.log('Modal kapatıldı');
            }
        });
    }
});
```

---

##### `UIService.modal.find(modalId)`
Modal'ı bulur ve modal nesnesini döndürür.

**Örnek Kullanım:**

```javascript
var modal = UIService.modal.find('my-modal');
if (modal) {
    modal.show();
    // veya
    modal.close();
    // veya
    modal.updateContent('<p>Yeni içerik</p>');
}
```

---

##### `UIService.modal.updateContent(modalId, content, options)`
Modal içeriğini günceller.

**Örnek Kullanım:**

```javascript
UIService.modal.updateContent('my-modal', '<p>Yeni içerik</p>', {
    updateTitle: true,
    title: 'Yeni Başlık',
    initTooltips: true
});
```

---

##### `UIService.modal.confirm(title, message, confirmCallback, cancelCallback, options)`
Onay modal'ı gösterir.

**Örnek Kullanım:**

```javascript
UIService.modal.confirm(
    'Silme Onayı',
    'Bu kaydı silmek istediğinizden emin misiniz?',
    function() {
        // Onaylandı
        ApiService.delete({
            url: '/api/properties/123',
            success: function() {
                UIService.notification.success('Silindi');
            }
        });
    },
    function() {
        // İptal edildi
    }
);
```

---

#### 4. Form (Form İşlemleri)

##### `UIService.form.reset(formSelector)`
Form inputlarını temizler.

**Örnek Kullanım:**

```javascript
UIService.form.reset('#my-form');
```

---

##### `UIService.form.validate(formSelector)`
Form validasyonunu yapar.

**Örnek Kullanım:**

```javascript
if (UIService.form.validate('#my-form')) {
    // Form geçerli, gönder
    ApiService.submitForm({
        form: '#my-form',
        url: '/api/submit'
    });
} else {
    UIService.notification.warning('Lütfen formu doldurun');
}
```

---

##### `UIService.form.getFormData(formSelector)`
Form verisini JSON olarak alır.

**Örnek Kullanım:**

```javascript
var formData = UIService.form.getFormData('#my-form');
console.log(formData);
// { name: 'Ahmet', email: 'ahmet@example.com', ... }
```

---

#### 5. Table (Tablo İşlemleri)

##### `UIService.table.filter(tableSelector, query, columns)`
Tablo satırlarını filtreler.

**Örnek Kullanım:**

```javascript
// Tüm sütunlarda ara
UIService.table.filter('#my-table', 'Ahmet');

// Belirli sütunlarda ara (0, 1, 2. sütunlar)
UIService.table.filter('#my-table', 'Ahmet', [0, 1, 2]);
```

---

##### `UIService.table.sort(tableSelector, colIndex, asc)`
Tablo satırlarını sıralar.

**Örnek Kullanım:**

```javascript
// 0. sütuna göre artan sırala
UIService.table.sort('#my-table', 0, true);

// 1. sütuna göre azalan sırala
UIService.table.sort('#my-table', 1, false);
```

---

#### 6. Tooltip ve Popover

##### `UIService.initTooltips(selector)`
Tooltip'leri başlatır.

**Örnek Kullanım:**

```javascript
// Tüm tooltip'leri başlat
UIService.initTooltips();

// Belirli selector için
UIService.initTooltips('[data-bs-toggle="tooltip"]');
```

---

##### `UIService.initPopovers(selector)`
Popover'ları başlatır.

**Örnek Kullanım:**

```javascript
UIService.initPopovers('[data-bs-toggle="popover"]');
```

---

## HelperService

**Dosya:** `helper.service.js`  
**Amaç:** Genel yardımcı fonksiyonlar

### Metodlar

#### `HelperService.createSlug(text)`
String'i SEO-friendly slug'a çevirir (Türkçe karakter desteği ile).

**Örnek Kullanım:**

```javascript
var slug = HelperService.createSlug('İstanbul Büyükşehir');
// Sonuç: 'istanbul-buyuksehir'

var slug = HelperService.createSlug('Kaş, Antalya');
// Sonuç: 'kas-antalya'
```

**Gerçek Proje Örneği:**

```javascript
// property.index.module.js içinde
function updateSlugInputs() {
    var locationName = locationNameInput.value.trim();
    if (locationName) {
        locationSlugInput.value = HelperService.createSlug(locationName);
    }
}
```

---

#### `HelperService.extractSlugFromUrl(url)`
URL'den slug çıkarır.

**Örnek Kullanım:**

```javascript
var slug = HelperService.extractSlugFromUrl('https://example.com/tr/villalar/marmaris-villa-123');
// Sonuç: 'marmaris-villa-123'
```

---

#### `HelperService.formatCurrency(amount, currency, culture)`
Para formatlar.

**Örnek Kullanım:**

```javascript
HelperService.formatCurrency(1500, 'TRY', 'tr-TR');
// Sonuç: '₺1.500'

HelperService.formatCurrency(1500, 'USD', 'en-US');
// Sonuç: '$1,500'
```

---

#### `HelperService.formatDate(date, format)`
Tarih formatlar.

**Örnek Kullanım:**

```javascript
HelperService.formatDate(new Date(), 'DD.MM.YYYY');
// Sonuç: '15.01.2024'

HelperService.formatDate('2024-01-15', 'YYYY-MM-DD');
// Sonuç: '2024-01-15'
```

---

#### `HelperService.calculateNights(checkIn, checkOut)`
Tarih aralığındaki gece sayısını hesaplar.

**Örnek Kullanım:**

```javascript
var nights = HelperService.calculateNights('2024-01-15', '2024-01-20');
// Sonuç: 5
```

**Gerçek Proje Örneği:**

```javascript
// property.booking.module.js içinde
var nights = HelperService.calculateNights(checkIn, checkOut);
var totalPrice = nights * nightlyPrice;
```

---

#### `HelperService.getLanguageFromUrl(url)`
URL'den dil kodunu çıkarır.

**Örnek Kullanım:**

```javascript
var lang = HelperService.getLanguageFromUrl('/tr/villalar');
// Sonuç: 'tr'

var lang = HelperService.getLanguageFromUrl();
// window.location.pathname'den otomatik alır
```

---

#### `HelperService.getCsrfToken()`
CSRF token'ı alır.

**Örnek Kullanım:**

```javascript
var token = HelperService.getCsrfToken();
ApiService.post({
    url: '/api/submit',
    data: { name: 'Test' },
    token: token
});
```

---

#### `HelperService.getQueryParam(name)`
Query string parametresini alır.

**Örnek Kullanım:**

```javascript
// URL: /tr/villalar?page=2&sort=price
var page = HelperService.getQueryParam('page');
// Sonuç: '2'
```

---

#### `HelperService.getQueryParams()`
Tüm query string parametrelerini objeye dönüştürür.

**Örnek Kullanım:**

```javascript
// URL: /tr/villalar?page=2&sort=price
var params = HelperService.getQueryParams();
// Sonuç: { page: '2', sort: 'price' }
```

---

#### `HelperService.buildUrl(baseUrl, params)`
URL oluşturur.

**Örnek Kullanım:**

```javascript
var url = HelperService.buildUrl('/api/properties', {
    page: 1,
    pageSize: 20,
    sort: 'price'
});
// Sonuç: '/api/properties?page=1&pageSize=20&sort=price'
```

---

#### `HelperService.escapeHtml(text)`
String'i HTML escape eder.

**Örnek Kullanım:**

```javascript
var safe = HelperService.escapeHtml('<script>alert("XSS")</script>');
// Sonuç: '&lt;script&gt;alert(&quot;XSS&quot;)&lt;/script&gt;'
```

---

#### `HelperService.unescapeHtml(html)`
HTML'i decode eder.

**Örnek Kullanım:**

```javascript
var text = HelperService.unescapeHtml('&lt;div&gt;Test&lt;/div&gt;');
// Sonuç: '<div>Test</div>'
```

---

#### `HelperService.debounce(func, wait)`
Debounce fonksiyonu oluşturur.

**Örnek Kullanım:**

```javascript
var debouncedSearch = HelperService.debounce(function(query) {
    ApiService.get({
        url: '/api/search',
        data: { q: query }
    });
}, 300);

// Input değiştiğinde
input.addEventListener('input', function() {
    debouncedSearch(this.value);
});
```

---

#### `HelperService.throttle(func, limit)`
Throttle fonksiyonu oluşturur.

**Örnek Kullanım:**

```javascript
var throttledScroll = HelperService.throttle(function() {
    console.log('Scroll event');
}, 100);

window.addEventListener('scroll', throttledScroll);
```

---

#### `HelperService.deepClone(obj)`
Deep clone yapar.

**Örnek Kullanım:**

```javascript
var original = { name: 'Test', nested: { value: 123 } };
var cloned = HelperService.deepClone(original);
cloned.nested.value = 456;
// original.nested.value hala 123
```

---

#### `HelperService.objectToQueryString(obj)`
Objeyi query string'e dönüştürür.

**Örnek Kullanım:**

```javascript
var query = HelperService.objectToQueryString({ page: 1, sort: 'price' });
// Sonuç: 'page=1&sort=price'
```

---

#### `HelperService.queryStringToObject(queryString)`
Query string'i objeye dönüştürür.

**Örnek Kullanım:**

```javascript
var obj = HelperService.queryStringToObject('page=1&sort=price');
// Sonuç: { page: '1', sort: 'price' }
```

---

## LocalizationService

**Dosya:** `localization.service.js`  
**Amaç:** JavaScript tarafında localization string'lerini yönetmek

### Metodlar

#### `LocalizationService.init(localizationData)`
Localization servisini başlatır.

**Örnek Kullanım:**

```javascript
// JSON script tag'den otomatik yüklenir
// Veya manuel olarak:
LocalizationService.init({
    Common: {
        Save: 'Kaydet',
        Cancel: 'İptal'
    },
    Review: {
        Success: 'Yorumunuz başarıyla gönderildi'
    }
});
```

**Razor View'da Kullanım:**

```cshtml
<script id="localization-data" type="application/json">
@Html.Raw(Json.Serialize(new {
    Common = new {
        Save = LocalizationService.GetString("Common.Save"),
        Cancel = LocalizationService.GetString("Common.Cancel")
    },
    Review = new {
        Success = LocalizationService.GetString("Review.Success")
    }
}))
</script>
```

---

#### `LocalizationService.autoInit()`
Sayfa yüklendiğinde otomatik olarak localization data'yı yükler (otomatik çağrılır).

---

#### `LocalizationService.get(key, defaultValue)`
Localization string'ini getirir.

**Örnek Kullanım:**

```javascript
var saveText = LocalizationService.get('Common.Save', 'Kaydet');
var cancelText = LocalizationService.get('Common.Cancel', 'İptal');
```

**Gerçek Proje Örneği:**

```javascript
// property.review.module.js içinde
var successMessage = LocalizationService.get("Review.Success", "Yorumunuz başarıyla gönderildi.");
UIService.notification.success(successMessage);
```

---

#### `LocalizationService.getWithParams(key, defaultValue, params)`
Localization string'ini parametrelerle getirir.

**Örnek Kullanım:**

```javascript
// Template: "Merhaba {name}, hoş geldiniz!"
var message = LocalizationService.getWithParams(
    'Welcome.Message',
    'Merhaba {name}, hoş geldiniz!',
    { name: 'Ahmet' }
);
// Sonuç: 'Merhaba Ahmet, hoş geldiniz!'

// Array parametreler
var message = LocalizationService.getWithParams(
    'Welcome.Message',
    'Merhaba {0}, hoş geldiniz!',
    ['Ahmet']
);
// Sonuç: 'Merhaba Ahmet, hoş geldiniz!'
```

---

#### `LocalizationService.getAll()`
Tüm localization cache'ini getirir.

**Örnek Kullanım:**

```javascript
var allLocalizations = LocalizationService.getAll();
console.log(allLocalizations);
```

---

#### `LocalizationService.clear()`
Localization cache'ini temizler.

**Örnek Kullanım:**

```javascript
LocalizationService.clear();
```

---

## AppInit

**Dosya:** `app.init.js`  
**Amaç:** Uygulamanın başlatılmasını ve tüm sayfalarda ortak ayarları yönetmek

### Metodlar

#### `AppInit.init(options)`
Uygulamayı başlatır (otomatik çağrılır).

**Parametreler:**
- `options.debug` (boolean, opsiyonel): Debug modu
- `options.baseUrl` (string, opsiyonel): Temel URL
- `options.dateFormat` (string, opsiyonel): Tarih formatı
- `options.timeFormat` (string, opsiyonel): Saat formatı
- `options.defaultPageSize` (number, opsiyonel): Varsayılan sayfa boyutu
- `options.autoInitTooltips` (boolean, opsiyonel): Tooltip'leri otomatik başlat
- `options.autoInitPopovers` (boolean, opsiyonel): Popover'ları otomatik başlat
- `options.ajaxTimeoutMs` (number, opsiyonel): AJAX timeout (ms)

**Örnek Kullanım:**

```javascript
// Otomatik çağrılır, manuel çağrıya gerek yok
// Ama özelleştirmek isterseniz:
AppInit.init({
    debug: true,
    baseUrl: '/',
    ajaxTimeoutMs: 60000
});
```

---

#### `AppInit.configure(options)`
Uygulama ayarlarını yapılandırır.

**Örnek Kullanım:**

```javascript
AppInit.configure({
    debug: true,
    defaultPageSize: 20
});
```

---

#### `AppInit.getSettings()`
Uygulama ayarlarını döndürür.

**Örnek Kullanım:**

```javascript
var settings = AppInit.getSettings();
console.log(settings.debug); // false
console.log(settings.baseUrl); // '/'
```

---

## ModuleFactory

**Dosya:** `module.factory.js`  
**Amaç:** CRUD modülleri oluşturmayı kolaylaştıran fabrika sınıfı

### Metodlar

#### `ModuleFactory.createBaseModule(options)`
Temel modül oluşturur.

**Parametreler:**
- `options.name` (string, zorunlu): Modül adı
- `options.debug` (boolean, opsiyonel): Debug modu
- `options.init` (function, opsiyonel): Init fonksiyonu
- `options.initEventListeners` (function, opsiyonel): Event listener'ları başlat
- `options.initComponents` (function, opsiyonel): Bileşenleri başlat
- `options.events` (object, opsiyonel): Event tanımları

**Örnek Kullanım:**

```javascript
var MyModule = ModuleFactory.createBaseModule({
    name: 'MyModule',
    debug: true,
    
    init: function(config) {
        this.log('Modül başlatıldı', config);
        // Özel init işlemleri
    },
    
    initEventListeners: function() {
        var self = this;
        document.addEventListener('click', function(e) {
            if (e.target.matches('.my-button')) {
                self.handleClick(e);
            }
        });
    },
    
    initComponents: function() {
        // Bileşenleri başlat
    },
    
    // Özel metodlar
    handleClick: function(e) {
        this.log('Butona tıklandı');
        this.trigger('button:clicked', { target: e.target });
    }
});

// Modülü başlat
MyModule.init({ someConfig: 'value' });

// Event dinle
MyModule.on('button:clicked', function(data) {
    console.log('Buton tıklandı:', data);
});
```

---

## GTM / dataLayer (TrimangoGtm)

**Dosya:** `gtm-data-layer.service.js`  
**İlgili:** `gtm-site-events.module.js` (site geneli click delegasyonu)

Google Tag Manager ile gönderilen özel olaylar `window.TrimangoGtm` üzerinden `dataLayer.push` edilir. Sunucu tarafında ayrı bir API key gerekmez; GTM container snippet yeterlidir. Ayrıntı: proje kökünde `docs/gtm-data-layer.md`.

### Konsol debug — gönderilen her olayı `console.log` ile izleme

Varsayılan **kapalıdır**. Açtıktan sonra `dataLayer`’a giden her push şu formatta görünür:

`[TrimangoGtm] dataLayer.push { event: "...", ... }`

#### `gtm.debug` — adres çubuğunu değiştirmeden

`gtm-data-layer.service.js` yüklendikten sonra `window.gtm` hazırdır. Konsolda:

```javascript
gtm.debug = true;   // konsol log açılır (yenileme gerekmez)
gtm.debug = false;  // kapat
```

#### 1) URL parametresi `?gtmDebug=1`

Mevcut sayfada adres çubuğuna ekle, **Enter** ile yenile:

```text
https://ornek.com/tr
→ https://ornek.com/tr?gtmDebug=1
```

Zaten `?` varsa:

```text
https://ornek.com/tr/villalar?bolge=antalya
→ https://ornek.com/tr/villalar?bolge=antalya&gtmDebug=1
```

`gtmDebug=true` de kabul edilir.

#### 2) `localStorage` (tarayıcıda kalıcı; tüm sekmeler aynı origin’de paylaşabilir)

Geliştirici araçları → **Console**:

```javascript
// Aç
localStorage.setItem('trimangoGtmDebug', '1');
location.reload();

// Kapat
localStorage.removeItem('trimangoGtmDebug');
location.reload();
```

#### 3) `TrimangoGtm.setConsoleDebug` — sayfa açıkken anında

Sadece o sekme / yenilemeden önce:

```javascript
// Konsolda: debug aç (sayfa yenilenene kadar veya flag kalır)
TrimangoGtm.setConsoleDebug(true);

// Aynı zamanda localStorage’a yaz → yeniledikten sonra da açık kalsın
TrimangoGtm.setConsoleDebug(true, true);

// Kapat (localStorage’ı da temizlemek için ikinci argüman)
TrimangoGtm.setConsoleDebug(false, true);
```

#### 4) `TrimangoGtm.isConsoleDebug()` — şu an log açık mı?

```javascript
TrimangoGtm.isConsoleDebug();
// true veya false
```

#### 5) Tek seferlik global bayrak (yenilemeden kaybolur)

```javascript
window.__TRIMANGO_GTM_DEBUG = true;
// Sonra sayfada bir olay tetikle (tıklama, arama vb.)
```

### Örnek oturum (kopyala-yapıştır)

```javascript
// 1. Debug aç
TrimangoGtm.setConsoleDebug(true);

// 2. Durumu kontrol et
console.log('GTM debug:', TrimangoGtm.isConsoleDebug());

// 3. Manuel test (gerçek bir event adı ile)
TrimangoGtm.pushEvent({
  event: TrimangoGtm.Events.CONTACT_CLICK,
  type: TrimangoGtm.ContactTypes.WHATSAPP
});
// Konsolda: [TrimangoGtm] dataLayer.push { event: "contact_click", type: "whatsapp" }

// 4. Kapat
TrimangoGtm.setConsoleDebug(false);
```

---

### ModuleFactory — gerçek proje örneği

```javascript
// property.details.module.js içinde
var PropertyDetailsModule = ModuleFactory.createBaseModule({
    name: 'PropertyDetailsModule',
    debug: false,
    
    init: function(config) {
        this._config = config || {};
        this.initStarRatings();
        this.initTabs();
        this.initFavoriteAction();
        // ...
    },
    
    initEventListeners: function() {
        var self = this;
        // Event listener'lar
    },
    
    initComponents: function() {
        // Bileşenleri başlat
    },
    
    // Özel metodlar
    initStarRatings: function() {
        // ...
    }
});

// Sayfa yüklendiğinde
$(document).ready(function() {
    var config = JSON.parse(document.getElementById('property-details-config').textContent);
    PropertyDetailsModule.init(config);
});
```

---

### Modül Metodları

#### `module.init(...args)`
Modülü başlatır.

#### `module.on(eventName, callback)`
Olay ekler.

#### `module.trigger(eventName, data)`
Olayı tetikler.

#### `module.log(message, data, isError)`
Log fonksiyonu.

**Örnek Kullanım:**

```javascript
MyModule.log('İşlem başarılı');
MyModule.log('Hata oluştu', error, true);
```

---

## Genel Kullanım Örnekleri

### Tam Örnek: Form Gönderimi

```javascript
// 1. Form validasyonu
if (!UIService.form.validate('#my-form')) {
    UIService.notification.warning('Lütfen formu doldurun');
    return;
}

// 2. Loader göster
UIService.loader.show('#form-container', 'Kaydediliyor...');

// 3. Form gönder
ApiService.submitForm({
    form: '#my-form',
    url: '/api/submit',
    success: function(response) {
        UIService.loader.hide('#form-container');
        
        if (response.success) {
            var message = LocalizationService.get('Common.SaveSuccess', 'Kayıt başarılı');
            UIService.notification.success(message, function() {
                window.location.reload();
            });
        } else {
            // Validation hatalarını göster
            displayValidationErrors(response.errors);
        }
    },
    error: function(error) {
        UIService.loader.hide('#form-container');
        var errorMessage = LocalizationService.get('Common.SaveError', 'Kayıt sırasında hata oluştu');
        UIService.notification.error(errorMessage);
    }
});
```

---

### Tam Örnek: AJAX ile Veri Yükleme

```javascript
// 1. Loader göster
UIService.loader.show('#properties-list', 'Yükleniyor...');

// 2. Query parametrelerini hazırla
var params = HelperService.getQueryParams();
params.page = 1;
params.pageSize = 20;

// 3. URL oluştur
var url = HelperService.buildUrl('/api/properties', params);

// 4. AJAX isteği
ApiService.get({
    url: url,
    useFetch: true,
    success: function(response) {
        UIService.loader.hide('#properties-list');
        
        if (response && response.data) {
            renderProperties(response.data);
        } else {
            UIService.notification.warning('Veri bulunamadı');
        }
    },
    error: function(error) {
        UIService.loader.hide('#properties-list');
        UIService.notification.error('Veri yüklenirken hata oluştu');
    }
});
```

---

### Tam Örnek: Modal ile Onay Dialog'u

```javascript
// Silme işlemi
function deleteProperty(id) {
    var title = LocalizationService.get('Common.ConfirmDelete', 'Silme Onayı');
    var message = LocalizationService.get('Common.ConfirmDeleteMessage', 'Bu kaydı silmek istediğinizden emin misiniz?');
    
    UIService.modal.confirm(
        title,
        message,
        function() {
            // Onaylandı
            UIService.loader.show('global', 'Siliniyor...');
            
            ApiService.delete({
                url: '/api/properties/' + id,
                success: function(response) {
                    UIService.loader.hide('global');
                    
                    if (response.success) {
                        var successMessage = LocalizationService.get('Common.DeleteSuccess', 'Silindi');
                        UIService.notification.success(successMessage);
                        
                        // DOM'dan kaldır
                        document.getElementById('property-' + id).remove();
                    }
                },
                error: function(error) {
                    UIService.loader.hide('global');
                    UIService.notification.error('Silme işlemi başarısız');
                }
            });
        },
        function() {
            // İptal edildi
            console.log('İptal edildi');
        }
    );
}
```

---

## Best Practices

1. **Her zaman core servisleri kullanın:** Direkt jQuery AJAX veya `fetch` kullanmak yerine `ApiService` kullanın.

2. **Localization kullanın:** Tüm kullanıcıya gösterilen metinler için `LocalizationService` kullanın.

3. **Error handling:** Tüm AJAX isteklerinde `error` callback'i tanımlayın ve `UIService.notification.error` kullanın.

4. **Loader gösterimi:** Uzun süren işlemlerde `UIService.loader.show` kullanın.

5. **ModuleFactory kullanın:** Yeni modüller oluştururken `ModuleFactory.createBaseModule` kullanın.

6. **HelperService'i kullanın:** Ortak işlemler için `HelperService` metodlarını kullanın (slug, format, vb.).

---

## Notlar

- Tüm core servisler global namespace'e eklenir (`window.ApiService`, `window.UIService`, vb.).
- Modüller içinde `window.` prefix'i kullanmadan direkt servis adlarını kullanabilirsiniz (`ApiService.get` gibi).
- `UIService.notification` varsayılan olarak Tailwind CSS tasarımını kullanır. SweetAlert için `options.forceSwal: true` geçin.
- `ApiService` varsayılan olarak jQuery AJAX kullanır. Fetch API için `useFetch: true` geçin.

