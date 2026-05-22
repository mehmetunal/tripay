/**
 * Outbox listesi — filtre sekmeleri AJAX link ile (kabuk).
 */
var OutboxIndexModule = ModuleFactory.createBaseModule({
    name: 'OutboxIndexModule',

    init: function () {
        this._setupListPage();
    },

    initEventListeners: function () {
        this._setupListPage();
    },

    _setupListPage: function () {
        var root = AdminPageBase.getContentRoot();
        if (!root) {
            return;
        }

        root.querySelectorAll('tbody tr').forEach(function (row) {
            var statusCell = row.querySelector('td:nth-child(4)');
            if (statusCell && statusCell.textContent.indexOf('Hayır') >= 0) {
                row.classList.add('bg-amber-50/50');
            }
        });
    }
});
