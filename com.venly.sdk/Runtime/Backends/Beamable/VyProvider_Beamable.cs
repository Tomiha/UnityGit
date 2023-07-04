using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Beamable;
using Beamable.Common;
using Newtonsoft.Json;
using UnityEngine;
using Venly.Core;
using Venly.Models.Shared;
using Venly.Utils;

//#if UNITY_EDITOR
//    using UnityEditor;
//#endif

namespace Venly.Backends.Beamable
{
    public class VyProvider_Beamable : VyProviderBase
    {
        public const string BeamContextDataKey = "Beamable-Context";
        private BeamContext _currContext = null;
        private object _targetClient = null;
        private MethodInfo _entryFunction = null;

        private bool HasContext => _currContext != null;
        private bool HasTarget => _targetClient != null || _entryFunction != null;

        public VyProvider_Beamable() : base(eVyBackendProvider.Beamable.GetMemberName())
        {
            ExtensionsSupported = true;
        }

//#if UNITY_EDITOR
//        [InitializeOnLoadMethod]
//#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void RegisterProvider()
        {
            VenlyAPI.RegisterProvider(new VyProvider_Beamable());
        }

        protected override void OnSetData(string key, object data)
        {
            if (key.Equals(BeamContextDataKey))
            {
                LoadServiceParameters(data as BeamContext);
            }
        }

        private void LoadServiceParameters(BeamContext ctx = null)
        {
            if (ctx == null)
            {
                ctx = BeamContext.Default;

                //todo. fix blocking code
                async void waitReady()
                {
                    await ctx.OnReady;
                }

                new Task(waitReady).RunSynchronously();
            }

            if (_currContext == ctx) return;

            _currContext = ctx;

            if (!_currContext.IsInitialized)
            {
                VenlyLog.Warning("VyProvider_Beamable::LoadServiceParameters >> BeamContext not yet initialized. (abort)");
                _currContext = null;
                return;
            }

            try
            {
                Assembly ass = Assembly.Load("Unity.Beamable.Customer.MicroserviceClients");
                var serviceName = $"{VenlySettings.BeamableBackendSettings.MicroserviceName}Client";
                var clientType = ass.ExportedTypes.FirstOrDefault(t => t.Name.Equals(serviceName));
                _targetClient = ctx.Microservices().GetType().GetMethod("GetClient").MakeGenericMethod(new[] {clientType}).Invoke(_currContext.Microservices(), null);
                _entryFunction = _targetClient.GetType().GetMethod(VenlySettings.BeamableBackendSettings.EntryFunctionName);
            }
            catch (Exception e)
            {
                VenlyLog.Exception(e);
                VenlyLog.Warning($"VyProvider_Beamable::LoadServiceParameters >> Failed to retrieve Microservice/EntryFunction (\'{VenlySettings.BeamableBackendSettings.EntryFunctionName}\') from Microservice Client (\'{VenlySettings.BeamableBackendSettings.MicroserviceName}Client\')");
                _currContext = null;
            }

            string errMsg;
            if(!VerifyEntryFunctionSignature(_entryFunction, out errMsg))
            {
                _targetClient = null;
                _entryFunction = null;

                VenlyLog.Warning($"VyProvider_Beamable::LoadServiceParameters >> The Entry Function does not match the required function signature (err={errMsg})");
            }
        }

        private bool VerifyEntryFunctionSignature(MethodInfo function, out string errMsg)
        {
            errMsg = null;
            if (function.ReturnType != typeof(Promise<string>))
            {
                errMsg = "Entry Function should have \'Promise<string>\' or \'Task<string>\' as return type";
                return false;
            }

            var functionParams = function.GetParameters();
            if (functionParams.Length == 0)
            {
                errMsg = "Entry Function should accept a parameter of type \'string\'";
                return false;
            }
            
            if (functionParams.Length > 1)
            {
                errMsg = $"Entry Function should only have a single parameter (\'string\'). (current length = \'{functionParams.Length}\')";
                return false;
            }

            if (functionParams[0].ParameterType != typeof(string))
            {
                errMsg = $"Entry function's parameters should be of type \'string\' (current type = \'{functionParams[0].ParameterType.Name}\')";
                return false;
            }

            return true;
        }

        public override VyTask<T> MakeRequest<T>(VyRequestData requestData)
        {
            if (!HasContext)
            {
                LoadServiceParameters();

                if(!HasContext)
                    return VyTask<T>.Failed(new Exception("[Beamable Provider] BeamContext not set."));
            }

            if (!HasTarget)
            {
                return VyTask<T>.Failed(new Exception("[Beamable Provider] Microservice Client and/or Entry Function not found"));
            }

            var taskNotifier = VyTask<T>.Create("beamable_provider_makeRequest");
            var resultPromise = (Promise<string>)_entryFunction.Invoke(_targetClient, new[] {JsonConvert.SerializeObject(requestData)});

            //Wait for result
            resultPromise
                .Then(result =>
                {
                    try
                    {
                        var serverResponse = JsonConvert.DeserializeObject<VyServerResponseDto>(result);
                        VyTaskResult<T> taskResult;

                        if (serverResponse is { Success: true })
                        {
                            if (serverResponse.IsRawResponse) //Requires Response Parsing
                            {
                                //Small trick to safely unescape the JSON encoded JSON string...
                                //object decodedJsonString = JsonConvert.DeserializeObject(serverResponse.Data);
                                taskResult = VenlyUtils.ProcessApiResponse<T>(serverResponse.Data, (HttpStatusCode)serverResponse.StatusCode, requestData);
                                taskNotifier.Notify(taskResult);
                                return;
                            }

                            //Response contains the requested object, no parsing required
                            T data = JsonConvert.DeserializeObject<T>(serverResponse.Data);
                            taskResult = new VyTaskResult<T>(data);
                            taskNotifier.Notify(taskResult);
                            return;
                        }

                        taskResult = new VyTaskResult<T>(new VyException(serverResponse.ErrorMessage));
                        taskNotifier.Notify(taskResult);
                    }
                    catch (Exception ex)
                    {
                        taskNotifier.NotifyFail(new VyException(ex));
                    }
                })
                .Error(taskNotifier.NotifyFail);

            return taskNotifier.Task;
        }
    }
}
