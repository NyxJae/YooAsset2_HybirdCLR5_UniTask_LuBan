using Cysharp.Threading.Tasks.Internal;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Cysharp.Threading.Tasks.Linq
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static IUniTaskAsyncEnumerable<TSource> Distinct<TSource>(this IUniTaskAsyncEnumerable<TSource> source)
        {
            return Distinct(source, EqualityComparer<TSource>.Default);
        }

        public static IUniTaskAsyncEnumerable<TSource> Distinct<TSource>(this IUniTaskAsyncEnumerable<TSource> source,
            IEqualityComparer<TSource> comparer)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(comparer, nameof(comparer));

            return new Distinct<TSource>(source, comparer);
        }

        public static IUniTaskAsyncEnumerable<TSource> Distinct<TSource, TKey>(
            this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return Distinct(source, keySelector, EqualityComparer<TKey>.Default);
        }

        public static IUniTaskAsyncEnumerable<TSource> Distinct<TSource, TKey>(
            this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(keySelector, nameof(keySelector));
            Error.ThrowArgumentNullException(comparer, nameof(comparer));

            return new Distinct<TSource, TKey>(source, keySelector, comparer);
        }

        public static IUniTaskAsyncEnumerable<TSource> DistinctAwait<TSource, TKey>(
            this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask<TKey>> keySelector)
        {
            return DistinctAwait(source, keySelector, EqualityComparer<TKey>.Default);
        }

        public static IUniTaskAsyncEnumerable<TSource> DistinctAwait<TSource, TKey>(
            this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask<TKey>> keySelector,
            IEqualityComparer<TKey> comparer)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(keySelector, nameof(keySelector));
            Error.ThrowArgumentNullException(comparer, nameof(comparer));

            return new DistinctAwait<TSource, TKey>(source, keySelector, comparer);
        }

        public static IUniTaskAsyncEnumerable<TSource> DistinctAwaitWithCancellation<TSource, TKey>(
            this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTask<TKey>> keySelector)
        {
            return DistinctAwaitWithCancellation(source, keySelector, EqualityComparer<TKey>.Default);
        }

        public static IUniTaskAsyncEnumerable<TSource> DistinctAwaitWithCancellation<TSource, TKey>(
            this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTask<TKey>> keySelector,
            IEqualityComparer<TKey> comparer)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(keySelector, nameof(keySelector));
            Error.ThrowArgumentNullException(comparer, nameof(comparer));

            return new DistinctAwaitWithCancellation<TSource, TKey>(source, keySelector, comparer);
        }
    }

    internal sealed class Distinct<TSource> : IUniTaskAsyncEnumerable<TSource>
    {
        readonly IEqualityComparer<TSource> comparer;
        readonly IUniTaskAsyncEnumerable<TSource> source;

        public Distinct(IUniTaskAsyncEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
        {
            this.source = source;
            this.comparer = comparer;
        }

        public IUniTaskAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new _Distinct(source, comparer, cancellationToken);
        }

        class _Distinct : AsyncEnumeratorBase<TSource, TSource>
        {
            readonly HashSet<TSource> set;

            public _Distinct(IUniTaskAsyncEnumerable<TSource> source, IEqualityComparer<TSource> comparer,
                CancellationToken cancellationToken)
                : base(source, cancellationToken)
            {
                this.set = new HashSet<TSource>(comparer);
            }

            protected override bool TryMoveNextCore(bool sourceHasCurrent, out bool result)
            {
                if (sourceHasCurrent)
                {
                    var v = SourceCurrent;
                    if (set.Add(v))
                    {
                        Current = v;
                        result = true;
                        return true;
                    }
                    else
                    {
                        result = default;
                        return false;
                    }
                }

                result = false;
                return true;
            }
        }
    }

    internal sealed class Distinct<TSource, TKey> : IUniTaskAsyncEnumerable<TSource>
    {
        readonly IEqualityComparer<TKey> comparer;
        readonly Func<TSource, TKey> keySelector;
        readonly IUniTaskAsyncEnumerable<TSource> source;

        public Distinct(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer)
        {
            this.source = source;
            this.keySelector = keySelector;
            this.comparer = comparer;
        }

        public IUniTaskAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new _Distinct(source, keySelector, comparer, cancellationToken);
        }

        class _Distinct : AsyncEnumeratorBase<TSource, TSource>
        {
            readonly Func<TSource, TKey> keySelector;
            readonly HashSet<TKey> set;

            public _Distinct(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector,
                IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
                : base(source, cancellationToken)
            {
                this.set = new HashSet<TKey>(comparer);
                this.keySelector = keySelector;
            }

            protected override bool TryMoveNextCore(bool sourceHasCurrent, out bool result)
            {
                if (sourceHasCurrent)
                {
                    var v = SourceCurrent;
                    if (set.Add(keySelector(v)))
                    {
                        Current = v;
                        result = true;
                        return true;
                    }
                    else
                    {
                        result = default;
                        return false;
                    }
                }

                result = false;
                return true;
            }
        }
    }

    internal sealed class DistinctAwait<TSource, TKey> : IUniTaskAsyncEnumerable<TSource>
    {
        readonly IEqualityComparer<TKey> comparer;
        readonly Func<TSource, UniTask<TKey>> keySelector;
        readonly IUniTaskAsyncEnumerable<TSource> source;

        public DistinctAwait(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask<TKey>> keySelector,
            IEqualityComparer<TKey> comparer)
        {
            this.source = source;
            this.keySelector = keySelector;
            this.comparer = comparer;
        }

        public IUniTaskAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new _DistinctAwait(source, keySelector, comparer, cancellationToken);
        }

        class _DistinctAwait : AsyncEnumeratorAwaitSelectorBase<TSource, TSource, TKey>
        {
            readonly Func<TSource, UniTask<TKey>> keySelector;
            readonly HashSet<TKey> set;

            public _DistinctAwait(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask<TKey>> keySelector,
                IEqualityComparer<TKey> comparer, CancellationToken cancellationToken)
                : base(source, cancellationToken)
            {
                this.set = new HashSet<TKey>(comparer);
                this.keySelector = keySelector;
            }

            protected override UniTask<TKey> TransformAsync(TSource sourceCurrent)
            {
                return keySelector(sourceCurrent);
            }

            protected override bool TrySetCurrentCore(TKey awaitResult, out bool terminateIteration)
            {
                if (set.Add(awaitResult))
                {
                    Current = SourceCurrent;
                    terminateIteration = false;
                    return true;
                }
                else
                {
                    terminateIteration = false;
                    return false;
                }
            }
        }
    }

    internal sealed class DistinctAwaitWithCancellation<TSource, TKey> : IUniTaskAsyncEnumerable<TSource>
    {
        readonly IEqualityComparer<TKey> comparer;
        readonly Func<TSource, CancellationToken, UniTask<TKey>> keySelector;
        readonly IUniTaskAsyncEnumerable<TSource> source;

        public DistinctAwaitWithCancellation(IUniTaskAsyncEnumerable<TSource> source,
            Func<TSource, CancellationToken, UniTask<TKey>> keySelector, IEqualityComparer<TKey> comparer)
        {
            this.source = source;
            this.keySelector = keySelector;
            this.comparer = comparer;
        }

        public IUniTaskAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new _DistinctAwaitWithCancellation(source, keySelector, comparer, cancellationToken);
        }

        class _DistinctAwaitWithCancellation : AsyncEnumeratorAwaitSelectorBase<TSource, TSource, TKey>
        {
            readonly Func<TSource, CancellationToken, UniTask<TKey>> keySelector;
            readonly HashSet<TKey> set;

            public _DistinctAwaitWithCancellation(IUniTaskAsyncEnumerable<TSource> source,
                Func<TSource, CancellationToken, UniTask<TKey>> keySelector, IEqualityComparer<TKey> comparer,
                CancellationToken cancellationToken)
                : base(source, cancellationToken)
            {
                this.set = new HashSet<TKey>(comparer);
                this.keySelector = keySelector;
            }

            protected override UniTask<TKey> TransformAsync(TSource sourceCurrent)
            {
                return keySelector(sourceCurrent, cancellationToken);
            }

            protected override bool TrySetCurrentCore(TKey awaitResult, out bool terminateIteration)
            {
                if (set.Add(awaitResult))
                {
                    Current = SourceCurrent;
                    terminateIteration = false;
                    return true;
                }
                else
                {
                    terminateIteration = false;
                    return false;
                }
            }
        }
    }
}