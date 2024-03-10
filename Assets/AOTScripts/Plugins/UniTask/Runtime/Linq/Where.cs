using Cysharp.Threading.Tasks.Internal;
using System;
using System.Threading;

namespace Cysharp.Threading.Tasks.Linq
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static IUniTaskAsyncEnumerable<TSource> Where<TSource>(this IUniTaskAsyncEnumerable<TSource> source,
            Func<TSource, Boolean> predicate)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(predicate, nameof(predicate));

            return new Where<TSource>(source, predicate);
        }

        public static IUniTaskAsyncEnumerable<TSource> Where<TSource>(this IUniTaskAsyncEnumerable<TSource> source,
            Func<TSource, Int32, Boolean> predicate)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(predicate, nameof(predicate));

            return new WhereInt<TSource>(source, predicate);
        }

        public static IUniTaskAsyncEnumerable<TSource> WhereAwait<TSource>(this IUniTaskAsyncEnumerable<TSource> source,
            Func<TSource, UniTask<Boolean>> predicate)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(predicate, nameof(predicate));

            return new WhereAwait<TSource>(source, predicate);
        }

        public static IUniTaskAsyncEnumerable<TSource> WhereAwait<TSource>(this IUniTaskAsyncEnumerable<TSource> source,
            Func<TSource, Int32, UniTask<Boolean>> predicate)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(predicate, nameof(predicate));

            return new WhereIntAwait<TSource>(source, predicate);
        }

        public static IUniTaskAsyncEnumerable<TSource> WhereAwaitWithCancellation<TSource>(
            this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTask<Boolean>> predicate)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(predicate, nameof(predicate));

            return new WhereAwaitWithCancellation<TSource>(source, predicate);
        }

        public static IUniTaskAsyncEnumerable<TSource> WhereAwaitWithCancellation<TSource>(
            this IUniTaskAsyncEnumerable<TSource> source,
            Func<TSource, Int32, CancellationToken, UniTask<Boolean>> predicate)
        {
            Error.ThrowArgumentNullException(source, nameof(source));
            Error.ThrowArgumentNullException(predicate, nameof(predicate));

            return new WhereIntAwaitWithCancellation<TSource>(source, predicate);
        }
    }

    internal sealed class Where<TSource> : IUniTaskAsyncEnumerable<TSource>
    {
        readonly Func<TSource, bool> predicate;
        readonly IUniTaskAsyncEnumerable<TSource> source;

        public Where(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            this.source = source;
            this.predicate = predicate;
        }

        public IUniTaskAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new _Where(source, predicate, cancellationToken);
        }

        sealed class _Where : MoveNextSource, IUniTaskAsyncEnumerator<TSource>
        {
            readonly CancellationToken cancellationToken;
            readonly Func<TSource, bool> predicate;
            readonly IUniTaskAsyncEnumerable<TSource> source;
            UniTask<bool>.Awaiter awaiter;
            IUniTaskAsyncEnumerator<TSource> enumerator;
            Action moveNextAction;

            int state = -1;

            public _Where(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, bool> predicate,
                CancellationToken cancellationToken)
            {
                this.source = source;
                this.predicate = predicate;
                this.cancellationToken = cancellationToken;
                this.moveNextAction = MoveNext;
                TaskTracker.TrackActiveTask(this, 3);
            }

            public TSource Current { get; private set; }

            public UniTask<bool> MoveNextAsync()
            {
                if (state == -2) return default;

                completionSource.Reset();
                MoveNext();
                return new UniTask<bool>(this, completionSource.Version);
            }

            public UniTask DisposeAsync()
            {
                TaskTracker.RemoveTracking(this);
                return enumerator.DisposeAsync();
            }

            void MoveNext()
            {
                REPEAT:
                try
                {
                    switch (state)
                    {
                        case -1: // init
                            enumerator = source.GetAsyncEnumerator(cancellationToken);
                            goto case 0;
                        case 0:
                            awaiter = enumerator.MoveNextAsync().GetAwaiter();
                            if (awaiter.IsCompleted)
                            {
                                goto case 1;
                            }
                            else
                            {
                                state = 1;
                                awaiter.UnsafeOnCompleted(moveNextAction);
                                return;
                            }
                        case 1:
                            if (awaiter.GetResult())
                            {
                                Current = enumerator.Current;
                                if (predicate(Current))
                                {
                                    goto CONTINUE;
                                }
                                else
                                {
                                    state = 0;
                                    goto REPEAT;
                                }
                            }
                            else
                            {
                                goto DONE;
                            }
                        default:
                            goto DONE;
                    }
                }
                catch (Exception ex)
                {
                    state = -2;
                    completionSource.TrySetException(ex);
                    return;
                }

                DONE:
                state = -2;
                completionSource.TrySetResult(false);
                return;

                CONTINUE:
                state = 0;
                completionSource.TrySetResult(true);
                return;
            }
        }
    }

    internal sealed class WhereInt<TSource> : IUniTaskAsyncEnumerable<TSource>
    {
        readonly Func<TSource, int, bool> predicate;
        readonly IUniTaskAsyncEnumerable<TSource> source;

        public WhereInt(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            this.source = source;
            this.predicate = predicate;
        }

        public IUniTaskAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new _Where(source, predicate, cancellationToken);
        }

        sealed class _Where : MoveNextSource, IUniTaskAsyncEnumerator<TSource>
        {
            readonly CancellationToken cancellationToken;
            readonly Func<TSource, int, bool> predicate;
            readonly IUniTaskAsyncEnumerable<TSource> source;
            UniTask<bool>.Awaiter awaiter;
            IUniTaskAsyncEnumerator<TSource> enumerator;
            int index;
            Action moveNextAction;

            int state = -1;

            public _Where(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, int, bool> predicate,
                CancellationToken cancellationToken)
            {
                this.source = source;
                this.predicate = predicate;
                this.cancellationToken = cancellationToken;
                this.moveNextAction = MoveNext;
                TaskTracker.TrackActiveTask(this, 3);
            }

            public TSource Current { get; private set; }

            public UniTask<bool> MoveNextAsync()
            {
                if (state == -2) return default;

                completionSource.Reset();
                MoveNext();
                return new UniTask<bool>(this, completionSource.Version);
            }

            public UniTask DisposeAsync()
            {
                TaskTracker.RemoveTracking(this);
                return enumerator.DisposeAsync();
            }

            void MoveNext()
            {
                REPEAT:
                try
                {
                    switch (state)
                    {
                        case -1: // init
                            enumerator = source.GetAsyncEnumerator(cancellationToken);
                            goto case 0;
                        case 0:
                            awaiter = enumerator.MoveNextAsync().GetAwaiter();
                            if (awaiter.IsCompleted)
                            {
                                goto case 1;
                            }
                            else
                            {
                                state = 1;
                                awaiter.UnsafeOnCompleted(moveNextAction);
                                return;
                            }
                        case 1:
                            if (awaiter.GetResult())
                            {
                                Current = enumerator.Current;
                                if (predicate(Current, checked(index++)))
                                {
                                    goto CONTINUE;
                                }
                                else
                                {
                                    state = 0;
                                    goto REPEAT;
                                }
                            }
                            else
                            {
                                goto DONE;
                            }
                        default:
                            goto DONE;
                    }
                }
                catch (Exception ex)
                {
                    state = -2;
                    completionSource.TrySetException(ex);
                    return;
                }

                DONE:
                state = -2;
                completionSource.TrySetResult(false);
                return;

                CONTINUE:
                state = 0;
                completionSource.TrySetResult(true);
                return;
            }
        }
    }

    internal sealed class WhereAwait<TSource> : IUniTaskAsyncEnumerable<TSource>
    {
        readonly Func<TSource, UniTask<bool>> predicate;
        readonly IUniTaskAsyncEnumerable<TSource> source;

        public WhereAwait(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask<bool>> predicate)
        {
            this.source = source;
            this.predicate = predicate;
        }

        public IUniTaskAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new _WhereAwait(source, predicate, cancellationToken);
        }

        sealed class _WhereAwait : MoveNextSource, IUniTaskAsyncEnumerator<TSource>
        {
            readonly CancellationToken cancellationToken;
            readonly Func<TSource, UniTask<bool>> predicate;
            readonly IUniTaskAsyncEnumerable<TSource> source;
            UniTask<bool>.Awaiter awaiter;
            UniTask<bool>.Awaiter awaiter2;
            IUniTaskAsyncEnumerator<TSource> enumerator;
            Action moveNextAction;

            int state = -1;

            public _WhereAwait(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask<bool>> predicate,
                CancellationToken cancellationToken)
            {
                this.source = source;
                this.predicate = predicate;
                this.cancellationToken = cancellationToken;
                this.moveNextAction = MoveNext;
                TaskTracker.TrackActiveTask(this, 3);
            }

            public TSource Current { get; private set; }

            public UniTask<bool> MoveNextAsync()
            {
                if (state == -2) return default;

                completionSource.Reset();
                MoveNext();
                return new UniTask<bool>(this, completionSource.Version);
            }

            public UniTask DisposeAsync()
            {
                TaskTracker.RemoveTracking(this);
                return enumerator.DisposeAsync();
            }

            void MoveNext()
            {
                REPEAT:
                try
                {
                    switch (state)
                    {
                        case -1: // init
                            enumerator = source.GetAsyncEnumerator(cancellationToken);
                            goto case 0;
                        case 0:
                            awaiter = enumerator.MoveNextAsync().GetAwaiter();
                            if (awaiter.IsCompleted)
                            {
                                goto case 1;
                            }
                            else
                            {
                                state = 1;
                                awaiter.UnsafeOnCompleted(moveNextAction);
                                return;
                            }
                        case 1:
                            if (awaiter.GetResult())
                            {
                                Current = enumerator.Current;

                                awaiter2 = predicate(Current).GetAwaiter();
                                if (awaiter2.IsCompleted)
                                {
                                    goto case 2;
                                }
                                else
                                {
                                    state = 2;
                                    awaiter2.UnsafeOnCompleted(moveNextAction);
                                    return;
                                }
                            }
                            else
                            {
                                goto DONE;
                            }
                        case 2:
                            if (awaiter2.GetResult())
                            {
                                goto CONTINUE;
                            }
                            else
                            {
                                state = 0;
                                goto REPEAT;
                            }
                        default:
                            goto DONE;
                    }
                }
                catch (Exception ex)
                {
                    state = -2;
                    completionSource.TrySetException(ex);
                    return;
                }

                DONE:
                state = -2;
                completionSource.TrySetResult(false);
                return;

                CONTINUE:
                state = 0;
                completionSource.TrySetResult(true);
                return;
            }
        }
    }

    internal sealed class WhereIntAwait<TSource> : IUniTaskAsyncEnumerable<TSource>
    {
        readonly Func<TSource, int, UniTask<bool>> predicate;
        readonly IUniTaskAsyncEnumerable<TSource> source;

        public WhereIntAwait(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, int, UniTask<bool>> predicate)
        {
            this.source = source;
            this.predicate = predicate;
        }

        public IUniTaskAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new _WhereAwait(source, predicate, cancellationToken);
        }

        sealed class _WhereAwait : MoveNextSource, IUniTaskAsyncEnumerator<TSource>
        {
            readonly CancellationToken cancellationToken;
            readonly Func<TSource, int, UniTask<bool>> predicate;
            readonly IUniTaskAsyncEnumerable<TSource> source;
            UniTask<bool>.Awaiter awaiter;
            UniTask<bool>.Awaiter awaiter2;
            IUniTaskAsyncEnumerator<TSource> enumerator;
            int index;
            Action moveNextAction;

            int state = -1;

            public _WhereAwait(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, int, UniTask<bool>> predicate,
                CancellationToken cancellationToken)
            {
                this.source = source;
                this.predicate = predicate;
                this.cancellationToken = cancellationToken;
                this.moveNextAction = MoveNext;
                TaskTracker.TrackActiveTask(this, 3);
            }

            public TSource Current { get; private set; }

            public UniTask<bool> MoveNextAsync()
            {
                if (state == -2) return default;

                completionSource.Reset();
                MoveNext();
                return new UniTask<bool>(this, completionSource.Version);
            }

            public UniTask DisposeAsync()
            {
                TaskTracker.RemoveTracking(this);
                return enumerator.DisposeAsync();
            }

            void MoveNext()
            {
                REPEAT:
                try
                {
                    switch (state)
                    {
                        case -1: // init
                            enumerator = source.GetAsyncEnumerator(cancellationToken);
                            goto case 0;
                        case 0:
                            awaiter = enumerator.MoveNextAsync().GetAwaiter();
                            if (awaiter.IsCompleted)
                            {
                                goto case 1;
                            }
                            else
                            {
                                state = 1;
                                awaiter.UnsafeOnCompleted(moveNextAction);
                                return;
                            }
                        case 1:
                            if (awaiter.GetResult())
                            {
                                Current = enumerator.Current;

                                awaiter2 = predicate(Current, checked(index++)).GetAwaiter();
                                if (awaiter2.IsCompleted)
                                {
                                    goto case 2;
                                }
                                else
                                {
                                    state = 2;
                                    awaiter2.UnsafeOnCompleted(moveNextAction);
                                    return;
                                }
                            }
                            else
                            {
                                goto DONE;
                            }
                        case 2:
                            if (awaiter2.GetResult())
                            {
                                goto CONTINUE;
                            }
                            else
                            {
                                state = 0;
                                goto REPEAT;
                            }
                        default:
                            goto DONE;
                    }
                }
                catch (Exception ex)
                {
                    state = -2;
                    completionSource.TrySetException(ex);
                    return;
                }

                DONE:
                state = -2;
                completionSource.TrySetResult(false);
                return;

                CONTINUE:
                state = 0;
                completionSource.TrySetResult(true);
                return;
            }
        }
    }

    internal sealed class WhereAwaitWithCancellation<TSource> : IUniTaskAsyncEnumerable<TSource>
    {
        readonly Func<TSource, CancellationToken, UniTask<bool>> predicate;
        readonly IUniTaskAsyncEnumerable<TSource> source;

        public WhereAwaitWithCancellation(IUniTaskAsyncEnumerable<TSource> source,
            Func<TSource, CancellationToken, UniTask<bool>> predicate)
        {
            this.source = source;
            this.predicate = predicate;
        }

        public IUniTaskAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new _WhereAwaitWithCancellation(source, predicate, cancellationToken);
        }

        sealed class _WhereAwaitWithCancellation : MoveNextSource, IUniTaskAsyncEnumerator<TSource>
        {
            readonly CancellationToken cancellationToken;
            readonly Func<TSource, CancellationToken, UniTask<bool>> predicate;
            readonly IUniTaskAsyncEnumerable<TSource> source;
            UniTask<bool>.Awaiter awaiter;
            UniTask<bool>.Awaiter awaiter2;
            IUniTaskAsyncEnumerator<TSource> enumerator;
            Action moveNextAction;

            int state = -1;

            public _WhereAwaitWithCancellation(IUniTaskAsyncEnumerable<TSource> source,
                Func<TSource, CancellationToken, UniTask<bool>> predicate, CancellationToken cancellationToken)
            {
                this.source = source;
                this.predicate = predicate;
                this.cancellationToken = cancellationToken;
                this.moveNextAction = MoveNext;
                TaskTracker.TrackActiveTask(this, 3);
            }

            public TSource Current { get; private set; }

            public UniTask<bool> MoveNextAsync()
            {
                if (state == -2) return default;

                completionSource.Reset();
                MoveNext();
                return new UniTask<bool>(this, completionSource.Version);
            }

            public UniTask DisposeAsync()
            {
                TaskTracker.RemoveTracking(this);
                return enumerator.DisposeAsync();
            }

            void MoveNext()
            {
                REPEAT:
                try
                {
                    switch (state)
                    {
                        case -1: // init
                            enumerator = source.GetAsyncEnumerator(cancellationToken);
                            goto case 0;
                        case 0:
                            awaiter = enumerator.MoveNextAsync().GetAwaiter();
                            if (awaiter.IsCompleted)
                            {
                                goto case 1;
                            }
                            else
                            {
                                state = 1;
                                awaiter.UnsafeOnCompleted(moveNextAction);
                                return;
                            }
                        case 1:
                            if (awaiter.GetResult())
                            {
                                Current = enumerator.Current;

                                awaiter2 = predicate(Current, cancellationToken).GetAwaiter();
                                if (awaiter2.IsCompleted)
                                {
                                    goto case 2;
                                }
                                else
                                {
                                    state = 2;
                                    awaiter2.UnsafeOnCompleted(moveNextAction);
                                    return;
                                }
                            }
                            else
                            {
                                goto DONE;
                            }
                        case 2:
                            if (awaiter2.GetResult())
                            {
                                goto CONTINUE;
                            }
                            else
                            {
                                state = 0;
                                goto REPEAT;
                            }
                        default:
                            goto DONE;
                    }
                }
                catch (Exception ex)
                {
                    state = -2;
                    completionSource.TrySetException(ex);
                    return;
                }

                DONE:
                state = -2;
                completionSource.TrySetResult(false);
                return;

                CONTINUE:
                state = 0;
                completionSource.TrySetResult(true);
                return;
            }
        }
    }

    internal sealed class WhereIntAwaitWithCancellation<TSource> : IUniTaskAsyncEnumerable<TSource>
    {
        readonly Func<TSource, int, CancellationToken, UniTask<bool>> predicate;
        readonly IUniTaskAsyncEnumerable<TSource> source;

        public WhereIntAwaitWithCancellation(IUniTaskAsyncEnumerable<TSource> source,
            Func<TSource, int, CancellationToken, UniTask<bool>> predicate)
        {
            this.source = source;
            this.predicate = predicate;
        }

        public IUniTaskAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new _WhereAwaitWithCancellation(source, predicate, cancellationToken);
        }

        sealed class _WhereAwaitWithCancellation : MoveNextSource, IUniTaskAsyncEnumerator<TSource>
        {
            readonly CancellationToken cancellationToken;
            readonly Func<TSource, int, CancellationToken, UniTask<bool>> predicate;
            readonly IUniTaskAsyncEnumerable<TSource> source;
            UniTask<bool>.Awaiter awaiter;
            UniTask<bool>.Awaiter awaiter2;
            IUniTaskAsyncEnumerator<TSource> enumerator;
            int index;
            Action moveNextAction;

            int state = -1;

            public _WhereAwaitWithCancellation(IUniTaskAsyncEnumerable<TSource> source,
                Func<TSource, int, CancellationToken, UniTask<bool>> predicate, CancellationToken cancellationToken)
            {
                this.source = source;
                this.predicate = predicate;
                this.cancellationToken = cancellationToken;
                this.moveNextAction = MoveNext;
                TaskTracker.TrackActiveTask(this, 3);
            }

            public TSource Current { get; private set; }

            public UniTask<bool> MoveNextAsync()
            {
                if (state == -2) return default;

                completionSource.Reset();
                MoveNext();
                return new UniTask<bool>(this, completionSource.Version);
            }

            public UniTask DisposeAsync()
            {
                TaskTracker.RemoveTracking(this);
                return enumerator.DisposeAsync();
            }

            void MoveNext()
            {
                REPEAT:
                try
                {
                    switch (state)
                    {
                        case -1: // init
                            enumerator = source.GetAsyncEnumerator(cancellationToken);
                            goto case 0;
                        case 0:
                            awaiter = enumerator.MoveNextAsync().GetAwaiter();
                            if (awaiter.IsCompleted)
                            {
                                goto case 1;
                            }
                            else
                            {
                                state = 1;
                                awaiter.UnsafeOnCompleted(moveNextAction);
                                return;
                            }
                        case 1:
                            if (awaiter.GetResult())
                            {
                                Current = enumerator.Current;

                                awaiter2 = predicate(Current, checked(index++), cancellationToken).GetAwaiter();
                                if (awaiter2.IsCompleted)
                                {
                                    goto case 2;
                                }
                                else
                                {
                                    state = 2;
                                    awaiter2.UnsafeOnCompleted(moveNextAction);
                                    return;
                                }
                            }
                            else
                            {
                                goto DONE;
                            }
                        case 2:
                            if (awaiter2.GetResult())
                            {
                                goto CONTINUE;
                            }
                            else
                            {
                                state = 0;
                                goto REPEAT;
                            }
                        default:
                            goto DONE;
                    }
                }
                catch (Exception ex)
                {
                    state = -2;
                    completionSource.TrySetException(ex);
                    return;
                }

                DONE:
                state = -2;
                completionSource.TrySetResult(false);
                return;

                CONTINUE:
                state = 0;
                completionSource.TrySetResult(true);
                return;
            }
        }
    }
}