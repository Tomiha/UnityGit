using System;
using Beamable.Server.Api.RealmConfig;
using Venly.Backends;
using Venly;
using Venly.Core;
using Venly.Models.Shared;

public static class VenlyBeamable
{
    private const string VENLY_NAMESPACE = "venly";
    private const string VENLY_CLIENT_ID = "client_id";
    private const string VENLY_CLIENT_SECRET = "client_secret";
    private const string VENLY_ENV = "environment";

    public static VyTask Initialize(IMicroserviceRealmConfigService realmConfigService)
    {
        return Initialize(realmConfigService, null);
    }

    public static VyTask Initialize(string clientId, string clientSecret, eVyEnvironment environment, bool force = false)
    {
        if (!force && VenlyAPI.IsInitialized)
            return VyTask.Succeeded();

        var taskNotifier = VyTask.Create();

        taskNotifier.Scope(async () =>
        {
            var serverProvider = new DefaultServerProvider(clientId, clientSecret);

            //Initialize Venly API
            var initResult = await VenlyAPI.Initialize(serverProvider, environment);
            if (!initResult.Success)
            {
                taskNotifier.NotifyFail(initResult.Exception);
            }

        });

        return taskNotifier.Task;
    }

    internal static VyTask Initialize(IMicroserviceRealmConfigService realmConfigService, Func<VyRequestData, object, VyTask<VyServerResponseDto>> handleExtensionsTarget, bool force = false)
    {
        if (!force && VenlyAPI.IsInitialized)
        {
            VenlyAPI.Provider.GetHandler<EventRequestHandler>().UpdateTarget(handleExtensionsTarget);
            return VyTask.Succeeded();
        }

        var taskNotifier = VyTask.Create();

        taskNotifier.Scope(async () =>
        {
            var realmConfig = await realmConfigService.GetRealmConfigSettings();
            var venlyConfig = realmConfig.GetNamespace(VENLY_NAMESPACE);
            if (!venlyConfig.TryGetValue(VENLY_CLIENT_ID, out string clientId))
            {
                taskNotifier.NotifyFail(new Exception($"Realm Config does not contain a value for \'{VENLY_CLIENT_ID}\' (namespace={VENLY_NAMESPACE})"));
                return;
            }

            if (!venlyConfig.TryGetValue(VENLY_CLIENT_SECRET, out string clientSecret))
            {
                taskNotifier.NotifyFail(new Exception($"Realm Config does not contain a value for '{VENLY_CLIENT_SECRET}' (namespace={VENLY_NAMESPACE})"));
                return;
            }

            if (!venlyConfig.TryGetValue(VENLY_ENV, out string environment))
            {
                taskNotifier.NotifyFail(new Exception($"Realm Config does not contain a value for '{VENLY_ENV}' (namespace={VENLY_NAMESPACE})"));
                return;
            }

            var envType = environment.Equals("staging", StringComparison.OrdinalIgnoreCase)
                ? eVyEnvironment.staging
                : eVyEnvironment.production;

            var serverProvider = new DefaultServerProvider(clientId, clientSecret, handleExtensionsTarget);

            //Initialize Venly API
            var initResult = await VenlyAPI.Initialize(serverProvider, envType);
            if (!initResult.Success)
            {
                taskNotifier.NotifyFail(initResult.Exception);
            }

        });

        return taskNotifier.Task;
    }
}
