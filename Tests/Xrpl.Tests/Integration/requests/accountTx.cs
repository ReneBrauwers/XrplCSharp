

// https://github.com/XRPLF/xrpl.js/blob/main/packages/xrpl/test/integration/requests/accountTx.ts

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xrpl.Models.Methods;

namespace XrplTests.Xrpl.ClientLib.Integration
{
    [TestClass]
    public class TestIAccountTx
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
            AccountTransactionsRequest request = new AccountTransactionsRequest(runner.wallet.ClassicAddress);
            AccountTransactions accountInfo = await runner.client.AccountTransactions(request);
            Assert.IsNotNull(accountInfo);
        }

        [TestMethod]
        public async Task TestRequestMethodSpecific()
        {
            AccountTransactionsRequest request = new AccountTransactionsRequest(runner.wallet.ClassicAddress)
            {
                Forward = false,
                LedgerIndexMax = 120
            };
            AccountTransactions accountInfo = await runner.client.AccountTransactions(request);
            Assert.IsNotNull(accountInfo);
        }

 
    }
}