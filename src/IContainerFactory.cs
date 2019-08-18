using System;

namespace Stator
{
    public interface IContainerFactory
    {
        object Resolve(Type type);
    }
}