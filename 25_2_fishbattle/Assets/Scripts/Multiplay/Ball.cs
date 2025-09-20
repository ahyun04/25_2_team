using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Ball : NetworkBehaviour
{
    [Networked] private TickTimer life { get; set; }   //퓨전에서 제공주는 Timer 
    public float damage = 10.0f;

    public void Init()
    {
        life = TickTimer.CreateFromSeconds(Runner, 5.0f);
    }

    public override void FixedUpdateNetwork()
    {
        if (life.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
        else
        {
            transform.position += 5 * transform.forward * Runner.DeltaTime;
            CheckCollision();
        }
    }

    private void CheckCollision()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.5f);

        foreach (var hitColiider in hitColliders)
        {
            Player hitPlayer = hitColiider.GetComponent<Player>();
            if (hitPlayer != null)
            {
                Runner.Despawn(Object);     //충돌 하면 볼을 제거합니다. 
                break;
            }
        }
    }
}