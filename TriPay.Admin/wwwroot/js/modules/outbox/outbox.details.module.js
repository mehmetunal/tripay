/**
 * Outbox detay — payload kopyalama.
 */
var OutboxDetailsModule = ModuleFactory.createBaseModule({
    name: 'OutboxDetailsModule',

    init: function () {
        this._setupDetailsPage();
    },

    initEventListeners: function () {
        this._setupDetailsPage();
    },

    _setupDetailsPage: function () {
        AdminPageBase.bindPayloadCopyOnClick();
    }
});
