/**
 * Gateway hata eşlemesi oluştur/düzenle formu.
 */
var GatewaysErrorFormModule = ModuleFactory.createBaseModule({
    name: 'GatewaysErrorFormModule',

    init: function () {
        this._setupErrorForm();
    },

    initEventListeners: function () {
        this._setupErrorForm();
    },

    _setupErrorForm: function () {
        AdminPageBase.bindValidationSummaryClear();
        AdminPageBase.focusFirstField('form input[name="ProviderErrorCode"]');

        var root = AdminPageBase.getContentRoot();
        if (!root) {
            return;
        }

        var localeSelect = root.querySelector('select[name="Locale"]');
        if (localeSelect && localeSelect.getAttribute('data-locale-bound') !== '1') {
            localeSelect.setAttribute('data-locale-bound', '1');
            localeSelect.addEventListener('change', function () {
                localeSelect.classList.toggle('font-semibold', localeSelect.value === 'tr');
            });
            localeSelect.dispatchEvent(new Event('change'));
        }
    }
});
