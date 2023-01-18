using System;
using System.Reflection;
using VenlySDK.Core;

namespace VenlySDK
{
    [AttributeUsage(AttributeTargets.Method)]
    public class VyExtensionRouteAttribute : Attribute
    {
        public readonly string ExtensionName;

        public VyExtensionRouteAttribute(string extensionName)
        {
            ExtensionName = extensionName;
        }
    }

    public class VyDefaultExtensionHandler : VyExtensionHandler
    {
        public override VyTask<string> Invoke(VyExtensionRequestData requestData)
        {
            return VyTask<string>.Failed($"No implementation found for Extension with name \'{requestData.ExtensionName}\'");
        }
    }

    public abstract class VyExtensionHandler
    {
        public Type EntryType { get; internal set; }

        protected VyExtensionHandler(Type entryType = null)
        {
            EntryType = entryType;
        }

        internal VyTask<string> InvokeInternal(VyExtensionRequestData requestData)
        {
            //Search for Implementation
            if (EntryType != null)
            {
                var methods = EntryType.GetMethods(BindingFlags.Static);
                foreach (var method in methods)
                {
                    var att = method.GetCustomAttribute<VyExtensionRouteAttribute>();
                    if (att != null)
                    {
                        if (att.ExtensionName.Equals(requestData.ExtensionName))
                        {
                            return method.Invoke(null, new object[] {requestData}) as VyTask<string>;
                        }
                    }
                }
            }

            return Invoke(requestData);
        }

        public abstract VyTask<string> Invoke(VyExtensionRequestData requestData);

        #region Interface

        #endregion
    }
}
