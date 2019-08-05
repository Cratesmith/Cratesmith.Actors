//#define DEBUG_LOG


using Cratesmith.AssetIcons;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace Cratesmith.Actors
{
    /// <summary>
    /// A base type for scripts that are unique entities and own all the gameObjects and components beneath them
    /// </summary>
    [SelectionBase]
    public class Actor : MonoBehaviour, IUseAsPrefabIcon, IUseAsHeirarchyIcon
    {
    }
}