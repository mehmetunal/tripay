/**
 * Sayfa modüllerini AdminPageRouter'a kaydeder.
 */
(function () {
    'use strict';

    if (typeof AdminPageRouter === 'undefined') {
        return;
    }

    var modules = [
        ['account.login', typeof AccountLoginModule !== 'undefined' ? AccountLoginModule : null],
        ['transactions.index', typeof TransactionsIndexModule !== 'undefined' ? TransactionsIndexModule : null],
        ['reports.index', typeof ReportsIndexModule !== 'undefined' ? ReportsIndexModule : null],
        ['merchants.index', typeof MerchantsIndexModule !== 'undefined' ? MerchantsIndexModule : null],
        ['merchants.edit', typeof MerchantsEditModule !== 'undefined' ? MerchantsEditModule : null],
        ['gateways.index', typeof GatewaysIndexModule !== 'undefined' ? GatewaysIndexModule : null],
        ['gateways.settings', typeof GatewaysSettingsModule !== 'undefined' ? GatewaysSettingsModule : null],
        ['gateways.errors', typeof GatewaysErrorsModule !== 'undefined' ? GatewaysErrorsModule : null],
        ['gateways.settingForm', typeof GatewaysSettingFormModule !== 'undefined' ? GatewaysSettingFormModule : null],
        ['gateways.errorForm', typeof GatewaysErrorFormModule !== 'undefined' ? GatewaysErrorFormModule : null],
        ['outbox.index', typeof OutboxIndexModule !== 'undefined' ? OutboxIndexModule : null],
        ['outbox.details', typeof OutboxDetailsModule !== 'undefined' ? OutboxDetailsModule : null],
        ['users.index', typeof UsersIndexModule !== 'undefined' ? UsersIndexModule : null],
        ['users.create', typeof UsersCreateModule !== 'undefined' ? UsersCreateModule : null],
        ['users.resetPassword', typeof UsersResetPasswordModule !== 'undefined' ? UsersResetPasswordModule : null],
        ['roles.index', typeof RolesIndexModule !== 'undefined' ? RolesIndexModule : null],
        ['roles.edit', typeof RolesEditModule !== 'undefined' ? RolesEditModule : null],
        ['system.index', typeof SystemIndexModule !== 'undefined' ? SystemIndexModule : null]
    ];

    modules.forEach(function (pair) {
        if (pair[1]) {
            AdminPageRouter.register(pair[0], pair[1]);
        }
    });
})();
