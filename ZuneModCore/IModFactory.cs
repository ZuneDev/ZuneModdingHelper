using Microsoft.Extensions.DependencyInjection;
using System;

namespace ZuneModCore;

public interface IModFactory<out T> where T : Mod
{
    ModMetadata Metadata { get; }

    T Create(IServiceProvider services);
}

/// <summary>
/// A base class that implements <see cref="IModFactory{T}"/> with
/// <see cref="Create(IServiceProvider)"/> using dependency injection.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class DIModFactoryBase<T> : IModFactory<T> where T : Mod
{
    public abstract ModMetadata Metadata { get; }

    public virtual T Create(IServiceProvider services)
    {
        return ActivatorUtilities.CreateInstance<T>(services, Metadata);
    }
}
