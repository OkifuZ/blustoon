using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ActiveRagdoll;


namespace ActiveRagdoll
{
    public class HammerWeapon : MonoBehaviour
    {
        public ActiveRagdoll targetCharacter;

        public float velocityThreshold = 10.0f;
        public float impulseHitScale = 1.0f;
        // Start is called before the first frame update
        void Start()
        {
                        
        }

        // Update is called once per frame
        void Update()
        {
            
        }
        
        void OnCollisionEnter(Collision collision)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                var go = contact.otherCollider.gameObject;
                if (go.transform.IsChildOf(targetCharacter.gameObject.transform))
                {
                    if (collision.relativeVelocity.magnitude > velocityThreshold)
                    {
                        Debug.Log(collision.relativeVelocity.magnitude);
                        targetCharacter._hitState.hit = true;
                        targetCharacter._hitState.initialVelocity -= contact.impulse * impulseHitScale;
                    }
                    var rb = go.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.velocity -= contact.impulse * impulseHitScale * 0.5f;
                    }
                }
                
            }
            
        }
        
        
    }
    
}
