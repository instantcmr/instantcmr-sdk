using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Icmr.Integration.v3.Api;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Icmr.Integration.MimeTypes;

namespace Icmr.Integration.v3
{
    namespace Api
    {
        public class Dubev
        {
            public Dubm[] rgdubm;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Kdubm { dosu, trip, user, room }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Ktroc { o, mfc, c }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Kredosu { accept, reject }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Kdocr
        {
            cmr,
            dlvryn,
            palletn,
            custd,
            acc,
            misc,
            wbt,
            miscph,
            thesc,
            sanid,
            gdam,
            wayb,
            wmad,
            dad,
            bol,
            rep,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Kstan {
            ld,
            ul,
            h,
            ds,
            owp,
            cuc,
            c,
            tun,
            rc,
            @ref,
            p,
            fer,
            wei,
            bc,
            res,
            acs,
            dcs,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Krole { driver, disp, rev, dia, admin, iep, chedit, chadmin }

        public class Dub<T>
        {
            public T obefore;
            public T oafter;

            public override string ToString() => oafter?.Let(after => $"* -> {after}") ?? obefore?.Let(before => $"{before} -> *");
        }

        public class Tripmeta
        {
            public string ostTripCode;
            public string ostOrderId;
            public string ostAddress;
            public string ostCustomer;
            public DateTime? odtuCreate;
            public string ostInstructions;
            public string ostNotes;
            public string ostHaulerPlate;
            public string ostTrailerPlate;
        }

        public class Docr
        {
            public string docrid;
            public DateTime dtuRequest;
            public Kdocr kdocr;
            public string ostShortName;
            public string ostDesc;
            public bool fRequired;
            public bool? ofDeleted;
        }

        public class Stan
        {
            public string stanxtid;
            public Kstan kstan;
            public DateTime? odtu;
            public string ostAddress;
            public string ostNotes;
            public double? okmDistance;
            public Cufis cufis;
            public string ostSortBy;
            public bool? ofDeleted;
        }

        public class Dubdocrref
        {
            public string docrid;
            public string tripxtid;
        }

        public class Dosumeta
        {
            public string ostOrderid;
            public string ostPlate;
            public string ostNotes;
        }

        public class Dubredoim
        {
            public string userxtid;
            public DateTime dtu;
            public Kredosu kredosu;
        }

        public class Urlv
        {
            public string url;
            public DateTime dtuValid;
        }

        public class Dubdoed
        {
            public string doedid;
            public DateTime dtu;
            public string userxtid;
        }

        public class Dubdosuimg
        {
            public string imgid;
            public Urlv urlv;
            public Dubredoim oredoim;
            public Dubdoed odoed;
        }

        public class Dubdosu
        {
            public string dosuxtid;
            public string userxtid;
            public string copid;
            public string ouid;
            public Kdocr kdocr;
            public Dubdocrref odocr;
            public DateTime dtuSubmit;
            public DateTime dtuUpload;
            public string loc;
            public Dosumeta dosumeta;
            public Dubdosuimg[] rgimg;

            public override string ToString() =>
                $"dosu {kdocr} {copid}:{ouid}:{dosuxtid} submitted-on {dtuSubmit.ToLocalTime():yyyy-MM-dd HH:mm:ss.fff} by {userxtid}{Maybe.OfNullable(odocr).Map(docr => $" for docr {docr.tripxtid}:{docr.docrid}").OrElse("")} with {rgimg.Length} images";
        }

        public class Dubrut
        {
            public string rutid;
            public string fpat;
            public long cb;
            public DateTime dtuUpload;
            public Urlv urlv;

            public override string ToString() =>
                $"rut {rutid} path '{fpat}' uploaded-on {dtuUpload.ToLocalTime():yyyy-MM-dd HH:mm:ss.fff} size {cb}B";
        }

        public class Dubruts
        {
            public Dubrut[] rgrut;

            public override string ToString() => $"ruts with {rgrut.Length} files";
        }

        public class Dubtrip
        {
            public string tripxtid;
            public string etag;
            public string copid;
            public string ouid;
            public Ktroc ktroc;
            public string ostSortBy;
            public DateTime dtuCreate;
            public DateTime? odtuMfc;
            public Tripmeta tripmeta;
            public string ouserxtidDriver;
            public Docr[] rgdocr;
            public Stan[] rgstan;
            public Dubdosu[] rgdosu;
            public Dubruts ruts;
            public Cufis cufis;

            public override string ToString() =>
                $"trip {copid}:{ouid}:{tripxtid}:{etag} ktroc {ktroc} driver {ouserxtidDriver} cdocr {rgdocr.Length} cstan {rgstan.Length} cdosu {rgdosu.Length} {ruts} {cufis}";
        }

        public class Dubulic
        {
            public string kid;
            public string ostDeviceModel;
            public string ostDeviceImei;
            public string ostPin;
            public string ostPhone;
            public string ostImsi;
            public string ostSubscription;

            public override string ToString() =>
                $"lic {kid}";
        }

        public class Dubusermeta
        {
            public string ostEmployeeId;
            public string ostVoicePhone;
            public string ostHaulerPlate;
            public string ostTrailerPlate;
        }

        public class Dubdbox
        {
            public string[] rguserxtidFollow;
            public string oshrn;

            public override string ToString() =>
                $"following {rguserxtidFollow.StJoin()} share-name {oshrn}";
        }

        public class Dubuser
        {
            public string userxtid;
            public string copid;
            public string ouid;
            public string usern;
            public Dubulic[] rgulic;
            public Dubusermeta usermeta;
            public Dubdbox dbox;
            public Krole[] rgkrole;
            public string[] rgtripxtid;

            public override string ToString() =>
                $"user {copid}:{ouid}:{userxtid} name {usern} devices {rgulic.StJoin()} roles {rgkrole.StJoin()} dbox {dbox} trips {rgtripxtid.StJoin()}";
        }

        public class Dubboma
        {
            public string userxtid;
            public string usern;
            public Dubusermeta usermeta;
            public string colBg;
            public Wetag<DateTime> owetagdtuSync;
            public Wetag<DateTime> owetagdtuRead;
            public bool fMuted;
        }

        public class Dubbomath
        {
            public string userxtid;
            public string usern;
            public Dubusermeta usermeta;
            public string colBg;
        }

        public class Dubroup
        {
            public Roce oroce;
            public Tise otise;
            public Usad ousad;
            public Uski ouski;
            public Usmu ousmu;
            public Usum ousum;

            public class Roce { }
            public class Tise { public string ostTitle; }
            public class Usad { public string[] rguserxtid; }
            public class Uski{ public string[] rguserxtid; }
            public class Usmu { public string[] rguserxtid; }
            public class Usum { public string[] rguserxtid; }
        }

        public interface Dubpayp
        {
            public class Update : Dubpayp { public Dubroup roup; }
            public class Msg : Dubpayp { public string st; }
        }

        public class Dubpost
        {
            public string postxtid;
            public string etagpost;
            public string userxtid;
            public DateTime dtu;
            public Dubpayp payp;
        }

        public class Dubroom
        {
            public string roomxtid;
            public string ouxtid;
            public Rovered rovered;
            public DateTime dtuCreate;
            public string ostTitle;
            public Dubboma[] rgboma;
            public Dubbomath[] rgbomath;
            public Dubpost[] rgpost;
        }

        public class Dubm
        {
            public string dubmid;
            public string rhnd;
            public Kdubm kdubm;
            public Dub<Dubdosu> odubdosu;
            public Dub<Dubtrip> odubtrip;
            public Dub<Dubuser> odubuser;
            public Dub<Dubroom> odubroom;

            public override string ToString() =>
                $"{kdubm} {dubmid.StTruncate()} {odubdosu ?? odubtrip ?? odubuser ?? odubroom as object}";
        }

        public class Ruted
        {
            public string rutid;
            public string fpat;
            public long cb;
            public DateTime dtuUpload;
            public Urlv urlv;
        }

        public class Rutsed
        {
            public Ruted[] rgrut;
        }

        public class Cufi
        {
            public string id;
            public string n;
            public string v;
        }

        public class Cufis
        {
            public Cufi[] rgcufi;
        }

        public abstract class Tripe
        {
            public string ouid;
            public Ktroc ktroc;
            public string ostSortBy;
            public Tripmeta tripmeta;
            public string ouserxtidDriver;
            public Docr[] rgdocr;
            public Stan[] rgstan;
            public Cufis cufis;
        }

        public class Tripeu : Tripe {}

        public class Triped: Tripe
        {
            public DateTime dtuCreate;
            public DateTime? odtuMfc;
            public Rutsed ruts;
        }

        public class Wetag<T>
        {
            public string etag;
            public T v;

            public override string ToString() => $"{etag}:{v}";
        }

        public class Compn
        {
            public string packagen;
            public string classn;
        }

        public interface Ev
        {
            public class Byte: Ev               {public byte v;}
            public class Short: Ev              {public short v;}
            public class Int: Ev                {public int v;}
            public class Long: Ev               {public long v;}
            public class Float: Ev              {public float v;}
            public class Double: Ev             {public double v;}
            public class Boolean: Ev            {public bool v;}
            public class String: Ev             {public string v;}
            public class Char: Ev               {public char v;}
            public class ByteArray: Ev          {public byte[] v;}
            public class ShortArray: Ev         {public short[] v;}
            public class IntArray: Ev           {public int[] v;}
            public class LongArray: Ev          {public long[] v;}
            public class FloatArray: Ev         {public float[] v;}
            public class DoubleArray: Ev        {public double[] v;}
            public class BooleanArray: Ev       {public bool[] v;}
            public class StringArray: Ev        {public string[] v;}
            public class CharArray: Ev          {public char[] v;}
            public class IntegerArrayList : Ev  {public int[] v;}
            public class StringArrayList : Ev   {public string[] v;}
            public class Json: Ev               {public object v;}
        }

        public class Inspe
        {
            public string oaction;
            public string[] orgcat;
            public Compn ocompn;
            public string odata;
            public Dictionary<string, Ev> ompextra;
        }

        public interface Buta
        {
            public class Sync : Buta {}
            public class Triplist : Buta {}
            public class Roomlist : Buta {}
            public class Dbox : Buta {}

            public class Docr : Buta
            {
                public Kdocr kdocr;
            }

            public class LaunchActivity : Buta
            {
                public Inspe inspe;
            }
        }

        public class Ptd
        {
            public float x;
            public float y;
        }

        public class Szd
        {
            public float dx;
            public float dy;
        }

        public interface Krund
        {
            public class Fixed : Krund
            {
                public float dp;
            }

            public class Relative : Krund
            {
                public int percent;
            }

            public class Unit : Krund
            {
                public float u;
            }
        }

        public class St18
        {
            public string stDefault;
            public Dictionary<string, string> mpstByLocale;
        }

        public class Busty
        {
            public St18 ost18Text;
            public string okico;
            public string ocolBg;
            public string ocolFg;
            public Krund okrund;
        }

        public class Notd
        {
            public string notid;
            public St18 ost18Message;
            public int? ocBadge;
        }

        public class Mbut
        {
            public Buta buta;
            public Ptd ptd;
            public Szd szd;
            public Busty busty;
            public Notd[] rgnotd;
        }

        public class Screne
        {
            public Mbut[] rgmbut;
        }

        public class Bomaeu
        {
            public string userxtid;
            public bool fMuted;
        }

        public class Roomeu
        {
            public string ouxtid;
            public string ostTitle;
            public Bomaeu[] rgboma;
        }

        public class Usermeta
        {
            public string ostEmployeeId;
            public string ostVoicePhone;
            public string ostHaulerPlate;
            public string ostTrailerPlate;

        }

        public class Bomaed
        {
            public string userxtid;
            public string usern;
            public Usermeta usermeta;
            public string colBg;
            public Wetag<DateTime> owetagdtuSync;
            public Wetag<DateTime> owetagdtuRead;
            public bool fMuted;
        }

        public class Bomath
        {
            public string etagpostLast;
            public string userxtid;
            public string usern;
            public Usermeta usermeta;
            public string colBg;
            public Wetag<DateTime> owetagdtuRead;
        }

        public class Roomed
        {
            public string copid;
            public string roomxtid;
            public string ouxtid;
            public Rovered rovered;
            public DateTime dtuCreate;
            public string ostTitle;
            public Bomaed[] rgboma;

            public override string ToString() => $"roomed {copid}:{ouxtid}:{roomxtid} etag {rovered.wetagdturoom.etag} created-on {dtuCreate} updated-on {rovered.wetagdturoom.v} with {rovered.wetagdtupost.etag} posts last-post-on {rovered.wetagdtupost.v} title {ostTitle?.Let(stTitle => $"'{stTitle}'") ?? "<none>"} with {rgboma.Length} members";
        }

        public class Posteu
        {
            public string userxtid;
            public string stMessage;
        }

        public class Roup
        {
            public Roce oroce;
            public Tise otise;
            public Usad ousad;
            public Uski ouski;
            public Usmu ousmu;
            public Usum ousum;

            public class Roce { }
            public class Tise { public string ostTitle; }
            public class Usad { public string[] rguserxtid; }
            public class Uski { public string[] rguserxtid; }
            public class Usmu { public string[] rguserxtid; }
            public class Usum { public string[] rguserxtid; }
        }

        public interface Payp {
            public class Update : Payp { public Roup roup; }
            public class Msg : Payp { public string st; }
        }

        public class Posted
        {
            public string roomxtid;
            public string etag;
            public string userxtid;
            public string postxtid;
            public DateTime dtu;
            public Payp payp;
        }

        public class Rovered
        {
            public Wetag<DateTime> wetagdturoom;
            public Wetag<DateTime> wetagdtupost;

            public override string ToString() => $"room {wetagdturoom} post {wetagdtupost}";
        }

        public class Qroom
        {
            public string oetagAfter;
            public string oetagLast;
        }

        public interface Dboxited
        {
            public string filid { get; set; }
            public string etag { get; set; }
            public string pat { get; set; }
            public DateTime dtuUpload { get; set; }

            public Dboxited WithPat(string pat);

            public class F : Dboxited
            {
                public string filid { get; set; }
                public string etag { get; set; }
                public string pat { get; set; }
                public DateTime dtuUpload { get; set; }
                public long cb;
                public Urlv urlv;

                public Dboxited WithPat(string pat) => new F
                {
                    filid = filid,
                    etag = etag,
                    pat = pat,
                    dtuUpload = dtuUpload,
                    cb = cb,
                    urlv = urlv,
                };
            }

            public class D : Dboxited
            {
                public string filid { get; set; }
                public string etag { get; set; }
                public string pat { get; set; }
                public DateTime dtuUpload { get; set; }

                public Dboxited WithPat(string pat) => new D
                {
                    filid = filid,
                    etag = etag,
                    pat = pat,
                    dtuUpload = dtuUpload,
                };
            }
        }

        public class Dboxsyced
        {
            public string filid;
            public string etag;
            public DateTime dtuDelivered;
        }

        public class Dboxsubed
        {
            public string userxtid;
        }

        public class Dboxed
        {
            public string userxtid;
            public Dboxited[] rgit;
            public Dboxsyced[] rgsyc;
            public Dboxsubed[] rgsub;
        }

        public class Filta
        {
            public string userxtid;
            public string pat;
        }
    }

    static class WetagU
    {
        public static async Task<Wetag<T>> ExtractWetag<T>(this HttpResponseMessage response) =>
            new Wetag<T>
            {
                etag = response.Headers.GetValues("ETag").First(),// TODO: parse strong/weak etag, remove quotes.
                v = await response.Extract<T>()
            };
    }

    public class IntegrationClient
    {
        private readonly HttpClient httpc;
        private readonly string copid;
        private readonly L l;

        public IntegrationClient(
            Uri uriEndpoint,
            string copid,
            string kid,
            string shs,
            Lf lf = null
        )
        {
            lf = lf ?? new Lf(new Lwcon());
            this.l = lf.L(this);
            this.copid = copid;

            this.httpc = new HttpClient(
                new HttpClientHandler()
                .AndThen(new Logi(lf))
                .AndThen(new Reqsi(kid, shs, lf))
            )
            {
                BaseAddress = uriEndpoint,
                Timeout = TimeSpan.FromSeconds(60),
            };

        }


        public async Task<Dubev> ReceiveDubAsync(string iepn, Maybe<string> orecid, CancellationToken ctok) =>
            await (
                await httpc.GetAsync(
                    $"./dub/{copid.Uri()}/{iepn.Uri()}/receive{orecid.Map(recid => $"?recid={recid.Uri()}").OrElse("")}",
                    ctok
                )
                .Timeout_Pkludge(ctok)
                .EnsureValid()
            )
            .Extract<Dubev>();

        public async Task DeleteDubmAsync(string iepn, Dubm dubm, CancellationToken ctok) =>
            await httpc.DeleteAsync($"./dub/{copid.Uri()}/{iepn.Uri()}/rhnd/{dubm.rhnd.Uri()}").Timeout_Pkludge(ctok).EnsureValid();

        public async Task<Blob> DownloadImageAsync(Dubdosuimg dosuimg, CancellationToken ctok) =>
            await (
                await httpc.GetAsync(dosuimg.urlv.url, ctok).Timeout_Pkludge(ctok).EnsureValid()
            )
            .Let(async (response) => new Blob(response.Content.Headers.ContentType.MediaType, await response.Content.ReadAsStreamAsync()));

        public async Task<Blob> DownloadRutAsync(Urlv urlv, CancellationToken ctok) =>
            await (
                await httpc.GetAsync(urlv.url, ctok).Timeout_Pkludge(ctok).EnsureValid()
            )
            .Let(async (response) => new Blob(response.Content.Headers.ContentType.MediaType, await response.Content.ReadAsStreamAsync()));

        public async Task<Blob> DownloadDboxfilAsync(Urlv urlv, CancellationToken ctok) =>
            await (
                await httpc.GetAsync(urlv.url, ctok).Timeout_Pkludge(ctok).EnsureValid()
            )
            .Let(async (response) => new Blob(response.Content.Headers.ContentType.MediaType, await response.Content.ReadAsStreamAsync()));

        public async Task<Wetag<Triped>> GetTripAsync(string tripxtid, bool fDeleted, CancellationToken ctok) =>
            await (
                await httpc.GetAsync($"./trip/{copid.Uri()}/{tripxtid.Uri()}{(fDeleted ? "?deleted" : "")}", ctok).Timeout_Pkludge(ctok).EnsureValid()
            )
            .ExtractWetag<Triped>();

        public async Task<Wetag<Triped>> PutTripAsync(string tripxtid, Wetag<Tripeu> wetagtripeu, CancellationToken ctok) =>
            await (
                await httpc.SendAsync(
                    new HttpRequestMessage(HttpMethod.Put, $"./trip/{copid.Uri()}/{tripxtid.Uri()}")
                        .WithOetag(wetagtripeu.etag, l)
                        .WithJsonContent(wetagtripeu.v, l),
                    ctok
                )
                .Timeout_Pkludge(ctok)
                .EnsureValid()
            )
            .ExtractWetag<Triped>();

        public async Task<Wetag<Triped>> PutRutAsync(string tripxtid, string filn, Stream strmContent, CancellationToken ctok) =>
            await (
                await httpc.PutAsync(
                    $"./trip/{copid.Uri()}/{tripxtid.Uri()}/rut/{filn.UriFromPathSegments()}",
                    new StreamContent(strmContent).Also(content => content.Headers.ContentType = new MediaTypeHeaderValue(Mime.Lookup(filn))),
                    ctok
                )
                .Timeout_Pkludge(ctok)
                .EnsureValid()
            )
            .ExtractWetag<Triped>();

        public async Task<Wetag<Triped>> DeleteRutAsync(string tripxtid, string filn, CancellationToken ctok) =>
            await (
                await httpc.DeleteAsync($"./trip/{copid.Uri()}/{tripxtid.Uri()}/rut/{filn.UriFromPathSegments()}", ctok)
                .Timeout_Pkludge(ctok)
                .EnsureValid()
            )
            .ExtractWetag<Triped>();

        public async Task<Wetag<Screne>> GetScrenAsync(string userxtid, CancellationToken ctok) =>
            await (
                await httpc.GetAsync($"./user/{copid.Uri()}/{userxtid.Uri()}/scren", ctok)
                .Timeout_Pkludge(ctok)
                .EnsureValid()
            )
            .ExtractWetag<Screne>();

        public async Task<Wetag<Screne>> PutScrenAsync(string userxtid, Wetag<Screne> wetagscrene, CancellationToken ctok) =>
            await (
                await httpc.SendAsync(
                    new HttpRequestMessage(HttpMethod.Put, $"./user/{copid.Uri()}/{userxtid.Uri()}/scren")
                        .WithOetag(wetagscrene.etag, l)
                        .WithJsonContent(wetagscrene.v, l),
                    ctok
                )
                .Timeout_Pkludge(ctok)
                .EnsureValid()
            )
            .ExtractWetag<Screne>();

        public async Task DeleteScrenAsync(string userxtid, CancellationToken ctok, string oetag) =>
            await httpc.SendAsync(
                new HttpRequestMessage(HttpMethod.Delete, $"./user/{copid.Uri()}/{userxtid.Uri()}/scren").WithOetag(oetag, l),
                ctok
            )
            .Timeout_Pkludge(ctok)
            .EnsureValid();

        public async Task<Wetag<Roomed>> GetRoomAsync(string roomxtid, CancellationToken ctok) =>
            await (
                await httpc.GetAsync($"./room/{copid.Uri()}/{roomxtid.Uri()}", ctok)
                .Timeout_Pkludge(ctok)
                .EnsureValid()
            )
            .ExtractWetag<Roomed>();

        public async Task<Wetag<Roomed>> PutRoomAsync(string roomxtid, Wetag<Roomeu> wetagroomeu, CancellationToken ctok) =>
            await (
                await httpc.SendAsync(
                    new HttpRequestMessage(HttpMethod.Put, $"./room/{copid.Uri()}/{roomxtid.Uri()}")
                    .WithOetag(wetagroomeu.etag, l)
                    .WithJsonContent(wetagroomeu.v, l),
                    ctok
                )
                .Timeout_Pkludge(ctok)
                .EnsureValid()
            )
            .ExtractWetag<Roomed>();

        public async Task<Wetag<Posted>> PutPostAsync(string roomxtid, string postxtid, Wetag<Posteu> wetagposteu, CancellationToken ctok) =>
            await (
                await httpc.SendAsync(
                    new HttpRequestMessage(HttpMethod.Put, $"./room/{copid.Uri()}/{roomxtid.Uri()}/post/{postxtid.Uri()}")
                    .WithOetag(wetagposteu.etag, l)
                    .WithJsonContent(wetagposteu.v, l),
                    ctok
                )
                .Timeout_Pkludge(ctok)
                .EnsureValid()
            )
            .ExtractWetag<Posted>();

        public async Task<Wetag<Roomed>> PutSrAsync(string roomxtid, string userxtid, string etagpost, CancellationToken ctok) =>
            await (
                await httpc.SendAsync(
                    new HttpRequestMessage(HttpMethod.Put, $"./room/{copid.Uri()}/{roomxtid.Uri()}/user/{userxtid.Uri()}/sr/{etagpost.Uri()}"),
                    ctok
                )
                .Timeout_Pkludge(ctok)
                .EnsureValid()
            )
            .ExtractWetag<Roomed>();

        public async Task<Wetag<Roomed>> PutRrAsync(string roomxtid, string userxtid, string etagpost, CancellationToken ctok) =>
            await (
                await httpc.SendAsync(
                    new HttpRequestMessage(HttpMethod.Put, $"./room/{copid.Uri()}/{roomxtid.Uri()}/user/{userxtid.Uri()}/rr/{etagpost.Uri()}"),
                    ctok
                )
                .Timeout_Pkludge(ctok)
                .EnsureValid()
            )
            .ExtractWetag<Roomed>();

        public async Task<Dboxed> GetDboxAsync(string userxtid, CancellationToken ctok) =>
            await (
                await httpc.GetAsync($"./dbox/{copid.Uri()}/{userxtid.Uri()}/fil", ctok)
                .Timeout_Pkludge(ctok)
                .EnsureValid()
            )
            .Extract<Dboxed>();

        public async Task PutDboxFileAsync(string userxtid, string pat, Stream strmContent, CancellationToken ctok) =>
            await httpc.PutAsync(
                $"./dbox/{copid.Uri()}/{userxtid.Uri()}/fil/{pat.UriFromPathSegments()}",
                new StreamContent(strmContent).Also(content => content.Headers.ContentType = new MediaTypeHeaderValue(Mime.Lookup(pat))),
                ctok
            )
            .Timeout_Pkludge(ctok)
            .EnsureValid();

        public async Task PutDboxFolderAsync(string userxtid, string pat, CancellationToken ctok) =>
            await httpc.PutAsync(
                $"./dbox/{copid.Uri()}/{userxtid.Uri()}/fil/{pat.UriFromPathSegments()}/",
                null,
                ctok
            )
            .Timeout_Pkludge(ctok)
            .EnsureValid();

        public async Task MoveDboxAsync(string userxtid, string patSrc, string patDst, CancellationToken ctok) =>
            await httpc.SendAsync(
                new HttpRequestMessage(
                    HttpMethod.Put, 
                    $"./dbox/{copid.Uri()}/{userxtid.Uri()}/mv/{patSrc.UriFromPathSegments(true)}"
                )
                .WithJsonContent(new Filta {userxtid = userxtid, pat = patDst}, l)
            )
            .Timeout_Pkludge(ctok)
            .EnsureValid();

        public async Task DeleteDboxAsync(string userxtid, string pat, CancellationToken ctok) =>
            await httpc.DeleteAsync($"./dbox/{copid.Uri()}/{userxtid.Uri()}/fil/{pat.UriFromPathSegments(true)}", ctok)
            .Timeout_Pkludge(ctok)
            .EnsureValid();
    }
}