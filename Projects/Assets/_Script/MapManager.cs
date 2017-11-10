#region

using System.Collections.Generic;
using Babybus.Uno;
using UFramework.Pool;
using UnityEngine;

#endregion

public class MapManager : MonoSingleton<MapManager>
{
    private static List<Chunk> chunks = new List<Chunk>();

    public int ChunkWidth { get; set; }
    public int ChunkHeight { get; set; }
    public int Seed { get; set; }
    public int ViewRange { get; set; }

    public static List<Chunk> Chunks
    {
        get { return chunks; }
    }

    public static void RegistCube(Chunk chunk)
    {
        if (!chunks.Contains(chunk))
            chunks.Add(chunk);
    }

    protected override void OnAwake()
    {
        Cursor.visible = false;

        Seed = 1000;
        ChunkWidth = 20;
        ChunkHeight = 20;
        ViewRange = 30;
    }

    // Update is called once per frame
    private void Update()
    {
        // 检查视野范围
        for (float x = transform.position.x - ViewRange; x < transform.position.x + ViewRange; x += ChunkWidth)
        {
            for (float z = transform.position.z - ViewRange; z < transform.position.z + ViewRange; z += ChunkWidth)
            {
                // Floor 小于等于f的最大整数
                Vector3 pos = new Vector3(Mathf.Floor(x / ChunkWidth) * ChunkWidth, 0,
                    Mathf.Floor(z / ChunkWidth) * ChunkWidth);

                Chunk chunk = Chunk.FindChunk(pos);
                if (chunk != null) continue;

                InsChunk(pos, Quaternion.identity);
            }
        }
    }

    public GameObject InsChunk(Vector3 pos, Quaternion rot)
    {
        return ObjectsPool.Spawn(CommonStr.ChunkPrefabName, pos, Quaternion.identity);
    }

    public void UnInsChunk(GameObject go)
    {
        ObjectsPool.Despawn(go);
    }
}