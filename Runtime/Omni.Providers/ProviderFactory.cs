#nullable enable

using System;
using UnityEngine;
using Omni.Providers.Interfaces;

namespace Omni.Providers
{
  public class ProviderFactory<TConcrete> : IProviderFactory
  where TConcrete : new()
  {
    private bool _isSingleton = true;
    private Func<TConcrete>? _singletonConstructor;
    private Func<TConcrete>? _transientConstructor;

    private TConcrete? ConstructSingleton()
    {
      return _singletonConstructor is null ? default : _singletonConstructor.Invoke();
    }

    private TConcrete? ConstructTransient()
    {
      return _transientConstructor is null ? default : _transientConstructor.Invoke();
    }

    public IProviderFactory AsSingleton()
    {
      TConcrete singleton = new();
      return AsSingleton(() => singleton);
    }

    public IProviderFactory AsSingleton<T>(T singletonValue)
    {
      return AsSingleton(() => singletonValue);
    }

    public IProviderFactory AsSingleton<T>(Func<T> constructor)
    {
      if (typeof(T) != typeof(TConcrete))
      {
        Debug.LogError($"{typeof(T)} does not match {typeof(TConcrete)} in singleton creation");
        Application.Quit();
      }

      _isSingleton          = true;
      _singletonConstructor = constructor as Func<TConcrete>;
      return this;
    }

    public IProviderFactory AsTransient()
    {
      return AsTransient(() => new TConcrete());
    }

    public IProviderFactory AsTransient<T>(Func<T> constructor)
    {
      if (typeof(T) != typeof(TConcrete))
      {
        Debug.LogError($"{typeof(T)} does not match {typeof(TConcrete)} in transient creation");
        Application.Quit();
      }

      _isSingleton          = false;
      _transientConstructor = constructor as Func<TConcrete>;
      return this;
    }

    public TResolved ResolveToTypedValue<TResolved>()
    where TResolved : class
    {
      return ((_isSingleton ? ConstructSingleton() : ConstructTransient()) as TResolved)!;
    }
  }
}