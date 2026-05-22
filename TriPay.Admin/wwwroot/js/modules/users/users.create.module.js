/**
 * Yeni kullanıcı formu.
 */
var UsersCreateModule = ModuleFactory.createBaseModule({
    name: 'UsersCreateModule',

    init: function () {
        this._setupCreateForm();
    },

    initEventListeners: function () {
        this._setupCreateForm();
    },

    _setupCreateForm: function () {
        AdminPageBase.bindValidationSummaryClear();
        AdminPageBase.focusFirstField('form input[name="Email"]');
    }
});
