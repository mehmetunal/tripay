/**
 * Üye işyeri düzenleme formu.
 */
var MerchantsEditModule = ModuleFactory.createBaseModule({
    name: 'MerchantsEditModule',

    init: function () {
        this._setupEditForm();
    },

    initEventListeners: function () {
        this._setupEditForm();
    },

    _setupEditForm: function () {
        AdminPageBase.bindValidationSummaryClear();
        AdminPageBase.focusFirstField('form input[name="Name"]');
    }
});
