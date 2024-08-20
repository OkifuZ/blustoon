using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesController: MonoBehaviour{
    public Color paintColor;
    
    public float minRadius = 0.05f;
    public float maxRadius = 0.2f;
    public float strength = 1;
    public float hardness = 1;
    [Space]
    ParticleSystem part;
    List<ParticleCollisionEvent> collisionEvents;

    private List<GameObject> spheres;
    private GameObject character;
    private GameObject bodyMeshPhys;
    private GameObject bodyMeshAnim;
    [Space]

    public float hitRagdollImpulseScale = 0.01f;
    public float hitRagdollYUPVel = 15.0f;
    public float hitObjectImpulseScale = 0.01f;


    public int targetPlayerID = 0;
    
    
    void Start(){
        if (part == null)
        {
            part = GetComponent<ParticleSystem>();
            collisionEvents = new List<ParticleCollisionEvent>();
            //var pr = part.GetComponent<ParticleSystemRenderer>();
            //Color c = new Color(pr.material.color.r, pr.material.color.g, pr.material.color.b, .8f);
            //paintColor = c;
            spheres = new List<GameObject>();
            if (targetPlayerID == 1)
            {
                character = GameObject.Find("Player1/Character1");
            }
            else
            {
                character = GameObject.Find("Player2/Character2");
            }
            if (character != null)
            {
                bodyMeshPhys = character.transform.Find("Physical/Body").gameObject;
                bodyMeshAnim = character.transform.Find("Animated/Body").gameObject;
            }
        }
    }

    void OnParticleCollision(GameObject other) {
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        // Debug.Log("num collision " + numCollisionEvents + " " + other.name);

        if (other.transform.IsChildOf(character.transform))
        {
            /*if (targetPlayerID == 1)
            {
                //Debug.Log("here");
            }*/
            Paintable p_phys = bodyMeshPhys.GetComponent<Paintable>();
            Paintable p_anim = bodyMeshAnim.GetComponent<Paintable>();
            if(p_phys != null){
                for  (int i = 0; i< numCollisionEvents; i++){
                    Vector3 pos = collisionEvents[i].intersection;
                    float radius = Random.Range(minRadius, maxRadius);
                    PaintManager.instance.paint(p_phys, pos, radius, hardness, strength, paintColor);
                }

                var ac = character.GetComponent<ActiveRagdoll.ActiveRagdoll>();
                if (ac != null)
                {
                    float angle = ac.GetTorsoAngle();
                    if (angle < 30)
                    {
                        for (int i = 0; i < numCollisionEvents; i++)
                        {
                            var vel = collisionEvents[i].velocity;
                            vel.y = hitRagdollYUPVel;
                            ac.PhysicalTorso.velocity += vel * hitRagdollImpulseScale;
                        
                        }
                    } 

                }
            }
            if(p_anim != null){
                for  (int i = 0; i< numCollisionEvents; i++){
                    Vector3 pos = collisionEvents[i].intersection;
                    float radius = Random.Range(minRadius, maxRadius);
                    PaintManager.instance.paint(p_anim, pos, radius, hardness, strength, paintColor);
                }
            }
        }
        else
        {
            
            Paintable p = other.GetComponent<Paintable>();
            if(p != null){
                for  (int i = 0; i< numCollisionEvents; i++){
                    Vector3 pos = collisionEvents[i].intersection;
                    float radius = Random.Range(minRadius, maxRadius);
                    PaintManager.instance.paint(p, pos, radius, hardness, strength, paintColor);
                }
                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    for (int i = 0; i < numCollisionEvents; i++)
                    {
                        rb.velocity += collisionEvents[i].velocity * hitObjectImpulseScale;
                    }
                }

                if (other.name == "Floor")
                {
                    if (targetPlayerID == 2)
                    {
                        WinnerControl.cnt_A += 1;
                    }
                    else
                    {
                        WinnerControl.cnt_B += 1;
                    }
                }
            }

           

        }

        


    }
}