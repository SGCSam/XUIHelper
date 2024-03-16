using Serilog.Events;
using Serilog;
using XUIHelper.Core;

namespace XUIHelper.Tests
{
    public class XUR8Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        public async Task<bool> CheckReadSuccessful(string filePath, ILogger? logger = null)
        {
            XUR8 xur = new XUR8(filePath, logger);
            bool successful = await xur.TryReadAsync();
            return successful;
        }

        public void RegisterExtensions(ILogger? logger = null)
        {
            XMLExtensionsManager v8Extensions = new XMLExtensionsManager(logger);
            _ = v8Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V8\XuiElements.xml");
            _ = v8Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V8\17559DashElements.xml");
            XUIHelperCoreConstants.VersionedExtensions[0x8] = v8Extensions;
        }
    }
}