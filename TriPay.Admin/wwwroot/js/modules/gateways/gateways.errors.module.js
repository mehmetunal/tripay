/**
 * Gateway hata eşlemesi listesi.
 */
var GatewaysErrorsModule = ModuleFactory.createBaseModule({
    name: 'GatewaysErrorsModule',

    init: function () {
        this._setupErrorsPage();
    },

    initEventListeners: function () {
        this._setupErrorsPage();
    },

    _setupErrorsPage: function () {
        var root = AdminPageBase.getContentRoot();
        if (!root) {
            return;
        }

        root.querySelectorAll('tbody tr').forEach(function (row) {
            var cells = row.querySelectorAll('td');
            if (cells.length >= 5 && (cells[4].textContent || '').trim() === 'Hayır') {
                row.classList.add('bg-slate-50');
            }
        });
    }
});
