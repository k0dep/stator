using System;

namespace Stator
{
    public class StatorUnresolvedException : Exception
    {
        public Type ResolveType { get; set; }

        public StatorUnresolvedException(Type resolveType)
        {
            ResolveType = resolveType;
        }
    }
}