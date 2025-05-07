using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    public float explosionRadius = 2f;
    public float explosionDepth = 1f;
    
    public ParticleSystem m_ExplosionParticles;         // Reference to the particles that will play on explosion.
    public AudioSource m_ExplosionAudio; 

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground") || collision.gameObject.CompareTag("enemy"))
        {
            // 충돌 지점 정보 가져오기
            ContactPoint contact = collision.contacts[0];
            Vector3 hitPoint = contact.point;

            // 충돌한 오브젝트에 MeshDeformation 스크립트가 있다면 호출
            MeshDeformation deform = collision.gameObject.GetComponent<MeshDeformation>();
            if (deform != null)
            {
                MeshFilter meshFilter = collision.gameObject.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    deform.DeformTerrain(meshFilter, hitPoint, explosionRadius, explosionDepth);
                }
            }

            // 포탄은 충돌 후 제거
            Destroy(gameObject);
        }
    }
}
