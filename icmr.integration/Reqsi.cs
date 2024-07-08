using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Icmr.Integration
{
    class Reqsi : DelegatingHandler
    {
        private const string dtuf = "yyyyMMdd.HHmmss.fff";
        private readonly L l;
        private readonly String kid;
        private readonly String shs;
        private TimeSpan durOffset = TimeSpan.Zero;

        public Reqsi(string kid, string shs, Lf lf)
        {
            this.l = lf.L(this);
            this.kid = kid;
            this.shs = shs;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ctok)
        {
            var nonce = NonceGenerate();
            var response = await base.SendAsync(Sign(request, DateTime.UtcNow, nonce), ctok);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && response.Headers.Contains(Atht.HTTP_HEADER_NAME))
            {
                var statht = response.Headers.GetValues(Atht.HTTP_HEADER_NAME).Single();
                var atht = Atht.Oparse(statht).OrElse(() => throw new ErReqsi($"Server responded with invalid auth header '{statht}'"));
                var athtComputed = new Athm(kid, atht.athm.ts, nonce, Maybe.Empty<string>()).Atht(Reqm.FromRequest(request), shs);

                if (atht.sig != athtComputed.sig)
                    throw new ErReqsi("Server says clock is skewed, but response seems to be forged");

                var dtuServer = DateTime.ParseExact(atht.athm.ts, dtuf, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime();
                AdjustDurOffset(dtuServer - DateTime.UtcNow);
                l.W($"Clock is sewed, server clock is {dtuServer} {dtuServer.Kind} (parsed from {atht.athm.ts}), adjust by {durOffset}");

                return await base.SendAsync(Sign(request, DateTime.UtcNow, NonceGenerate()), ctok);
            }

            return response;
        }

        private string NonceGenerate() => Guid.NewGuid().ToString("N");
        private void AdjustDurOffset(TimeSpan durOffset)
        {
            this.durOffset = durOffset;
        }

        private HttpRequestMessage Sign(HttpRequestMessage req, DateTime dtuTimestamp, String nonce)
        {
            var stTimestamp = (dtuTimestamp + durOffset).ToUniversalTime().ToString(dtuf);
            var sigdec = new Sigdec(
                new Athm(kid, stTimestamp, nonce, Maybe.Empty<string>()),
                Reqm.FromRequest(req));

            if (durOffset != TimeSpan.Zero)
                l.V($"Using previous time offset {durOffset}");
            l.V($"Authenticate with '{sigdec.St()}'");

            req.Headers.Remove(Atht.HTTP_HEADER_NAME);
            req.Headers.Add(Atht.HTTP_HEADER_NAME, sigdec.Atht(shs).St());

            return req;
        }
    }

    class Athm
    {
        public readonly string kid;
        public readonly string ts;
        public readonly string nonce;
        public readonly Maybe<string> ocosha;

        public Athm(string kid, string ts, string nonce, Maybe<string> ocosha)
        {
            this.kid = kid;
            this.ts = ts;
            this.nonce = nonce;
            this.ocosha = ocosha;
        }

        public IEnumerable<string> Rgp() => new string[] { kid, ts, nonce, ocosha.OrElse(Integration.Atht.ST_NO_TOKEN) };
        public Atht Atht(Reqm reqm, String shs) => new Sigdec(this, reqm).Atht(shs);
    }

    class Reqm
    {
        public string method;
        public string pat;
        public Maybe<long> ocbContent;
        public Maybe<string> otypContent;

        Reqm(string method, string pat, Maybe<long> ocbContent, Maybe<string> otypContent)
        {
            this.method = method;
            this.pat = pat;
            this.ocbContent = ocbContent;
            this.otypContent = otypContent;
        }

        public IEnumerable<string> Rgp() => new[] {
            method.ToUpperInvariant(),
            pat,
            ocbContent.Map(cb => cb.ToString()).OrElse(Atht.ST_NO_TOKEN),
            otypContent.OrElse(Atht.ST_NO_TOKEN)
        };

        public static Reqm FromRequest(HttpRequestMessage req) => new Reqm(
            req.Method.Method,
            req.RequestUri.PathAndQuery,
            Maybe.OfNullable(req.Content)
                .FlatMap(content => Maybe.OfNullable(content.Headers.ContentLength))
                .Or(() => {
                    switch(req.Method.Method)
                    {
                        case "POST":
                        case "PUT":
                        case "DELETE":
                        case "HEAD":
                        case "OPTIONS":
                        case "TRACE":
                            return Maybe.Of(0L);

                        case "GET":
                            return Maybe.Empty<long>();

                        default:
                            throw new ArgumentOutOfRangeException("requestMethod", req.Method, "requestMethod is unknown");
                    }
                }),
            Maybe.OfNullable(req.Content).Map(content => content.Headers.ContentType.ToString())
        );
    }

    class Atht
    {
        public static readonly string ST_NO_TOKEN = "-";
        public static readonly char ST_TOKEN_DELIMITER = ' ';
        public static readonly string HTTP_HEADER_NAME = "x-icmr-auth-1";
        public readonly Athm athm;
        public readonly string sig;

        public Atht(Athm athm, String sig)
        {
            this.athm = athm;
            this.sig = sig;
        }

        public String St() => Enumerable
            .Concat(athm.Rgp(), new[] { sig })
            .StJoin(ST_TOKEN_DELIMITER.ToString());

        public static Maybe<Atht> Oparse(String stAtht) => Maybe
            .Of(stAtht.Split(ST_TOKEN_DELIMITER))
            .Filter(rgp=>rgp.Length == 5)
            .Map(rgp=> new Atht(
                new Athm(
                    rgp[0],
                    rgp[1],
                    rgp[2],
                    "".Equals(rgp[3]) ? Maybe.Empty<string>() : Maybe.Of(rgp[3])
                ),
                rgp[4]
            ));
    }

    class Sigdec
    {
        public readonly Athm athm;
        public readonly Reqm reqm;

        public Sigdec(Athm athm, Reqm reqm)
        {
            this.athm = athm;
            this.reqm = reqm;
        }

        public String St() => Enumerable
            .Concat(athm.Rgp(), reqm.Rgp())
            .Select(st => st.Trim())
            .StJoin(Integration.Atht.ST_TOKEN_DELIMITER.ToString());

        public Atht Atht(String shs) => new Atht(athm, Sig(shs));
        public String Sig(String shs) => Hmac(shs, St());

        private String Hmac(String shs, String sigdec)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(shs)))
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(sigdec)), Base64FormattingOptions.None);
        }
    }

    class ErReqsi : Exception
    {
        public ErReqsi(String stMessage) : base(stMessage) {}
    }
}