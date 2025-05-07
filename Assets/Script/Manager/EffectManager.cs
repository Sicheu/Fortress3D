using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    None,
    Explosion,
    B
}

[System.Serializable]
public class Effect
{
    public EffectType type;
    public GameObject prefab;
    public int poolSize;
}

public class EffectManager : Singleton<EffectManager>
{
    public float EffectSpeed;
    
    public List<Effect> effects;  // 이펙트 리스트
    private Dictionary<EffectType, Queue<ParticleSystem>> particlePools = new Dictionary<EffectType, Queue<ParticleSystem>>(); // 인스턴스화 된 파티클 시스템 풀링
    private Dictionary<ParticleSystem, EffectType> particleToEffectTypeMap = new Dictionary<ParticleSystem, EffectType>(); // 파티클 시스템과 인덱스를 매핑

    void Awake()
    {
        InitializeParticleSystems();
    }
    
    private void InitializeParticleSystems() // 파티클 시스템 오브젝트 풀링
    {
        particlePools.Clear(); // 기존 파티클 시스템 풀링 초기화
        particleToEffectTypeMap.Clear(); // 파티클 시스템 인덱스 맵 초기화

        foreach (Effect effect in effects)
        {
            Queue<ParticleSystem> pool = new Queue<ParticleSystem>(); // 파티클 시스템 변수 선언
            for (int j = 0; j < effect.poolSize; j++) // 하나의 파티클 프리팹 종류에 설정된 사이즈 수의 시스템을 미리 만들어서 풀해둔다
            {
                ParticleSystem ps = Instantiate(effect.prefab).GetComponent<ParticleSystem>(); // 특정 파티클 프리팹의 시스템 생성
                ps.Stop(); // 재생을 멈춰두고
                ps.gameObject.SetActive(false); // 꺼놔서 리소스 덜 잡아먹게 최적화
                pool.Enqueue(ps); // 큐에 생성된 파티클 저장
                particleToEffectTypeMap[ps] = effect.type; // 파티클 시스템과 인덱스를 매핑
            }
            particlePools.Add(effect.type, pool); // 딕셔너리에 저장
        }
    }
    
    private ParticleSystem GetPooledParticleSystem(EffectType type) // 파티클 오브젝트 풀에서 이펙트 꺼내옴 or 새로 만듬
    {
        if (particlePools[type].Count > 0) // 딕셔너리가 비어있지 않다면
        {
            return particlePools[type].Dequeue(); // 해당 인덱스에 저장된 파티클 시스템 반환
        }
        else // 비어있다면 = 미리 저장해둔 파티클 시스템 다 썻다면
        {
            Effect effect = effects.Find(e => e.type == type);
            ParticleSystem ps = Instantiate(effect.prefab).GetComponent<ParticleSystem>();
            ps.Stop();
            ps.gameObject.SetActive(false);
            particleToEffectTypeMap[ps] = type; // 새로 생성된 파티클 시스템과 인덱스를 매핑
            return ps; // 다시 만들어서 저장해두고 반환
        }
    }
    
    private void PlayFromPool(ParticleSystem ps, Vector3 playerPosition) // 풀에서 꺼내와 이펙트 재생
    {
        ps.transform.position = playerPosition;
        ps.gameObject.SetActive(true);
        ps.Play();
    }
    
    private void ReturnToPool(ParticleSystem ps) // 이펙트를 끄고 다시 풀로 되돌림
    {
        ps.Stop();
        ps.gameObject.SetActive(false);
        EffectType type = particleToEffectTypeMap[ps];
        particlePools[type].Enqueue(ps);
    }
    
    private bool TryPlayEffect(EffectType type, out ParticleSystem ps) // 해당 타입이 존재하는지 확인
    {
        if (particlePools.ContainsKey(type))
        {
            ps = GetPooledParticleSystem(type);
            return ps != null;
        }

        Debug.LogError($"Invalid EffectType: {type}");
        ps = null;
        return false;
    }
    
    // 실제로 사용할 함수 필드

    public void PlayEffect(EffectType type, Vector3 position) // 오브젝트 위치에 이펙트 생성(단발성)
    {
        if (TryPlayEffect(type, out ParticleSystem ps))
        {
            PlayFromPool(ps, position);
            StartCoroutine(DeactivateAfterPlay(ps));
        }
    }

    public void PlayEffectRotation(EffectType type, Vector3 position, Transform target) // 오브젝트 위치에 이펙트 생성, 타겟을 향해 방향 설정
    {
        if (TryPlayEffect(type, out ParticleSystem ps))
        {
            PlayFromPool(ps, position);
            StartCoroutine(UpdateRotation(ps, target)); // 타겟을 지속적으로 추적하는 코루틴 시작
            StartCoroutine(DeactivateAfterPlay(ps));
        }
    }
    
    public void DeactivateEffect(GameObject particleObject) // 작동중인 파티클 끄는 메서드
    {
        ParticleSystem ps = particleObject.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ReturnToPool(ps);
        }
    }
    
    public GameObject PlayEffectFollow(EffectType type, Transform target) // 이펙트가 지속적이라, 오브젝트를 따라다녔으면 좋겠을 때 사용하는 함수, 단 사용시 해당 오브젝트 스크립트에 게임오브젝트 형식의 변수를 추가해야함.
    {
        if (TryPlayEffect(type, out ParticleSystem ps))
        {
            PlayFromPool(ps, target.position);
            StartCoroutine(FollowTarget(ps, target));
            return ps.gameObject;
        }

        return null;
    }
    
    public GameObject PlayEffectShot(EffectType type, Vector3 playerPosition, Transform targetPosition) // 타겟에게 이펙트를 발사하는 메서드
    {
        if (TryPlayEffect(type, out ParticleSystem ps))
        {
            PlayFromPool(ps, playerPosition);
            StartCoroutine(MoveEffect(ps, targetPosition));
            return ps.gameObject;
        }

        return null;
    }
    
    // 함수에서 사용하는 코루틴 필드
    
    private IEnumerator DeactivateAfterPlay(ParticleSystem ps) // 파티클 시스템의 재생이 끝났음을 감지하고 풀로 되돌림
    {
        while (ps != null && ps.IsAlive(true)) // null 붙인이유 : 씬이 재시작되거나 넘어가면서 이전 씬에서 파괴된 파티클 시스템을 참조하려해서
        {
            yield return null;
        }

        if (ps != null) // 객체가 이미 파괴된 상태에서 해당 객체에 접근하는것을 방지
        {
            ReturnToPool(ps);
        }
    }
    
    private IEnumerator FollowTarget(ParticleSystem ps, Transform target) // 오브젝트 따라다니는 코루틴
    {
        while (ps != null && ps.isPlaying) 
        {
            if (target != null)
            {
                ps.transform.position = target.position;
            }
            else
            {
                break; // 타겟이 사라진 경우, 루프를 중단
            }
            
            yield return null;
        }

        if (ps != null)
        {
            ReturnToPool(ps);
        }
    }
    
    private IEnumerator MoveEffect(ParticleSystem ps, Transform targetPosition) // 타겟 오브젝트를 추적하여 이동하는 코루틴
    {
        while (ps != null && ps.isPlaying && targetPosition != null && Vector3.Distance(ps.transform.position, targetPosition.position) > 0.1f) // targetPosition 널 체크로 날아가는 중에 데미지 계산이 먼저 되어 타겟이 Destroy 되어도 문제없게 함
        {
            if (ps == null || targetPosition == null)
            {
                yield break;
            }
            Vector3 nextPosition = Vector3.MoveTowards(ps.transform.position, targetPosition.position, EffectSpeed * Time.deltaTime);
            ps.transform.position = nextPosition;
    
            yield return null;
        }
    
        if (ps != null)
        {
            if (targetPosition != null && Vector3.Distance(ps.transform.position, targetPosition.position) < 0.1f) // targetPosition 널 체크로 날아가는 중에 데미지 계산이 먼저 되어 타겟이 Destroy 되어도 문제없게 함
            {
                ps.transform.position = targetPosition.position; // 타겟에 정확히 맞춤
            }
            
            ReturnToPool(ps);
        }
    }
    
    private IEnumerator UpdateRotation(ParticleSystem ps, Transform target) // 타겟 이동에 따라 로테이션을 변경하는 코루틴
    {
        while (ps.isPlaying)
        {
            if (target != null)
            {
                // 타겟을 향한 방향 계산
                Vector3 direction = (target.position - ps.transform.position).normalized;
                Quaternion rotation = Quaternion.LookRotation(direction);

                // 파티클 시스템의 회전 업데이트
                ps.transform.rotation = rotation;
            }

            // 매 프레임마다 회전 업데이트
            yield return null;
        }
    }
}
