/**
 * Gateway ayar oluştur/düzenle formu.
 */
var GatewaysSettingFormModule = ModuleFactory.createBaseModule({
    name: 'GatewaysSettingFormModule',

    init: function () {
        this._setupSettingForm();
    },

    initEventListeners: function () {
        this._setupSettingForm();
    },

    _setupSettingForm: function () {
        AdminPageBase.bindValidationSummaryClear();
        AdminPageBase.bindSettingKeyPreview();
        AdminPageBase.focusFirstField('form input[name="SettingKey"]');
    }
});
