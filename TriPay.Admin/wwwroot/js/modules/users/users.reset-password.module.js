/**
 * Şifre sıfırlama formu.
 */
var UsersResetPasswordModule = ModuleFactory.createBaseModule({
    name: 'UsersResetPasswordModule',

    init: function () {
        this._setupResetForm();
    },

    initEventListeners: function () {
        this._setupResetForm();
    },

    _setupResetForm: function () {
        AdminPageBase.bindValidationSummaryClear();
        AdminPageBase.focusFirstField('form input[name="Password"]');
    }
});
