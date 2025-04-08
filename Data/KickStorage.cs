namespace Kick.Data
{
    using System.Collections.Generic;
    using UnityEngine;

    internal static class KickStorage
    {
        public static readonly Dictionary<PlayerAvatar, Coroutine> BallCoroutines = new Dictionary<PlayerAvatar, Coroutine>();
    }
}
