using System.Collections;
using System.Collections.Generic;
using Corndog.Systems;
using JetBrains.Annotations;
using UnityEngine;

namespace Corndog.Interactables
{
    public interface IDefaultInteraction
    {
        void DefaultInteraction(GameObject obj);
    }

    public interface IAttachable
    {   
        Attachments GetAttachments();
    }

    public interface IInventory
    {
        InventorySystem GetInventorySystem();
    }
}
