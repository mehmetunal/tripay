/**
 * Admin giriş formu (AJAX JSON — kabuk form gönderimini bağlar).
 */
var AccountLoginModule = ModuleFactory.createBaseModule({
    name: 'AccountLoginModule',

    init: function () {
        this._setupLoginPage();
    },

    initEventListeners: function () {
        this._setupLoginPage();
    },

    _setupLoginPage: function () {
        AdminPageBase.bindValidationSummaryClear();
        AdminPageBase.focusFirstField('form input[name="Email"], form input[type="email"]');
    }
});
