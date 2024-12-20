using System.Threading;
using UnityEngine;

namespace WayfarerKit.UI.Base
{
    public interface IWidget
    {
        public Awaitable Show(CancellationToken token = default, bool immediate = false);
        public Awaitable Hide(CancellationToken token = default, bool immediate = false);
    }
}