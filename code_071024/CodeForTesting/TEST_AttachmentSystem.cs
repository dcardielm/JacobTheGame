using System.Collections;
using System.Collections.Generic;
using Corndog.Interactables;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

namespace Corndog.Testing
{
    public class TEST_AttachmentSystem : MonoBehaviour, IAttachable
    {

        public bool isController = false;
        [SerializeField] Attachments attachments;
        public GameObject target;

        void Start()
        {
            attachments.Enable();

            attachments.OnChildAttached += HandleAttach;
            attachments.OnChildDetached += HandleDetach;
        }

        void Update()
        {
            if (isController)
            {
                if (Input.GetKeyUp(KeyCode.A))
                {
                    attachments.TryAttachObject(target);
                }

                if (Input.GetKeyUp(KeyCode.G))
                {
                    attachments.DetachObject(target);

                    target.transform.position = Vector3.zero;
                }
                
            }
        }

        void HandleAttach(GameObject obj)
        {
            print($"from : {attachments.GetHashCode()} >>> {obj.name} attached!");
        }

        void HandleDetach(GameObject obj)
        {
            print($"from : {attachments.GetHashCode()} >>> {obj.name} detached!");
        }

        public Attachments GetAttachments()
        {
            return attachments;
        }

    }
}

