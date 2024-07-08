using System;
using System.Linq;
using System.Threading;

namespace Icmr.Integration
{
    struct Col
    {
        public readonly ConsoleColor? Fg;
        public readonly ConsoleColor? Bg;

        public Col(ConsoleColor? fg = null, ConsoleColor? bg = null)
        {
            Fg = fg;
            Bg = bg;
        }

        public static IDisposable Save()
        {
            return new RestoreCol(new Col(Console.ForegroundColor, Console.BackgroundColor));
        }

        public IDisposable Apply()
        {
            var restorecol = Save();
            Console.BackgroundColor = Bg ?? Console.BackgroundColor;
            Console.ForegroundColor = Fg ?? Console.ForegroundColor;
            return restorecol;
        }

        private class RestoreCol : IDisposable
        {
            private readonly Col colOriginal;

            public RestoreCol(Col colOriginal)
            {
                this.colOriginal = colOriginal;
            }

            public void Dispose()
            {
                colOriginal.Apply();
            }
        }
    }

    public interface L
    {
        void V(string st);
        void D(string st);
        void I(string st);
        void W(string st);
        void E(string st);
        void Write(Severity severity, string st);
    }

    public class Lf
    {
        private readonly Lw lw;

        public Lf(Lw lw)
        {
            this.lw = lw;
        }

        public L L<T>(T t = null) where T : class
        {
            return new Li<T>(lw);
        }
    }

    public static class Lu
    {
        public static Lw Filter(this Lw lw, Func<Severity, string, bool> dgPredicate) => new Lwfilter(dgPredicate, lw);
        public static Lw Filter(this Lw lw, Func<Severity, bool> dgPredicate) => lw.Filter((severity, stModule) => dgPredicate(severity));
        public static Lw Filter(this Lw lw, Severity severity) => lw.Filter(severityT => severityT >= severity);
    }

    public enum Severity
    {
        VERBOSE,
        DEBUG,
        INFO,
        WARN,
        ERROR
    }

    public interface Lw
    {
        void Write(Severity severity, string stModule, string stMsg);
    }

    public class Lwcon : Lw
    {
        private Col colFromSeverity(Severity severity)
        {
            switch (severity)
            {
                case Severity.VERBOSE: return new Col(fg: ConsoleColor.DarkGray);
                case Severity.DEBUG: return new Col(fg: ConsoleColor.Gray);
                case Severity.INFO: return new Col(fg: ConsoleColor.White);
                case Severity.WARN: return new Col(fg: ConsoleColor.Yellow);
                case Severity.ERROR: return new Col(fg: ConsoleColor.Red);
                default: throw new ArgumentOutOfRangeException($"unknown severity '{severity}'");
            }
        }

        public void Write(Severity severity, string stModule, string stMsg)
        {
            var th = Thread.CurrentThread;
            using (colFromSeverity(severity).Apply())
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [{th.Name}:{(!th.IsBackground ? "main-" : "")}{(th.IsThreadPoolThread ? "pool-" : "")}{th.ManagedThreadId}] {severity} {stModule} - {stMsg}");
        }
    }

    class Lwfilter : Lw
    {
        private readonly Func<Severity, string, bool> dgPredicate;
        private readonly Lw lwInner;

        public Lwfilter(Func<Severity, string, bool> dgPredicate, Lw lwInner)
        {
            this.dgPredicate = dgPredicate;
            this.lwInner = lwInner;
        }

        public void Write(Severity severity, string stModule, string stMsg)
        {
            if (dgPredicate(severity, stModule))
                lwInner.Write(severity, stModule, stMsg);
        }
    }

    class Li<T> : L
    {
        private readonly Lw lw;

        public Li(Lw lw)
        {
            this.lw = lw;
        }

        public void V(string st)
        {
            Write(Severity.VERBOSE, st);
        }

        public void D(string st)
        {
            Write(Severity.DEBUG, st);
        }

        public void I(string st)
        {
            Write(Severity.INFO, st);
        }

        public void W(string st)
        {
            Write(Severity.WARN, st);
        }

        public void E(string st)
        {
            Write(Severity.ERROR, st);
        }

        public void Write(Severity severity, string st)
        {
            lw.Write(severity, typeof(T).Name, st);
        }
    }
}