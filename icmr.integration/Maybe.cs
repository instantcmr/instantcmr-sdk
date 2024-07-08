using System;

namespace Icmr.Integration
{
    public static class Maybe
    {
        public static Maybe<T> Empty<T>() => Maybe<T>.Empty();
        public static Maybe<T> Of<T>(T tValue) => Maybe<T>.Of(tValue);
        public static Maybe<T> OfNullable<T>(T otValue) where T : class => Maybe<T>.OfNullable(otValue);
        public static Maybe<T> OfNullable<T>(T? otValue) where T : struct => Maybe<T>.OfNullable(otValue);
    }

    public struct Maybe<T>
    {
        private readonly T tValue;
        private readonly bool fDefined;

        private Maybe(T tValue, bool fDefined)
        {
            this.tValue = tValue;
            this.fDefined = fDefined;
        }

        public static Maybe<T> Empty() => new Maybe<T>(default(T), false);

        public static Maybe<T> Of(T tValue)
        {
            if (tValue == null)
                throw new ArgumentNullException("tValue");

            return new Maybe<T>(tValue, true);
        }

        public static Maybe<U> OfNullable<U>(U otValue) where U : class =>
            new Maybe<U>(otValue, otValue != null);

        public static Maybe<U> OfNullable<U>(U? otValue) where U : struct =>
            new Maybe<U>(otValue.GetValueOrDefault(), otValue != null);

        public Maybe<U> Map<U>(Func<T, U> dg) => FlatMap(tValue => Maybe.Of(dg(tValue)));
        public Maybe<U> FlatMap<U>(Func<T, Maybe<U>> dg) => fDefined ? dg(tValue) : Maybe.Empty<U>();
        public Maybe<T> Filter(Func<T, bool> dg) => FlatMap(t => dg(t) ? Maybe.Of(t) : Maybe.Empty<T>());

        public bool FDefined { get => fDefined; }
        public bool FEmpty { get => !fDefined; }

        public T Get()
        {
            if (!fDefined)
                throw new NullReferenceException();

            return tValue;
        }

        public T OrElse(T tDefault) => fDefined ? tValue : tDefault;
        public T OrElse(Func<T> dgtDefault) => fDefined ? tValue : dgtDefault();
        public Maybe<T> Or(Func<Maybe<T>> dgotDefault) => fDefined ? this : dgotDefault();

        public override bool Equals(object obj) => obj is Maybe<T> ? Equals((Maybe<T>)obj) : false;

        public bool Equals(Maybe<T> ot) =>
            fDefined && ot.fDefined && Equals(tValue, ot.tValue) ||
            !fDefined && !ot.fDefined;

        public override int GetHashCode() => (Map(t => t.GetHashCode()).OrElse(0)*397) ^ fDefined.GetHashCode();

        public override string ToString() => Map(t => $"Some({t})").OrElse("None");
    }
}