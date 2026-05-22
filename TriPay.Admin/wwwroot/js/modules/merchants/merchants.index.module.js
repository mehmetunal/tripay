/**
 * Üye işyeri listesi.
 */
var MerchantsIndexModule = ModuleFactory.createBaseModule({
    name: 'MerchantsIndexModule',

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

        root.querySelectorAll('tbody tr').forEach(function (row) {
            var cells = row.querySelectorAll('td');
            if (cells.length < 3) {
                return;
            }
            var activeText = (cells[4].textContent || '').trim();
            if (activeText === 'Hayır' || activeText === 'Pasif') {
                row.classList.add('opacity-70');
            }
        });
    }
});
