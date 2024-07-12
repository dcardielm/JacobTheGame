using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using Corndog.Interactables;
using JetBrains.Annotations;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace Corndog
{
    [System.Serializable]
    public class AttachmentPoint
    {
        [SerializeField] private PointName m_Name;
        [SerializeField] private Transform m_OffsetTransform;
        // [SerializeField] private PointType m_PointType;

        public PointName name => m_Name;
        public Transform offsetTransform => m_OffsetTransform;
        // public PointType pointType => m_PointType;

        public System.Action<GameObject> OnAttach;
        public System.Action<GameObject> OnDetach;

        private GameObject _child;
        public GameObject child => _child;

        public void Attach(GameObject obj, bool zeroOutTransform=true)
        {
            _child = obj;
            obj.transform.SetParent(m_OffsetTransform);
            
            if (zeroOutTransform)
            {
                obj.transform.localPosition = Vector3.zero;

                // TODO
                obj.transform.localRotation = Quaternion.identity; 
            }
        }

        public void Attach(GameObject obj, AttachmentPoint point)
        {
            _child = obj;
            obj.transform.SetParent(m_OffsetTransform);
            obj.transform.localPosition = Vector3.zero - point.offsetTransform.localPosition;

            // TODO
            obj.transform.localRotation = point.offsetTransform.localRotation;

            OnAttach?.Invoke(obj);
        }

        public void Detach()
        {
            if (_child != null)
            {
                _child.transform.SetParent(null);
                OnDetach?.Invoke(_child);
            }
            _child = null;
        }


        // Name comparison
        public static bool operator ==(PointName p, AttachmentPoint a) => p == a.name;
        public static bool operator ==(AttachmentPoint a, PointName p) => p == a.name;
        public static bool operator !=(PointName p, AttachmentPoint a) => p != a.name;
        public static bool operator !=(AttachmentPoint a, PointName p) => p != a.name;
        
        // Gameobject comparisons
        public static bool operator ==(GameObject g, AttachmentPoint a) => g.GetHashCode() == a._child.GetHashCode();
        public static bool operator ==(AttachmentPoint a, GameObject g) => g.GetHashCode() == a._child.GetHashCode();
        public static bool operator !=(GameObject g, AttachmentPoint a) => g.GetHashCode() != a._child.GetHashCode();
        public static bool operator !=(AttachmentPoint a, GameObject g) => g.GetHashCode() != a._child.GetHashCode();


        public static explicit operator PointName(AttachmentPoint a) => a.name;
        public static explicit operator GameObject(AttachmentPoint a) => a._child;
        public static explicit operator Transform(AttachmentPoint a) => a.offsetTransform;
        public static explicit operator string(AttachmentPoint a) => a.name.ToString();

        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode()
        {
            if (child == null)
            {
                return child.GetHashCode();
            }
            return 0;
        }

        public bool TryGetComponent<T>(out T component)
        {
            if (_child.TryGetComponent(out T c))
            {
                component = c;
                return true;
            }
            component = default;
            return false;
        }
    }


    // TODO, the naming convention of this class is hard to follow.
    // NOTE : Attachments fires off its own event to who is listening as well
    // as an event in the other Attachments. That way those who need to listen, can.
    [System.Serializable]
    public class Attachments
    {   
        [Tooltip("Attachment points on 'this' that others can attach to. 'Other' becomes the child of 'this'.")]
        [SerializeField] private AttachmentPoint[] m_AttachOtherToPoints;

        [Tooltip("Attachment points 'this' can attach to. 'This' becomes the child of the 'others' AttachOtherToPoints.")]
        [SerializeField] private AttachmentPoint[] m_AttachSelfToPoints;

        /// <summary>
        /// A child GameObject on 'this' has been attached.
        /// </summary>
        public System.Action<GameObject> OnChildAttached;

        /// <summary>
        /// A child GameObject on 'this' has been detached.
        /// </summary>
        public System.Action<GameObject> OnChildDetached;
        

        /// <summary>
        /// Fake index number for lookup tables, if returned, the lookup was false.
        /// </summary>
        private const int lookUpFalse = 9999;


        public void Enable()
        {
            foreach (var ap in m_AttachOtherToPoints)
            {
                ap.OnAttach += ForwardAttachMessage;
                ap.OnDetach += ForwardDetachMessage;
            }

            foreach (var ap in m_AttachSelfToPoints)
            {
                ap.OnAttach += ForwardAttachMessage;
                ap.OnDetach += ForwardDetachMessage;
            }
        }

        public void Disable()
        {
            foreach (var ap in m_AttachOtherToPoints)
            {
                ap.OnAttach -= ForwardAttachMessage;
                ap.OnDetach -= ForwardDetachMessage;
            }

            foreach (var ap in m_AttachSelfToPoints)
            {
                ap.OnAttach -= ForwardAttachMessage;
                ap.OnDetach -= ForwardDetachMessage;
            }
        }


        private void ForwardDetachMessage(GameObject gameObject)
        {
            OnChildDetached?.Invoke(gameObject);
        }

        private void ForwardAttachMessage(GameObject gameObject)
        {
            OnChildAttached?.Invoke(gameObject);
        }


        private static bool CompareGameObjects(AttachmentPoint[] attachmentPoints, GameObject obj)
        {
            foreach (var ap in attachmentPoints)
            {
                if ((GameObject)ap == obj)
                {
                    return true;
                }
            }
            return false;
        }

        private static int[] GenerateAttachedObjectsHashArray(AttachmentPoint[] attachmentPoints)
        {
            int[] hashs = new int[attachmentPoints.Length]; 

            for (int i = 0; i < attachmentPoints.Length; i++)
            {
                hashs[i] = attachmentPoints[i].GetHashCode();
            }
            return hashs;
        }

        private static int IndexOfGameObject(AttachmentPoint[] attachmentPoints, GameObject obj)
        {
            var target_hash = obj.GetHashCode();
            var hashs = GenerateAttachedObjectsHashArray(attachmentPoints);

            for (int i = 0; i < hashs.Length; i++)
            {
                if (hashs[i] == target_hash)
                {
                    return i;
                }
            }
            return lookUpFalse;
        }

        private static int IndexOfAttachment(AttachmentPoint[] attachmentPoints, AttachmentPoint attachmentPoint)
        {
            for (int i = 0; i < attachmentPoints.Length; i++)
            {
                if (attachmentPoints[i] == attachmentPoint)
                {
                    return i;
                }
            }

            return lookUpFalse;
        }

        private static bool ContainsPointName(AttachmentPoint[] attachmentPoints, PointName name)
        {
            foreach (var ap in attachmentPoints)
            {
                if (ap == name) { return true; }
            }
            return false;
        }

        private static AttachmentPoint GetAttachmentFrom(AttachmentPoint[] attachmentPoints ,PointName name)
        {
            foreach (var ap in attachmentPoints)
            {
                if (ap == name) { return ap; }
            }
            return null;
        }

        private static bool ComparePointNames(AttachmentPoint[] lhaps, AttachmentPoint[] rhaps)
        {
            for (int l = 0; l < lhaps.Length; l++)
            {
                for (int r = 0; r < rhaps.Length; r++)
                {
                    if ((PointName)lhaps[l] == (PointName)rhaps[r])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool ComparePointNames(AttachmentPoint[] lhaps, AttachmentPoint[] rhaps, out int lhIndex, out int rhIndex)
        {
            for (int l = 0; l < lhaps.Length; l++)
            {
                for (int r = 0; r < rhaps.Length; r++)
                {
                    if ((PointName)lhaps[l] == (PointName)rhaps[r])
                    {
                        lhIndex = l;
                        rhIndex = r;
                        return true;
                    }
                }
            }
            lhIndex = rhIndex = lookUpFalse;
            return false;
        }


    #region PublicAbilities

        // TODO
        public bool TryAttachObject(GameObject obj, PointName name)
        {
            return false;
        }

        // TODO, This needs work...
        /// <summary>
        /// Return's true on a successful attachment of the GameObject to 'this'.
        /// </summary>
        public bool TryAttachObject(GameObject obj)
        {
            if (obj.TryGetComponent(out IAttachable attachable))
            {
                var attachments = attachable.GetAttachments();

                if (ComparePointNames(m_AttachOtherToPoints, attachments.m_AttachSelfToPoints, out int cIndex, out int pIndex))
                {
                    var childPoint = m_AttachOtherToPoints[cIndex];

                    if ((GameObject)childPoint != obj)
                    {
                        if ((GameObject)childPoint != null)
                        {
                            // OnChildDetached?.Invoke(childPoint.child);
                            // attachments.OnChildDetached?.Invoke(childPoint.child);
                            childPoint.Detach();
                        }

                    attachments.OnChildAttached += OnChildAttached;
                    attachments.OnChildDetached += OnChildDetached;

                    childPoint.Attach(obj, attachments.m_AttachSelfToPoints[pIndex]);
                    // OnChildAttached?.Invoke(obj);
                    return true;
                    }
                }
            }
            return false;
        }

        public void SwapAttachedObjects(PointName n1, PointName p2)
        {
            // TODO, Swap the objects at these two points.
        }

        public void DetachObject(GameObject obj)
        {
            foreach (var ap in m_AttachOtherToPoints)
            {
                if ((GameObject)ap != null && ap == obj)
                {
                    // OnChildDetached?.Invoke(obj);
                    ap.Detach();
                }
            }
        }

        public GameObject DetachObject(PointName name)
        {
            var ap = GetAttachmentFrom(m_AttachOtherToPoints, name);
            var obj = (GameObject)ap;

            if (obj != null)
            {
                // OnChildDetached?.Invoke(obj);
                ap.Detach();
                return obj;
            }
            return null;
        }

        /// <summary>
        /// Take a GameObject as the reference. If the game object's hash is not found, GameObject will not be detached.
        /// <para>Returns true on the successful detachmant of GameObject from 'this'.</para>
        /// </summary>
        public bool TryDetachObject(GameObject obj)
        {
            foreach (var ap in m_AttachOtherToPoints)
            {
                if ((GameObject)ap != null && ap == obj)
                {
                    // OnChildDetached?.Invoke(obj);
                    ap.Detach();
                    return true;
                }
            }
            return false;
        }

    #endregion

    #region PublicGetValues    

        public bool ContainsPointName(PointName name)
        {
            return ContainsPointName(m_AttachOtherToPoints, name);
        }

        public int[] GetAttachmentPointHashs()
        {
            return GenerateAttachedObjectsHashArray(m_AttachOtherToPoints);
        }


        public string[] GetAttachmentPointNames()
        {
            string[] names = new string[m_AttachOtherToPoints.Length];

            for (int i = 0; i < m_AttachOtherToPoints.Length; i++)
            {
                names[i] = (string)m_AttachOtherToPoints[i];
            }
            return names;
        }

        public Transform GetAttachmentPointTransform(PointName name)
        {
             foreach (var ap in m_AttachOtherToPoints)
            {
                if ((PointName)ap == name)
                {
                    return (Transform)ap;
                }
            }
            return null;
        }

        public GameObject GetAttachedGameObject(PointName name)
        {
            foreach (var ap in m_AttachOtherToPoints)
            {
                if ((GameObject)ap != null && (PointName)ap == name)
                {
                    return (GameObject)ap;
                }
            }
            return null;
        }

        public GameObject[] GetAttachedGameObjects()
        {
            List<GameObject> objs = new List<GameObject>();

            foreach (var ap in m_AttachOtherToPoints)
            {
                if ((GameObject)ap != null)
                {
                    objs.Add((GameObject)ap);
                }
            }
            return objs.ToArray();
        }

        public GameObject[] GetAttachedGameObjects(PointName[] names)
        {
            List<GameObject> objs = new List<GameObject>();

            foreach (var ap in m_AttachOtherToPoints)
            {
                foreach (var n in names)
                {
                    if ((PointName)ap == n && (GameObject)ap != null)
                    {
                        objs.Add((GameObject)ap);
                    }
                }
            }
            return objs.ToArray();
        }

    #endregion
    }


    // TODO, make an enumerable AttachmentPoint (maybe)

    public enum PointName
    {
        CharacterHands,
        CharacterLeftHand,
        CharacterRightHand,
        CharacterBack,
        GunMuzzle, 
        GunSight,
        GunMagazine,
        DoesNotExist = 9999
    }

    public enum PointType
    {
        ParentPoint,
        ChildPoint,
        ParentOrChildPoint
    }
}

