

// https://github.com/XRPLF/xrpl.js/blob/main/packages/xrpl/test/integration/requests/noRippleCheck.ts

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xrpl.Models.Common;
using Xrpl.Models.Ledger;
using Xrpl.Models.Methods;

namespace XrplTests.Xrpl.ClientLib.Integration
{
    [TestClass]
    public class TestINoRippleCheck
    {
        // private static int Timeout = 20;
        public TestContext TestContext { get; set; }
        public static SetupIntegration runner;

        [ClassInitialize]
        public static async Task MyClassInitializeAsync(TestContext testContext)
        {
            runner = await new SetupIntegration().SetupClient(ServerUrl.serverUrl);
        }

        [TestMethod]
        public async Task TestRequestMethod()
        {
            LedgerIndex index = new LedgerIndex(LedgerIndexType.Validated);
            NoRippleCheckRequest request = new NoRippleCheckRequest(runner.wallet.ClassicAddress);
            NoRippleCheck response = await runner.client.NoRippleCheck(request);
            Assert.IsNotNull(response);
        }
    }
}