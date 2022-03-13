using ezviz.net.domain;
using ezviz.net.exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net
{
    internal class SessionIdDelegatingHandler : DelegatingHandler
    {
        internal const string PROPERTY_ANON = "Anonymous";
        private readonly SessionIdProvider sessionProvider;
        public SessionIdDelegatingHandler(SessionIdProvider sessionProvider)
        {
            this.sessionProvider = sessionProvider;
            InnerHandler = new HttpClientHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!request.Options.Any(kvp => kvp.Key == PROPERTY_ANON))
            {
                var token = await sessionProvider.GetSessionId();
                request.Headers.Add("sessionId", token);
            }
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }

    internal class SessionIdProvider
    {
        internal LoginSession? Session { private get; set; }
        
        public async Task<string> GetSessionId()
        {
            if (Session == null)
            {
                throw new EzvizNetException("Not logged in to Ezviz API");
            }
            if (Session.SessionExpiry <= DateTime.UtcNow - TimeSpan.FromMinutes(10))
            {
                await Login();
            }
            return Session.SessionId;
        }

        public Func<Task> Login { private get; set; } = null!;
    }
}
