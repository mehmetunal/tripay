/**
 * Rol listesi — Admin rolü satırını işaretle.
 */
var RolesIndexModule = ModuleFactory.createBaseModule({
    name: 'RolesIndexModule',

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
            if (row.textContent.indexOf('Yönetici') >= 0 || row.textContent.indexOf('Admin') >= 0) {
                row.classList.add('bg-tripay-50/30');
            }
        });
    }
});
