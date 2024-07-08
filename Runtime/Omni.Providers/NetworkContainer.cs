using Omni.Providers.Interfaces;
using Unity.Netcode;

namespace Omni.Providers
{
  public abstract class NetworkContainer : NetworkBehaviour, IContainerBehaviour
  {
    private ContainerProvider _container = new();
    public ContainerProvider Container => _container;
    
    public override void OnNetworkSpawn()
    {
      OnBind(_container);
      if (IsServer)
        OnServerBind(_container);
      if (IsOwner)
        OnOwnerBind(_container);
    }

    protected virtual void OnOwnerBind(ContainerProvider container)
    {
      
    }

    protected virtual void OnServerBind(ContainerProvider container)
    {
      
    }

    protected virtual void OnBind(ContainerProvider container)
    {
      
    }
  }
}