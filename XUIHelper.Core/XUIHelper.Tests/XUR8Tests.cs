using Serilog.Events;
using Serilog;
using XUIHelper.Core;

namespace XUIHelper.Tests
{
    public class XUR8Tests : XURTests
    {
        //TODO: XUR8 write support
        //TODO: XUR8 unit tests
        //TODO: Decouple XUI version from extensions, make it more of a group IDed by a string, as we should be able to write a XUR8 as a XUR5
        //TODO: Support for ignore properties (ones that aren't supported in XuiTool)
        //TODO: Function library API
        //TODO: Console app
        //TODO: GUI app

        protected override void RegisterExtensions(ILogger? logger = null)
        {
            XMLExtensionsManager v8Extensions = new XMLExtensionsManager(logger);
            _ = v8Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V8\XuiElements.xml");
            _ = v8Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V8\17559DashElements.xml");
            XUIHelperCoreConstants.VersionedExtensions[0x8] = v8Extensions;
        }

        protected override IXUR GetXUR(string filePath, ILogger? logger = null)
        {
            return new XUR8(filePath, logger);
        }

        [Test]
        public void DebugTest()
        {
            Assert.True(true);
        }
    }
}