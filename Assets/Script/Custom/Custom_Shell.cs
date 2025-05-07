using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Custom_Shell : MonoBehaviour 
{
    public int shellDamage = 10;
    public float explosionRadius = 2f;

    public ParticleSystem m_ExplosionParticles;
    public AudioSource m_ExplosionAudio;

    public GameObject shooter;
    
    public Action onExplosion;
    public Action<GameObject> onExplosionWithShooter;

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "ground")
        {
            Debug.Log("땅");
            Explode();
        }
        else if (col.gameObject.tag == "Player")
        {
            if (col.gameObject != shooter)
            {
                Debug.Log("플레이어");
                Explode();
            }
        }
    }

    private void Explode()
    {
        // 이펙트 및 사운드 재생
        m_ExplosionParticles.Play();
        m_ExplosionAudio.Play();

        // 포탄 비활성화
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;
        GetComponent<Renderer>().enabled = false;

        // 폭발 범위 처리
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in colliders)
        {
            if (hit.CompareTag("ground"))
            {
                var target = hit.GetComponent<Ground>();
                if (target != null)
                {
                    target.Exploded();
                }
            }
            else if (hit.CompareTag("Player"))
            {
                var target = hit.GetComponent<TankController>(); // 예시: 탱크가 enemy라면
                if (target != null && target.gameObject != shooter)
                {
                    target.Damage(shellDamage);
                }
            }
        }

        onExplosion?.Invoke();
        onExplosionWithShooter?.Invoke(shooter); // shooter 정보도 전달, 턴 넘김을 위해
        Destroy(gameObject, 2f);
    }
}