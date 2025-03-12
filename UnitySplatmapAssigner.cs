using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class UnitySplatmapAssigner : MonoBehaviour
{
    [Tooltip("Terrain tiles")]
    public Terrain[] terrains;

    [Tooltip("Number of terrain tiles")]
    public Vector2Int terrainSize;


    [Tooltip("Resolution of splatmaps")]
    public int splatResolution = 4096;


    [Tooltip("Number of splat maps per terrain tile.")]
    public int splatmapPerTile = 2;

    [Tooltip("Splatmaps in order of terrain tile. (e.g. if there are 2 splatmaps index 0 and 1 will be applied to terrain tile 0 and so on)")]
    public Texture2D[] splatMaps;

    [Tooltip("Terrain layers in order of splatmaps, this will override your existing layers on all terrain.")]
    public TerrainLayer[] terrainLayers;
    public void AssignSplat()
    {
        if (terrainLayers == null) { Debug.LogError("Terrain Layers Array NUll"); return; }

        Debug.Log("Applying splatmap on terrains: " + terrains.Length);
        if (terrains.Length != terrainSize.x * terrainSize.y)
        {
            Debug.LogError(
                           "Terrain Tile Size Incorrect " +
                               terrains.Length + " =/= " + terrainSize);
            return;
        }
        if (terrains.Length * splatmapPerTile != splatMaps.Length)
        {
            Debug.LogError(
                "Splatmap count not correct " +
                    terrains.Length + " * " + splatmapPerTile + " = " + (splatMaps.Length * splatmapPerTile) + " =/= " + splatMaps.Length);
            return;
        }

        Texture2D[][][] splatNested =
            Enumerable.Range(0, terrainSize.x)
                .Select(a => Enumerable.Range(0, terrainSize.y)
                .Select(a => new Texture2D[splatmapPerTile]).ToArray()).ToArray();

        for (int i = 0; i < splatMaps.Length; i++)
        {
            Vector2Int splatIndex = GetTerrainIndex(splatMaps[i].name);
            splatNested[splatIndex.x][splatIndex.y][i % splatmapPerTile] = splatMaps[i];
        }

        for (int t = 0; t < terrains.Length; t++)
        {
            Terrain terrain = terrains[t];
            TerrainData terrainData = terrain.terrainData;
            terrainData.alphamapResolution = splatResolution;
            terrainData.terrainLayers = terrainLayers;

            Vector2Int terrainIndex = GetTerrainIndex(terrain.name);

            float[,,] splatmapData = new float[splatResolution,
                                               splatResolution,
                                               terrainData.alphamapLayers];
            Texture2D[] splatsForTerrain = splatNested[terrainIndex.x][terrainIndex.y];

            int len = 3; // RGB
            for (var y = 0; y < terrainData.alphamapWidth; y++)
            {
                for (var x = 0; x < terrainData.alphamapWidth; x++)
                {

                    for (int splatIndex = 0; splatIndex < splatsForTerrain.Length; splatIndex++)
                    {
                        Color rgb = splatsForTerrain[splatIndex].GetPixel(x, y);
                        splatmapData[splatResolution - 1 - y, x, (splatIndex * len)] = rgb.r;
                        if ((splatIndex * len) + 1 >= terrainLayers.Length) continue;
                        splatmapData[splatResolution - 1 - y, x, (splatIndex * len) + 1] = rgb.g;
                        if ((splatIndex * len) + 2 >= terrainLayers.Length) continue;
                        splatmapData[splatResolution - 1 - y, x, (splatIndex * len) + 2] = rgb.b;
                    }
                }
            }
            terrainData.SetAlphamaps(0, 0, splatmapData);
        }
        Debug.Log("Terrain Splatmaps: Done");
    }

    // Using default Gaea naming to index terrains and splatmaps
    Vector2Int GetTerrainIndex(string n)
    {
        string[] s = n.Split("_");
        if (s == null || s.Length < 2) return Vector2Int.zero;
        string xString = Regex.Replace(s[1], @"[^\d]", "");
        string yString = Regex.Replace(s[2], @"[^\d]", "");

        return new Vector2Int(
            int.Parse(xString),
            int.Parse(yString)
        );
    }
}
