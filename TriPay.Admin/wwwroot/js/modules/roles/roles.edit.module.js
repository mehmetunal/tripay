/**
 * Rol yetki düzenleme formu.
 */
var RolesEditModule = ModuleFactory.createBaseModule({
    name: 'RolesEditModule',

    init: function () {
        this._setupEditForm();
    },

    initEventListeners: function () {
        this._setupEditForm();
    },

    _setupEditForm: function () {
        AdminPageBase.bindValidationSummaryClear();
        AdminPageBase.bindRolePanelAccessGuard();
    }
});
