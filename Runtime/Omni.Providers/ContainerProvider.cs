#nullable enable

using System;
using System.Collections.Generic;
using Omni.Providers.Interfaces;
using UnityEngine;

namespace Omni.Providers
{
  public abstract class ContainerProvider : MonoBehaviour
  {
    private static Dictionary<Type, IProviderFactory> _globalProviders = new();
    private Dictionary<Type, IProviderFactory> _localProviders = new();

    protected ProviderFactory<TConcrete> Bind<TInterface, TConcrete>()
    where TInterface : class
    where TConcrete : TInterface, new()
    {
      ProviderFactory<TConcrete> provider = new();
      _globalProviders[typeof(TInterface)] = provider;
      return provider;
    }

    protected ProviderFactory<TConcrete> LocalBind<TInterface, TConcrete>()
    where TInterface : class
    where TConcrete : TInterface, new()
    {
      ProviderFactory<TConcrete> provider = new();
      _localProviders[typeof(TInterface)] = provider;
      return provider;
    }

    public TInterface? GetLocalInstanceOf<TInterface>()
    where TInterface : class
    {
      if (_localProviders.ContainsKey(typeof(TInterface)))
        return _localProviders[typeof(TInterface)].ResolveToTypedValue<TInterface>();
      return null;
    }

    public static TInterface? GetInstanceOf<TInterface>()
    where TInterface : class
    {
      if (_globalProviders.ContainsKey(typeof(TInterface)))
        return _globalProviders[typeof(TInterface)].ResolveToTypedValue<TInterface>();
      return null;
    }
  }
}