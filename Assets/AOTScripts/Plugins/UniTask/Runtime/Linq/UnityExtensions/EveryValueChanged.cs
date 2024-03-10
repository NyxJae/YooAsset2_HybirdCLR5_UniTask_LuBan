using Cysharp.Threading.Tasks.Internal;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Cysharp.Threading.Tasks.Linq
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static IUniTaskAsyncEnumerable<TProperty> EveryValueChanged<TTarget, TProperty>(TTarget target,
            Func<TTarget, TProperty> propertySelector, PlayerLoopTiming monitorTiming = PlayerLoopTiming.Update,
            IEqualityComparer<TProperty> equalityComparer = null, bool cancelImmediately = false)
            where TTarget : class
        {
            var unityObject = target as UnityEngine.Object;
            var isUnityObject = target is UnityEngine.Object; // don't use (unityObject == null)

            if (isUnityObject)
            {
                return new EveryValueChangedUnityObject<TTarget, TProperty>(target, propertySelector,
                    equalityComparer ?? UnityEqualityComparer.GetDefault<TProperty>(), monitorTiming,
                    cancelImmediately);
            }
            else
            {
                return new EveryValueChangedStandardObject<TTarget, TProperty>(target, propertySelector,
                    equalityComparer ?? UnityEqualityComparer.GetDefault<TProperty>(), monitorTiming,
                    cancelImmediately);
            }
        }
    }

    internal sealed class EveryValueChangedUnityObject<TTarget, TProperty> : IUniTaskAsyncEnumerable<TProperty>
    {
        readonly bool cancelImmediately;
        readonly IEqualityComparer<TProperty> equalityComparer;
        readonly PlayerLoopTiming monitorTiming;
        readonly Func<TTarget, TProperty> propertySelector;
        readonly TTarget target;

        public EveryValueChangedUnityObject(TTarget target, Func<TTarget, TProperty> propertySelector,
            IEqualityComparer<TProperty> equalityComparer, PlayerLoopTiming monitorTiming, bool cancelImmediately)
        {
            this.target = target;
            this.propertySelector = propertySelector;
            this.equalityComparer = equalityComparer;
            this.monitorTiming = monitorTiming;
            this.cancelImmediately = cancelImmediately;
        }

        public IUniTaskAsyncEnumerator<TProperty> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new _EveryValueChanged(target, propertySelector, equalityComparer, monitorTiming, cancellationToken,
                cancelImmediately);
        }

        sealed class _EveryValueChanged : MoveNextSource, IUniTaskAsyncEnumerator<TProperty>, IPlayerLoopItem
        {
            readonly CancellationToken cancellationToken;
            readonly CancellationTokenRegistration cancellationTokenRegistration;
            readonly IEqualityComparer<TProperty> equalityComparer;
            readonly Func<TTarget, TProperty> propertySelector;
            readonly TTarget target;
            readonly UnityEngine.Object targetAsUnityObject;
            TProperty currentValue;
            bool disposed;

            bool first;

            public _EveryValueChanged(TTarget target, Func<TTarget, TProperty> propertySelector,
                IEqualityComparer<TProperty> equalityComparer, PlayerLoopTiming monitorTiming,
                CancellationToken cancellationToken, bool cancelImmediately)
            {
                this.target = target;
                this.targetAsUnityObject = target as UnityEngine.Object;
                this.propertySelector = propertySelector;
                this.equalityComparer = equalityComparer;
                this.cancellationToken = cancellationToken;
                this.first = true;

                if (cancelImmediately && cancellationToken.CanBeCanceled)
                {
                    cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(state =>
                    {
                        var source = (_EveryValueChanged)state;
                        source.completionSource.TrySetCanceled(source.cancellationToken);
                    }, this);
                }

                TaskTracker.TrackActiveTask(this, 2);
                PlayerLoopHelper.AddAction(monitorTiming, this);
            }

            public bool MoveNext()
            {
                if (disposed || targetAsUnityObject == null)
                {
                    completionSource.TrySetResult(false);
                    DisposeAsync().Forget();
                    return false;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    completionSource.TrySetCanceled(cancellationToken);
                    return false;
                }

                TProperty nextValue = default(TProperty);
                try
                {
                    nextValue = propertySelector(target);
                    if (equalityComparer.Equals(currentValue, nextValue))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    completionSource.TrySetException(ex);
                    DisposeAsync().Forget();
                    return false;
                }

                currentValue = nextValue;
                completionSource.TrySetResult(true);
                return true;
            }

            public TProperty Current => currentValue;

            public UniTask<bool> MoveNextAsync()
            {
                if (disposed) return CompletedTasks.False;

                completionSource.Reset();

                if (cancellationToken.IsCancellationRequested)
                {
                    completionSource.TrySetCanceled(cancellationToken);
                    return new UniTask<bool>(this, completionSource.Version);
                }

                if (first)
                {
                    first = false;
                    if (targetAsUnityObject == null)
                    {
                        return CompletedTasks.False;
                    }

                    this.currentValue = propertySelector(target);
                    return CompletedTasks.True;
                }

                return new UniTask<bool>(this, completionSource.Version);
            }

            public UniTask DisposeAsync()
            {
                if (!disposed)
                {
                    cancellationTokenRegistration.Dispose();
                    disposed = true;
                    TaskTracker.RemoveTracking(this);
                }

                return default;
            }
        }
    }

    internal sealed class EveryValueChangedStandardObject<TTarget, TProperty> : IUniTaskAsyncEnumerable<TProperty>
        where TTarget : class
    {
        readonly bool cancelImmediately;
        readonly IEqualityComparer<TProperty> equalityComparer;
        readonly PlayerLoopTiming monitorTiming;
        readonly Func<TTarget, TProperty> propertySelector;
        readonly WeakReference<TTarget> target;

        public EveryValueChangedStandardObject(TTarget target, Func<TTarget, TProperty> propertySelector,
            IEqualityComparer<TProperty> equalityComparer, PlayerLoopTiming monitorTiming, bool cancelImmediately)
        {
            this.target = new WeakReference<TTarget>(target, false);
            this.propertySelector = propertySelector;
            this.equalityComparer = equalityComparer;
            this.monitorTiming = monitorTiming;
            this.cancelImmediately = cancelImmediately;
        }

        public IUniTaskAsyncEnumerator<TProperty> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new _EveryValueChanged(target, propertySelector, equalityComparer, monitorTiming, cancellationToken,
                cancelImmediately);
        }

        sealed class _EveryValueChanged : MoveNextSource, IUniTaskAsyncEnumerator<TProperty>, IPlayerLoopItem
        {
            readonly CancellationToken cancellationToken;
            readonly CancellationTokenRegistration cancellationTokenRegistration;
            readonly IEqualityComparer<TProperty> equalityComparer;
            readonly Func<TTarget, TProperty> propertySelector;
            readonly WeakReference<TTarget> target;
            TProperty currentValue;
            bool disposed;

            bool first;

            public _EveryValueChanged(WeakReference<TTarget> target, Func<TTarget, TProperty> propertySelector,
                IEqualityComparer<TProperty> equalityComparer, PlayerLoopTiming monitorTiming,
                CancellationToken cancellationToken, bool cancelImmediately)
            {
                this.target = target;
                this.propertySelector = propertySelector;
                this.equalityComparer = equalityComparer;
                this.cancellationToken = cancellationToken;
                this.first = true;

                if (cancelImmediately && cancellationToken.CanBeCanceled)
                {
                    cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(state =>
                    {
                        var source = (_EveryValueChanged)state;
                        source.completionSource.TrySetCanceled(source.cancellationToken);
                    }, this);
                }

                TaskTracker.TrackActiveTask(this, 2);
                PlayerLoopHelper.AddAction(monitorTiming, this);
            }

            public bool MoveNext()
            {
                if (disposed || !target.TryGetTarget(out var t))
                {
                    completionSource.TrySetResult(false);
                    DisposeAsync().Forget();
                    return false;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    completionSource.TrySetCanceled(cancellationToken);
                    return false;
                }

                TProperty nextValue = default(TProperty);
                try
                {
                    nextValue = propertySelector(t);
                    if (equalityComparer.Equals(currentValue, nextValue))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    completionSource.TrySetException(ex);
                    DisposeAsync().Forget();
                    return false;
                }

                currentValue = nextValue;
                completionSource.TrySetResult(true);
                return true;
            }

            public TProperty Current => currentValue;

            public UniTask<bool> MoveNextAsync()
            {
                if (disposed) return CompletedTasks.False;

                completionSource.Reset();

                if (cancellationToken.IsCancellationRequested)
                {
                    completionSource.TrySetCanceled(cancellationToken);
                    return new UniTask<bool>(this, completionSource.Version);
                }

                if (first)
                {
                    first = false;
                    if (!target.TryGetTarget(out var t))
                    {
                        return CompletedTasks.False;
                    }

                    this.currentValue = propertySelector(t);
                    return CompletedTasks.True;
                }

                return new UniTask<bool>(this, completionSource.Version);
            }

            public UniTask DisposeAsync()
            {
                if (!disposed)
                {
                    cancellationTokenRegistration.Dispose();
                    disposed = true;
                    TaskTracker.RemoveTracking(this);
                }

                return default;
            }
        }
    }
}