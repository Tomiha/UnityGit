using VenlySDK.Models;

namespace VenlySDK.Core
{
    public class VyExtensionRequestData
    {
        public readonly string ExtensionName;

        private VyRequestData _baseRequest;
        public VyRequestData BaseRequest => _baseRequest;

        private VyExtensionRequestData(string extensionName)
        {
            ExtensionName = extensionName;
            _baseRequest = VyRequestData.Get(extensionName, eVyApiEndpoint.Extension);
        }

        public static VyExtensionRequestData Create(string extensionName)
        {
            return new VyExtensionRequestData(extensionName);
        }

        public VyExtensionRequestData AddBinaryContent(byte[] content)
        {
            _baseRequest.AddBinaryContent(content);
            return this;
        }

        public VyExtensionRequestData AddJsonContent<T>(T content)
        {
            _baseRequest.AddJsonContent(content);
            return this;
        }

        public VyExtensionRequestData AddFormContent<T>(T content)
        {
            _baseRequest.AddFormContent(content);
            return this;
        }
    }
}
