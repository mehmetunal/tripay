/**
 * İşlem listesi — filtre formu HTML AJAX.
 */
var TransactionsIndexModule = ModuleFactory.createBaseModule({
    name: 'TransactionsIndexModule',

    init: function () {
        this._setupFilterForm();
    },

    initEventListeners: function () {
        this._setupFilterForm();
    },

    _setupFilterForm: function () {
        AdminPageBase.bindAutoSubmitHtmlForm({
            formSelector: 'form[data-admin-ajax="html"]',
            triggerSelector: 'select'
        });
    }
});
