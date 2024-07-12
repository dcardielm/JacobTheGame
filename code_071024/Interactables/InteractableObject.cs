using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;

namespace Corndog.Interactables
{
    public abstract class InteractableObject : MonoBehaviour
    {

        /*  The core an interactable is that it is an object the player can either
            - Trigger 
            - attach
            - store
            Right now, things seems okay, however some of these systems already seem flimsy and look like issues will happen down the road.
            Maybe another class, the Observer could just be something that repeats events within interactables to establish communications?
            The way the interactables are setting themselves up to subscribe to specific events seems good.


        */
        public GameObject Initiator { get; protected set; }
        public virtual void DefaultInteraction(GameObject caller){ Initiator = caller; Debug.LogWarning($"No override found for DefaultInteraction(GameObject [{caller}])."); }
        public virtual void Attach(GameObject caller) { Initiator = caller; Debug.LogWarning($"No override found for Attach(GameObject [{caller}])."); }
        public virtual void Detach(GameObject caller) { Initiator = caller; Debug.LogWarning($"No override found for Detach(GameObject [{caller}])."); }
        public virtual void ToStorage(GameObject caller) { Initiator = caller; Debug.LogWarning($"No override found for ToStorage(GameObject [{caller}])."); }
        public virtual void FromStorage(GameObject caller) { Initiator = caller; Debug.LogWarning($"No override found for FromStorage(GameObject [{caller}])."); }

        public virtual void Attach(Attachments attachments) { Debug.LogWarning("No override found for Attach(Attachments attachments)" ); }

        public virtual void Attach() { Debug.LogWarning("No override found for Attach()."); }
        public virtual void Detach() { Debug.LogWarning("No override found for Detach()."); }
        public virtual void ToStorage() { Debug.LogWarning("No override found for ToStorage()."); }
        public virtual void FromStorage() { Debug.LogWarning("No override found for FromStorage()."); }

        /// <summary>
        /// Actions to perform when the pointer hovers over the object.
        /// </summary>
        public virtual void PointerHover(bool isHovering) { Debug.LogWarning($"No override found for PointerHover(bool [{isHovering}])."); }

        /// <summary>
        /// Actions to perform when the pointer selects the object.
        /// </summary>
        public virtual void PointerSelect(bool isSelected) { Debug.LogWarning($"No override found for PointerSelect(bool [{isSelected}])."); }

    }
}




