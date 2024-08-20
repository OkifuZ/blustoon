using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll
{
    public class Weaponize : MonoBehaviour
    {
        public GameObject _character1;
        public GameObject _character2;

        private ActiveRagdoll _activeRagdoll1;
        private ActiveRagdoll _activeRagdoll2;

        private uint holdingAcID = 12345;
        
        
        // Start is called before the first frame update
        void Start()
        {
            _character1 = GameObject.Find("Character1");
            _character2 = GameObject.Find("Character2");
            _activeRagdoll1 = _character1.GetComponent<ActiveRagdoll>();
            _activeRagdoll2 = _character2 == null ? null : _character2.GetComponent<ActiveRagdoll>();
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void setMaster(uint id)
        {
            holdingAcID = id;
        }

        public void ReleaseWeapon()
        {
            holdingAcID = 12345;
        }
        
        private bool BodyInPassiveState(ActiveRagdoll ac)
        {
            return ac.isGettingUp ||
                   ac.beaten ||
                   ac.isGettingUpAnimated;
        }
        
        void OnCollisionEnter(Collision collision)
        {
            if (holdingAcID == 12345) return;
            foreach (ContactPoint contact in collision.contacts)
            {
                var go = contact.otherCollider.gameObject;
                if (_activeRagdoll1 != null &&  holdingAcID == _activeRagdoll1.ID)
                {
                    if (go.transform.IsChildOf(_activeRagdoll2.gameObject.transform))
                    {
                        if (!BodyInPassiveState(_activeRagdoll2))
                        {
                            _activeRagdoll2._hitState.hit = true;
                            _activeRagdoll2._hitState.initialVelocity += contact.impulse * 0.03f;
                            var rb = go.GetComponent<Rigidbody>();
                            if (rb != null)
                            {
                                rb.velocity += contact.impulse * 0.05f;
                            }
                        }
                    }
                }
                else if (_activeRagdoll1 != null && holdingAcID == _activeRagdoll2.ID)
                {
                    if (go.transform.IsChildOf(_activeRagdoll1.gameObject.transform))
                    {
                        if (!BodyInPassiveState(_activeRagdoll1))
                        {
                            _activeRagdoll1._hitState.hit = true;
                            _activeRagdoll1._hitState.initialVelocity += contact.impulse * 0.03f;
                            var rb = go.GetComponent<Rigidbody>();
                            if (rb != null)
                            {
                                rb.velocity += contact.impulse * 0.05f;
                            }
                        }
                    }
                }
            }
            
        }
    }

}

