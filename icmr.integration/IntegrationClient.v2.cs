using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Icmr.Integration.v2
{
    public class Upload
    {
        public string UploadId;
        public string UserName;
        public string CompanyId;
        public DateTime UtcSubmitted;
        public DateTime UtcUploaded;
        public string Kind;
        public string SubmitLocation;
        public Fields Fields;
        public Image[] Images;

        internal static Upload From(IntegrationClient.Upl upl) => new Upload
        {
            UploadId = upl.gidupl,
            UserName = upl.usern,
            CompanyId = upl.cid,
            UtcSubmitted = upl.dtuSubmit,
            UtcUploaded = upl.dtuUpload,
            Kind = upl.kupl,
            SubmitLocation = upl.loc,
            Fields = Fields.From(upl.fld),
            Images = upl.rgimg.Select(Image.From).ToArray()
        };
}

    public class Image
    {
        public string ImageId;
        public string TemporaryImageUrl;
        public DateTime DtuUrlExpire;

        internal static Image From(IntegrationClient.Img img) => new Image
        {
            ImageId = img.imgid,
            TemporaryImageUrl = img.url,
            DtuUrlExpire = img.dtuValid
        };
    }

    public class Fields
    {
        public string OrderNumber;
        public string LicensePlate;
        public string Notes;

        internal static Fields From(IntegrationClient.Fld fld) => new Fields
        {
            OrderNumber = fld.orderid,
            LicensePlate = fld.plate,
            Notes = fld.desc
        };
    }

    public class IntegrationClient
    {
        internal class Fld
        {
            public string orderid;
            public string plate;
            public string desc;
        }

        internal class Img
        {
            public string imgid;
            public string url;
            public DateTime dtuValid;
        }

        internal class Upl
        {
            public string gidupl;
            public string usern;
            public string cid;
            public DateTime dtuSubmit;
            public DateTime dtuUpload;
            public string kupl;
            public string loc;
            public Fld fld;
            public Img[] rgimg;
            public string rhnd;

            public override string ToString()
            {
                return $"{kupl} {gidupl.StTruncate()} submitted on {dtuSubmit.ToLocalTime():yyyy-MM-dd HH:mm:ss.fff} by {usern}@{cid}";
            }
        }

        private class Rec
        {
            public Upl[] rgupl;
        }

        private readonly HttpClient httpc;
        private readonly TimeSpan durRetryMin;
        private readonly TimeSpan durRetryMax;
        private readonly double expRetry;
        private readonly L l;

        public IntegrationClient(
            Uri uriEndpoint,
            string kid,
            string shs,
            TimeSpan? odurRetryMin = null, 
            TimeSpan? odurRetryMax = null, 
            double expRetry = 2.0, 
            Lf lf = null
        )
        {
            lf = lf ?? new Lf(new Lwcon());
            this.l = lf.L(this);

            this.httpc = new HttpClient(
                new HttpClientHandler()
                .AndThen(new Logi(lf))
                .AndThen(new Reqsi(kid, shs, lf))
            ) {
                BaseAddress = uriEndpoint,
                Timeout = TimeSpan.FromSeconds(60),
            };

            this.durRetryMin = odurRetryMin ?? TimeSpan.FromMinutes(1);
            this.durRetryMax = odurRetryMax ?? TimeSpan.FromMinutes(15);
            this.expRetry = expRetry;
        }

        public StoppableTask Listen(Func<IntegrationClient, Upload, CancellationToken, Task> dgProcess)
        {
            var ctoks = new CancellationTokenSource();
            return new StoppableTask(TaskEx.Run(() => ReceiveLoop(dgProcess, ctoks.Token), ctoks.Token), ctoks);
        }

        private async Task<bool> ProcessUpl(Func<IntegrationClient, Upload, CancellationToken, Task> dgProcess, Upl upl, CancellationToken ctok)
        {
            try
            {
                await dgProcess(this, Upload.From(upl), ctok);
                l.D($"delete {upl}");
                await DeleteAsync(upl, ctok);
                return true;
            }
            catch (TaskCanceledException) { throw; }
            catch (Exception er)
            {
                l.E($"processing {upl} failed with {er.ToString()}");
                return false;
            }
        }

        private async Task<bool> ProcessRec(Func<IntegrationClient, Upload, CancellationToken, Task> dgProcess, Rec rec, CancellationToken ctok)
        {
            var fSuccess = true;
            var iupl = 0;
            foreach (var upl in rec.rgupl)
            {
                l.I($"processing {++iupl} of {rec.rgupl.Length}: {upl}");
                fSuccess = await ProcessUpl(dgProcess, upl, ctok) && fSuccess;
            }
            return fSuccess;
        }

        private async Task ReceiveLoop(Func<IntegrationClient, Upload, CancellationToken, Task> dgProcess, CancellationToken ctok)
        {
            var durRetry = durRetryMin;
            while (true)
            {
                bool fSuccess;
                try
                {
                    l.D("receiving...");
                    var rec = await ReceiveAsync(ctok);

                    if (rec.rgupl.Length > 0)
                        l.I($"got {rec.rgupl.Length} items");
                    else
                        l.D($"got no items");

                    fSuccess = await ProcessRec(dgProcess, rec, ctok);
                }
                catch (TaskCanceledException)
                {
                    l.I("received abort signal, stopping...");
                    throw;
                }
                catch (Exception er)
                {
                    l.E($"failed with {er.ToString()}");
                    fSuccess = false;
                }

                if (!fSuccess)
                {
                    l.W($"waiting for retry ({durRetry})...");
                    await TaskEx.Delay(durRetry, ctok);
                    l.W($"retrying now...");
                    durRetry = TimeSpan.FromTicks(Math.Min((long)Math.Round(durRetry.Ticks * expRetry), durRetryMax.Ticks));
                } else
                {
                    if (durRetry > durRetryMin)
                        l.I($"recovered");

                    durRetry = durRetryMin;
                }
            }
        }

        private async Task<Rec> ReceiveAsync(CancellationToken ctok)
        {
            return await (await httpc.GetAsync($"./receive", ctok).Timeout_Pkludge(ctok).EnsureValid()).Extract<Rec>();
        }

        private async Task DeleteAsync(Upl upl, CancellationToken ctok)
        {
            await httpc.DeleteAsync($"./hnd/{upl.rhnd.Uri()}").Timeout_Pkludge(ctok).EnsureValid();
        }

        public async Task<Blob> DownloadImageAsync(Upload upload, Image image, CancellationToken ctok)
        {
            var response = await httpc.GetAsync($"./img/{upload.UploadId.Uri()}/{image.ImageId.Uri()}", ctok).Timeout_Pkludge(ctok).EnsureValid();
            return new Blob(response.Content.Headers.ContentType.MediaType, await response.Content.ReadAsStreamAsync());
        }

        public async Task<Blob> DownloadImageAsync(Image image, CancellationToken ctok)
        {
            var response = await httpc.GetAsync(image.TemporaryImageUrl, ctok).Timeout_Pkludge(ctok).EnsureValid();
            return new Blob(response.Content.Headers.ContentType.MediaType, await response.Content.ReadAsStreamAsync());
        }
    }
}
