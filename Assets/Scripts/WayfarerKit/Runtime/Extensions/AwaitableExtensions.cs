using System;
using System.Threading;
using UnityEngine;

namespace WayfarerKit.Extensions
{
    public static class AwaitableExtensions
    {
        public static async Awaitable WaitUntil(Func<bool> condition, CancellationToken cancellationToken = default)
        {
            while (!condition()) await Awaitable.EndOfFrameAsync(cancellationToken);
        }
    }
}