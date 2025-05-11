using System;
using System.Collections.Generic;
using System.Threading;

namespace Entities.Utilities
{
    public sealed class TokenStore
    {
        TokenStore() { }
        private static readonly object oLock = new object();
        private static TokenStore instance = null;
        public static Dictionary<Guid, CancellationTokenSource> Store = new Dictionary<Guid, CancellationTokenSource>();

        public static TokenStore Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (oLock)
                    {
                        if (instance == null)
                        {
                            instance = new TokenStore();
                        }
                    }
                }
                return instance;
            }
        }
    }
}
