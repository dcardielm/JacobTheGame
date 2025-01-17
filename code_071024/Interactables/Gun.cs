using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Corndog.GameplayTriggers;
using Corndog.Ref;
using Corndog.Systems;
using JetBrains.Annotations;
using UnityEditor.EditorTools;
using UnityEngine;


/*
    Events
        - plugs into CharacterTriggers, fire, reload, toInventory, drop events
        - plugs into Inventory Add, Remove events
        - Default interaction is attach
*/


namespace Corndog.Interactables
{   
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(Attachments))]
    public class Gun : MonoBehaviour, IDefaultInteraction
    {   
        [SerializeField] Attachments gunAttachments;

        [Tooltip("Speed of the bullets when fired.")]
        [SerializeField] private float m_TravelSpeed;

        [Tooltip("Rate of fire in bullets per second.")]
        [SerializeField] private float m_FireRate;

        [Tooltip("The type of ammo this gun will accept.")]
        [SerializeField] private AmmoType m_AmmoType;

        [Tooltip("Audio clips to play on Fire.")]
        [SerializeField] private AudioClip[] m_FireAudio;

        [Tooltip("Audio clips to play on Fire when no ammo is available.")]
        [SerializeField] private AudioClip[] m_EmptyAudio;
        
        [Tooltip("Raycasting Properties.")]
        [SerializeField] private RaycastUtil m_RaycastUtil;
    
        // Components
        private AudioSource _audioSource;
        private Magazine _magazine;
        private InventorySystem _inventory;

        public Magazine magazine
        {
            get { return magazine; }
            set 
            { 
                magazine = value; 

                if (value != null)
                {
                    travelSpeed = m_TravelSpeed * magazine.TravelSpeedMultiplier;
                }
            }
        }   

        // private fields
        private float travelSpeed;
        private float _nextFireTime = 0f;
        private bool _reloadRunning;
        private GameObject _initiator;


        // public fields
        public Transform t_Muzzle { get; private set; }
        public Transform t_Magazine { get; private set; }


        void Start()
        {
            t_Muzzle = gunAttachments.GetAttachmentPointTransform(PointName.GunMuzzle);
            t_Magazine = gunAttachments.GetAttachmentPointTransform(PointName.GunMagazine);
        }


    #region EventPlugins
        void OnEnable()
        {
            // Subscribe to own attachments messages.
            gunAttachments.OnChildAttached += HandleAttachment;
            gunAttachments.OnChildDetached += HandleDetachment;
        }

        void OnDisable()
        {
            // Unsubscribe to own attachments messages.
            gunAttachments.OnChildAttached -= HandleAttachment;
            gunAttachments.OnChildDetached += HandleDetachment;
        }

        private void HandleAttachment(GameObject obj)
        {
            // Message generated by the players attachments
            if (obj == gameObject)
            {
                // Subscribe to players attachment messages
                if (_initiator.TryGetComponent(out Attachments attachments))
                {
                    attachments.OnChildAttached += HandleAttachment;
                    attachments.OnChildDetached += HandleDetachment;
                }

                // Subscribe from players triggers.
                if (_initiator.TryGetComponent(out CharacterTriggers triggers))
                {
                    triggers.OnFire += Fire;
                    triggers.OnReload += Reload;
                    triggers.OnDrop += Drop;
                }    
                
                // Subscribe to player inventory messages.
                if (_initiator.TryGetComponent(out InventorySystem inventory))
                {
                    inventory.OnStoreObject += HandleToStorage;
                    inventory.OnUnstoreObject += HandleFromStorage;
                }
            }

            if (obj.CompareTag("GunMagazine"))
            {
                // todo
            }

            if (obj.CompareTag("GunMod"))
            {
                // todo
            }
        }

        private void HandleDetachment(GameObject obj)
        {
            // Message generated by the players attachments
            if (obj == gameObject)
            {
                // Unsubscribe from players attachment messages
                // if (_initiator.TryGetComponent(out Attachments attachments))
                // {
                //     attachments.OnChildAttached -= HandleAttachment;
                //     attachments.OnChildDetached -= HandleDetachment;
                // }

                // Unsubscribe from players triggers.
                if (_initiator.TryGetComponent(out CharacterTriggers triggers))
                {
                    triggers.OnFire -= Fire;
                    triggers.OnReload -= Reload;
                    triggers.OnDrop -= Drop;
                }

                // Unbscribe to player inventory messages.
                // if (_initiator.TryGetComponent(out InventorySystem inventory))
                // {
                //     inventory.OnStoreObject -= HandleToStorage;
                //     inventory.OnUnstoreObject -= HandleFromStorage;
                // }
            }

            if (obj.CompareTag("GunMagazine"))
            {
                // todo
            }

            if (obj.CompareTag("GunMod"))
            {
                // todo
            }
        }

        private void HandleToStorage(GameObject obj)
        {
            if (obj == gameObject)
            {
                
            }

            if (obj.CompareTag("GunMagazine"))
            {
                // todo
            }

            if (obj.CompareTag("GunMod"))
            {
                // todo
            }
        }

        private void HandleFromStorage(GameObject obj)
        {
            if (obj == gameObject)
            {

            }

            if (obj.CompareTag("GunMagazine"))
            {
                // todo
            }

            if (obj.CompareTag("GunMod"))
            {
                // todo
            }
        }

        public void DefaultInteraction(GameObject initiator)
        {
            Attach(initiator);
        }

        public void Attach(GameObject initiator)
        {
            if (initiator.TryGetComponent(out Attachments attachments) && attachments.TryAttachObject(gameObject))
            {            
                _initiator = initiator;

                HandleAttachment(gameObject);
            }
        }
    
    #endregion

    #region Abilities

        private void Fire()
        {
            if (Time.time > _nextFireTime)
            {
                // null check magazine... as it is object
                if (_magazine?.ammo > 0)
                {   
                    // AudioVisual 
                    _audioSource.PlayOneShot(RandomClip(m_FireAudio));
                    
                    // Raycasting
                    m_RaycastUtil.AutoNegotiateRaycast(t_Muzzle.position, t_Muzzle.forward);
                    RaycastHit[] hits = m_RaycastUtil.Hits;

                    // Object travel
                    var projectile_obj = _magazine.Bullet.GetInstance(t_Muzzle.position, Quaternion.identity);

                    switch (hits.Length)
                    {
                        case 0:
                            StartCoroutine(TravelToPoint(projectile_obj, t_Muzzle.forward * 100f));
                            break;
                        case 1:
                            StartCoroutine(TravelToHit(projectile_obj, hits[0]));
                            break;
                        default:
                            StartCoroutine(TravelToHits(projectile_obj, hits));
                            break;
                    }

                    // Finalization
                    _nextFireTime = Time.time + (60.0f / m_FireRate / 60.0f);
                   _magazine.ammo -= 1;
                }

                // DO NOT auto reload, must be called manually.
                else if (!_reloadRunning)
                {
                    // play no ammo audio if not playing
                    // firing with no ammo takes more time
                    _nextFireTime = Time.time + (180.0f / m_FireRate / 60.0f);
                }
            }
        } 

        // TODO
        private void Reload()
        {
            if (!_reloadRunning)
            {
                Magazine[] mags = _inventory.GetObjectsByType<Magazine>();

                foreach (Magazine mag in mags)
                {
                    if (mag.CompareAmmoType(m_AmmoType))
                    {
                        if (_magazine != null)
                        {
                            _magazine.Detach(this);
                            _inventory.AddToInventory(mag.gameObject);
                        }
                        _inventory.RemoveFromInventory(mag.gameObject);
                        mag.Attach(this);

                        return;
                    }
                }
            }
        }

        // TODO
        private void Drop()
        {
            // var force = Mathf.InverseLerp(0.05f, F_Constant.maxThrowInputTime, power);
        
            // GetComponent<Rigidbody>().AddForce(
            //     direction * Mathf.Lerp(1f, maxThrowForce, force), ForceMode.Impulse);

            // Reset();
        }

        // private void HandleMagazinePickUp(GameObject gameObject)
        // {
        //     if (gameObject.TryGetComponent(out Magazine mag))
        //     {
        //         if (magazine == null)
        //         {
        //             mag.AttachTo(m_MagazineAttachmentPoint);
        //         }
        //     }
   

        // }

    #endregion 
    
    #region Utilities
        
        // Travel to a single point.
        private IEnumerator TravelToHit(GameObject obj, RaycastHit hit)
        {
            yield return StartCoroutine(ToPoint(obj.transform, hit.point));
            _magazine.AmmoHitEffect(hit);
        }
        
        // Travel to multiple hit points, process hit actions and return obj to prefab pool.
        private IEnumerator TravelToHits(GameObject obj, RaycastHit[] hits)
        {
            int last_hash = 0;

            for (int i = 0; i < hits.Length; i++)
            {   
                int current_hash = hits[i].collider.GetHashCode();

                yield return StartCoroutine(ToPoint(obj.transform, hits[i].point));

                if (current_hash != last_hash)
                {
                    //TODO do on hit shit.
                    last_hash = current_hash;
                }
            }

            // TODO
            _magazine.Bullet.ReturnInstanceToPool(obj, t_Muzzle);
        }

        // Travel to a given point, don't process hit actions. Return obj to pool.
        private IEnumerator TravelToPoint(GameObject obj, Vector3 point)
        {
            yield return StartCoroutine(ToPoint(obj.transform, point));

            _magazine.Bullet.ReturnInstanceToPool(obj, t_Muzzle);
        }

        // Travel to a given point, timing depends on travel speed.
        private IEnumerator ToPoint(Transform objTransform, Vector3 point)
        {
            float arrival_time = Time.time + (point - objTransform.position).magnitude / travelSpeed;

            Vector3 direction = (point - objTransform.position).normalized;

            while (Time.time <= arrival_time)
            {
                objTransform.Translate(direction * travelSpeed * Time.deltaTime);
                yield return null;
            }
        }

        // TODO
        // private IEnumerator Reloading()
        // {
        //     _reloadRunning = true;
        //     // float end_time = Time.time + m_ReloadTime;

        //     while (Time.time < end_time)
        //     {
        //         //TODO do reload shit
        //         yield return null;
        //     }

        //     reserveMags -= 1;
        //     _currentAmmo = maxProjectiles;
        //     _reloadRunning = false;

        // }

        private AudioClip RandomClip(AudioClip[] clips)
        {
            return clips[(int)Random.Range(0f, clips.Length)];
        }



    #endregion

    #region Overrides


        // // TODO
        // public override void Attach()
        // {
        //     // if (Initiator.TryGetComponent(out Attachments attachments) 
        //     //     && attachments.TryAttachGameObjectToSlot(gameObject, attachTo))
        //     // {
        //         var abilities = Initiator.GetComponent<CharacterTriggers>();
        //         abilities.OnFire += Fire;
        //         // abilities.OnDrop += Drop;
        //         // abilities.OnReload += GunReload;
        //     // }
            
        // }
        
        // // TODO
        // public override void Detach()
        // {
        //     Initiator.GetComponent<GameplayAbilites>().OnFire -= Fire;
        // }

        // // TODO
        // public override void ToStorage()
        // {
        //     // var abilities = Initiator.GetComponent<GameplayAbilites>();
        //     // abilities.OnFire -= Fire;
        //     // abilities.OnDrop -= Drop;
        //     // abilities.OnReload -= GunReload;

        //     // gameObject.SetActive(false);
        // }

        #endregion
    }

    public class DefaultGunState : EventState<Gun>
    {

        public DefaultGunState(Gun caller) : base(caller) {}
        public override void Enter()
        {
            
        }

        public override void Exit()
        {
        }
    }
}

