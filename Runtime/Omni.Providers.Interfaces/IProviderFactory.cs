using System;

namespace Omni.Providers.Interfaces
{
  public interface IProviderFactory
  {
    IProviderFactory AsSingleton();
    IProviderFactory AsSingleton<T>(T singletonValue);
    IProviderFactory AsSingleton<T>(Func<T> constructor);
    IProviderFactory AsTransient();
    IProviderFactory AsTransient<T>(Func<T> constructor);
    T                ResolveToTypedValue<T>() where T : class;
  }
}