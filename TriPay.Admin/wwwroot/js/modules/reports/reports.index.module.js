/**
 * Raporlar — tarih ve filtre formu (HTML AJAX).
 */
var ReportsIndexModule = ModuleFactory.createBaseModule({
    name: 'ReportsIndexModule',

    init: function () {
        this._setupReportsPage();
    },

    initEventListeners: function () {
        this._setupReportsPage();
    },

    _setupReportsPage: function () {
        AdminPageBase.bindAutoSubmitHtmlForm({
            formSelector: 'form[data-admin-ajax="html"]',
            triggerSelector: 'select'
        });
    }
});
