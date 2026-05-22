/**
 * Sistem durumu ekranı.
 */
var SystemIndexModule = ModuleFactory.createBaseModule({
    name: 'SystemIndexModule',

    init: function () {
        this._setupSystemPage();
    },

    initEventListeners: function () {
        this._setupSystemPage();
    },

    _setupSystemPage: function () {
        var root = AdminPageBase.getContentRoot();
        if (!root) {
            return;
        }

        root.querySelectorAll('dl div').forEach(function (block) {
            var dt = block.querySelector('dt');
            var dd = block.querySelector('dd');
            if (!dt || !dd) {
                return;
            }
            var label = (dt.textContent || '').toLowerCase();
            if (label.indexOf('redis') >= 0 || label.indexOf('veritabanı') >= 0 || label.indexOf('database') >= 0) {
                var ok = (dd.textContent || '').toLowerCase().indexOf('evet') >= 0 ||
                    (dd.textContent || '').toLowerCase().indexOf('bağlı') >= 0 ||
                    (dd.textContent || '').toLowerCase().indexOf('ok') >= 0;
                dd.classList.toggle('text-green-700', ok);
                dd.classList.toggle('text-red-700', !ok);
            }
        });
    }
});
