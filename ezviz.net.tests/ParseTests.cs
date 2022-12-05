using ezviz.net.domain;
using ezviz.net.domain.deviceInfo;
using ezviz.net.util;
using NSubstitute;
using NUnit.Framework;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace ezviz.net.tests
{
    public class ParseTests
    {
        [Test]
        //[Ignore("Manual Test")]
        public void TestThatSampleJsonCanBeParsed()
        {
            //Copy sample JSON from the API into these files before testing
            var deviceInfo = JsonSerializer.Deserialize<EzvizDeviceInfo>(Assembly.GetExecutingAssembly().GetManifestResourceStream("ezviz.net.tests.responses.deviceInfo.json"));
            var pagedResponse = JsonSerializer.Deserialize<PagedListResponse>(Assembly.GetExecutingAssembly().GetManifestResourceStream("ezviz.net.tests.responses.pagedResponse.json"));

            var mockApi = Substitute.For<IEzvizApi>();
            var mockLogger = Substitute.For<IRequestResponseLogger>();
            var sessionIdProvider = new SessionIdProvider()
            {
                Session = new LoginSession()
                {
                    SessionId = "MOCKSESSIONID",
                    SessionExpiry = System.DateTime.MaxValue
                },

                Login = () =>
                {
                    return Task.CompletedTask;
                }
            };

            var client = new EzvizClient(mockApi, mockLogger, sessionIdProvider);            
            var camera = new Camera(deviceInfo,pagedResponse,client);
        }
    }
}