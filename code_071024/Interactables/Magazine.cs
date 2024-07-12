using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Corndog.GameplayTriggers;
using Unity.VisualScripting;
using UnityEngine;
using Corndog.Ref;


namespace Corndog.Interactables
{

    /*
        Concerns, 
            - current amount of ammo
            - effects of ammo on hit
            - prefab used
            - should hold both the max ammo and the effects
                - if say armor piercing or fire bullets
    */ 
    public class Magazine : InteractableObject
    {
        [SerializeField] private Transform m_GunAttachmentPoint;
        [SerializeField] private AmmoType m_AmmoType;
        [SerializeField] private int m_MaxCapacity;
        [SerializeField] private PrefabPool m_PrefabPool;
        [SerializeField] private float m_TravelSpeedMultiplier = 1f;
        [SerializeField] private float m_Damage;
        [SerializeField] private float m_Knockback;
        [SerializeField] private float m_EffectRadius;
        [SerializeField] AudioClip[] m_ImpactClips;

        public float TravelSpeedMultiplier => m_TravelSpeedMultiplier;

        public bool CompareAmmoType(AmmoType type)
        {
            return type == m_AmmoType;
        }

        private int _current;
        public int ammo 
        {
            get { return _current; }
            set { _current = value; }
        }

        public int maxAmmo => m_MaxCapacity;


        public PrefabPool Bullet
        {
            get { return m_PrefabPool; }
        }



        public GameObject GetInstance()
        {
            return m_PrefabPool.GetInstance(_gun.t_Muzzle.position, Quaternion.identity);
        }

        private Gun _gun;



        public override void DefaultInteraction(GameObject caller)
        {   
            Initiator = caller;
            ToStorage(caller);
        }

        public override void ToStorage(GameObject caller)
        {
            if (caller.TryGetComponent(out Inventory inventory))
            {
                //Todo, invenetory 
            }
        }

        public void Attach(Gun gun)
        {
            _gun = gun;
            m_PrefabPool.Enable(_gun.t_Muzzle);
            _gun.magazine = this;
        }

        public void Detach(Gun gun)
        {
            m_PrefabPool.Disable(_gun.t_Muzzle);

        }

        public void AttachTo(Transform transform_)
        {
            transform.SetParent(transform_);
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }


        public override void Attach(GameObject caller)
        {
            if (caller.TryGetComponent(out Gun gun))
            {
                
            }
        }


        public void AmmoHitEffect(RaycastHit hit)
        {
            // do damage, really dont need an interface for this
        }
    }

}

