/**
 * Ödeme kanalları ana liste.
 */
var GatewaysIndexModule = ModuleFactory.createBaseModule({
    name: 'GatewaysIndexModule',

    init: function () {
        this._setupIndexPage();
    },

    initEventListeners: function () {
        this._setupIndexPage();
    },

    _setupIndexPage: function () {
        var root = AdminPageBase.getContentRoot();
        if (!root) {
            return;
        }

        root.querySelectorAll('[data-admin-module="gateways.index"] ~ .grid > div, .grid.gap-4 > div').forEach(function (card) {
            var status = card.querySelector('p.text-xs');
            if (status && status.textContent.indexOf('Pasif') >= 0) {
                card.classList.add('border-amber-200');
            }
        });
    }
});
