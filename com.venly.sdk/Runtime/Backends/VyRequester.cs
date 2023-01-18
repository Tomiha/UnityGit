using System.Collections.Generic;
using VenlySDK.Core;

namespace VenlySDK.Backends
{
    public abstract class VyRequester
    {
        public abstract VyTask<T> MakeRequest<T>(VyRequestData requestData);

        public VyTask<T> MakeRequest<T>(VyExtensionRequestData requestData)
        {
            return MakeRequest<T>(requestData.BaseRequest);
        }

        #region Requester Data

        protected Dictionary<string, object> _requesterData = new();

        protected virtual object GetData(string key)
        {
            if (_requesterData.ContainsKey(key)) return _requesterData[key];
            return null;
        }

        protected bool HasData(string key)
        {
            return _requesterData.ContainsKey(key);
        }

        public virtual bool RemoveData(string key)
        {
            if (_requesterData.ContainsKey(key))
            {
                _requesterData.Remove(key);
                return true;
            }

            return false;
        }

        public virtual void SetData(string key, object data)
        {
            _requesterData.Add(key, data);
        }

        #endregion
    }
}