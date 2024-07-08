using JsonSubTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Api = Icmr.Integration.v3.Api;

namespace Icmr.Integration
{
    public enum Kmr { L, R, B }
    public class MR<L, R, K> : IKeyed<K>, IEquatable<MR<L, R, K>> where L : class, IKeyed<K> where R : class, IKeyed<K>
    {
        public MR(K k, L ol, R or)
        {
            this.k = k;
            this.ol = ol;
            this.or = or;
        }

        public Kmr kmr { get => ol == null ? Kmr.R : or == null ? Kmr.L : Kmr.B; }
        public readonly K k;
        public readonly L ol;
        public readonly R or;

        public K Key => k;

        public override string ToString()
        {
            switch (kmr)
            {
                case Kmr.L: return $"L: {ol}";
                case Kmr.R: return $"R: {or}";
                case Kmr.B: return $"B: {ol} -> {or}";
                default: throw new InvalidOperationException();
            }
        }

        #region equality
        public override bool Equals(object obj)
        {
            return Equals(obj as MR<L, R, K>);
        }

        public bool Equals(MR<L, R, K> other)
        {
            return other != null &&
                   kmr == other.kmr &&
                   EqualityComparer<L>.Default.Equals(ol, other.ol) &&
                   EqualityComparer<R>.Default.Equals(or, other.or);
        }

        public override int GetHashCode()
        {
            int hashCode = 919040399;
            hashCode = hashCode * -1521134295 + kmr.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<L>.Default.GetHashCode(ol);
            hashCode = hashCode * -1521134295 + EqualityComparer<R>.Default.GetHashCode(or);
            return hashCode;
        }

        public static bool operator ==(MR<L, R, K> left, MR<L, R, K> right)
        {
            return EqualityComparer<MR<L, R, K>>.Default.Equals(left, right);
        }

        public static bool operator !=(MR<L, R, K> left, MR<L, R, K> right)
        {
            return !(left == right);
        }
        #endregion
    }

    public interface IKeyed<out K>
    {
        K Key { get; }
    }

    public static class MRU
    {
        public static IEnumerable<MR<L, R, K>> MergeQQQ<L, R, K>(this IEnumerable<L> rgl, IEnumerable<R> rgr) where L : class, IKeyed<K> where R : class, IKeyed<K>
        {
            var mpl = rgl.ToDictionary(l => l.Key);
            var mpr = rgr.ToDictionary(r => r.Key);
            return mpl.Keys.Union(mpr.Keys).Select(k => {
                mpl.TryGetValue(k, out L ol);
                mpr.TryGetValue(k, out R or);
                return new MR<L, R, K>(k, ol, or);
            });
        }
    }

    public static class EnU
    {
        public static IEnumerable<T> Cons<T>(this T first) => new T[] { first };

        public static T Foldl<T, E>(this IEnumerable<E> ene, T tInitial, Func<T, E, int, T> dg) =>
            ene.Select((e, ie) => Tuple.Create(e, ie)).Aggregate(tInitial, (tAccum, eieT) => dg(tAccum, eieT.Item1, eieT.Item2));

        public static IDictionary<K, V> Merge<K, V>(this IDictionary<K, V> mp1, IDictionary<K, V> mp2) =>
            mp1.Concat(mp2).ToDictionary(kv => kv.Key, kv => kv.Value);

        public static bool FIn<T>(this T t, params T[] rgt) => rgt.Contains(t);

        public static string StJoin<T>(this IEnumerable<T> en, string stDelim = ", ") => string.Join(stDelim, en);

        public static Array ToArrayWithRtyElement<T>(this IEnumerable<T> en, Type rtyElement)
        {
            var rg = en.ToArray();
            var rgT = Array.CreateInstance(rtyElement, rg.Length);
            Array.Copy(rg, rgT, rg.Length);
            return rgT;
        }

        public static Dictionary<K, V> ToDictionary<K, V>(this IEnumerable<KeyValuePair<K, V>> enkv) =>
            enkv.ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    public static class Mpu
    {
        public static V Oget<K, V>(this IDictionary<K, V> mp, K k) => mp.TryGetValue(k, out V v) ? v : default;
    }

    public static class StU
    {
        public static string StJoin(this IEnumerable<string> rgst, string stSep) => string.Join(stSep, rgst);
        public static string StTruncate(this string st, int cchMax = 30, string stCont = "...") =>
            st.Length < cchMax ? st : $"{st.Substring(0, cchMax - (int)Math.Ceiling(stCont.Length / 2.0))}{stCont}{st.Substring(st.Length - cchMax + (int)Math.Floor(stCont.Length / 2.0))}";
        public static string Uri(this string st) => System.Uri.EscapeDataString(st);
        public static string UriFromPathSegments(this string fpat, bool fPreserveTrailingSlash = false) => 
            fpat
            .Split('/', '\\')
            .Where(pe => !string.IsNullOrWhiteSpace(pe))
            .Select(pe => pe.Uri())
            .StJoin("/")
            + (fPreserveTrailingSlash && fpat != "/" && fpat.EndsWith("/") ? "/" : "");
        public static IEnumerable<string> EnstSplitByUppercase(this string st)
        {
            var sb = new StringBuilder();
            foreach (var ch in st) {
                if (char.IsUpper(ch) && sb.Length > 0)
                {
                    yield return sb.ToString();
                    sb.Clear();
                }
                sb.Append(ch);
            }

            if (sb.Length > 0)
                yield return sb.ToString();
        }
        public static string StKebabCase(this string st) => st.EnstSplitByUppercase().Select(st => st.ToLowerInvariant()).StJoin("-");
    }

    public static class DirU
    {
        public static void DeleteDirectoryIfEmpty(string dpat)
        {
            if (Directory.GetFiles(dpat).Length == 0)
                Directory.Delete(dpat);
        }

        public static void DeleteFileAndRemoveDirectoryIfEmptyOrLog(string fpat, L l)
        {
            DeleteFileOrLog(fpat, l);
            DeleteDirectoryIfEmpty(Path.GetDirectoryName(fpat));
        }

        public static void DeleteFileOrLog(string fpat, L l)
        {
            if (File.Exists(fpat))
                File.Delete(fpat);
            else
                l.W($"could not delete {fpat}, file not exists. have we missed an update?");
        }
    }

    public static class HttpU
    {
        /*
         * PKLUDGE: 
         *  HttpClient throws TaskCancelledException (instead of any meaningful exception, such as TimeoutException or WebException)
         *  when there is a connection timeout on the request making it super hard to distinguish between a connection timeout 
         *  (i.e. network error) and an explicit user cancellation (i.e. calling CancellationToken.Cancel)
         *  
         *  Since apparently MS does not give a fuck about this (see MSDN forum link below) and frankly do not even seem to actually 
         *  understand this, we need this heuristic below.
         *  
         *  How wonderful!
         *  
         *  See also:
         *  - https://social.msdn.microsoft.com/Forums/en-US/d8d87789-0ac9-4294-84a0-91c9fa27e353/bug-in-httpclientgetasync-should-throw-webexception-not-taskcanceledexception?forum=netfxnetcom
         *  - https://stackoverflow.com/questions/29179848/httpclient-a-task-was-cancelled
         */
        public static async Task<T> Timeout_Pkludge<T>(this Task<T> task, CancellationToken ctok)
        {
            try { return await task; }
            catch (TaskCanceledException erCancelled) when (erCancelled.CancellationToken != ctok && !ctok.IsCancellationRequested)
            {
                throw new ErConnectionTimeout(erCancelled);
            }
        }

        public static HttpRequestMessage WithOetag(this HttpRequestMessage request, string oetag, L l)
        {
            try
            {
                return request.Also(httpreqT => {
                    if (oetag == null)
                        httpreqT.Headers.IfNoneMatch.Add(EntityTagHeaderValue.Any);
                    else
                        httpreqT.Headers.IfMatch.Add(new EntityTagHeaderValue($"{oetag}"));
                });
            }
            catch (Exception er) { l.E(er.ToString()); throw; }
        }

        public static HttpRequestMessage WithJsonContent<T>(this HttpRequestMessage request, T content, L l)
        {
            try
            {
                return request.Also(httpreqT => httpreqT.Content = content.JsonHttpContent());
            }
            catch (Exception er) { l.E(er.ToString()); throw; }
        }

        public static async Task<HttpResponseMessage> EnsureValid(this Task<HttpResponseMessage> task)
        {
            var response = await task;
            if (!response.IsSuccessStatusCode) throw new ErHttp(response, ostBody: await response.Content.ReadAsStringAsync());
            return response;
        }

        public async static Task<T> Extract<T>(this HttpResponseMessage response)
        {
            using (var strr = new StreamReader(await response.Content.ReadAsStreamAsync()))
            using (var jsonr = new JsonTextReader(strr))
                return JsonSerializer.Create(jsons).Deserialize<T>(jsonr);
        }

        private static JsonSerializerSettings jsons = new JsonSerializerSettings() { 
            MissingMemberHandling = MissingMemberHandling.Ignore, 
            NullValueHandling = NullValueHandling.Ignore, 
            Converters = new List<JsonConverter>() { 
                new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ" },
                JsonSubtypesConverterBuilder
                    .Of(typeof(Api.Buta), "k")
                    .RegisterSubtype(typeof(Api.Buta.Sync), "sync")
                    .RegisterSubtype(typeof(Api.Buta.Triplist), "triplist")
                    .RegisterSubtype(typeof(Api.Buta.Roomlist), "roomlist")
                    .RegisterSubtype(typeof(Api.Buta.Dbox), "dbox")
                    .RegisterSubtype(typeof(Api.Buta.Docr), "docr")
                    .RegisterSubtype(typeof(Api.Buta.LaunchActivity), "launchactivity")
                    .SerializeDiscriminatorProperty()
                    .Build(),
                JsonSubtypesConverterBuilder
                    .Of(typeof(Api.Krund), "k")
                    .RegisterSubtype(typeof(Api.Krund.Fixed), "fixed")
                    .RegisterSubtype(typeof(Api.Krund.Relative), "relative")
                    .RegisterSubtype(typeof(Api.Krund.Unit), "unit")
                    .SerializeDiscriminatorProperty()
                    .Build(),
                JsonSubtypesConverterBuilder
                    .Of(typeof(Api.Ev), "k")
                    .RegisterSubtype(typeof(Api.Ev.Byte), "byte")
                    .RegisterSubtype(typeof(Api.Ev.Short), "short")
                    .RegisterSubtype(typeof(Api.Ev.Int), "int")
                    .RegisterSubtype(typeof(Api.Ev.Long), "long")
                    .RegisterSubtype(typeof(Api.Ev.Float), "float")
                    .RegisterSubtype(typeof(Api.Ev.Double), "double")
                    .RegisterSubtype(typeof(Api.Ev.Boolean), "boolean")
                    .RegisterSubtype(typeof(Api.Ev.String), "string")
                    .RegisterSubtype(typeof(Api.Ev.Char), "char")
                    .RegisterSubtype(typeof(Api.Ev.ByteArray), "bytearray")
                    .RegisterSubtype(typeof(Api.Ev.ShortArray), "shortarray")
                    .RegisterSubtype(typeof(Api.Ev.IntArray), "intarray")
                    .RegisterSubtype(typeof(Api.Ev.LongArray), "longarray")
                    .RegisterSubtype(typeof(Api.Ev.FloatArray), "floatarray")
                    .RegisterSubtype(typeof(Api.Ev.DoubleArray), "doublearray")
                    .RegisterSubtype(typeof(Api.Ev.BooleanArray), "booleanarray")
                    .RegisterSubtype(typeof(Api.Ev.StringArray), "stringarray")
                    .RegisterSubtype(typeof(Api.Ev.CharArray), "chararray")
                    .RegisterSubtype(typeof(Api.Ev.IntegerArrayList), "integerarraylist")
                    .RegisterSubtype(typeof(Api.Ev.StringArrayList), "stringarraylist")
                    .RegisterSubtype(typeof(Api.Ev.Json), "json")
                    .SerializeDiscriminatorProperty()
                    .Build(),
                new St18JsonConverter(),
                JsonSubtypesConverterBuilder
                    .Of(typeof(Api.Payp), "k")
                    .RegisterSubtype(typeof(Api.Payp.Msg), "msg")
                    .RegisterSubtype(typeof(Api.Payp.Update), "update")
                    .SerializeDiscriminatorProperty()
                    .Build(),
                JsonSubtypesConverterBuilder
                    .Of(typeof(Api.Dubpayp), "k")
                    .RegisterSubtype(typeof(Api.Dubpayp.Msg), "msg")
                    .RegisterSubtype(typeof(Api.Dubpayp.Update), "update")
                    .SerializeDiscriminatorProperty()
                    .Build(),
                JsonSubtypesConverterBuilder
                    .Of(typeof(Api.Dboxited), "k")
                    .RegisterSubtype(typeof(Api.Dboxited.F), "f")
                    .RegisterSubtype(typeof(Api.Dboxited.D), "d")
                    .SerializeDiscriminatorProperty()
                    .Build(),
            } 
        };
        public static HttpContent JsonHttpContent<T>(this T tContent) =>
            new StringContent(JsonConvert.SerializeObject(tContent, jsons), Encoding.UTF8, "application/json").Also(content => content.Headers.ContentType.Also(ct => ct.CharSet = ct.CharSet.ToUpperInvariant()));

        public static HttpContent IfMatch(this HttpContent httpcontent, string ifmatch) =>
            httpcontent.Also(httpcontentT => httpcontent.Headers.Add("If-Match", ifmatch));

        public static DelegatingHandler AndThen(this HttpMessageHandler handlerInner, DelegatingHandler handlerOuter)
        {
            handlerOuter.InnerHandler = handlerInner;
            return handlerOuter;
        }

        public static async Task<T> GetOrNull<T>(this L l, Func<Task<T>> dgGet) where T : class
        {
            try
            {
                return await dgGet();
            }
            catch (ErHttp er) when (er.cod == 404)
            {
                return null;
            }
        }

        public static async Task<T> RunWithRetry<T>(this L l, CancellationToken ctok, Func<Task<T>> dgRequest)
        {
            var msDelay = 200;
            while (true)
            {
                try
                {
                    return await dgRequest();
                }
                catch (ErHttp er) when (er.cod == 429 || er.cod >= 500 && er.cod < 600)
                {
                    msDelay =
                        er.response.Headers.RetryAfter?.Date?.Let(dtu => dtu.Subtract(DateTime.UtcNow).TotalMilliseconds.Let(d => (int)d)) ??
                        er.response.Headers.RetryAfter?.Delta?.Let(ts => ts.TotalMilliseconds.Let(d => (int)d)) ??
                        msDelay;

                    l.E($"received {er.ust()}, will retry after {msDelay / 1000}s...");
                }
                catch (ErConnectionTimeout er)
                {
                    l.E($"received {er.ust()}, will retry after { msDelay / 1000}s...");
                }
                catch (HttpRequestException er)
                {
                    l.E($"received {er.ust()}, will retry after {msDelay / 1000}s...");
                }

                await TaskEx.Delay(msDelay, ctok);
                msDelay = Math.Min(msDelay * 2, 60000);
                l.I($"retrying...");
            }
        }

        public static async Task RunWithRetry(this L l, CancellationToken ctok, Func<Task> dgRequest) =>
            await l.RunWithRetry(ctok, async () => { await dgRequest(); return 42; });
    }

    public class ErHttp : Exception
    {
        public int cod => (int)response.StatusCode;
        public string status => response.ReasonPhrase;
        public readonly HttpResponseMessage response;
        public ErHttp(HttpResponseMessage response, string ostBody = null) : base($"{response.StatusCode}: {response.ReasonPhrase}\n{ostBody}")
        {
            this.response = response;
        }
    }

    public class ErConnectionTimeout : Exception {
        public ErConnectionTimeout(Exception erInner) : base("Http connection timed out", erInner) {}
    }

    public static class ErU
    {
        public static string ust(this Exception er) =>
            $"{er.GetType().FullName}: {er.Message}{(er.InnerException != null ? $"\n --> {er.InnerException.ust()}" : "")}";
    }

    public class St18JsonConverter : JsonConverter
    {
        public override bool CanConvert(Type rty)
        {
            return rty == typeof(Api.St18);
        }

        public override object ReadJson(JsonReader reader, Type rty, object obj, JsonSerializer serializer)
        {
            switch(reader.TokenType)
            {
                case JsonToken.StartObject:
                    var mpstByLocale = serializer.Deserialize<Dictionary<string, string>>(reader);
                    var stDefault = mpstByLocale["_"];
                    mpstByLocale.Remove("_");
                    return new Api.St18 { stDefault = stDefault, mpstByLocale = mpstByLocale };

                case JsonToken.String:
                    return new Api.St18 { stDefault = (string)reader.Value, mpstByLocale = new Dictionary<string, string>() };

                case JsonToken.Null:
                    return null;

                default:
                    throw new JsonSerializationException("invalid St18");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var st18 = value as Api.St18;
            if (st18.mpstByLocale.Count == 0)
                serializer.Serialize(writer, st18.stDefault);
            else
                serializer.Serialize(writer, st18.mpstByLocale.Concat(new KeyValuePair<string, string>("_", st18.stDefault).Cons()).ToDictionary());
        }
    }

    public static class LU
    {
        public static U Let<T, U>(this T t, Func<T, U> dg) => dg(t);
        public static async Task<U> LetAwait<T, U>(this T t, Func<T, Task<U>> dg) => await dg(t);
        public static T Also<T>(this T t, Action<T> dg) { dg(t); return t; }
        public static T Require<T>(this T t, Func<Exception> dgErr) { if (t == null) throw dgErr(); return t; }
    }
}