using System;
using System.Collections.Generic;
using System.Text;

namespace Dapps.CqrsCore.Utilities
{
    public interface ICache<TK, T>
    {
        T this[TK key] { get; }
        T Get(TK key);
        bool TryGet(TK key, out T value);

        bool Exists(TK key);

        void Add(TK key, T value);
        void Add(TK key, T value, int timeout, bool restartTimer = false);
        void Remove(TK key);
        void Remove(Predicate<TK> pattern);

        void Clear();
        void Dispose();
    }
}
