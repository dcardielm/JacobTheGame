using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace Corndog.Utils
{
    [System.Serializable]
    public class RaycastUtil
    {
        [SerializeField] private LayerMask targetLayer;

        [Tooltip("Max distance is consumed on reflections.")]
        [SerializeField] private float m_MaxDistance = Mathf.Infinity;

        [Tooltip("Maximum number of reflections the ray will make.")]
        [SerializeField][Range(0, 10)] private int m_MaxReflections = 0;

        [Tooltip("If value is less than 0.01, the type of cast will be a standard Raycast, else it will cast a sphere.")]
        [SerializeField][Range(0f, 10f)] private float m_CastRadius = 0f;
        [SerializeField] private QueryTriggerInteraction queryTriggerInteraction;

        [Tooltip(@"How reflective the ray is against the sufurce of objects. 
            If value is zero, ray will pass directly through while still registering hits depending on reflection count.")]
        [SerializeField][Range(0f, 1f)] public float m_RayReflectivity;

        [Tooltip(@"Controls how the ray warps when passing through objects, 
            recommend not having all the way to '1', causes weird issues.
            If value is zero, no warping will occur while passing through objects.")]
        [SerializeField][Range(0f, 1f)] public float m_RayInternalWarp;

        private RaycastHit _hit;
        private List<RaycastHit> _raycastHits;

        public RaycastHit Hit => _hit;
        public RaycastHit[] Hits { get; private set; }

        
        private delegate bool CastType(Vector3 origin, Vector3 direction, float distance, out RaycastHit hit);
        private delegate void CastMode(Vector3 origin, Vector3 direction);
        private CastType castType;
        private CastMode castMode;
        
        
        /// <summary>
        /// Get/Set the rays max distance, clamped between 0.01f and float.MaxValue.
        /// Reflections and internal warps consume max distance.
        /// </summary>
        public float MaxDistance
        {
            get { return m_MaxDistance; }
            set
            {
                m_MaxDistance = Mathf.Clamp(value, 0.01f, float.MaxValue);
            }
        }
        
        /// <summary>
        /// Get/Set the cast radius, values above 0.01f will set the cast type to SphereCast.
        /// Values below 0.01f will set the cast type to Raycast.
        /// Value is clamped between 0f and 10f.
        /// </summary>
        public float CastRadius
        {
            get { return m_CastRadius; }
            set
            {
                m_CastRadius = Mathf.Clamp(value, 0f, 10f);
                castType = value < 0.01f ? CastRay : CastSphere;
            }
        }
        
        /// <summary>
        /// Get/Set the max reflections of the ray, value is clamped between 0 and 10.
        /// Internal warp reflections do not consume max reflections.
        /// </summary>
        public int MaxReflections
        {
            get { return m_MaxReflections; }
            set
            {
                m_MaxReflections = System.Math.Clamp(value, 0, 10);
                castMode = value > 0 ? ReflectiveRaycast : NonReflectiveCast;
            }
        }

        public void Enable()
        {
            _raycastHits = new List<RaycastHit>();
            m_MaxDistance = Mathf.Clamp(m_MaxDistance, 0.01f, float.MaxValue);
            m_CastRadius = Mathf.Clamp(m_CastRadius, 0f, 10f);
            castType = m_CastRadius < 0.01f ? CastRay : CastSphere;
            m_MaxReflections = System.Math.Clamp(m_MaxReflections, 0, 10);
            castMode = m_MaxReflections > 0 ? ReflectiveRaycast : NonReflectiveCast;
        }

        public void Reset()
        {
            Enable();
        }
        
        /// <summary>
        /// Allows automatic switching between raycasting modes based on set values.
        /// <para>Always results in updating a RaycastHit Array, the field "RaycastUtil.Hits[]".</para>
        /// <para>Modes = [ Reflective, NonReflective ]</para>
        /// </summary>
        public void AutoNegotiateRaycast(Vector3 origin, Vector3 direction)
        {
            castMode(origin, direction);
        }

        public Vector3 HitPoint()
        {
            return _hit.point;
        }

        public Vector3[] HitPoints()
        {
            Vector3[] points = new Vector3[_raycastHits.Count];

            for (int i = 0; i < _raycastHits.Count; i++)
            {
                points[i] = _raycastHits[i].point;
            }
            return points;
        }

        public Vector3[] HitPoints(Vector3 origin)
        {
            Vector3[] points = new Vector3[_raycastHits.Count + 1];

            points[0] = origin;

            for (int i = 0; i < _raycastHits.Count; i++)
            {
                points[i + 1] = _raycastHits[i].point;
            }
            return points;
        }

        public Vector3[] HitPoints(Vector3 origin, Vector3 offset)
        {
            Vector3[] points = new Vector3[_raycastHits.Count + 1];

            points[0] = origin;

            for (int i = 0; i < _raycastHits.Count; i++)
            {
                points[i + 1] = _raycastHits[i].point + offset;
            }
            return points;
        }


        public bool CastRay(Vector3 origin, Vector3 direction, float distance, out RaycastHit hit)
        {
            if (Physics.Raycast(origin, direction, out hit, distance, targetLayer, queryTriggerInteraction))
            {
                return true;
            }
            return false;
        }

        public bool CastSphere(Vector3 origin, Vector3 direction, float distance, out RaycastHit hit)
        {
            if (Physics.SphereCast(origin, m_CastRadius, direction, out hit, distance, targetLayer, queryTriggerInteraction))
            {
                return true;
            }
            return false;
        }

        public void NonReflectiveCast(Vector3 origin, Vector3 direction)
        {
            RaycastHit hit;

            if (castType(origin, direction, m_MaxDistance, out hit))
            {
                Hits = new RaycastHit[] {hit}; 
            }
        }

        public void ReflectiveRaycast(Vector3 origin, Vector3 direction)
        {
            _raycastHits.Clear();

            var current_origin = origin;
            var current_direction = direction;
            var current_distance = m_MaxDistance;
            
            for (int i = 0; i < m_MaxReflections + 1; i++)
            {
                // new surface hitinfo
                RaycastHit surface_hit;

                // Check for sureface hit
                if (castType(current_origin, current_direction, current_distance, out surface_hit))
                {
                    // Get a new direction based on reflectivity
                    Vector3 new_direction = DampReflect(current_direction, surface_hit.normal, m_RayReflectivity);

                    // Add surface hit to list
                    _raycastHits.Add(surface_hit);

                    // Get the distance from the hit collider normal to the other side of collider normal
                    float internal_distance = surface_hit.collider.bounds.SqrDistance(surface_hit.normal) * 1.1f;

                    // Get the position just on the opposite side of the collider normal
                    Vector3 opposite_hit_origin = surface_hit.point + (direction * internal_distance);

                    // new "internal" hitinfo
                    RaycastHit internal_hit;

                    // Raycast "through" the hit surface
                    if (Physics.Raycast(opposite_hit_origin, -new_direction.normalized, out internal_hit, internal_distance, targetLayer))
                    {
                        _raycastHits.Add(internal_hit);

                        current_origin = internal_hit.point;
                        current_direction = DampReflect(new_direction, internal_hit.normal, m_RayInternalWarp);
                        current_distance = ClampDistance(current_distance, surface_hit.distance + internal_distance);
                    }
                    else
                    {
                        current_origin = surface_hit.point;
                        current_direction = new_direction;
                        current_distance = ClampDistance(current_distance, surface_hit.distance);
                    }
                }
            }

            if (_raycastHits.Count > 0) { Hits = _raycastHits.ToArray(); }
        }


        private float ClampDistance(float current_value, float value)
        {
            if (current_value - value < 0f) { return 0f; }
            if (current_value - value > m_MaxDistance) { return m_MaxDistance; }
            return current_value - value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 DampReflect(Vector3 inDirection, Vector3 inNormal, float value)
        {
            if (value < 0f) { value = 0f; }
            if (value > 1f) { value = 1f; }

            float num = -2f * (inDirection.x * inNormal.x + inDirection.y * inNormal.y + inDirection.z * inNormal.z) * value;
            return new Vector3(num * inNormal.x + inDirection.x, num * inNormal.y + inDirection.y, num * inNormal.z + inDirection.z);
        }


        [System.Obsolete("Requires rework, do not use for now.", true)]
        public static RaycastHit[] MultiRaycast(Vector3 origin, Vector3 direction, int m_MaxReflections,
            float m_MaxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            List<RaycastHit> hits = new List<RaycastHit>();

            var current_distance = m_MaxDistance;
            var current_origin = origin;
            var current_direction = direction;

            for (int i = 0; i < m_MaxReflections + 1; i++)
            {
                RaycastHit hit;

                if (Physics.Raycast(current_origin, current_direction, out hit, current_distance, layerMask, queryTriggerInteraction))
                {
                    hits.Add(hit);

                    current_direction = Vector3.Reflect(current_direction, hit.normal);
                    current_origin = hit.point;
                    current_distance = Mathf.Clamp(current_distance - hit.distance, 0f, m_MaxDistance);
                }
            }

            return hits.ToArray();
            
        }


        [System.Obsolete("Requires rework, do not use for now.", true)]
        public static RaycastHit[] MultiSphereCast(Vector3 origin, Vector3 direction, int m_MaxReflections, float radius,
            float m_MaxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            List<RaycastHit> hits = new List<RaycastHit>();

            var current_distance = m_MaxDistance;
            var current_origin = origin;
            var current_direction = direction;

            for (int i = 0; i < m_MaxReflections + 1; i++)
            {
                RaycastHit hit;

                if (Physics.SphereCast(current_origin, radius, current_direction, out hit, current_distance, layerMask, queryTriggerInteraction))
                {
                    hits.Add(hit);

                    current_direction = Vector3.Reflect(current_direction, hit.normal);
                    current_origin = hit.point;
                    current_distance = Mathf.Clamp(current_distance - hit.distance, 0f, m_MaxDistance);
                }
            }

            return hits.ToArray();
        }
    } 
}