using System.Threading;
using UnityEngine;

namespace WayfarerKit.Helpers.UI
{
    public interface IToolkitWidget
    {
        public Awaitable Show(CancellationToken token = default);
        public Awaitable Hide(CancellationToken token = default);
    }
}