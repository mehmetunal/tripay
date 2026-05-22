/**
 * Gateway ayar listesi.
 */
var GatewaysSettingsModule = ModuleFactory.createBaseModule({
    name: 'GatewaysSettingsModule',

    init: function () {
        this._setupSettingsPage();
    },

    initEventListeners: function () {
        this._setupSettingsPage();
    },

    _setupSettingsPage: function () {
        var root = AdminPageBase.getContentRoot();
        if (!root) {
            return;
        }

        root.querySelectorAll('tbody tr').forEach(function (row) {
            var cells = row.querySelectorAll('td');
            if (cells.length >= 4 && (cells[3].textContent || '').trim() === 'Hayır') {
                row.classList.add('bg-slate-50');
            }
        });
    }
});
