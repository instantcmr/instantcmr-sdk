using Icmr.Integration;
using v3 = Icmr.Integration.v3;
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using CommandLine;
using CommandLine.Text;
using Icmr.Integration.v3.Api;
using System.Collections.Generic;
using JsonSubTypes;
using System.Reflection;

namespace Icmr.Samples.Integration
{
    class IntegrationSample
    {
        public class Jsondb
        {
            JsonSerializer json;
            
            public Jsondb(JsonSerializer json)
            {
                this.json = json;
            }

            public T OreadFile<T>(string fpat) where T : class
            {
                if (!File.Exists(fpat)) return null;

                using (var filstr = File.OpenRead(fpat))
                using (var strmr = new StreamReader(filstr))
                using (var jsonr = new JsonTextReader(strmr))
                    return json.Deserialize<T>(jsonr);
            }

            public T ReadFile<T>(string fpat) where T : class => OreadFile<T>(fpat).Require(() => new Exception($"file {fpat} does not exist"));

            public void WriteFile<T>(string fpat, T data) where T : class
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fpat));
                using (var filstr = File.Create(fpat))
                using (var strmw = new StreamWriter(filstr))
                    PipeTo(strmw, data);
            }

            public string Serialize<T>(T data) where T : class
            {
                using (var stw = new StringWriter()) {
                    PipeTo(stw, data);
                    return stw.ToString();
                }
            }

            public void PipeTo<T>(TextWriter strmw, T data) where T : class
            {
                using (var jsonw = new JsonTextWriter(strmw))
                    json.Serialize(jsonw, data);
            }
        }

        public class Receiver
        {
            CancellationToken ctok;
            v3.IntegrationClient igr;
            Jsondb jsondb;
            string dpatRoot;
            string iepn;
            L l;

            public Receiver(
                CancellationToken ctok,
                v3.IntegrationClient igr,
                string dpatRoot,
                string iepn,
                Lf lf,
                Jsondb jsondb
            )
            {
                this.l = lf.L(this);
                this.ctok = ctok;
                this.igr = igr;
                this.dpatRoot = dpatRoot;
                this.iepn = iepn;
                this.jsondb = jsondb;
            }

            public async Task ProcessImages(string dpatRoot, string dosuxtid, Dosu odosuOld, Dosu odosuNew, v3.Api.Dubdosu dubdosu)
            {
                var rgimgOld = odosuOld?.rgimg ?? new Dosuimg[0];
                var rgimgNew = odosuNew?.rgimg ?? new Dosuimg[0];
                var rgmrimg = rgimgOld.MergeQQQ<Dosuimg, Dosuimg, string>(rgimgNew);
                foreach (var mrimg in rgmrimg)
                {
                    var fpatImg = Path.Combine(dpatRoot, $"./{dosuxtid.Replace(':', '_')}-{mrimg.k}.jpg"); // todo: extension from mime type
                    Directory.CreateDirectory(Path.GetDirectoryName(fpatImg));

                    switch (mrimg.kmr)
                    {
                        case Kmr.L:
                            l.I($"delete {mrimg.ol} from {fpatImg}");
                            DirU.DeleteFileOrLog(fpatImg, l);
                            break;

                        case Kmr.R:
                            l.I($"download {mrimg.or} into {fpatImg}");
                            using (var blob = await l.RunWithRetry(ctok, () => igr.DownloadImageAsync(dubdosu.rgimg.First(img => img.imgid == mrimg.or.imgid), CancellationToken.None)))
                            using (var filstr = File.Create(fpatImg))
                            {
                                l.I($"saving into {filstr.Name}");
                                await blob.Stream.CopyToAsync(filstr);
                            }
                            break;

                        case Kmr.B:
                            l.V($"ignore {mrimg.ol} -> {mrimg.or}");
                            break;
                    }
                }
            }

            public async Task ProcessRuts(string dpatRoot, string tripxtid, Ruts orutsOld, Ruts orutsNew, v3.Api.Dubruts dubruts)
            {
                var rgrutOld = orutsOld?.rgrut ?? new Rut[0];
                var rgrutNew = orutsNew?.rgrut ?? new Rut[0];
                var rgmrrut = rgrutOld.MergeQQQ<Rut, Rut, string>(rgrutNew).OrderBy(mr => mr.or != null);

                foreach(var mrrut in rgmrrut)
                {
                    string fpatRut(Rut rut) => Path.Combine(dpatRoot, $"./{tripxtid.Replace(':', '_')}/{rut.fpat}");

                    switch (mrrut.kmr)
                    {
                        case Kmr.L:
                        {
                            var fpat = fpatRut(mrrut.ol);
                            l.I($"delete {mrrut.ol} from {fpat}");
                            DirU.DeleteFileAndRemoveDirectoryIfEmptyOrLog(fpat, l);
                            break;
                        }
                        case Kmr.R:
                        {
                            var fpat = fpatRut(mrrut.or);
                            Directory.CreateDirectory(Path.GetDirectoryName(fpat));
                            l.I($"download {mrrut.or} into {fpat}");
                            try
                            {
                                using (var blob = await l.RunWithRetry(ctok, () => igr.DownloadRutAsync(dubruts.rgrut.First(rut => rut.rutid == mrrut.or.rutid).urlv, CancellationToken.None)))
                                using (var filstr = File.Create(fpat))
                                {
                                    l.I($"saving into {filstr.Name}");
                                    await blob.Stream.CopyToAsync(filstr);
                                }
                            }
                            catch (ErHttp er) when (er.cod == 404)
                            {
                                l.W($"file not found, maybe it has been removed since?");
                            }
                            break;
                        }
                        case Kmr.B:
                            if (mrrut.ol.fpat != mrrut.or.fpat)
                            {
                                var fpatOld = fpatRut(mrrut.ol);
                                var fpatNew = fpatRut(mrrut.or);
                                l.I($"rename {fpatOld} -> {fpatNew}");
                                Directory.CreateDirectory(Path.GetDirectoryName(fpatNew));
                                File.Delete(fpatNew); // this will lose a file when the same update swaps two files
                                File.Move(fpatOld, fpatNew);
                                DirU.DeleteDirectoryIfEmpty(Path.GetDirectoryName(fpatOld));
                            }
                            else l.V($"ignore {mrrut.ol} -> {mrrut.or}");
                            break;
                        
                    }
                }
            }

            public async Task ProcessDosu(Dub<Dubdosu> dub) =>
                await Process(dub, dubdosu => dubdosu.dosuxtid, dubdosu => dubdosu?.decode(), async (dubdosu, fpat, odosuOld, odosuNew) => {
                    await ProcessImages(Path.GetDirectoryName(fpat), (odosuOld ?? odosuNew).dosuxtid, odosuOld, odosuNew, dubdosu.oafter);
                });

            public async Task ProcessTrip(Dub<Dubtrip> dub) =>
                await Process(dub, dubtrip => dubtrip.tripxtid, dubtrip => dubtrip?.decode(), async (dubtrip, fpat, otripOld, otripNew) => {
                    var rgdosuOld = otripOld?.value.rgdosu ?? new Dosu[0];
                    var rgdosuNew = otripNew?.value.rgdosu ?? new Dosu[0];
                    var rgmrdosu = rgdosuOld.MergeQQQ<Dosu, Dosu, string>(rgdosuNew);
                    foreach (var mrdosu in rgmrdosu)
                    {
                        var dosuxtid = (mrdosu.or ?? mrdosu.ol).dosuxtid;
                        await ProcessImages(Path.GetDirectoryName(fpat), dosuxtid, mrdosu.ol, mrdosu.or, dubtrip.oafter?.rgdosu.FirstOrDefault(dubdosu => dubdosu.dosuxtid == dosuxtid));
                    }

                    await ProcessRuts(
                        Path.GetDirectoryName(fpat),
                        (otripNew ?? otripOld).value.tripxtid,
                        otripOld?.value?.ruts,
                        otripNew?.value?.ruts,
                        dubtrip.oafter?.ruts
                    );
                });

            public async Task ProcessUser(Dub<Dubuser> dub) => 
                await Process(dub, dubuser => dubuser.userxtid, dubuser => dubuser?.decode(), async (dubuser, fpat, ouserOld, ouserNew) => { });

            public async Task ProcessRoom(Dub<Dubroom> dub) =>
                await ProcessDiff(dub, dubroom => dubroom.roomxtid, dubroom => dubroom?.decode(), async (dubroom, fpat, oroomOld, oroomNew) => { });

            public async Task Process<Tdub, Tdb>(Dub<Tdub> dub, Func<Tdub, string> dgId, Func<Tdub, Tdb> dgDecode, Func<Dub<Tdub>, string, Tdb, Tdb, Task> dgProcess) 
                where Tdub: class 
                where Tdb: class
            {
                var rtyn = typeof(Tdub).Name.ToLowerInvariant();
                var xtid = dgId(dub.oafter ?? dub.obefore);
                var fpat = Path.Combine(dpatRoot, $"./{rtyn}/{xtid.Replace(':', '_')}.{rtyn}.json");
                var odbOld = jsondb.OreadFile<Tdb>(fpat);
                var odbBefore = dgDecode(dub.obefore);
                var odbNew = dgDecode(dub.oafter);

                if (odbOld.FEq(odbNew)) { l.W($"received duplicate: {xtid}"); return; }
                if (odbOld.FNe(odbBefore))
                {
                    l.W($"state on disk does not match dub.before, have we missed an update?!");
                    MDU.Update(odbOld, odbOld, odbBefore, MDU.Log(l, Severity.WARN));
                }

                Directory.CreateDirectory(Path.GetDirectoryName(fpat));

                l.I($"performing update");
                MDU.Update(odbBefore, odbBefore, odbNew, MDU.Log(l, Severity.INFO));

                await dgProcess(dub, fpat, odbOld, odbNew);

                if (odbNew != null)
                    jsondb.WriteFile(fpat, odbNew);
                else DirU.DeleteFileOrLog(fpat, l);
            }     
            
            public async Task ProcessDiff<Tdub, Tdb>(Dub<Tdub> dub, Func<Tdub, string> dgId, Func<Tdub, Tdb> dgDecode, Func<Dub<Tdub>, string, Tdb, Tdb, Task> dgProcess) 
                where Tdub: class 
                where Tdb: class
            {
                var rtyn = typeof(Tdub).Name.ToLowerInvariant();
                var xtid = dgId(dub.oafter ?? dub.obefore);
                var fpat = Path.Combine(dpatRoot, $"./{rtyn}/{xtid.Replace(':', '_')}.{rtyn}.json");
                var odbOld = jsondb.OreadFile<Tdb>(fpat);
                var odbBefore = dgDecode(dub.obefore);
                var odbAfter = dgDecode(dub.oafter);

                Directory.CreateDirectory(Path.GetDirectoryName(fpat));

                var odbNew = MDU.Update(odbOld, odbBefore, odbAfter, MDU.Log(l, Severity.INFO));

                await dgProcess(dub, fpat, odbOld, odbNew);

                if (odbNew != null)
                    jsondb.WriteFile(fpat, odbNew);
                else DirU.DeleteFileOrLog(fpat, l);
            }

            public async Task Receive()
            {
                l.I("starting up");
                while(!ctok.IsCancellationRequested)
                {
                    l.D($"receiving from iepn '{iepn}'...");
                    var orecid = Maybe.Of(Guid.NewGuid().ToString());
                    var dubev = await l.RunWithRetry(ctok, () => igr.ReceiveDubAsync(iepn, orecid, ctok));

                    if (dubev.rgdubm.Length == 0) l.D("received no updates");
                    else l.I($"received {dubev.rgdubm.Length} updates");

                    var idubm = 0;
                    foreach(var dubm in dubev.rgdubm)
                    {
                        l.I($"processing dubm {++idubm}/{dubev.rgdubm.Length}: {dubm}");
                        switch (dubm.kdubm)
                        {
                            case Kdubm.dosu: await ProcessDosu(dubm.odubdosu); break;
                            case Kdubm.trip: await ProcessTrip(dubm.odubtrip); break;
                            case Kdubm.user: await ProcessUser(dubm.odubuser); break;
                            case Kdubm.room: await ProcessRoom(dubm.odubroom); break;
                        }
                        l.D($"delete {dubm}");
                        await l.RunWithRetry(ctok, () => igr.DeleteDubmAsync(iepn, dubm, ctok));
                    }
                }
                l.I("finished");
            }
        }

        static async Task ReceiveDub(Lf lf, v3.IntegrationClient igr, Jsondb jsondb, string dpatRoot, string iepn)
        {
            var l = lf.L<IntegrationSample>();
            l.I($"will donwload to {Path.GetFullPath(dpatRoot)}");

            var ctoks = new CancellationTokenSource();
            var receiver = new Receiver(ctoks.Token, igr, dpatRoot, iepn, lf, jsondb);
            var task = new StoppableTask(TaskEx.Run(async () => await receiver.Receive(), ctoks.Token), ctoks);

            _ = TaskEx.Run(() =>
              {
                  l.I("press any key to stop");
                  Console.ReadKey();
                  l.I("cancelling...");
                  task.StopAsync();
              });

            await task.Task();
        }

        static async Task DownloadRut(L l, v3.IntegrationClient igr, Ruted rut, string dpat, CancellationToken ctok)
        {
            var fpat = Path.GetFullPath(Path.Combine(dpat, $"./{rut.fpat}"));
            Directory.CreateDirectory(Path.GetDirectoryName(fpat));
            l.I($"download {rut} into {fpat}");
            try
            {
                using (var blob = await l.RunWithRetry(ctok, () => igr.DownloadRutAsync(rut.urlv, ctok)))
                using (var filstr = File.Create(fpat))
                {
                    l.I($"saving into {filstr.Name}");
                    await blob.Stream.CopyToAsync(filstr);
                }
            }
            catch (ErHttp er) when(er.cod == 404)
            {
                l.W($"file not found, maybe it has been removed since?");
            }
        }

        static async Task<Wetag<Trip>> GetTrip(L l, v3.IntegrationClient igr, string tripxtid, CancellationToken ctok, string odpatDownloadRut = null) {
            var wetagtriped = await l.RunWithRetry(ctok, () => igr.GetTripAsync(tripxtid, false, ctok));

            if (odpatDownloadRut != null)
                foreach (var ruted in wetagtriped.v.ruts.rgrut)
                    await DownloadRut(l, igr, ruted, odpatDownloadRut, ctok);

            return wetagtriped.decode(tripxtid);
        }

        static async Task<Wetag<Trip>> UpdateTrip(L l, v3.IntegrationClient igr, string tripxtid, CancellationToken ctok, Func<Trip, Trip> dgUpdate)
        {
            while(true)
            {
                try
                {
                    var wetagtripSource = await GetTrip(l, igr, tripxtid, ctok);
                    l.D($"received {wetagtripSource}, updating...");
                    return await PutTrip(l, igr, ctok, wetagtripSource.etag, dgUpdate(wetagtripSource.value));
                } 
                catch (ErHttp er) when(er.cod == 412)
                {
                    l.W($"conflict, retrying");
                }
            }
        }

        static async Task<Wetag<Trip>> PutTrip(L l, v3.IntegrationClient igr, CancellationToken ctok, string oetag, Trip tripTarget)
        {
            var wetagtrip = new Wetag<Trip> { etag = oetag, value = tripTarget };
            l.D($"PUT {wetagtrip}");
            return (await l.RunWithRetry(ctok, () => igr.PutTripAsync(tripTarget.tripxtid, wetagtrip.encode(), ctok))).decode(tripTarget.tripxtid);
        }

        static async Task<Wetag<Trip>> InsertOrUpdateTrip(L l, v3.IntegrationClient igr, CancellationToken ctok, Wetag<Trip> owetagtripOrigin, Trip tripTarget)
        {
            try
            {
                if ((owetagtripOrigin?.value ?? null).FEq(tripTarget))
                    l.W("target matches local cache");
                else
                {
                    l.D("target differs from local cache");
                    MDU.Update(owetagtripOrigin?.value, owetagtripOrigin?.value, tripTarget, MDU.Log(l, Severity.DEBUG));
                }

                return await PutTrip(l, igr, ctok, owetagtripOrigin?.etag, tripTarget);
            }
            catch (ErHttp er) when (er.cod == 412 && owetagtripOrigin == null)
            {
                l.W($"conflicting insert, giving up");
                return await GetTrip(l, igr, tripTarget.tripxtid, ctok);
            }
            catch (ErHttp er) when (er.cod == 412 && owetagtripOrigin != null)
            {
                l.W("conflicting update");
                return await UpdateTrip(l, igr, tripTarget.tripxtid, ctok, tripSource => MDU.Update(tripSource, owetagtripOrigin.value, tripTarget, MDU.Log(l, Severity.WARN)));
            }
        }

        static async Task ImportTrip(Lf lf, v3.IntegrationClient igr, Jsondb jsondb, CmdImportTrip cmd)
        {
            var l = lf.L<IntegrationSample>();
            var fpat = Path.GetFullPath(cmd.Fpat);

            var dpatRoot = Path.GetFullPath("./db/");
            var tripTarget = jsondb.ReadFile<Trip>(fpat);
            var fpatDb = Path.Combine(dpatRoot, $"./{tripTarget.tripxtid}.json");
            l.I($"importing {fpat} into {fpatDb}");
            var owetagtripOrigin = jsondb.OreadFile<Wetag<Trip>>(fpatDb);

            Directory.CreateDirectory(Path.GetDirectoryName(fpatDb));

            var ctoks = new CancellationTokenSource();
            var ctok = ctoks.Token;

            (await InsertOrUpdateTrip(l, igr, ctok, owetagtripOrigin, tripTarget))
                .Also(wetagtrip => l.I($"updated: {wetagtrip}"))
                .Also(wetagtrip => jsondb.WriteFile(fpatDb, wetagtrip))
                .Also(wetagtrip => jsondb.WriteFile(fpat, wetagtrip.value));
        }

        static async Task ExportTrip(Lf lf, v3.IntegrationClient igr, Jsondb jsondb, CmdExportTrip cmd)
        {
            var l = lf.L<IntegrationSample>();

            var dpatRoot = Path.GetFullPath("./db/");
            var fpatDb = Path.Combine(dpatRoot, $"./{cmd.Tripxtid}.json");

            if (cmd.Fforce || cmd.FDownloadRut)
            {
                l.I($"downloading {cmd.Tripxtid} from backend");
                Directory.CreateDirectory(Path.GetDirectoryName(fpatDb));
                var ctoks = new CancellationTokenSource();
                var ctok = ctoks.Token;

                var ofpatRut = cmd.FDownloadRut ? Path.GetFullPath(Path.Combine(Path.GetDirectoryName(cmd.Fpat), $"./{cmd.Tripxtid}")) : null;

                (await GetTrip(l, igr, cmd.Tripxtid, ctok, ofpatRut)).Also(wetagtripT => jsondb.WriteFile(fpatDb, wetagtripT));
            }

            var fpat = Path.GetFullPath(cmd.Fpat);
            l.I($"exporting {fpatDb} -> {fpat}");
            jsondb.WriteFile(fpat, jsondb.ReadFile<Wetag<Trip>>(fpatDb).value);
        }

        static async Task UploadRut(Lf lf, v3.IntegrationClient igr, Jsondb jsondb, CmdUploadRut cmd)
        {
            var l = lf.L<IntegrationSample>();

            var dpatRoot = Path.GetFullPath("./db/");
            var fpatTrip = Path.GetFullPath(cmd.FpatTrip);
            var tripTarget = jsondb.ReadFile<Trip>(fpatTrip);
            var fpatDb = Path.Combine(dpatRoot, $"./{tripTarget.tripxtid}.json");
            var fpatRutLocal = Path.Combine(Path.GetDirectoryName(fpatTrip), $"./{cmd.FpatRutLocal}");
            var fpatRutRemote = cmd.OfpatRutRemote ?? Path.GetFileName(cmd.FpatRutLocal);
            l.I($"uploading {fpatRutLocal} for trip {tripTarget.tripxtid} as {fpatRutRemote}");

            var ctoks = new CancellationTokenSource();
            var ctok = ctoks.Token;

            using (var filstrmRut = File.OpenRead(fpatRutLocal))
                (await l.RunWithRetry(ctok, () => igr.PutRutAsync(tripTarget.tripxtid, fpatRutRemote, filstrmRut, ctok)))
                    .decode(tripTarget.tripxtid)
                    .Also(wetagtrip => l.I($"updated: {wetagtrip}"))
                    .Also(wetagtrip => jsondb.WriteFile(fpatDb, wetagtrip))
                    .Also(wetagtrip => jsondb.WriteFile(fpatTrip, wetagtrip.value));
        }

        static async Task DeleteRut(Lf lf, v3.IntegrationClient igr, Jsondb jsondb, CmdDeleteRut cmd)
        {
            var l = lf.L<IntegrationSample>();

            var dpatRoot = Path.GetFullPath("./db/");
            var fpatTrip = Path.GetFullPath(cmd.FpatTrip);
            var tripTarget = jsondb.ReadFile<Trip>(fpatTrip);
            var fpatDb = Path.Combine(dpatRoot, $"./{tripTarget.tripxtid}.json");
            var fpatRutRemote = cmd.FpatRutRemote;
            l.I($"removing {fpatRutRemote} from trip {tripTarget.tripxtid}");

            var ctoks = new CancellationTokenSource();
            var ctok = ctoks.Token;

            (await l.RunWithRetry(ctok, () => igr.DeleteRutAsync(tripTarget.tripxtid, fpatRutRemote, ctok)))
                .decode(tripTarget.tripxtid)
                .Also(wetagtrip => l.I($"updated: {wetagtrip}"))
                .Also(wetagtrip => jsondb.WriteFile(fpatDb, wetagtrip))
                .Also(wetagtrip => jsondb.WriteFile(fpatTrip, wetagtrip.value));
        }

        static async Task<Wetag<Scren>> GetScren(L l, v3.IntegrationClient igr, string userxtid, CancellationToken ctok)
        {
            var wetagscren = (await l.RunWithRetry(ctok, () => igr.GetScrenAsync(userxtid, ctok)));
            return wetagscren.decode(userxtid);
        }

        static async Task<Wetag<Scren>> UpdateScren(L l, v3.IntegrationClient igr, string userxtid, CancellationToken ctok, Func<Scren, Scren> dgUpdate)
        {
            while (true)
            {
                try
                {
                    var wetagscrenSource = await GetScren(l, igr, userxtid, ctok);
                    l.D($"received {wetagscrenSource}, updating...");
                    return await PutScren(l, igr, ctok, wetagscrenSource.etag, dgUpdate(wetagscrenSource.value));
                }
                catch (ErHttp er) when (er.cod == 412)
                {
                    l.W($"conflict, retrying");
                }
            }
        }

        static async Task<Wetag<Scren>> PutScren(L l, v3.IntegrationClient igr, CancellationToken ctok, string oetag, Scren screnTarget)
        {
            var wetagscren = new Wetag<Scren> { etag = oetag, value = screnTarget };
            l.D($"PUT {wetagscren}");
            return (await l.RunWithRetry(ctok, () => igr.PutScrenAsync(screnTarget.userxtid, wetagscren.encode(), ctok))).decode(screnTarget.userxtid);
        }

        static async Task<Wetag<Scren>> InsertOrUpdateScren(L l, v3.IntegrationClient igr, CancellationToken ctok, Wetag<Scren> owetagscrenOrigin, Scren screnTarget)
        {
            try
            {
                if ((owetagscrenOrigin?.value ?? null).FEq(screnTarget))
                    l.W("target matches local cache");
                else
                {
                    l.D("target differs from local cache");
                    MDU.Update(owetagscrenOrigin?.value, owetagscrenOrigin?.value, screnTarget, MDU.Log(l, Severity.DEBUG));
                }

                return await PutScren(l, igr, ctok, owetagscrenOrigin?.etag, screnTarget);
            }
            catch (ErHttp er) when (er.cod == 412 && owetagscrenOrigin == null)
            {
                l.W($"conflicting insert, giving up");
                return await GetScren(l, igr, screnTarget.userxtid, ctok);
            }
            catch (ErHttp er) when (er.cod == 412 && owetagscrenOrigin != null)
            {
                l.W("conflicting update");
                try
                {
                    return await UpdateScren(l, igr, screnTarget.userxtid, ctok, tripSource => MDU.Update(tripSource, owetagscrenOrigin.value, screnTarget, MDU.Log(l, Severity.WARN)));
                }
                catch (ErHttp erT) when (erT.cod == 404)
                {
                    l.W($"scren for user {screnTarget.userxtid} does not exist, inserting...");
                    return await PutScren(l, igr, ctok, null, screnTarget);
                }
            }
        }

        static async Task DeleteScrenI(L l, v3.IntegrationClient igr, string userxtid, CancellationToken ctok, string oetagScrenOrigin)
        {
            var oetag = oetagScrenOrigin;
            while (true)
            {
                try
                {
                    await l.RunWithRetry(ctok, () => igr.DeleteScrenAsync(userxtid, ctok, oetag));
                    return;
                }
                catch (ErHttp er) when (er.cod == 412)
                {
                    l.W($"conflict, retrying");
                    try
                    {
                        oetag = (await GetScren(l, igr, userxtid, ctok)).etag;
                        l.D($"received {oetag}");
                    }
                    catch (ErHttp erT) when (erT.cod == 404)
                    {
                        l.W("scren already removed, giving up");
                        return;
                    }
                }
            }
        }

        static async Task DeleteScren(Lf lf, v3.IntegrationClient igr, Jsondb jsondb, CmdDeleteScren cmd)
        {
            var l = lf.L<IntegrationSample>();

            var dpatRoot = Path.GetFullPath("./db/");
            var fpatDb = Path.Combine(dpatRoot, $"./{cmd.Userxtid}.scren.json");

            l.I($"deleting {fpatDb}");
            var ctoks = new CancellationTokenSource();
            var ctok = ctoks.Token;
            await DeleteScrenI(l, igr, cmd.Userxtid, ctok, jsondb.OreadFile<Wetag<Scren>>(fpatDb)?.etag);
            File.Delete(fpatDb);
        }

        static async Task ImportScren(Lf lf, v3.IntegrationClient igr, Jsondb jsondb, CmdImportScren cmd)
        {
            var l = lf.L<IntegrationSample>();
            var fpat = Path.GetFullPath(cmd.Fpat);

            var dpatRoot = Path.GetFullPath("./db/");
            var screnTarget = jsondb.ReadFile<Scren>(fpat);
            var fpatDb = Path.Combine(dpatRoot, $"./{screnTarget.userxtid}.scren.json");
            l.I($"importing {fpat} into {fpatDb}");
            var owetagscrenOrigin = jsondb.OreadFile<Wetag<Scren>>(fpatDb);

            Directory.CreateDirectory(Path.GetDirectoryName(fpatDb));

            var ctoks = new CancellationTokenSource();
            var ctok = ctoks.Token;

            (await InsertOrUpdateScren(l, igr, ctok, owetagscrenOrigin, screnTarget))
                .Also(wetagscren => l.I($"updated: {wetagscren}"))
                .Also(wetagscren => jsondb.WriteFile(fpatDb, wetagscren))
                .Also(wetagscren => jsondb.WriteFile(fpat, wetagscren.value));
        }

        static async Task ExportScren(Lf lf, v3.IntegrationClient igr, Jsondb jsondb, CmdExportScren cmd)
        {
            var l = lf.L<IntegrationSample>();

            var dpatRoot = Path.GetFullPath("./db/");
            var fpatDb = Path.Combine(dpatRoot, $"./{cmd.Userxtid}.scren.json");
            var fpatScren = Path.GetFullPath(cmd.Fpat);

            if (cmd.Fforce)
            {
                l.I($"downloading scren of {cmd.Userxtid} from backend");
                Directory.CreateDirectory(Path.GetDirectoryName(fpatDb));
                var ctoks = new CancellationTokenSource();
                var ctok = ctoks.Token;

                (await GetScren(l, igr, cmd.Userxtid, ctok)).Also(wetagscrenT => jsondb.WriteFile(fpatDb, wetagscrenT));
            }

            l.I($"exporting {fpatDb} -> {fpatScren}");
            jsondb.WriteFile(fpatScren, jsondb.ReadFile<Wetag<Scren>>(fpatDb).value);
        }

        static async Task<Tres> Put<Tin, Tout, Tres>(
            L l, 
            CancellationToken ctok,
            Func<Task<Wetag<Tout>>> dgGet, 
            Func<Wetag<Tin>, Task<Wetag<Tres>>> dgPut, 
            Func<Tout, Tin> dginFromOut,
            Func<Tin, Task<Tin>> dgUpdate
        ) 
            where Tin: class 
            where Tout: class
            where Tres: class
        {
            while(true)
            {
                var owetagoutBefore = await l.RunWithRetry(ctok, () => l.GetOrNull(() => dgGet()));
                l.I($"received {owetagoutBefore?.ToString() ?? "<none>"}, updating...");

                var owetaginBefore = owetagoutBefore?.Map(dginFromOut);

                var wetaginUpdated = new Wetag<Tin>
                {
                    etag = owetagoutBefore?.etag,
                    value = await dgUpdate(owetaginBefore?.value),
                };

                MDU.Update(owetaginBefore, owetaginBefore, wetaginUpdated, MDU.Log(l, Severity.INFO));

                try
                {
                    return (await l.RunWithRetry(ctok, () => dgPut(wetaginUpdated)))
                        .Also(owetagoutAfter => l.I($"updated, {owetagoutAfter?.ToString() ?? "<none>"}"))
                        .Let(owetagoutAfter => owetagoutAfter?.value);
                }
                catch(ErHttp er) when (er.cod == 412)
                {
                    l.W($"conflicting update, retrying");
                    continue;
                }
            }
        }

        static async Task PutRoom(Lf lf, v3.IntegrationClient igr, Jsondb jsondb, CmdPutRoom cmd)        
        {
            var l = lf.L<IntegrationSample>();

            var ctoks = new CancellationTokenSource();
            var ctok = ctoks.Token;

            var oroom = await Put(
                l,
                ctok,
                async () => (await igr.GetRoomAsync(cmd.Roomxtid, ctok)).decode(),
                async (wetagroomeu) => (await igr.PutRoomAsync(cmd.Roomxtid, wetagroomeu.encode(), ctok)).decodeMap(roomed => roomed.decode()),
                room => room.ToRoomeu(),
                oroomeuBefore => TaskEx.FromResult(new Roomeu
                {
                    ouxtid = cmd.Oouxtid ?? oroomeuBefore?.ouxtid ?? throw new Exception("Ouxtid is required"),
                    ostTitle =
                        cmd.OstTitle != null ?
                            cmd.OstTitle?.Let(stTitle => stTitle.Trim() == "~" ? null : stTitle)
                            : oroomeuBefore?.Let(roomeuBefore => roomeuBefore.ostTitle)
                    ,
                    rgbomaeu =
                        (oroomeuBefore?.Let(roomeuBefore => roomeuBefore.rgbomaeu) ?? new Bomaeu[0]).ToArray()
                        .Where(bomaeu => !cmd.RguserxtidRemove.Contains(bomaeu.userxtid) && !cmd.RguserxtidAddMuted.Contains(bomaeu.userxtid) && !cmd.RguserxtidAddUmuted.Contains(bomaeu.userxtid))
                        .Let(enbomaeu => enbomaeu.Concat(cmd.RguserxtidAddUmuted.Select(userxtid => new Bomaeu { userxtid = userxtid, fMuted = false })))
                        .Let(enbomaeu => enbomaeu.Concat(cmd.RguserxtidAddMuted.Select(userxtid => new Bomaeu { userxtid = userxtid, fMuted = true })))
                        .Let(enuserxtid => enuserxtid.ToDictionary(bomaeu => bomaeu.userxtid).Values.ToArray())
                    ,
                })
            );

            cmd.OfpatOut?.Let(fpatOut =>
                oroom?.Let(room =>
                    Path.Combine(Path.GetFullPath(fpatOut), $"{room.roomxtid}.room.json")
                    .Also(fpatOut => l.I($"exporting json as {fpatOut}"))
                    .Also(fpatOut => jsondb.WriteFile(fpatOut, room))
                )
            );
        }

        static async Task PutPost(Lf lf, v3.IntegrationClient igr, Jsondb jsondb, CmdPutPost cmd)
        {
            var l = lf.L<IntegrationSample>();

            var ctoks = new CancellationTokenSource();
            var ctok = ctoks.Token;
            var postxtid = Guid.NewGuid().ToString();

            var opost = await Put<Posteu, Room, Post>(
                l,
                ctok,
                async () => (await igr.GetRoomAsync(cmd.Roomxtid, ctok))?.Let(wetagroomed => new Wetag<Room> { etag = $"\"{wetagroomed.v.rovered.wetagdtupost.etag}\"", value = wetagroomed.v.decode() }),
                async (wetagposteu) => (await igr.PutPostAsync(cmd.Roomxtid, postxtid, wetagposteu.encode(), ctok)).decode(),
                _ => null,
                _ => TaskEx.FromResult(new Posteu
                {
                    userxtid = cmd.Userxtid,
                    stMessage = cmd.StMessage,
                })
            );

            cmd.OfpatOut?.Let(fpatOut =>
                opost?.Let(post =>
                    Path.Combine(fpatOut, $"{cmd.Roomxtid}-{post.etagpost}.post.json")
                    .Also(fpatOut => l.I($"exporting json as {fpatOut}"))
                    .Also(fpatOut => jsondb.WriteFile(fpatOut, post))
                )
            );
        }

        static async Task PutReceipt(Lf lf, Jsondb jsondb, CmdPutReceipt cmd, Func<string, string, string, CancellationToken, Task<v3.Api.Wetag<v3.Api.Roomed>>> dgPutReceipt)
        {
            var l = lf.L<IntegrationSample>();

            var ctok = new CancellationTokenSource().Token;

            l.I($"roomxtid {cmd.Roomxtid} userxtid {cmd.Userxtid} etag {cmd.Etagpost}");
            var oroom = (await l.RunWithRetry(ctok, () => dgPutReceipt(cmd.Roomxtid, cmd.Userxtid, cmd.Etagpost, ctok))).decode()?.value;

            cmd.OfpatOut?.Let(fpatOut =>
                oroom?.Let(room =>
                    Path.Combine(fpatOut, $"{cmd.Roomxtid}.room.json")
                    .Also(fpatOut => l.I($"exporting json as {fpatOut}"))
                    .Also(fpatOut => jsondb.WriteFile(fpatOut, room))
                )
            );
        }

        static async Task PutDeliveryReceipt(Lf lf, v3.IntegrationClient igr, Jsondb jsondb, CmdPutReceipt cmd) =>
            await PutReceipt(lf, jsondb, cmd, igr.PutSrAsync);

        static async Task PutReadReceipt(Lf lf, v3.IntegrationClient igr, Jsondb jsondb, CmdPutReceipt cmd) =>
            await PutReceipt(lf, jsondb, cmd, igr.PutRrAsync);

        static string[] OrgdirnReroot(string pat, string[] rgdirnRoot, bool fRecursive)
        {
            var rgdirn = pat.Split('/');
            var rgdirnCommon = rgdirnRoot.Take(rgdirnRoot.Length - 1).ToArray();

            if (!rgdirn.Take(rgdirnCommon.Length).SequenceEqual(rgdirnCommon)) 
                return null;

            var rgdirnRerooted = rgdirn.Skip(rgdirnCommon.Length).ToArray();

            if (rgdirnRerooted.Length == 0)
                return null;

            if (rgdirnRoot.Last() != "")
            {
                if (!rgdirnRerooted.SequenceEqual(new string[] { rgdirnRoot.Last() }))
                   return null;
                return rgdirnRerooted;
            }

            if (rgdirnRerooted.First() == "")
                return null;

            if (!fRecursive && rgdirnRerooted.Length > 1 && !rgdirnRerooted.Skip(1).SequenceEqual(new string[] { "" }))
                return null;

            return rgdirnRerooted;
        }

        static Dboxed DboxedFilter(Dboxed dboxed, string pat, bool fRecursive)
        {
            if (!pat.StartsWith("/")) pat = $"/{pat}";
            var rgdirnPat = pat.Split('/');

            return new Dboxed
            {
                rgit = 
                    dboxed.rgit.SelectMany(dboxit => 
                        new [] {OrgdirnReroot(dboxit.pat, rgdirnPat, fRecursive)}
                        .Where(orgdirn => orgdirn != null)
                        .Select(rgdirn => dboxit.WithPat(rgdirn.StJoin("/")))
                    ).ToArray(),
                rgsub = dboxed.rgsub,
                rgsyc = dboxed.rgsyc,
                userxtid = dboxed.userxtid,
            };
        }

        static async Task<(string, Dboxed)> ProcessDbox(L l, v3.IntegrationClient igr, string userxtid, string Opat, bool fRecursive, CancellationToken ctok)
        {
            l.I($"listing contents of document storage for '{userxtid}'");

            var dboxed = await l.RunWithRetry(ctok, () => igr.GetDboxAsync(userxtid, ctok));

            var patRemote = Opat ?? "/";
            return (patRemote, DboxedFilter(dboxed, patRemote, fRecursive));
        }

        static async Task ListDbox(Lf lf, v3.IntegrationClient igr, CmdListDbox cmd)
        {
            var l = lf.L<IntegrationSample>();
            var ctok = new CancellationTokenSource().Token;

            var (patRemote, dboxed) = await ProcessDbox(l, igr, cmd.Userxtid, cmd.PatRemote, cmd.FRecursive, ctok);
            var mpdboxsycedByFilid = dboxed.rgsyc.ToDictionary(dboxsyced => dboxsyced.filid);

            l.I($"contents of document storage of '{dboxed.userxtid}' at '{patRemote}'");

            var cmaxchPat = dboxed.rgit.Select(dboxited => dboxited.pat.Length).Concat(new[] { "path".Length }).Max();
            l.I($"{"path".PadRight(cmaxchPat)}   {"size",9}   {"uploaded",-22}   {"delivered",-22}   {"etag",5}");
            foreach(var dboxited in dboxed.rgit.OrderBy(dboxited => dboxited.pat))
            {
                switch(dboxited)
                {
                    case Dboxited.F dboxitedf:
                        l.I($"{dboxitedf.pat.PadRight(cmaxchPat)}   {dboxitedf.cb,9}   {dboxitedf.dtuUpload,-22}   {mpdboxsycedByFilid.Oget(dboxitedf.filid)?.Let(dboxsyced => $"{dboxsyced.dtuDelivered}") ?? "",-22}   {dboxitedf.etag,5}");
                        break;
                    case Dboxited.D dboxitedd:
                        l.I($"{dboxitedd.pat.PadRight(cmaxchPat)}   {"[folder]",9}   {dboxitedd.dtuUpload,-22}   {mpdboxsycedByFilid.Oget(dboxitedd.filid)?.Let(dboxsyced => $"{dboxsyced.dtuDelivered}") ?? "",-22}   {dboxitedd.etag,5}");
                        break;
                }
            }

            if (dboxed.rgsub.Length > 0)
                l.I($"'{dboxed.userxtid}' is subscribed to document storages of {dboxed.rgsub.Select(dboxsub => $"'{dboxsub.userxtid}'").StJoin(", ")}");
        }

        static async Task DownloadDbox(Lf lf, v3.IntegrationClient igr, CmdDownloadDbox cmd)
        {
            var l = lf.L<IntegrationSample>();
            var ctok = new CancellationTokenSource().Token;
            var (patRemote, dboxed) = await ProcessDbox(l, igr, cmd.Userxtid, cmd.PatRemote, cmd.FRecursive, ctok);

            foreach(var dboxited in dboxed.rgit.OrderBy(dboxited => dboxited.pat))
            {
                var patLocal = Path.GetFullPath(Path.Combine(cmd.PatLocal ?? "./", Path.Combine("./", dboxited.pat)));
                l.I($"downloading '{dboxed.userxtid}/{dboxited.pat}' -> '{patLocal}'");

                switch(dboxited)
                {
                    case Dboxited.D dboxitedd: 
                        Directory.CreateDirectory(patLocal); break;
                    case Dboxited.F dboxitedf:
                        try
                        {
                            using (var blob = await l.RunWithRetry(ctok, () => igr.DownloadDboxfilAsync(dboxitedf.urlv, ctok)))
                            using (var filstr = File.Create(patLocal))
                                await blob.Stream.CopyToAsync(filstr);
                        }
                        catch (ErHttp er) when (er.cod == 404)
                        {
                            l.W($"file not found, maybe it has been removed since?");
                        }
                        break;
                }
            }
        }

        static bool FDir(string patl) => File.GetAttributes(patl).HasFlag(FileAttributes.Directory);

        static IEnumerable<string> EnpatlChildren(string patl, string filn, bool fRecursive)
        {
            foreach(var patlChild in Directory.EnumerateFileSystemEntries(patl, filn))
            {
                yield return patlChild;
                if (fRecursive && FDir(patlChild))
                    foreach (var patlChildT in EnpatlChildren(patlChild, "*", fRecursive))
                        yield return patlChildT;
            }
        }

        static async Task UploadDbox(Lf lf, v3.IntegrationClient igr, CmdUploadDbox cmd)
        {
            var l = lf.L<IntegrationSample>();
            var ctok = new CancellationTokenSource().Token;

            var patlRoot = Path.GetFullPath(Path.Combine("./", cmd.PatLocal ?? "./"));
            var fFilRoot = !FDir(patlRoot);
            string filnl = "*";
            if (fFilRoot)
            {
                filnl = Path.GetFileName(patlRoot);
                patlRoot = $"{Path.GetDirectoryName(patlRoot)}\\";
            } else if (!patlRoot.EndsWith("\\")) patlRoot = $"{patlRoot}\\";

            var patrRoot = cmd.PatRemote ?? "/";
            if (!patrRoot.StartsWith("/")) patrRoot = $"/{patrRoot}";
            if (!fFilRoot && !patrRoot.EndsWith("/")) patrRoot = $"{patrRoot}/";

            l.I($"uploading '{patlRoot}' '{Path.Combine(patlRoot, filnl)}' into '{patrRoot}'");

            foreach (var patl in EnpatlChildren(patlRoot, filnl, cmd.FRecursive))
            {
                var fFil = !FDir(patl);
                var patr = $"{patrRoot}{(!patrRoot.EndsWith("/") ? "" : patl.Substring(patlRoot.Length))}{(fFil ? "" : "/")}".Replace('\\', '/');
                l.I($"uploading '{patl}' -> '{patr}'");

                if (fFil)
                    using (var filstrm = File.OpenRead(patl))
                        await l.RunWithRetry(ctok, () => igr.PutDboxFileAsync(cmd.Userxtid, patr, filstrm, ctok));
                else
                    await l.RunWithRetry(ctok, () => igr.PutDboxFolderAsync(cmd.Userxtid, patr, ctok));
            }
        }

        static async Task DeleteDbox(Lf lf, v3.IntegrationClient igr, CmdDeleteDbox cmd)
        {
            var l = lf.L<IntegrationSample>();
            var ctok = new CancellationTokenSource().Token;

            await l.RunWithRetry(ctok, () => igr.DeleteDboxAsync(cmd.Userxtid, cmd.PatRemote, ctok));
        }

        static async Task MoveDbox(Lf lf, v3.IntegrationClient igr, CmdMoveDbox cmd)
        {
            var l = lf.L<IntegrationSample>();
            var ctok = new CancellationTokenSource().Token;

            await l.RunWithRetry(ctok, () => igr.MoveDboxAsync(cmd.Userxtid, cmd.PatSource, cmd.PatDestination, ctok));
        }

        class CmdReceive {}

        class CmdExportTrip
        {
            [Option('t', "tripxtid", Required = true, HelpText = "Tripxtid of the trip to export")]
            public string Tripxtid { get; set; }

            [Option('o', "output", Required = true, HelpText = "Path to the trip json file to write into")]
            public string Fpat { get; set; }

            [Option('f', "force", DefaultValue = false, HelpText = "Force download, even if trip doesn't exist locally")]
            public bool Fforce { get; set; }

            [Option('d', "download", DefaultValue = false, HelpText = "Download all files from the dropbox")]
            public bool FDownloadRut { get; set; }
        }

        class CmdImportTrip 
        {
            [Option('i', "input", Required = true, HelpText = "Path to the trip json file to read from")]
            public string Fpat { get; set; }
        }

        class CmdUploadRut
        {
            [Option('i', "input", Required = true, HelpText = "Path to the trip json file to read from")]
            public string FpatTrip { get; set; }

            [Option('f', "filn-local", Required = true, HelpText = "Path to the document to upload (relative to the trip json file)")]
            public string FpatRutLocal { get; set; }

            [Option('o', "filn-remote", Required = false, HelpText = "Name of the file in the trip dropbox. If omitted, will use filn-local")]
            public string OfpatRutRemote { get; set; }
        }

        class CmdDeleteRut
        {
            [Option('i', "input", Required = true, HelpText = "Path to the trip json file to read from")]
            public string FpatTrip { get; set; }

            [Option('o', "filn-remote", Required = true, HelpText = "Name of the file in the trip dropbox to delete")]
            public string FpatRutRemote { get; set; }
        }

        class CmdImportScren
        {
            [Option('i', "input", Required = true, HelpText = "Path to the scren json file to read from")]
            public string Fpat { get; set; }
        }

        class CmdExportScren
        {
            [Option('u', "userxtid", Required = true, HelpText = "Userxtid of the user to export scren from")]
            public string Userxtid { get; set; }

            [Option('o', "output", Required = true, HelpText = "Path to the scren json file to write into")]
            public string Fpat { get; set; }

            [Option('f', "force", DefaultValue = false, HelpText = "Force download, even if scren doesn't exist locally")]
            public bool Fforce { get; set; }
        }

        class CmdDeleteScren
        {
            [Option('u', "userxtid", Required = true, HelpText = "Userxtid of the user to delete scren of")]
            public string Userxtid { get; set; }
        }

        [AttributeUsage(AttributeTargets.Property)]
        class RequiredAttribute: Attribute 
        {
            public static bool fValid<Tcmd>(Lf lf, Tcmd cmd)
            {
                var l = lf.L<IntegrationSample>();
                var fValid = true;

                foreach(var rpi in cmd.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (rpi.GetCustomAttributes(typeof(RequiredAttribute), true).Length > 0 && rpi.GetValue(cmd, null) == null)
                    {
                        l.E($"<{rpi.Name.StKebabCase()}> is required");
                        fValid = false;
                    }
                }

                return fValid;
            }
        }

        class CmdPutRoom
        {
            [ValueOption(0), Required]
            public string Roomxtid { get; set; }

            [ValueOption(1)]
            public string Oouxtid { get; set; }

            [Option('t', DefaultValue = null, HelpText = "Discussion topic title ('~' to remove)")]
            public string OstTitle { get; set; }

            [OptionArray('u', DefaultValue = new string[0], HelpText = "List of members to add to or remove from the room (coma separated list of userxtids, prepend with '+' to add unmuted, '!' to add muted or '~' to remove)")]
            public string[] Rguserxtid { get; set; }

            [Option('o', DefaultValue = null, HelpText = "Write updated json into file")]
            public string OfpatOut { get; set; }

            public IEnumerable<string> RguserxtidAddUmuted => Rguserxtid.Where(userxtid => userxtid.StartsWith("+")).Select(userxtid => userxtid.Substring(1));
            public IEnumerable<string> RguserxtidAddMuted => Rguserxtid.Where(userxtid => userxtid.StartsWith("!")).Select(userxtid => userxtid.Substring(1));
            public IEnumerable<string> RguserxtidRemove => Rguserxtid.Where(userxtid => userxtid.StartsWith("~")).Select(userxtid => userxtid.Substring(1));
        }

        class CmdPutPost
        {
            [ValueOption(0), Required]
            public string Roomxtid { get; set; }

            [ValueOption(1), Required]
            public string Userxtid { get; set; }

            [ValueOption(2), Required]
            public string StMessage { get; set; }

            [Option('o', DefaultValue = null, HelpText = "Write updated json into file")]
            public string OfpatOut { get; set; }
        }

        class CmdPutReceipt
        {
            [ValueOption(0)] 
            public string Roomxtid { get; set; }
            
            [ValueOption(1)] 
            public string Userxtid { get; set; }
            
            [ValueOption(2)] 
            public string Etagpost { get; set; }
            
            [Option('o', DefaultValue = null, HelpText = "Write updated json into file")]
            public string OfpatOut { get; set; }
        }

        class CmdPutDeliveryReceipt : CmdPutReceipt { }
        class CmdPutReadReceipt : CmdPutReceipt { }

        class CmdListDbox
        {
            [ValueOption(0), Required]
            public string Userxtid { get; set; }

            [ValueOption(1)]
            public string PatRemote { get; set; }

            [Option('r', DefaultValue = false, HelpText = "Recursive")]
            public bool FRecursive { get; set; }
        }

        class CmdDownloadDbox
        {
            [ValueOption(0), Required]
            public string Userxtid { get; set; }

            [ValueOption(1)]
            public string PatRemote { get; set; }

            [ValueOption(2)]
            public string PatLocal { get; set; }

            [Option('r', DefaultValue = false, HelpText = "Recursive")]
            public bool FRecursive { get; set; }
        }

        class CmdUploadDbox
        {
            [ValueOption(0), Required]
            public string Userxtid { get; set; }

            [ValueOption(1)]
            public string PatLocal { get; set; }

            [ValueOption(2)]
            public string PatRemote { get; set; }

            [Option('r', DefaultValue = false, HelpText = "Recursive")]
            public bool FRecursive { get; set; }
        }

        class CmdDeleteDbox
        {
            [ValueOption(0), Required]
            public string Userxtid { get; set; }

            [ValueOption(1), Required]
            public string PatRemote { get; set; }
        }

        class CmdMoveDbox
        {
            [ValueOption(0), Required]
            public string Userxtid { get; set; }

            [ValueOption(1), Required]
            public string PatSource { get; set; }

            [ValueOption(2), Required]
            public string PatDestination { get; set; }
        }

        class Clp2
        {
            public Clp2()
            {
                receive = new CmdReceive();
                importtrip = new CmdImportTrip();
                exporttrip = new CmdExportTrip();
                uploadrut = new CmdUploadRut();
                deleterut = new CmdDeleteRut();
                importscren = new CmdImportScren();
                exportscren = new CmdExportScren();
                putroom = new CmdPutRoom();
                putpost = new CmdPutPost();
                putdeliveryreceipt = new CmdPutDeliveryReceipt();
                putreadreceipt = new CmdPutReadReceipt();
                listdbox = new CmdListDbox();
                downloaddbox = new CmdDownloadDbox();
                uploaddbox = new CmdUploadDbox();
                deletedbox = new CmdDeleteDbox();
                movedbox = new CmdMoveDbox();
            }

            [VerbOption("receive", HelpText = "receive data updates")]
            public CmdReceive receive { get; set; }

            [VerbOption("import-trip", HelpText = "import trip")]
            public CmdImportTrip importtrip { get; set; }

            [VerbOption("export-trip", HelpText = "export trip")]
            public CmdExportTrip exporttrip { get; set; }

            [VerbOption("upload-rut", HelpText = "upload a file to a trip's dropbox")]
            public CmdUploadRut uploadrut { get; set; }

            [VerbOption("delete-rut", HelpText = "delete a file from a trip's dropbox")]
            public CmdDeleteRut deleterut { get; set; }

            [VerbOption("import-scren", HelpText = "import scren")]
            public CmdImportScren importscren { get; set; }

            [VerbOption("export-scren", HelpText = "export scren")]
            public CmdExportScren exportscren { get; set; }

            [VerbOption("delete-scren", HelpText = "delete scren")]
            public CmdDeleteScren deletescren { get; set; }

            [VerbOption("room", HelpText = "create, update or retrieve a chat room")]
            public CmdPutRoom putroom { get; set; }

            [VerbOption("say", HelpText = "send a message into a chat room")]
            public CmdPutPost putpost { get; set; }

            [VerbOption("read-receipt", HelpText = "update read receipt of a member of a chat room")]
            public CmdPutReadReceipt putreadreceipt { get; set; }

            [VerbOption("delivery-receipt", HelpText = "update delivery receipt of a member of a chat room")]
            public CmdPutDeliveryReceipt putdeliveryreceipt { get; set; }

            [VerbOption("list-dbox", HelpText = "list contents of a user's document storage")]
            public CmdListDbox listdbox { get; set; }

            [VerbOption("download-dbox", HelpText = "download contents of a user's document storage")]
            public CmdDownloadDbox downloaddbox { get; set; }

            [VerbOption("upload-dbox", HelpText = "upload files to a user's document storage")]
            public CmdUploadDbox uploaddbox { get; set; }

            [VerbOption("delete-dbox", HelpText = "delete contents of a user's document storage")]
            public CmdDeleteDbox deletedbox { get; set; }

            [VerbOption("move-dbox", HelpText = "move contents of a user's document storage")]
            public CmdMoveDbox movedbox { get; set; }

            [HelpVerbOption("help")]
            public string Help(string cmdn) => HelpText.AutoBuild(this, cmdn);

            [ParserState]
            public IParserState LastParserState { get; set; }
        }

        public class St18JsonConverter : JsonConverter
        {
            public override bool CanConvert(Type rty)
            {
                return rty == typeof(St18);
            }

            public override object ReadJson(JsonReader reader, Type rty, object obj, JsonSerializer serializer)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.StartObject:
                        var mpstByLocale = serializer.Deserialize<Dictionary<string, string>>(reader);
                        var stDefault = mpstByLocale["_"];
                        mpstByLocale.Remove("_");
                        return new St18 { stDefault = stDefault, mpstByLocale = mpstByLocale };

                    case JsonToken.String:
                        return new St18 { stDefault = (string)reader.Value, mpstByLocale = new Dictionary<string, string>() };

                    case JsonToken.Null:
                        return null;

                    default:
                        throw new JsonSerializationException("invalid St18");
                }
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var st18 = value as St18;
                if (st18.mpstByLocale.Count == 0)
                    serializer.Serialize(writer, st18.stDefault);
                else
                    serializer.Serialize(writer, st18.mpstByLocale.Concat(new KeyValuePair<string, string>("_", st18.stDefault).Cons()).ToDictionary());
            }
        }

        public static void Main(string[] args)
        {
            var clp = new Clp2();
            string cmdn = null;
            object cmd = null;
            if (!Parser.Default.ParseArgumentsStrict(args, clp, (cmdnT, cmdT) => { cmdn = cmdnT; cmd = cmdT; })) return;

            var lf = new Lf(new Lwcon().Filter(Properties.Settings.Default.loglevel));
            var l = lf.L<IntegrationSample>();
            if (!RequiredAttribute.fValid(lf, cmd)) return;

            var jsondb = new Jsondb(
                JsonSerializer.Create(new JsonSerializerSettings { 
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters = new List<JsonConverter>() {
                        JsonSubtypesConverterBuilder
                            .Of(typeof(Buta), "k")
                            .RegisterSubtype(typeof(Buta.Sync), "sync")
                            .RegisterSubtype(typeof(Buta.Triplist), "triplist")
                            .RegisterSubtype(typeof(Buta.Roomlist), "roomlist")
                            .RegisterSubtype(typeof(Buta.Dbox), "dbox")
                            .RegisterSubtype(typeof(Buta.Docr), "docr")
                            .RegisterSubtype(typeof(Buta.LaunchActivity), "launchactivity")
                            .SerializeDiscriminatorProperty()
                            .Build(),
                        JsonSubtypesConverterBuilder
                            .Of(typeof(Krund), "k")
                            .RegisterSubtype(typeof(Krund.Fixed), "fixed")
                            .RegisterSubtype(typeof(Krund.Relative), "relative")
                            .RegisterSubtype(typeof(Krund.Unit), "unit")
                            .SerializeDiscriminatorProperty()
                            .Build(),
                        JsonSubtypesConverterBuilder
                            .Of(typeof(Ev), "k")
                            .RegisterSubtype(typeof(Ev.Byte), "byte")
                            .RegisterSubtype(typeof(Ev.Short), "short")
                            .RegisterSubtype(typeof(Ev.Int), "int")
                            .RegisterSubtype(typeof(Ev.Long), "long")
                            .RegisterSubtype(typeof(Ev.Float), "float")
                            .RegisterSubtype(typeof(Ev.Double), "double")
                            .RegisterSubtype(typeof(Ev.Boolean), "boolean")
                            .RegisterSubtype(typeof(Ev.String), "string")
                            .RegisterSubtype(typeof(Ev.Char), "char")
                            .RegisterSubtype(typeof(Ev.ByteArray), "bytearray")
                            .RegisterSubtype(typeof(Ev.ShortArray), "shortarray")
                            .RegisterSubtype(typeof(Ev.IntArray), "intarray")
                            .RegisterSubtype(typeof(Ev.LongArray), "longarray")
                            .RegisterSubtype(typeof(Ev.FloatArray), "floatarray")
                            .RegisterSubtype(typeof(Ev.DoubleArray), "doublearray")
                            .RegisterSubtype(typeof(Ev.BooleanArray), "booleanarray")
                            .RegisterSubtype(typeof(Ev.StringArray), "stringarray")
                            .RegisterSubtype(typeof(Ev.CharArray), "chararray")
                            .RegisterSubtype(typeof(Ev.IntegerArrayList), "integerarraylist")
                            .RegisterSubtype(typeof(Ev.StringArrayList), "stringarraylist")
                            .RegisterSubtype(typeof(Ev.Json), "json")
                            .SerializeDiscriminatorProperty()
                            .Build(),
                        new St18JsonConverter(),
                        JsonSubtypesConverterBuilder
                            .Of(typeof(Payp), "k")
                            .RegisterSubtype(typeof(Payp.Msg), "msg")
                            .RegisterSubtype(typeof(Payp.Update), "update")
                            .SerializeDiscriminatorProperty()
                            .Build(),
                    }
                })
            ); 
            l.I($"starting up, performing {cmdn}");

            var dpatRoot = Properties.Settings.Default.dpatRoot;
            var iepn = Properties.Settings.Default.iepn;
            var igr = new v3.IntegrationClient(
                uriEndpoint: new Uri(Properties.Settings.Default.url),
                copid: Properties.Settings.Default.copid,
                kid: Properties.Settings.Default.kid,
                shs: Properties.Settings.Default.shs,
                lf: lf
            );

            try
            {
                l.I("waiting for task to finish...");
                switch (cmd)
                {
                    case CmdReceive cmdreceive: ReceiveDub(lf, igr, jsondb, dpatRoot, iepn).Wait(); break;
                    case CmdImportTrip cmdimporttrip: ImportTrip(lf, igr, jsondb, cmdimporttrip).Wait(); break;
                    case CmdExportTrip cmdexporttrip: ExportTrip(lf, igr, jsondb, cmdexporttrip).Wait(); break;
                    case CmdUploadRut cmduploadrut: UploadRut(lf, igr, jsondb, cmduploadrut).Wait(); break;
                    case CmdDeleteRut cmddeleterut: DeleteRut(lf, igr, jsondb, cmddeleterut).Wait(); break;
                    case CmdImportScren cmdimportscren: ImportScren(lf, igr, jsondb, cmdimportscren).Wait(); break;
                    case CmdExportScren cmdexportscren: ExportScren(lf, igr, jsondb, cmdexportscren).Wait(); break;
                    case CmdDeleteScren cmddeletescren: DeleteScren(lf, igr, jsondb, cmddeletescren).Wait(); break;
                    case CmdPutRoom cmdputroom: PutRoom(lf, igr, jsondb, cmdputroom).Wait(); break;
                    case CmdPutPost cmdputpost: PutPost(lf, igr, jsondb, cmdputpost).Wait(); break;
                    case CmdPutReadReceipt cmdputreceipt: PutReadReceipt(lf, igr, jsondb, cmdputreceipt).Wait(); break;
                    case CmdPutDeliveryReceipt cmdputreceipt: PutDeliveryReceipt(lf, igr, jsondb, cmdputreceipt).Wait(); break;
                    case CmdListDbox cmdlistdbox: ListDbox(lf, igr, cmdlistdbox).Wait(); break;
                    case CmdDownloadDbox cmddownloaddbox: DownloadDbox(lf, igr, cmddownloaddbox).Wait(); break;
                    case CmdDeleteDbox cmddeletedbox: DeleteDbox(lf, igr, cmddeletedbox).Wait(); break;
                    case CmdUploadDbox cmduploaddbox: UploadDbox(lf, igr, cmduploaddbox).Wait(); break;
                    case CmdMoveDbox cmdmovedbox: MoveDbox(lf, igr, cmdmovedbox).Wait(); break;
                }
            }
            catch (Exception er)
            {
                l.E($"failed with {er}");
            }

            l.I("finished");
        }
    }
}