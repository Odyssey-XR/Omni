#nullable enable

using System;
using System.Collections.Generic;
using Omni.Providers.Interfaces;
using UnityEngine;

namespace Omni.Providers
{
  public class ContainerProvider
  {
    public static Dictionary<Type, IProviderFactory> GlobalProviders { get; } = new();
    public        Dictionary<Type, IProviderFactory> LocalProviders  { get; } = new();

    public ProviderFactory<TConcrete> Bind<TInterface, TConcrete>()
    where TInterface : class
    where TConcrete : TInterface, new()
    {
      if (GlobalProviders.ContainsKey(typeof(TInterface)))
        return (GlobalProviders[typeof(TInterface)] as ProviderFactory<TConcrete>)!;

      ProviderFactory<TConcrete> provider = new();
      GlobalProviders[typeof(TInterface)] = provider;
      return provider;
    }

    public ProviderFactory<TConcrete> LocalBind<TInterface, TConcrete>()
    where TInterface : class
    where TConcrete : TInterface, new()
    {
      if (LocalProviders.ContainsKey(typeof(TInterface)))
        return (LocalProviders[typeof(TInterface)] as ProviderFactory<TConcrete>)!;

      ProviderFactory<TConcrete> provider = new();
      LocalProviders[typeof(TInterface)] = provider;
      return provider;
    }

    public TInterface? GetLocalInstanceOf<TInterface>()
    where TInterface : class
    {
      if (LocalProviders.ContainsKey(typeof(TInterface)))
        return LocalProviders[typeof(TInterface)].ResolveToTypedValue<TInterface>();
      return null;
    }

    public static TInterface? GetInstanceOf<TInterface>()
    where TInterface : class
    {
      if (GlobalProviders.ContainsKey(typeof(TInterface)))
        return GlobalProviders[typeof(TInterface)].ResolveToTypedValue<TInterface>();
      return null;
    }

    public static TInterface? GetInstanceOf<TInterface>(GameObject? gameObject)
    where TInterface : class
    {
      if (gameObject is null)
      {
        if (GlobalProviders.ContainsKey(typeof(TInterface)))
          return GlobalProviders[typeof(TInterface)].ResolveToTypedValue<TInterface>();
        return null;
      }

      IContainerBehaviour containerBehaviour = gameObject.GetComponent<MonoContainer>() ??
                                               (IContainerBehaviour)gameObject.GetComponent<NetworkContainer>();
      if (containerBehaviour is null)
        return GetInstanceOf<TInterface>(gameObject.transform.parent.gameObject);

      TInterface? component = containerBehaviour.Container.GetLocalInstanceOf<TInterface>();
      if (component is not null)
        return component;
      return GetInstanceOf<TInterface>(gameObject.transform.parent?.gameObject ?? null);
    }
  }
}