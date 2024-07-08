using System;
using Omni.Providers.Interfaces;
using UnityEngine;

namespace Omni.Providers
{
  public class MonoContainer : MonoBehaviour, IContainerBehaviour
  {
    private ContainerProvider _container = new();
    public ContainerProvider Container => _container;
  }
}