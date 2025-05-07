using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("블록 설정")]
    public GameObject blockPrefab;
    public Vector3 blockSize = Vector3.one; // 블록 크기

    [Header("맵 크기 설정 (격자 수)")]
    public int sizeX = 100;
    public int sizeZ = 100;

    [Header("층수 설정")]
    public int flatMinHeight = 2; // 평지 최소 층수
    public int flatMaxHeight = 5; // 평지 최대 층수

    [Header("노이즈 설정")]
    public float noiseScale = 0.05f;
    public float mountainThreshold = 0.5f;  // 산이 시작되는 노이즈 기준
    public float mountainBoostCurve = 2.5f; // 산 부스트 곡선 조정

    [Header("산 층수 설정")]
    public int mountainMaxHeight = 15; // 산의 최대 층수

    [Header("스무딩 설정")]
    public int maxSlope = 1; // 인접 블록 간 최대 높이차 제한

    private int[,] heightMap;

    public void GenerateTerrain()
    {
        ClearChildren();
        GenerateHeightMap();
        SmoothHeightMap();
        BuildTerrain();
    }

    private void GenerateHeightMap()
    {
        heightMap = new int[sizeX, sizeZ];

        for (int x = 0; x < sizeX; x++)
        {
            for (int z = 0; z < sizeZ; z++)
            {
                // 블록 크기를 고려해서 노이즈 좌표 계산
                float noise = Mathf.PerlinNoise(
                    (x * blockSize.x) * noiseScale,
                    (z * blockSize.z) * noiseScale
                );

                int finalHeight;

                if (noise <= mountainThreshold)
                {
                    finalHeight = Mathf.RoundToInt(Mathf.Lerp(flatMinHeight, flatMaxHeight, noise / mountainThreshold));
                }
                else
                {
                    float boosted = (noise - mountainThreshold) / (1f - mountainThreshold);
                    boosted = Mathf.Pow(boosted, mountainBoostCurve);
                    finalHeight = Mathf.RoundToInt(Mathf.Lerp(flatMaxHeight, mountainMaxHeight, boosted));
                }

                heightMap[x, z] = Mathf.Max(finalHeight, 1); // 최소 1층은 생성
            }
        }
    }

    private void SmoothHeightMap()
    {
        for (int x = 1; x < sizeX; x++)
        {
            for (int z = 1; z < sizeZ; z++)
            {
                int currentHeight = heightMap[x, z];

                int leftHeight = heightMap[x - 1, z];
                if (Mathf.Abs(currentHeight - leftHeight) > maxSlope)
                {
                    heightMap[x, z] = leftHeight + Mathf.Clamp(currentHeight - leftHeight, -maxSlope, maxSlope);
                }

                int downHeight = heightMap[x, z - 1];
                if (Mathf.Abs(currentHeight - downHeight) > maxSlope)
                {
                    heightMap[x, z] = downHeight + Mathf.Clamp(currentHeight - downHeight, -maxSlope, maxSlope);
                }
            }
        }
    }

    private void BuildTerrain()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int z = 0; z < sizeZ; z++)
            {
                int height = heightMap[x, z];
                for (int y = 0; y < height; y++)
                {
                    Vector3 pos = new Vector3(
                        x * blockSize.x,
                        (y + 1) * blockSize.y, // <- 여기 수정
                        z * blockSize.z
                    );
                    Instantiate(blockPrefab, pos, Quaternion.identity, transform);
                }
            }
        }
    }


    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}
