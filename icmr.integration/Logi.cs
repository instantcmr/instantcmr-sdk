using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Icmr.Integration
{
    class Logi : DelegatingHandler
    {
        private readonly L l;

        public Logi(Lf lf)
        {
            l = lf.L(this);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ctok)
        {
            l.V($"---> {request.Method} {request.RequestUri.AbsoluteUri} {request.Headers.Concat(request.Content?.Headers ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>()).Select(kvHeader => $"{kvHeader.Key}:{kvHeader.Value.Select(st => $"'{st}'").StJoin(", ")}").StJoin(" ")}");
            var response = await base.SendAsync(request, ctok);
            l.V($"<--- [{(int)response.StatusCode} {response.ReasonPhrase}] {response.Headers.Concat(response.Content?.Headers ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>()).Select(kvHeader => $"{kvHeader.Key}:{kvHeader.Value.Select(st => $"'{st}'").StJoin(", ")}").StJoin(" ")}");
            return response;
        }
    }
}
