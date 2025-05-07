using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDeformation : MonoBehaviour
{
    public void DeformTerrain(MeshFilter meshFilter, Vector3 hitPoint, float radius, float depth)
    {
        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();

        // 충돌 범위 내의 버텍스를 찾아서 삭제하고, 나머지 부분을 이어주기
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = meshFilter.transform.TransformPoint(vertices[i]);
            float dist = Vector3.Distance(worldPos, hitPoint);

            if (dist < radius) // 범위 내의 버텍스만 삭제
            {
                continue; // 이 버텍스를 삭제하고 지나감
            }
            else
            {
                newVertices.Add(vertices[i]); // 삭제되지 않은 버텍스만 저장
            }
        }

        // 구멍을 메우기 위한 삼각형 재구성
        // 이 예시는 간단히 가장 가까운 3개의 버텍스를 이어서 삼각형을 만듭니다.
        // 실제로는 더 정교한 알고리즘이 필요할 수 있습니다.

        for (int i = 0; i < newVertices.Count - 2; i++)
        {
            // 삼각형을 구성
            newTriangles.Add(i);      // 첫 번째 버텍스
            newTriangles.Add(i + 1);  // 두 번째 버텍스
            newTriangles.Add(i + 2);  // 세 번째 버텍스
        }

        // 새로운 메쉬에 재구성된 버텍스와 삼각형 적용
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();  // 삼각형 인덱스를 재구성

        mesh.RecalculateNormals(); // 새로 구성된 삼각형에 맞게 노멀 재계산
        meshFilter.mesh = mesh; // 메쉬 적용

        // MeshCollider 업데이트
        MeshCollider meshCollider = meshFilter.gameObject.GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            Destroy(meshCollider); // 기존 MeshCollider 삭제
            meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>(); // 새로운 MeshCollider 추가
            meshCollider.sharedMesh = mesh; // 변형된 메쉬를 콜라이더에 설정
        }
    }

    // // 포탄 충돌 시 호출
    // public void DeformTerrain(MeshFilter meshFilter, Vector3 hitPoint, float radius, float depth)
    // {
    //     // 메쉬 데이터 가져옴
    //     Mesh mesh = meshFilter.mesh;
    //     Vector3[] vertices = mesh.vertices;
    //
    //     for (int i = 0; i < vertices.Length; i++)
    //     {
    //         Vector3 worldPos = meshFilter.transform.TransformPoint(vertices[i]); // 버텍스를 로컬 좌표 > 월드 좌표 로 변경
    //         float dist = Vector3.Distance(worldPos, hitPoint); // 충격 지점과 해당 버텍스의 거리 계산
    //
    //         if (dist < radius) // 충격 범위 내의 버텍스만
    //         {
    //             // Y 값을 줄여서 움푹 파이게
    //             float falloff = Mathf.Pow(1 - (dist / radius), 2f); // 포물선처럼 부드럽게
    //             vertices[i].y -= depth * falloff;
    //         }
    //     }
    //
    //     mesh.vertices = vertices; // 변경된 값을 메시에 적용
    //     mesh.RecalculateNormals(); // 지형이 변형됐으니 노멀(빛 방향 반응) 재계산
    //     meshFilter.mesh = mesh; // 최종적으로 메시를 갱신하여 반영
    //     
    //     MeshCollider meshCol = meshFilter.GetComponent<MeshCollider>();
    //     if (meshCol != null)
    //     {
    //         // 기존 MeshCollider를 삭제하고 새로운 메쉬를 적용한 새로운 MeshCollider 추가
    //         Destroy(meshCol); // 기존 콜라이더 제거
    //         meshCol = meshFilter.gameObject.AddComponent<MeshCollider>(); // 새 콜라이더 추가
    //         meshCol.sharedMesh = mesh; // 새 메쉬를 콜라이더에 적용
    //     }
    //     Debug.Log("지형 변경 적용됨");
    // }

}
