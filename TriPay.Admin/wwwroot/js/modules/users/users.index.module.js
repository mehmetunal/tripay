/**
 * Kullanıcı listesi — kilitli satırları vurgula.
 */
var UsersIndexModule = ModuleFactory.createBaseModule({
    name: 'UsersIndexModule',

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
            if (row.textContent.indexOf('Kilitli') >= 0) {
                row.classList.add('bg-red-50/40');
            }
        });
    }
});
