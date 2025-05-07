using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Enemy_Hp : MonoBehaviour
{
    public RectTransform HpBar;
    public TankController _soldier;
    private float InitialHp = 100;

    private void Awake()
    {
        HpBar = GetComponent<RectTransform>();
        _soldier = GetComponentInGrandParentParent<TankController>();
    }
    
    // 부모의 부모의 부모 컴포넌트 가져오기 위한 커스텀 겟컴포넌트
    private T GetComponentInGrandParentParent<T>() where T : Component
    {
        Transform parent = transform.parent;
        if (parent != null)
        {
            Transform grandParent = parent.parent;
            if (grandParent != null)
            {
                Transform grandParentParent = grandParent.parent;
                if (grandParentParent != null)
                {
                    return grandParentParent.GetComponent<T>();
                }
            }
        }
        return null;
    }

    private void Update()
    {
        UpdateHpBar();

        if (!_soldier.m_dead && _soldier.currentHealth <= 0)
        {
            _soldier.m_dead = true;
        }
    }

    void UpdateHpBar()
    {
        float hpRatio = Mathf.Max(_soldier.currentHealth / InitialHp, 0); // 데미지 입은만큼의 수로 설정하되, 0 을 넘지 않게 한다
        HpBar.localScale = new Vector3(hpRatio, HpBar.localScale.y, HpBar.localScale.z);
    }
}
