using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Icmr.Integration;
using Api = Icmr.Integration.v3.Api;
using System.Reflection;
using System.Diagnostics;

namespace Icmr.Samples.Integration
{
    public static class ProtocolConversions
    {
        public static Docrref decode(this Api.Dubdocrref dubdocrref) =>
            new Docrref
            {
                docrid = dubdocrref.docrid,
                tripxtid = dubdocrref.tripxtid
            };

        public static Dosumeta decode(this Api.Dosumeta dosumeta) =>
            new Dosumeta
            {
                ostNotes = dosumeta.ostNotes,
                ostOrderid = dosumeta.ostOrderid,
                ostPlate = dosumeta.ostPlate
            };

        public static Redoim decode(this Api.Dubredoim dubredoim) =>
            new Redoim
            {
                dtu = dubredoim.dtu,
                kredosu = dubredoim.kredosu,
                userxtid = dubredoim.userxtid
            };

        public static Doed decode(this Api.Dubdoed dubdoed) =>
            new Doed
            {
                doedid = dubdoed.doedid,
                dtu = dubdoed.dtu,
                userxtid = dubdoed.userxtid
            };

        public static Dosuimg decode(this Api.Dubdosuimg dubdosuimg) =>
            new Dosuimg
            {
                imgid = dubdosuimg.imgid,
                oredoim = dubdosuimg.oredoim?.decode(),
                odoed = dubdosuimg.odoed?.decode(),
            };

        public static Dosu decode(this Api.Dubdosu dubdosu) =>
            new Dosu
            {
                dosuxtid = dubdosu.dosuxtid,
                userxtid = dubdosu.userxtid,
                copid = dubdosu.copid,
                ouid = dubdosu.ouid,
                kdocr = dubdosu.kdocr,
                odocr = dubdosu.odocr?.decode(),
                dosumeta = dubdosu.dosumeta.decode(),
                dtuSubmit = dubdosu.dtuSubmit,
                dtuUpload = dubdosu.dtuUpload,
                loc = dubdosu.loc,
                rgimg = dubdosu.rgimg.Select(dubdosuimg => dubdosuimg.decode()).ToArray()
            };

        public static Api.Wetag<U> encodeMap<T, U>(this Wetag<T> wetag, Func<T, U> dgEncode) =>
            new Api.Wetag<U>
            {
                etag = wetag.etag,
                v = dgEncode(wetag.value)
            };

        public static Wetag<U> decodeMap<T, U>(this Api.Wetag<T> wetag, Func<T, U> dgDecode) =>
            new Wetag<U>
            {
                etag = wetag.etag,
                value = dgDecode(wetag.v)
            };

        public static Api.Tripmeta encode(this Tripmeta tripmeta) =>
            new Api.Tripmeta
            {
                odtuCreate = tripmeta.odtuCreate,
                ostAddress = tripmeta.ostAddress,
                ostCustomer = tripmeta.ostCustomer,
                ostHaulerPlate = tripmeta.ostHaulerPlate,
                ostInstructions = tripmeta.ostInstructions,
                ostNotes = tripmeta.ostNotes,
                ostOrderId = tripmeta.ostOrderId,
                ostTrailerPlate = tripmeta.ostTrailerPlate,
                ostTripCode = tripmeta.ostTripCode
            };

        public static Tripmeta decode(this Api.Tripmeta tripmeta) =>
            new Tripmeta
            {
                odtuCreate = tripmeta.odtuCreate,
                ostAddress = tripmeta.ostAddress,
                ostCustomer = tripmeta.ostCustomer,
                ostHaulerPlate = tripmeta.ostHaulerPlate,
                ostInstructions = tripmeta.ostInstructions,
                ostNotes = tripmeta.ostNotes,
                ostOrderId = tripmeta.ostOrderId,
                ostTrailerPlate = tripmeta.ostTrailerPlate,
                ostTripCode = tripmeta.ostTripCode
            };

        public static Api.Docr encode(this Docr docr) =>
            new Api.Docr
            {
                docrid = docr.docrid,
                dtuRequest = docr.dtuRequest,
                fRequired = docr.fRequired,
                kdocr = docr.kdocr,
                ofDeleted = docr.ofDeleted,
                ostDesc = docr.ostDesc,
                ostShortName = docr.ostShortName
            };

        public static Docr decode(this Api.Docr docr) =>
            new Docr
            {
                docrid = docr.docrid,
                dtuRequest = docr.dtuRequest,
                fRequired = docr.fRequired,
                kdocr = docr.kdocr,
                ofDeleted = docr.ofDeleted,
                ostDesc = docr.ostDesc,
                ostShortName = docr.ostShortName
            };

        public static Api.Stan encode(this Stan stan) =>
            new Api.Stan
            {
                stanxtid = stan.stanxtid,
                kstan = stan.kstan,
                odtu = stan.odtu,
                ofDeleted = stan.ofDeleted,
                okmDistance = stan.okmDistance,
                cufis = stan.cufis.encode(),
                ostAddress = stan.ostAddress,
                ostNotes = stan.ostNotes,
                ostSortBy = stan.ostSortBy
            };

        public static Stan decode(this Api.Stan stan) =>
            new Stan
            {
                stanxtid = stan.stanxtid,
                kstan = stan.kstan,
                odtu = stan.odtu,
                ofDeleted = stan.ofDeleted,
                okmDistance = stan.okmDistance,
                cufis = stan.cufis.decode(),
                ostAddress = stan.ostAddress,
                ostNotes = stan.ostNotes,
                ostSortBy = stan.ostSortBy
            };

        public static Api.Tripeu encode(this Trip trip) =>
            new Api.Tripeu
            {
                ktroc = trip.ktroc,
                ostSortBy = trip.ostSortBy,
                ouid = trip.ouid,
                ouserxtidDriver = trip.ouserxtidDriver,
                rgdocr = trip.rgdocr.Select(docr => docr.encode()).ToArray(),
                rgstan = trip.rgstan.Select(stan => stan.encode()).ToArray(),
                tripmeta = trip.tripmeta.encode(),
                cufis = trip.cufis.encode(),
            };

        public static Rut decode(this Api.Ruted rut) =>
            new Rut
            {
                rutid = rut.rutid,
                fpat = rut.fpat,
                cb = rut.cb,
                dtuUpload = rut.dtuUpload,
            };

        public static Ruts decode(this Api.Rutsed ruts) =>
            new Ruts
            {
                rgrut = ruts.rgrut.Select(rut => rut.decode()).ToArray(),
            };

        public static Cufi decode(this Api.Cufi cufi) =>
            new Cufi
            {
                id = cufi.id,
                n = cufi.n,
                v = cufi.v,
            };

        public static Api.Cufi encode(this Cufi cufi) =>
            new Api.Cufi
            {
                id = cufi.id,
                n = cufi.n,
                v = cufi.v,
            };

        public static Cufis decode(this Api.Cufis cufis) =>
            new Cufis
            {
                rgcufi = cufis.rgcufi.Select(cufi => cufi.decode()).ToArray(),
            };

        public static Api.Cufis encode(this Cufis cufis) =>
            new Api.Cufis
            {
                rgcufi = cufis.rgcufi.Select(cufi => cufi.encode()).ToArray(),
            };

        public static Trip decode(this Api.Triped trip) =>
            new Trip
            {
                ktroc = trip.ktroc,
                ostSortBy = trip.ostSortBy,
                ouid = trip.ouid,
                ouserxtidDriver = trip.ouserxtidDriver,
                rgdocr = trip.rgdocr.Select(docr => docr.decode()).ToArray(),
                rgstan = trip.rgstan.Select(stan => stan.decode()).ToArray(),
                tripmeta = trip.tripmeta.decode(),
                rgdosu = new Dosu[0],
                dtuCreate = trip.dtuCreate,
                odtuMfc = trip.odtuMfc,
                ruts = trip.ruts.decode(),
                cufis = trip.cufis.decode(),
            };

        public static Rut decode(this Api.Dubrut rut) =>
            new Rut
            {
                rutid = rut.rutid,
                fpat = rut.fpat,
                cb = rut.cb,
                dtuUpload = rut.dtuUpload,
            };

        public static Ruts decode(this Api.Dubruts ruts) =>
            new Ruts
            {
                rgrut = ruts.rgrut.Select(rut => rut.decode()).ToArray()
            };

        public static Wetag<Trip> decode(this Api.Dubtrip trip) =>
            new Wetag<Trip>
            {
                etag = trip.etag,
                value = new Trip
                {
                    tripxtid = trip.tripxtid,
                    ktroc = trip.ktroc,
                    ostSortBy = trip.ostSortBy,
                    ouid = trip.ouid,
                    ouserxtidDriver = trip.ouserxtidDriver,
                    rgdocr = trip.rgdocr.Select(docr => docr.decode()).ToArray(),
                    rgstan = trip.rgstan.Select(stan => stan.decode()).ToArray(),
                    rgdosu = trip.rgdosu.Select(dosu => dosu.decode()).ToArray(),
                    tripmeta = trip.tripmeta.decode(),
                    dtuCreate = trip.dtuCreate,
                    odtuMfc = trip.odtuMfc,
                    ruts = trip.ruts.decode(),
                    cufis = trip.cufis.decode(),
                }
            };
        public static Ulic decode(this Api.Dubulic ulic) =>
            new Ulic
            {
                kid = ulic.kid,
                ostDeviceImei = ulic.ostDeviceImei,
                ostDeviceModel = ulic.ostDeviceModel,
                ostImsi = ulic.ostImsi,
                ostPhone = ulic.ostPhone,
                ostPin = ulic.ostPin,
                ostSubscription = ulic.ostSubscription,
            };
        public static Usermeta decode(this Api.Dubusermeta usermeta) =>
            new Usermeta
            {
                ostEmployeeId = usermeta.ostEmployeeId,
                ostHaulerPlate = usermeta.ostHaulerPlate,
                ostTrailerPlate = usermeta.ostTrailerPlate,
                ostVoicePhone = usermeta.ostVoicePhone,
            };
        public static Dbox decode(this Api.Dubdbox dbox) =>
            new Dbox
            {
                rguserxtidFollow = dbox.rguserxtidFollow,
                oshrn = dbox.oshrn,
            };
        public static User decode(this Api.Dubuser user) =>
            new User
            {
                userxtid = user.userxtid,
                copid = user.copid,
                ouid = user.ouid,
                usern = user.usern,
                rgulic = user.rgulic.Select(ulic => ulic.decode()).ToArray(),
                usermeta = user.usermeta.decode(),
                dbox = user.dbox.decode(),
                rgkrole = user.rgkrole,
                rgtripxtid = user.rgtripxtid,
            };

        public static Wetag<Trip> decode(this Api.Wetag<Api.Triped> wetagtriped, string tripxtid) =>
            wetagtriped.decodeMap(triped => triped
                .decode()
                .Copy(b =>
                    b.withTripxtid(tripxtid)
                )
            );

        public static Api.Wetag<Api.Tripeu> encode(this Wetag<Trip> wetagtrip) =>
            wetagtrip.encodeMap(trip => trip.encode());

        public static Compn decode(this Api.Compn compn) =>
            new Compn
            {
                packagen = compn.packagen,
                classn = compn.classn,
            };
        public static Api.Compn encode(this Compn compn) =>
            new Api.Compn
            {
                packagen = compn.packagen,
                classn = compn.classn,
            };

        public static Ev decode(this Api.Ev ev) => ev switch
        {
            Api.Ev.Byte evT => new Ev.Byte { v = evT.v },
            Api.Ev.Short evT => new Ev.Short { v = evT.v },
            Api.Ev.Int evT => new Ev.Int { v = evT.v },
            Api.Ev.Long evT => new Ev.Long { v = evT.v },
            Api.Ev.Float evT => new Ev.Float { v = evT.v },
            Api.Ev.Double evT => new Ev.Double { v = evT.v },
            Api.Ev.Boolean evT => new Ev.Boolean { v = evT.v },
            Api.Ev.String evT => new Ev.String { v = evT.v },
            Api.Ev.Char evT => new Ev.Char { v = evT.v },
            Api.Ev.ByteArray evT => new Ev.ByteArray { v = evT.v },
            Api.Ev.ShortArray evT => new Ev.ShortArray { v = evT.v },
            Api.Ev.IntArray evT => new Ev.IntArray { v = evT.v },
            Api.Ev.LongArray evT => new Ev.LongArray { v = evT.v },
            Api.Ev.FloatArray evT => new Ev.FloatArray { v = evT.v },
            Api.Ev.DoubleArray evT => new Ev.DoubleArray { v = evT.v },
            Api.Ev.BooleanArray evT => new Ev.BooleanArray { v = evT.v },
            Api.Ev.StringArray evT => new Ev.StringArray { v = evT.v },
            Api.Ev.CharArray evT => new Ev.CharArray { v = evT.v },
            Api.Ev.IntegerArrayList evT => new Ev.IntegerArrayList { v = evT.v },
            Api.Ev.StringArrayList evT => new Ev.StringArrayList { v = evT.v },
            Api.Ev.Json evT => new Ev.Json { v = evT.v },
            _ => throw new Exception($"unknown ev {ev.GetType().FullName}"),
        };
        public static Api.Ev encode(this Ev ev) => ev switch
        {
            Ev.Byte evT => new Api.Ev.Byte { v = evT.v },
            Ev.Short evT => new Api.Ev.Short { v = evT.v },
            Ev.Int evT => new Api.Ev.Int { v = evT.v },
            Ev.Long evT => new Api.Ev.Long { v = evT.v },
            Ev.Float evT => new Api.Ev.Float { v = evT.v },
            Ev.Double evT => new Api.Ev.Double { v = evT.v },
            Ev.Boolean evT => new Api.Ev.Boolean { v = evT.v },
            Ev.String evT => new Api.Ev.String { v = evT.v },
            Ev.Char evT => new Api.Ev.Char { v = evT.v },
            Ev.ByteArray evT => new Api.Ev.ByteArray { v = evT.v },
            Ev.ShortArray evT => new Api.Ev.ShortArray { v = evT.v },
            Ev.IntArray evT => new Api.Ev.IntArray { v = evT.v },
            Ev.LongArray evT => new Api.Ev.LongArray { v = evT.v },
            Ev.FloatArray evT => new Api.Ev.FloatArray { v = evT.v },
            Ev.DoubleArray evT => new Api.Ev.DoubleArray { v = evT.v },
            Ev.BooleanArray evT => new Api.Ev.BooleanArray { v = evT.v },
            Ev.StringArray evT => new Api.Ev.StringArray { v = evT.v },
            Ev.CharArray evT => new Api.Ev.CharArray { v = evT.v },
            Ev.IntegerArrayList evT => new Api.Ev.IntegerArrayList { v = evT.v },
            Ev.StringArrayList evT => new Api.Ev.StringArrayList { v = evT.v },
            Ev.Json evT => new Api.Ev.Json { v = evT.v },
            _ => throw new Exception($"unknown ev {ev.GetType().FullName}"),
        };

        public static Inspe decode(this Api.Inspe inspe) =>
            new Inspe
            {
                oaction = inspe.oaction,
                ocompn = inspe.ocompn?.decode(),
                odata = inspe.odata,
                ompextra = inspe.ompextra?.Select(kv => new KeyValuePair<string, Ev>(kv.Key, kv.Value.decode())).ToDictionary(),
            };
        public static Api.Inspe encode(this Inspe inspe) =>
            new Api.Inspe
            {
                oaction = inspe.oaction,
                ocompn = inspe.ocompn?.encode(),
                odata = inspe.odata,
                ompextra = inspe.ompextra?.Select(kv => new KeyValuePair<string, Api.Ev>(kv.Key, kv.Value.encode())).ToDictionary(),
            };

        public static Buta decode(this Api.Buta buta) => buta switch
        {
            Api.Buta.Sync _ => new Buta.Sync { },
            Api.Buta.Triplist _ => new Buta.Triplist { },
            Api.Buta.Roomlist _ => new Buta.Roomlist { },
            Api.Buta.Dbox _ => new Buta.Dbox { },
            Api.Buta.Docr docr => new Buta.Docr { kdocr = docr.kdocr },
            Api.Buta.LaunchActivity la => new Buta.LaunchActivity { inspe = la.inspe.decode() },
            _ => throw new Exception($"unknown buta {buta.GetType().FullName}"),
        };
        public static Api.Buta encode(this Buta buta) => buta switch
        {
            Buta.Sync _ => new Api.Buta.Sync { },
            Buta.Triplist _ => new Api.Buta.Triplist { },
            Buta.Roomlist _ => new Api.Buta.Roomlist { },
            Buta.Dbox _ => new Api.Buta.Dbox { },
            Buta.Docr docr => new Api.Buta.Docr { kdocr = docr.kdocr },
            Buta.LaunchActivity la => new Api.Buta.LaunchActivity { inspe = la.inspe.encode() },
            _ => throw new Exception($"unknown buta {buta.GetType().FullName}"),
        };

        public static Api.Ptd encode(this Ptd ptd) => new Api.Ptd { x = ptd.x, y = ptd.y };
        public static Ptd decode(this Api.Ptd ptd) => new Ptd { x = ptd.x, y = ptd.y };

        public static Api.Szd encode(this Szd szd) => new Api.Szd{ dx = szd.dx, dy = szd.dy };
        public static Szd decode(this Api.Szd szd) => new Szd{ dx = szd.dx, dy = szd.dy };

        public static Api.Krund encode(this Krund krund) => krund switch
        {
            Krund.Fixed krundT => new Api.Krund.Fixed { dp = krundT.dp },
            Krund.Relative krundT => new Api.Krund.Relative { percent = krundT.percent },
            Krund.Unit krundT => new Api.Krund.Unit { u = krundT.u },
            _ => throw new Exception($"unknown krund {krund.GetType().FullName}"),
        };
        public static Krund decode(this Api.Krund krund) => krund switch
        {
            Api.Krund.Fixed krundT => new Krund.Fixed { dp = krundT.dp },
            Api.Krund.Relative krundT => new Krund.Relative { percent = krundT.percent },
            Api.Krund.Unit krundT => new Krund.Unit { u = krundT.u },
            _ => throw new Exception($"unknown krund {krund.GetType().FullName}"),
        };

        public static Api.St18 encode(this St18 st18) =>
            new Api.St18
            {
                stDefault = st18.stDefault,
                mpstByLocale = st18.mpstByLocale.ToDictionary(),
            };

        public static St18 decode(this Api.St18 st18) =>
            new St18
            {
                stDefault = st18.stDefault,
                mpstByLocale = st18.mpstByLocale.ToDictionary(),
            };

        public static Api.Busty encode(this Busty busty) =>
            new Api.Busty
            {
                ost18Text = busty.ost18Text?.encode(),
                okico = busty.okico,
                ocolBg = busty.ocolBg,
                ocolFg = busty.ocolFg,
                okrund = busty.okrund?.encode(),
            };
        public static Busty decode(this Api.Busty busty) =>
            new Busty
            {
                ost18Text = busty.ost18Text?.decode(),
                okico = busty.okico,
                ocolBg = busty.ocolBg,
                ocolFg = busty.ocolFg,
                okrund = busty.okrund?.decode(),
            };

        public static Api.Notd encode(this Notd notd) =>
            new Api.Notd
            {
                notid = notd.notid,
                ost18Message = notd.ost18Message?.encode(),
                ocBadge = notd.ocBadge,
            };
        public static Notd decode(this Api.Notd notd) =>
            new Notd
            {
                notid = notd.notid,
                ost18Message = notd.ost18Message?.decode(),
                ocBadge = notd.ocBadge,
            };

        public static Api.Mbut encode(this Mbut mbut) =>
            new Api.Mbut
            {
                buta = mbut.buta.encode(),
                ptd = mbut.ptd.encode(),
                szd = mbut.szd.encode(),
                busty = mbut.busty?.encode(),
                rgnotd = mbut.rgnotd.Select(notd => notd.encode()).ToArray(),
            };
        public static Mbut decode(this Api.Mbut mbut) =>
            new Mbut
            {
                buta = mbut.buta.decode(),
                ptd = mbut.ptd.decode(),
                szd = mbut.szd.decode(),
                busty = mbut.busty?.decode(),
                rgnotd = mbut.rgnotd.Select(notd => notd.decode()).ToArray(),
            };

        public static Api.Screne encode(this Scren scren) =>
            new Api.Screne
            {
                rgmbut = scren.rgmbut.Select(mbut => mbut.encode()).ToArray(),
            };
        public static Scren decode(this Api.Screne screne) =>
            new Scren
            {
                rgmbut = screne.rgmbut.Select(mbut => mbut.decode()).ToArray(),
            };

        public static Api.Wetag<Api.Screne> encode(this Wetag<Scren> wetagscren) =>
            wetagscren.encodeMap(scren => scren.encode());
        public static Wetag<Scren> decode(this Api.Wetag<Api.Screne> wetagscrene, string userxtid) =>
            wetagscrene.decodeMap(screne => screne
                .decode()
                .Copy(b => b.withUserxtid(userxtid))
            );

        public static Rover decode(this Api.Rovered rover) =>
            new Rover
            {
                wetagdturoom = rover.wetagdturoom.decodeMap(x => x),
                wetagdtupost = rover.wetagdtupost.decodeMap(x => x),
            };

        public static Usermeta decode(this Api.Usermeta usermeta) =>
            new Usermeta
            {
                ostEmployeeId = usermeta.ostEmployeeId,
                ostHaulerPlate = usermeta.ostHaulerPlate,
                ostTrailerPlate = usermeta.ostTrailerPlate,
                ostVoicePhone = usermeta.ostVoicePhone,
            };

        public static Boma decode(this Api.Bomaed boma) =>
            new Boma
            {
                userxtid = boma.userxtid,
                usern = boma.usern,
                usermeta = boma.usermeta.decode(),
                colBg = boma.colBg,
                owetagdtuRead = boma.owetagdtuRead?.decodeMap(x => x),
                owetagdtuSync = boma.owetagdtuSync?.decodeMap(x => x),
                fMuted = boma.fMuted,
            };

        public static Boma decode(this Api.Dubboma boma) =>
            new Boma
            {
                userxtid = boma.userxtid,
                usern = boma.usern,
                usermeta = boma.usermeta.decode(),
                colBg = boma.colBg,
                owetagdtuRead = boma.owetagdtuRead?.decodeMap(x => x),
                owetagdtuSync = boma.owetagdtuSync?.decodeMap(x => x),
                fMuted = boma.fMuted,
            };

        public static Bomath decode(this Api.Dubbomath bomath) =>
            new Bomath
            {
                userxtid = bomath.userxtid,
                usern = bomath.usern,
                usermeta = bomath.usermeta.decode(),
                colBg = bomath.colBg,
            };

        public static Roup decode(this Api.Dubroup roup) =>
            new Roup
            {
                oroce = roup.oroce?.Let(_ => new Roup.Roce { }),
                otise = roup.otise?.Let(tise => new Roup.Tise { ostTitle = tise.ostTitle, }),
                ousad = roup.ousad?.Let(usad => new Roup.Usad { rguserxtid = usad.rguserxtid }),
                ouski = roup.ouski?.Let(uski => new Roup.Uski { rguserxtid = uski.rguserxtid }),
                ousmu = roup.ousmu?.Let(usmu => new Roup.Usmu { rguserxtid = usmu.rguserxtid }),
                ousum = roup.ousum?.Let(usum => new Roup.Usum { rguserxtid = usum.rguserxtid }),
            };

        public static Payp decode(this Api.Dubpayp payp) => payp switch
        {
            Api.Dubpayp.Msg msg => new Payp.Msg { st = msg.st },
            Api.Dubpayp.Update update => new Payp.Update { roup = update.roup.decode() },
            _ => throw new Exception($"unknown dubpayp {payp.GetType().FullName}"),
        };

        public static Post decode(this Api.Dubpost post) =>
            new Post
            {
                etagpost = post.etagpost,
                postxtid = post.postxtid,
                userxtid = post.userxtid,
                dtu = post.dtu,
                payp = post.payp.decode(),
            };

        public static Api.Bomaeu encode(this Bomaeu bomaeu) =>
            new Api.Bomaeu
            {
                userxtid = bomaeu.userxtid,
                fMuted = bomaeu.fMuted,
            };

        public static Api.Roomeu encode(this Roomeu roomeu) =>
            new Api.Roomeu
            {
                ouxtid = roomeu.ouxtid,
                ostTitle = roomeu.ostTitle,
                rgboma = roomeu.rgbomaeu.Select(bomaeu => bomaeu.encode()).ToArray(),
            };
        public static Api.Wetag<Api.Roomeu> encode(this Wetag<Roomeu> wetagroomeu) =>
            wetagroomeu.encodeMap(roomeu => roomeu.encode());

        public static Room decode(this Api.Roomed room) =>
            new Room
            {
                roomxtid = room.roomxtid,
                ouxtid = room.ouxtid,
                rover = room.rovered.decode(),
                ostTitle = room.ostTitle,
                dtuCreate = room.dtuCreate,
                rgboma = room.rgboma.Select(boma => boma.decode()).ToArray(),
                rgbomath = new Bomath[0],
                rgpost = new Post[0],
            };

        public static Wetag<Room> decode(this Api.Wetag<Api.Roomed> wetagroomed) =>
            wetagroomed.decodeMap(roomed => roomed.decode());

        public static Wetag<Room> decode(this Api.Dubroom room) =>
            new Wetag<Room>
            {
                etag = room.rovered.wetagdturoom.etag,
                value = new Room
                {
                    roomxtid = room.roomxtid,
                    ouxtid = room.ouxtid,
                    rover = room.rovered.decode(),
                    ostTitle = room.ostTitle,
                    dtuCreate = room.dtuCreate,
                    rgboma = room.rgboma.Select(boma => boma.decode()).ToArray(),
                    rgbomath = room.rgbomath.Select(bomath => bomath.decode()).ToArray(),
                    rgpost = room.rgpost.Select(post => post.decode()).ToArray(),
                },
            };

        public static Roup decode(this Api.Roup roup) =>
            new Roup
            {
                oroce = roup.oroce?.Let(_ => new Roup.Roce { }),
                otise = roup.otise?.Let(tise => new Roup.Tise { ostTitle = tise.ostTitle }),
                ousad = roup.ousad?.Let(usad => new Roup.Usad { rguserxtid = usad.rguserxtid }),
                ouski = roup.ouski?.Let(uski => new Roup.Uski { rguserxtid = uski.rguserxtid }),
                ousmu = roup.ousmu?.Let(usmu => new Roup.Usmu { rguserxtid = usmu.rguserxtid }),
                ousum = roup.ousum?.Let(usum => new Roup.Usum { rguserxtid = usum.rguserxtid }),
            };

        public static Payp decode(this Api.Payp payp) => payp switch
        {
            Api.Payp.Msg msg => new Payp.Msg { st = msg.st },
            Api.Payp.Update update => new Payp.Update { roup = update.roup.decode() },
            _ => throw new Exception($"unknown dubpayp {payp.GetType().FullName}"),
        };

        public static Post decode(this Api.Posted posted) =>
            new Post
            {
                etagpost = posted.etag,
                postxtid = posted.postxtid,
                userxtid = posted.userxtid,
                dtu = posted.dtu,
                payp = posted.payp.decode(),
            };

        public static Api.Posteu encode(this Posteu posteu) =>
            new Api.Posteu
            {
                userxtid = posteu.userxtid,
                stMessage = posteu.stMessage,
            };

        public static Wetag<Post> decode(this Api.Wetag<Api.Posted> wetagposted) =>
            wetagposted.decodeMap(posted => posted.decode());

        public static Api.Wetag<Api.Posteu> encode(this Wetag<Posteu> wetagposteu) =>
            wetagposteu.encodeMap(posteu => posteu.encode());

        public static IEnumerable<Type> RgrtyInterface(this Type rty) =>
            rty.GetInterfaces().SelectMany(rtyT => EnU.Cons(rtyT).Concat(rtyT.RgrtyInterface()));

        public static IEnumerable<Type> RgrtyAncestorOrSelf(this Type rty)
        {
            while (rty != null)
            {
                yield return rty;
                rty = rty.BaseType;
            }
        }

        public static bool FImplements(this Type rty, Type rtyInterface) =>
            rty.Cons().Concat(rty.RgrtyInterface()).Any(rtyT => rtyT == rtyInterface || rtyT.IsGenericType && rtyT.GetGenericTypeDefinition() == rtyInterface);

        public static IEnumerable<FieldInfo> Rgrfi(this Type rty) =>
            rty.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        private class Eqc<T> : IEqualityComparer<T>
        {
            public bool Equals(T x, T y) => x.FEq(y);
            public int GetHashCode(T obj) => obj.Hc();

            public static readonly Eqc<T> I = new Eqc<T>();
        }

        public static bool FEq(this object left, object right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left == null || right == null) return false;
            var rty = left.GetType();
            if (rty != right.GetType()) return false;
            if (left is IEnumerable)
                return ((IEnumerable)left).Cast<object>().SequenceEqual(((IEnumerable)right).Cast<object>(), Eqc<object>.I);
            if (left is IBuildable)
                return rty.Rgrfi().All(rfi => rfi.GetValue(left).FEq(rfi.GetValue(right)));
            return Equals(left, right);
        }

        public static bool FNe(this object left, object right) => !left.FEq(right);

        public static int Hc(this object o)
        {
            if (o == null) return 1317597723;
            if (o is IEnumerable)
                return ((IEnumerable)o).Cast<object>().Aggregate(491628642, (hc, elem) => hc * -1521134295 + elem.Hc());
            if (o is IBuildable)
                return o.GetType().Rgrfi().Aggregate(491628642, (hc, rfi) => hc * -1521134295 + rfi.GetValue(o).Hc());
            return o.GetHashCode();
        }
    }

    public class MDP
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class KeyedAttribute : Attribute {}

    public interface Keyof<in T, out K>
    {
        K Key(T t);
    }

    public class KeyofWith<T, K>: Keyof<T, K>
    {
        private Func<T, K> dg;
        public KeyofWith(Func<T, K> dg)
        {
            this.dg = dg;
        }

        K Keyof<T,K>.Key(T t) => dg(t);
    }

    public class MD: IKeyed<object>
    {
        public object Key { get; set; }
        public object Value { get; set; }
        public MD OmdParent { get; set; }
        public Type Rty { get; set; }

        public IEnumerable<MD> RgmdAncestor()
        {
            var md = this;
            while (md != null)
            {
                yield return md;
                md = md.OmdParent;
            }
        }

        public string TstoKey => Key switch
        {
            FieldInfo rfi => rfi.Name,
            string st => $"[{st}]",
            MDU.KScalar _ => "<scalar>",
            _ => throw new InvalidOperationException(),
        };

        public Keyof<object, string> Okeyof
        {
            get
            {
                if (!Rty.IsArray) return null;

                if (Key is FieldInfo rfi && rfi.GetCustomAttributes(typeof(KeyedAttribute), true).FirstOrDefault() != null) return 
                    new KeyofWith<string, string>(st => st).CastIn<string, string, object>();

                if (Rty.GetElementType().IsEnum) return
                    new KeyofWith<object, string>(o => o.ToString());

                if (Rty.GetElementType().FImplements(typeof(IKeyed<string>))) return
                    new KeyofWith<IKeyed<string>, string>(keyed => keyed.Key).CastIn<IKeyed<string>, string, object>();

                return null;
            }
        }

        public string TstoMd { get => RgmdAncestor().Reverse().Select(md => $"{md.TstoKey}").StJoin("."); }

        public string ToString(string otsto) => $"{Rty.Name} {TstoMd}: {otsto?.Let(tsto => $"{tsto} ") ?? ""}{Value}";
        public override string ToString() => ToString(null);

        public bool FKeyedArray => Rty.IsArray && Okeyof != null;
        public bool FBuildable => Rty.RgrtyInterface().Any(rtyT => rtyT.FImplements(typeof(IBuildable)));

        public IEnumerable<MD> Rgmd
        {
            get
            {
                var okeyof = Okeyof;
                if (okeyof != null) return (Value.AsObjectArray() ?? new object[0]).Select(eT => new MD() { Key = okeyof.Key(eT), Value = eT, Rty = Rty.GetElementType(), OmdParent = this });
                if (FBuildable) return Value.AsBuildable()?.Let(v => Rty.Rgrfi().Select(rfi => new MD() { Key = rfi, Value = rfi.GetValue(v), Rty = rfi.FieldType, OmdParent = this })) ?? new MD[0];
                return Value?.Let(v => new MD[] { Rty.Scalar(v, this) }) ?? new MD[0]; // this is slightly weird
            }
        }

        public string TstoValue(object ov)
        {
            if (ov == null) return "<nil>";
            if (FKeyedArray) return $"keyof-array({ov.AsObjectArray().Count()}";
            if (FBuildable) return $"{Rty.Name}(...)";
            if (Rty.IsArray) return $"array({ov.AsObjectArray().Count()})";
            return ov.ToString();
        }
    }

    public static class MDU 
    {
        public static MD Scalar(this Type rty, object value, MD omdParent = null) => new MD { Key = KScalar.I, Rty = rty, Value = value, OmdParent = omdParent };
        public class KScalar {
            public static readonly KScalar I = new KScalar();
        };

        public static Keyof<T1, K> CastIn<T, K, T1>(this Keyof<T, K> keyof) => new KeyofWith<T1, K>(t => keyof.Key((T)(object)t));

        public static IEnumerable<object> AsObjectArray(this object v) => ((IEnumerable)v).Cast<object>();
        public static IEnumerable<IKeyed<string>> AsKeyedArray(this object v) => (IEnumerable<IKeyed<string>>)v;
        public static IBuildable AsBuildable(this object v) => (IBuildable)v;

        public static IEnumerable<MR<MD, MD, object>> Decompose(MD mdo, MD mdt) =>
            mdo.Rgmd.MergeQQQ<MD, MD, object>(mdt.Rgmd).Where(mr => (mr.ol?.Value).FNe(mr.or?.Value));

        public static Action<string> Log(L l, Severity severity) => (st) => l.Write(severity, st);

        // apply changes from `o` -> `t` onto `s`
        public static T Update<T>(T s, T o, T t, Action<string> dgLog) => (T)Update(
            dgLog,
            typeof(T).Scalar(s),
            typeof(T).Scalar(o),
            typeof(T).Scalar(t)
        );

        public static MD[] UpdateMDAndLog(Action<string> dgLog, MR<MD, MR<MD, MD, object>, object> mrsot) =>
            UpdateMD(dgLog, mrsot)
            .Also(rgmdR => {
                var md = (mrsot.or?.or ?? mrsot.or?.ol ?? mrsot.ol);
                var omdr = rgmdR.FirstOrDefault();
                var rty = md.Rty;
                var ovl = mrsot.ol?.Value;
                var ovr = omdr?.Value;
                if ((mrsot.ol?.Value).FEq(omdr?.Value)) return;
                if ((md.FKeyedArray || md.FBuildable) && ((ovl == null) == (ovr == null))) return;
                dgLog($"{md.TstoMd}: {rty.Name} = {md.TstoValue(mrsot.ol?.Value)} -> {md.TstoValue(omdr?.Value)}");
            });

        public static MD[] UpdateMD(Action<string> dgLog, MR<MD, MR<MD, MD, object>, object> mrsot)
        {
            if (mrsot.kmr == Kmr.L) return new MD[] { mrsot.ol };               // o==t => no update
            if (mrsot.or.kmr == Kmr.R) return new MD[] { mrsot.or.or };         // t created => replace o with t
            if (mrsot.or.kmr == Kmr.L) return new MD[0];                        // t removed => delete o
            Debug.Assert(mrsot.or.kmr == Kmr.B);
            if (mrsot.kmr == Kmr.R) return new MD[] { mrsot.or.or };            // o missing, t updated => re-create t
            Debug.Assert(mrsot.kmr == Kmr.B);
            Debug.Assert(mrsot.ol.Rty == mrsot.or.ol.Rty);
                                                                                // t updated => merge t onto o
            var vMerged = mrsot.ol.Let(md => md.FBuildable || md.FKeyedArray) ? Update(dgLog, mrsot.ol, mrsot.or.ol, mrsot.or.or) : mrsot.or.or.Value;

            return new MD[] { new MD() { Key = mrsot.Key, Rty = mrsot.ol.Rty, Value = vMerged } };
        }

        private static object Update(Action<string> dgLog, MD mds, MD mdo, MD mdt)
        {
            var rgmds = mds.Rgmd;
            var rgmrot = Decompose(mdo, mdt);
            var rgmrsot = rgmds.MergeQQQ<MD, MR<MD, MD, object>, object>(rgmrot);
            var rgmdn = rgmrsot.SelectMany(mrsot => UpdateMDAndLog(dgLog, mrsot)).ToArray();

            if (mds.FKeyedArray) return rgmdn.Select(md => md.Value).ToArrayWithRtyElement(mds.Rty.GetElementType());
            if (mds.FBuildable)
            {
                if (rgmdn.Length == 0) return null;
                var n = (mds.Value.AsBuildable()?.Clone() ?? Activator.CreateInstance(mds.Rty)).AsBuildable();
                foreach (var mdn in rgmdn)
                    ((FieldInfo)mdn.Key).SetValue(n, mdn.Value);
                return n;
            }

            return rgmdn.FirstOrDefault()?.Value;
        }
    }

    public interface IBuildable {
        object Clone();
    }

    public abstract class Buildable<T,B>: IBuildable, IEquatable<T> where T: Buildable<T,B> where B: Buildable<T,B>.Builder, new()
    {
        public abstract class Builder
        {
            public T t;
            public T build() => t;
        }

        public T Copy(Func<B,B> dgBuild) => dgBuild(new B { t = (T)MemberwiseClone() }).build();
        public bool Equals(T other) => this.FEq(other);
        public override bool Equals(object obj) => this.FEq(obj);
        public override int GetHashCode() => this.Hc();
        object IBuildable.Clone() => (T)MemberwiseClone();
    }

    public class Docrref : Buildable<Docrref, Docrref.B>
    {
        public string docrid;
        public string tripxtid;
        
        public class B : Builder
        {
            public B withDocrid(string docrid) => this.Also(b => t.docrid = docrid);
            public B withTripxtid(string tripxtid) => this.Also(b => t.tripxtid = tripxtid);
        }
    }

    public class Docr : Buildable<Docr, Docr.B>, IKeyed<string>
    {
        public string docrid;
        public DateTime dtuRequest;
        public Api.Kdocr kdocr;
        public string ostShortName;
        public string ostDesc;
        public bool fRequired;
        public bool? ofDeleted;

        string IKeyed<string>.Key => docrid;

        public class B : Builder
        {
            public B withDocrid(string docrid) => this.Also(b => t.docrid = docrid);
            public B withDtuRequest(DateTime dtuRequest) => this.Also(b => t.dtuRequest = dtuRequest);
            public B withKdocr(Api.Kdocr kdocr) => this.Also(b => t.kdocr = kdocr);
            public B withOstShortName(string ostShortName) => this.Also(b => t.ostShortName = ostShortName);
            public B withOstDesc(string ostDesc) => this.Also(b => t.ostDesc = ostDesc);
            public B withFRequired(bool fRequired) => this.Also(b => t.fRequired = fRequired);
            public B withOfDeleted(bool? ofDeleted) => this.Also(b => t.ofDeleted = ofDeleted);
        }
    }

    public class Stan : Buildable<Stan, Stan.B>, IKeyed<string>
    {
        public string stanxtid;
        public Api.Kstan kstan;
        public DateTime? odtu;
        public string ostAddress;
        public string ostNotes;
        public double? okmDistance;
        public Cufis cufis;
        public string ostSortBy;
        public bool? ofDeleted;

        string IKeyed<string>.Key => stanxtid;

        public class B : Builder
        {
            public B withStanxtid(string stanxtid) => this.Also(b => t.stanxtid = stanxtid);
            public B withKstan(Api.Kstan kstan) => this.Also(b => t.kstan = kstan);
            public B withOdtu(DateTime? odtu) => this.Also(b => t.odtu = odtu);
            public B withOstAddress(string ostAddress) => this.Also(b => t.ostAddress = ostAddress);
            public B withOstNotes(string ostNotes) => this.Also(b => t.ostNotes = ostNotes);
            public B withOkmDistance(double? okmDistance) => this.Also(b => t.okmDistance = okmDistance);
            public B withCufis(Cufis cufis) => this.Also(b => t.cufis = cufis);
            public B withOstSortBy(string ostSortBy) => this.Also(b => t.ostSortBy = ostSortBy);
            public B withOfDeleted(bool? ofDeleted) => this.Also(b => t.ofDeleted = ofDeleted);
        }
    }

    public class Tripmeta : Buildable<Tripmeta, Tripmeta.B>
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

        public class B : Builder
        {
            public B withOstTripCode(string ostTripCode) => this.Also(b => t.ostTripCode = ostTripCode);
            public B withOstOrderId(string ostOrderId) => this.Also(b => t.ostOrderId = ostOrderId);
            public B withOstAddress(string ostAddress) => this.Also(b => t.ostAddress = ostAddress);
            public B withOstCustomer(string ostCustomer) => this.Also(b => t.ostCustomer = ostCustomer);
            public B withOdtuCreate(DateTime? odtuCreate) => this.Also(b => t.odtuCreate = odtuCreate);
            public B withOstInstructions(string ostInstructions) => this.Also(b => t.ostInstructions = ostInstructions);
            public B withOstNotes(string ostNotes) => this.Also(b => t.ostNotes = ostNotes);
            public B withOstHaulerPlate(string ostHaulerPlate) => this.Also(b => t.ostHaulerPlate = ostHaulerPlate);
            public B withOstTrailerPlate(string ostTrailerPlate) => this.Also(b => t.ostTrailerPlate = ostTrailerPlate);
        }
    }

    public class Dosumeta : Buildable<Dosumeta, Dosumeta.B>
    {
        public string ostOrderid;
        public string ostPlate;
        public string ostNotes;

        public class B : Builder
        {
            public B withOstOrderid(string ostOrderid) => this.Also(b => t.ostOrderid = ostOrderid);
            public B withOstPlate(string ostPlate) => this.Also(b => t.ostPlate = ostPlate);
            public B withOstNotes(string ostNotes) => this.Also(b => t.ostNotes = ostNotes);
        }
    }

    public class Redoim : Buildable<Redoim, Redoim.B>
    {
        public string userxtid;
        public DateTime dtu;
        public Api.Kredosu kredosu;

        public override string ToString() => $"redoim {kredosu} on {dtu.ToLocalTime():yyyy-MM-dd HH:mm:ss.fff} by {userxtid}";

        public class B : Builder
        {
            public B withUserxtid(string userxtid) => this.Also(b => t.userxtid = userxtid);
            public B withDtu(DateTime dtu) => this.Also(b => t.dtu = dtu);
            public B withKredosu(Api.Kredosu kredosu) => this.Also(b => t.kredosu = kredosu);
        }
    }

    public class Doed : Buildable<Doed, Doed.B>
    {
        public string doedid;
        public DateTime dtu;
        public string userxtid;

        public override string ToString() => $"doed {doedid} on {dtu.ToLocalTime():yyyy-MM-dd HH:mm:ss.fff} by {userxtid}";

        public class B : Builder
        {
            public B withDoedid(string doedid) => this.Also(b => t.doedid = doedid);
            public B withDtu(DateTime dtu) => this.Also(b => t.dtu = dtu);
            public B withUserxtid(string userxtid) => this.Also(b => t.userxtid = userxtid);
        }
    }

    public class Dosuimg : Buildable<Dosuimg, Dosuimg.B>, IKeyed<string>
    {
        public string imgid;
        public Redoim oredoim;
        public Doed odoed;

        string IKeyed<string>.Key { get => $"{imgid}{odoed?.Let(doed => $"_{doed.doedid}") ?? ""}"; }
        public override string ToString() => $"dosuimg {imgid} review {oredoim?.ToString() ?? "pending"} edits {odoed?.ToString() ?? "none"}";

        public class B : Builder
        {
            public B withImgid(string imgid) => this.Also(b => t.imgid = imgid);
            public B withOredoim(Redoim oredoim) => this.Also(b => t.oredoim = oredoim);
            public B withOdoed(Doed odoed) => this.Also(b => t.odoed = odoed);
        }
    }

    public class Dosu : Buildable<Dosu, Dosu.B>, IKeyed<string>
    {
        public string dosuxtid;
        public string userxtid;
        public string copid;
        public string ouid;
        public Api.Kdocr kdocr;
        public Docrref odocr;
        public DateTime dtuSubmit;
        public DateTime dtuUpload;
        public string loc;
        public Dosumeta dosumeta;
        public Dosuimg[] rgimg;

        string IKeyed<string>.Key => dosuxtid;

        public override string ToString() =>
            $"dosu {kdocr} {copid}:{ouid}:{dosuxtid} submitted-on {dtuSubmit.ToLocalTime():yyyy-MM-dd HH:mm:ss.fff} by {userxtid}{Maybe.OfNullable(odocr).Map(docr => $" for docr {docr.tripxtid}:{docr.docrid}").OrElse("")} with {rgimg.Length} images";


        public class B : Builder
        {
            public B withDosuxtid(string dosuxtid) => this.Also(b => t.dosuxtid = dosuxtid);
            public B withUserxtid(string userxtid) => this.Also(b => t.userxtid = userxtid);
            public B withCopid(string copid) => this.Also(b => t.copid = copid);
            public B withOuid(string ouid) => this.Also(b => t.ouid = ouid);
            public B withKdocr(Api.Kdocr kdocr) => this.Also(b => t.kdocr = kdocr);
            public B withOdocr(Docrref odocr) => this.Also(b => t.odocr = odocr);
            public B withDtuSubmit(DateTime dtuSubmit) => this.Also(b => t.dtuSubmit = dtuSubmit);
            public B withDtuUpload(DateTime dtuUpload) => this.Also(b => t.dtuUpload = dtuUpload);
            public B withLoc(string loc) => this.Also(b => t.loc = loc);
            public B withDosumeta(Dosumeta dosumeta) => this.Also(b => t.dosumeta = dosumeta);
            public B withRgimg(Dosuimg[] rgimg) => this.Also(b => t.rgimg = rgimg);
        }
    }

    public class Ulic : Buildable<Ulic, Ulic.B>, IKeyed<string>
    {
        public string kid;
        public string ostDeviceModel;
        public string ostDeviceImei;
        public string ostPin;
        public string ostPhone;
        public string ostImsi;
        public string ostSubscription;

        string IKeyed<string>.Key => kid;

        public override string ToString() =>
            $"lic {kid}";

        public class B : Builder
        {
            public B withKid(string kid) => this.Also(b => t.kid = kid);
            public B withOstDeviceModel(string ostDeviceModel) => this.Also(b => t.ostDeviceModel = ostDeviceModel);
            public B withOstDeviceImei(string ostDeviceImei) => this.Also(b => t.ostDeviceImei = ostDeviceImei);
            public B withOstPin(string ostPin) => this.Also(b => t.ostPin = ostPin);
            public B withOstPhone(string ostPhone) => this.Also(b => t.ostPhone = ostPhone);
            public B withOstImsi(string ostImsi) => this.Also(b => t.ostImsi = ostImsi);
            public B withOstSubscription(string ostSubscription) => this.Also(b => t.ostSubscription = ostSubscription);
        }
    }

    public class Usermeta : Buildable<Usermeta, Usermeta.B>
    {
        public string ostEmployeeId;
        public string ostVoicePhone;
        public string ostHaulerPlate;
        public string ostTrailerPlate;

        public override string ToString() =>
            $"usermeta employee-id {ostEmployeeId} voice-phone {ostVoicePhone} hauler-plate {ostHaulerPlate} trailer-plate {ostTrailerPlate}";

        public class B : Builder
        {
            public B withOstEmployeeId(string ostEmployeeId) => this.Also(b => t.ostEmployeeId = ostEmployeeId);
            public B withOstVoicePhone(string ostVoicePhone) => this.Also(b => t.ostVoicePhone = ostVoicePhone);
            public B withOstHaulerPlate(string ostHaulerPlate) => this.Also(b => t.ostHaulerPlate = ostHaulerPlate);
            public B withOstTrailerPlate(string ostTrailerPlate) => this.Also(b => t.ostTrailerPlate = ostTrailerPlate);
        }
    }

    public class Dbox: Buildable<Dbox, Dbox.B>
    {
        public string[] rguserxtidFollow;
        public string oshrn;

        public override string ToString() =>
            $"following {rguserxtidFollow} share-name {oshrn}";

        public class B : Builder
        {
            public B withRguserxtidFollow(string[] rguserxtidFollow) => this.Also(b => t.rguserxtidFollow = rguserxtidFollow);
            public B withOshrn(string oshrn) => this.Also(b => t.oshrn = oshrn);
        }
    }

    public class User : Buildable<User, User.B>, IKeyed<string>
    {
        public string userxtid;
        public string copid;
        public string ouid;
        public string usern;
        public Ulic[] rgulic;
        public Usermeta usermeta;
        public Dbox dbox;
        public Api.Krole[] rgkrole;
        public string[] rgtripxtid;

        string IKeyed<string>.Key => userxtid;

        public override string ToString() =>
            $"user {copid}:{ouid}:{userxtid} name {usern} devices {rgulic.StJoin()} roles {rgkrole.StJoin()} dbox ${dbox} trips {rgtripxtid.StJoin()}";

        public class B : Builder
        {
            public B withUserxtid(string userxtid) => this.Also(b => t.userxtid = userxtid);
            public B withCopid(string copid) => this.Also(b => t.copid = copid);
            public B withOuid(string ouid) => this.Also(b => t.ouid = ouid);
            public B withUsern(string usern) => this.Also(b => t.usern = usern);
            public B withRgulic(Ulic[] rgulic) => this.Also(b => t.rgulic = rgulic);
            public B withUsermeta(Usermeta usermeta) => this.Also(b => t.usermeta = usermeta);
            public B withDbox(Dbox dbox) => this.Also(b => t.dbox = dbox);
            public B withRgkrole(Api.Krole[] rgkrole) => this.Also(b => t.rgkrole = rgkrole);
            public B withRgtripxtid(string[] rgtripxtid) => this.Also(b => t.rgtripxtid = rgtripxtid);
        }
    }

    public class Wetag<T> : Buildable<Wetag<T>, Wetag<T>.B>
    {
        public string etag;
        public T value;

        public override string ToString() => $"{typeof(T).Name}:{Maybe.OfNullable(value as IKeyed<string>).Map(keyed => keyed.Key).OrElse("<unknown-key>")}:{Maybe.OfNullable(etag).OrElse("<none>")}";
        public Wetag<U> Map<U>(Func<T, U> dg) => new Wetag<U> { etag = etag, value = dg(value) };

        public class B : Builder
        {
            public B withEtag(string etag) => this.Also(b => t.etag = etag);
            public B withValue(T value) => this.Also(b => t.value = value);
        }
    }

    public class Rut : Buildable<Rut, Rut.B>, IKeyed<string>
    {
        public string rutid;
        public string fpat;
        public long cb;
        public DateTime dtuUpload;

        string IKeyed<string>.Key => rutid;
        public override string ToString() => $"rut {rutid} path '{fpat}' uploaded-on {dtuUpload.ToLocalTime():yyyy-MM-dd HH:mm:ss.fff} size {cb}B";

        public class B : Builder
        {
            public B withRutid(string rutid) => this.Also(b => t.rutid = rutid);
            public B withFpat(string fpat) => this.Also(b => t.fpat = fpat);
            public B withCb(long cb) => this.Also(b => t.cb = cb);
            public B withDtuUpload(DateTime dtuUpload) => this.Also(b => t.dtuUpload = dtuUpload);
        }
    }

    public class Ruts : Buildable<Ruts, Ruts.B>
    {
        public Rut[] rgrut;

        public override string ToString() => $"ruts with {rgrut.Length} files";

        public class B : Builder
        {
            public B withRgrut(Rut[] rgrut) => this.Also(b => t.rgrut = rgrut);
        }
    }

    public class Cufi : Buildable<Cufi, Cufi.B>, IKeyed<string>
    {
        public string id;
        public string n;
        public string v;

        string IKeyed<string>.Key => id;
        public override string ToString() => $"cufi {id}: {n} = {v}";

        public class B : Builder
        {
            public B withId(string id) => this.Also(b => t.id = id);
            public B withN(string n) => this.Also(b => t.n = n);
            public B withV(string v) => this.Also(b => t.v = v);
        }
    }

    public class Cufis: Buildable<Cufis, Cufis.B>
    {
        public Cufi[] rgcufi;

        public override string ToString() => $"cufis {rgcufi.Select(cufi => cufi.ToString()).StJoin(", ")}";

        public class B : Builder
        {
            public B withRgcufi(Cufi[] rgcufi) => this.Also(b => t.rgcufi = rgcufi);
        }
    }

    public class Trip : Buildable<Trip, Trip.B>, IKeyed<string>
    {
        public string tripxtid;
        public string ouid;
        public Api.Ktroc ktroc;
        public string ostSortBy;
        public DateTime dtuCreate;
        public DateTime? odtuMfc;
        public Tripmeta tripmeta;
        public string ouserxtidDriver;
        public Docr[] rgdocr;
        public Stan[] rgstan;
        public Dosu[] rgdosu;
        public Ruts ruts;
        public Cufis cufis;

        string IKeyed<string>.Key => tripxtid;

        public class B : Builder
        {
            public B withTripxtid(string tripxtid) => this.Also(b => t.tripxtid = tripxtid);
            public B withOuid(string ouid) => this.Also(b => t.ouid = ouid);
            public B withKtroc(Api.Ktroc ktroc) => this.Also(b => t.ktroc = ktroc);
            public B withOstSortBy(string ostSortBy) => this.Also(b => t.ostSortBy = ostSortBy);
            public B withDtuCreate(DateTime dtuCreate) => this.Also(b => t.dtuCreate = dtuCreate);
            public B withOdtuMfc(DateTime? odtuMfc) => this.Also(b => t.odtuMfc = odtuMfc);
            public B withTripmeta(Tripmeta tripmeta) => this.Also(b => t.tripmeta = tripmeta);
            public B withOuserxtidDriver(string ouserxtidDriver) => this.Also(b => t.ouserxtidDriver = ouserxtidDriver);
            public B withRgdocr(Docr[] rgdocr) => this.Also(b => t.rgdocr = rgdocr);
            public B withRgstan(Stan[] rgstan) => this.Also(b => t.rgstan = rgstan);
            public B withRgdosu(Dosu[] rgdosu) => this.Also(b => t.rgdosu = rgdosu);
            public B withRuts(Ruts ruts) => this.Also(b => t.ruts = ruts);
            public B withCufis(Cufis cufis) => this.Also(b => t.cufis = cufis);
        }
    }

    public class Compn : Buildable<Compn, Compn.B>
    {
        public string packagen;
        public string classn;
        public class B : Builder {}
    }

    public abstract class Ev: Buildable<Ev, Ev.B>
    {
        public class B : Builder { }

        public class Byte : Ev { public byte v; }
        public class Short : Ev { public short v; }
        public class Int : Ev { public int v; }
        public class Long : Ev { public long v; }
        public class Float : Ev { public float v; }
        public class Double : Ev { public double v; }
        public class Boolean : Ev { public bool v; }
        public class String : Ev { public string v; }
        public class Char : Ev { public char v; }
        public class ByteArray : Ev { public byte[] v; }
        public class ShortArray : Ev { public short[] v; }
        public class IntArray : Ev { public int[] v; }
        public class LongArray : Ev { public long[] v; }
        public class FloatArray : Ev { public float[] v; }
        public class DoubleArray : Ev { public double[] v; }
        public class BooleanArray : Ev { public bool[] v; }
        public class StringArray : Ev { public string[] v; }
        public class CharArray : Ev { public char[] v; }
        public class IntegerArrayList : Ev { public int[] v; }
        public class StringArrayList : Ev { public string[] v; }
        public class Json : Ev { public object v; }
    }

    public class Inspe: Buildable<Inspe, Inspe.B> 
    {
        public string oaction;
        public string[] orgcat;
        public Compn ocompn;
        public string odata;
        public Dictionary<string, Ev> ompextra;
        public class B : Builder { }
    }

    public abstract class Buta: Buildable<Buta, Buta.B> {
        public class B : Builder {}

        public class Sync: Buta{}
        public class Triplist: Buta {}
        public class Roomlist: Buta {}
        public class Dbox: Buta {}
        public class Docr : Buta
        {
            public Api.Kdocr kdocr;
        }

        public class LaunchActivity : Buta 
        {
            public Inspe inspe;
        }
    }

    public class Ptd: Buildable<Ptd, Ptd.B> {
        public float x;
        public float y;

        public class B : Builder {}
    }

    public class Szd: Buildable<Szd, Szd.B> {
        public float dx;
        public float dy;
        public class B : Builder {}
    }

    public abstract class Krund: Buildable<Krund, Krund.B>
    {
        public class B : Builder { }

        public class Fixed: Krund
        {
            public float dp;
        }

        public class Relative: Krund
        {
            public int percent;
        }

        public class Unit: Krund
        {
            public float u;
        }
    }

    public class St18: Buildable<St18, St18.B>
    {
        public string stDefault;
        public Dictionary<string, string> mpstByLocale;
        public class B : Builder {}
    }

    public class Busty: Buildable<Busty, Busty.B> {
        public St18 ost18Text;
        public string okico;
        public string ocolBg;
        public string ocolFg;
        public Krund okrund;
        public class B : Builder {}
    }

    public class Notd: Buildable<Notd, Notd.B>, IKeyed<string>
    {
        public string notid;
        public St18 ost18Message;
        public int? ocBadge;
        string IKeyed<string>.Key => notid;
        public class B: Builder {}
    }

    public class Mbut: Buildable<Mbut, Mbut.B>
    {
        public Buta buta;
        public Ptd ptd;
        public Szd szd;
        public Busty busty;
        public Notd[] rgnotd;
        public class B : Builder {}
    }

    public class Scren: Buildable<Scren, Scren.B>, IKeyed<string>
    {
        public string userxtid;
        public Mbut[] rgmbut;
        string IKeyed<string>.Key => userxtid;
        public class B : Builder {
            public B withUserxtid(string userxtid) => this.Also(b => t.userxtid = userxtid);
        }
    }

    public class Rover: Buildable<Rover, Rover.B>
    {
        public Wetag<DateTime> wetagdturoom;
        public Wetag<DateTime> wetagdtupost;

        public class B: Builder
        {
            public B withWetagdturoom(Wetag<DateTime> wetagdturoom) => this.Also(b => t.wetagdturoom = wetagdturoom);
            public B withWetagdtupost(Wetag<DateTime> wetagdtupost) => this.Also(b => t.wetagdtupost = wetagdtupost);
        }
    }

    public class Boma: Buildable<Boma, Boma.B>, IKeyed<string>
    {
        public string userxtid;
        public string usern;
        public Usermeta usermeta;
        public string colBg;
        public Wetag<DateTime> owetagdtuSync;
        public Wetag<DateTime> owetagdtuRead;
        public bool fMuted;

        string IKeyed<string>.Key => userxtid;

        public class B : Builder
        {
            public B withUserxtid(string userxtid) => this.Also(b => t.userxtid = userxtid);
            public B withUsern(string usern) => this.Also(b => t.usern = usern);
            public B withUsermeta(Usermeta usermeta) => this.Also(b => t.usermeta = usermeta);
            public B withColBg(string colBg) => this.Also(b => t.colBg = colBg);
            public B withOwetagdtuSync(Wetag<DateTime> owetagdtuSync) => this.Also(b => t.owetagdtuSync = owetagdtuSync);
            public B withOwetagdtuRead(Wetag<DateTime> owetagdtuRead) => this.Also(b => t.owetagdtuRead = owetagdtuRead);
            public B withFMuted(bool fMuted) => this.Also(b => t.fMuted = fMuted);
        }
    }

    public class Bomath: Buildable<Bomath, Bomath.B>, IKeyed<string>
    {
        public string userxtid;
        public string usern;
        public Usermeta usermeta;
        public string colBg;

        string IKeyed<string>.Key => userxtid;

        public class B : Builder
        {
            public B withUserxtid(string userxtid) => this.Also(b => t.userxtid = userxtid);
            public B withUsern(string usern) => this.Also(b => t.usern = usern);
            public B withUsermeta(Usermeta usermeta) => this.Also(b => t.usermeta = usermeta);
            public B withColBg(string colBg) => this.Also(b => t.colBg = colBg);
        }
    }

    public class Roup: Buildable<Roup, Roup.B>
    {
        public Roce oroce;
        public Tise otise;
        public Usad ousad;
        public Uski ouski;
        public Usmu ousmu;
        public Usum ousum;

        public class B : Builder { }

        public class Roce { }
        public class Tise { public string ostTitle; }
        public class Usad { public string[] rguserxtid; }
        public class Uski { public string[] rguserxtid; }
        public class Usmu { public string[] rguserxtid; }
        public class Usum { public string[] rguserxtid; }
    }

    public abstract class Payp: Buildable<Payp, Payp.B>
    {
        public class B : Builder { }

        public class Update : Payp { public Roup roup; }
        public class Msg : Payp { public string st; }
    }

    public class Post: Buildable<Post, Post.B>, IKeyed<string>
    {
        public string postxtid;
        public string etagpost;
        public string userxtid;
        public DateTime dtu;
        public Payp payp;

        string IKeyed<string>.Key => etagpost;

        public class B : Builder
        {
            public B withPostxtid(string postxtid) => this.Also(b => t.postxtid = postxtid);
            public B withEtagpost(string etagpost) => this.Also(b => t.etagpost = etagpost);
            public B withUserxtid(string userxtid) => this.Also(b => t.userxtid = userxtid);
            public B withDtu(DateTime dtu) => this.Also(b => t.dtu = dtu);
            public B withPayp(Payp payp) => this.Also(b => t.payp = payp);
        }
    }

    public class Posteu: Buildable<Posteu, Posteu.B>
    {
        public string userxtid;
        public string stMessage;

        public class B: Builder
        {
            public B withUserxtid(string userxtid) => this.Also(b => t.userxtid = userxtid);
            public B withStMessage(string stMessage) => this.Also(b => t.stMessage = stMessage);
        }
    }

    public class Room: Buildable<Room, Room.B>, IKeyed<string>
    {
        public string roomxtid;
        public string ouxtid;
        public Rover rover;
        public DateTime dtuCreate;
        public string ostTitle;
        public Boma[] rgboma;
        public Bomath[] rgbomath;
        public Post[] rgpost;

        public string etagroom => rover.wetagdturoom.etag;
        public DateTime dtuUpdate => rover.wetagdturoom.value;
        public string etagpost => rover.wetagdtupost.etag;
        public DateTime dtuLastPost => rover.wetagdtupost.value;


        string IKeyed<string>.Key => roomxtid;
        public override string ToString() => $"room {roomxtid} etag {etagroom} created-on {dtuCreate} updated-on {dtuUpdate} with {etagpost} posts last-post-on {dtuLastPost} title {ostTitle?.Let(stTitle => $"'{stTitle}'") ?? "<none>"} with {rgboma.Length} members";

        public Roomeu ToRoomeu() => new Roomeu
        {
            ouxtid = ouxtid,
            ostTitle = ostTitle,
            rgbomaeu = rgboma
                .Select(boma => new Bomaeu
                    {
                        userxtid = boma.userxtid,
                        fMuted = boma.fMuted,
                    }
                )
                .ToArray(),
        };

        public class B: Builder
        {
            public B withRoomxtid(string roomxtid) => this.Also(b => t.roomxtid = roomxtid);
            public B withRover(Rover rover) => this.Also(b => t.rover = rover);
            public B withDtuCreate(DateTime dtuCreate) => this.Also(b => t.dtuCreate = dtuCreate);
            public B withOstTitle(string ostTitle) => this.Also(b => t.ostTitle = ostTitle);
            public B withRgboma(Boma[] rgboma) => this.Also(b => t.rgboma = rgboma);
            public B withRgbomath(Bomath[] rgbomath) => this.Also(b => t.rgbomath = rgbomath);
            public B withRgpost(Post[] rgpost) => this.Also(b => t.rgpost = rgpost);
        }
    }

    public class Bomaeu: Buildable<Bomaeu, Bomaeu.B>, IKeyed<string>
    {
        public string userxtid;
        public bool fMuted;
        string IKeyed<string>.Key => userxtid;

        public class B: Builder
        {
            public B withUserxtid(string userxtid) => this.Also(b => t.userxtid = userxtid);
            public B withFMuted(bool fMuted) => this.Also(b => t.fMuted = fMuted);
        }
    }

    public class Roomeu: Buildable<Roomeu, Roomeu.B>
    {
        public string ouxtid;
        public string ostTitle;
        public Bomaeu[] rgbomaeu;

        public class B: Builder
        {
            public B withOuxtid(string ouxtid) => this.Also(b => t.ouxtid = ouxtid);
            public B withOstTitle(string ostTitle) => this.Also(b => t.ostTitle = ostTitle);
            public B withRgbomaeu(Bomaeu[] rgbomaeu) => this.Also(b => t.rgbomaeu = rgbomaeu);
        }
    }
}