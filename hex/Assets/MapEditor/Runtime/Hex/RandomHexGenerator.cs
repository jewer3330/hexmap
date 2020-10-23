using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;

public class RandomHexGenerator : MonoBehaviour
{
    public Vector2 mapGridSize = new Vector2(30, 30);
    public int minBuilding = 10;
    public int maxBuilding = 20;

    public int minRoute = 10;
    public int maxRoute = 15;

    protected World world;
    public Texture hexTexture;
    public int minBuildingDist = 4;

    Dictionary<Vector2, Hex> buildings = new Dictionary<Vector2, Hex>();
    Dictionary<Vector2, Hex> routes = new Dictionary<Vector2, Hex>();
    Dictionary<Hex, List<Hex>> linkedRoutes = new Dictionary<Hex, List<Hex>>();
    Dictionary<Hex, List<Hex>> paths = new Dictionary<Hex, List<Hex>>();

    // Start is called before the first frame update
    void Awake()
    {
        world = gameObject.AddComponent<World>();
        world.gridX = (int)mapGridSize.x;
        world.gridY = (int)mapGridSize.y;

        world.Generate();
    
    }

    private void Start()
    {
        for(int i=0; i<(int)mapGridSize.x; i++)
        {
            for (int j = 0; j < (int)mapGridSize.y; j++)
            {
                Vector2 idx = new Vector2(i, j);
                Material material = new Material(Shader.Find("Diffuse"));
                material.color = Color.blue;
                material.mainTexture = hexTexture;
                world.Hexes[idx].HexModel.GetComponent<MeshRenderer>().sharedMaterial = material;
            }
        }
        RandomBuildings();
        RandomRoutes();

        //Link building and route
        LinkBuildingRoutes();
        LinkRoutes();
    }


    void LinkRoutes()
    {
        List<Hex> keys = new List<Hex>(paths.Keys);
        foreach(KeyValuePair<Hex, List<Hex>> kv in paths)
        {
            Hex route = kv.Key;
            Hex result = null;
            float minDist = 999f;
            foreach(Hex target in keys)
            {
                if (kv.Key == target)
                    continue;
                foreach (Hex path in paths[target])
                {
                    float dist = Vector3.Distance(path.HexModel.transform.position, route.HexModel.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        result = path;
                    }
                }
            }
            keys.Remove(route);
            if (result != null)
            {
                List<Hex> list = World.searchRoute(route, result);
                for (int i = 0; i < list.Count - 1; i++)
                {
                    Hex hex = list[i];
                    hex.HexModel.GetComponent<MeshRenderer>().sharedMaterial.color = Color.green;
                }
            }
        }
    }

    void LinkBuildingRoutes()
    {
        foreach(KeyValuePair<Vector2, Hex> kv in buildings)
        {
            float dist = 9999f;
            Hex buildingHex = kv.Value;
            Hex routeHex = null;
            foreach (KeyValuePair<Vector2, Hex> kv2 in routes)
            {
                float currentDist = Vector3.Distance(kv2.Value.transform.position, buildingHex.transform.position);
                if (currentDist <dist)
                {
                    dist = currentDist;
                    routeHex = kv2.Value;
                }

            }

            if(routeHex!=null && buildingHex.linkedHex != null)
            {
                
                List<Hex> path = LinkBuildingRoute(buildingHex.linkedHex, routeHex);
                if(!paths.ContainsKey(routeHex))
                    paths.Add(routeHex, path);
                else
                {
                    paths[routeHex].AddRange(path);
                }

                if(!linkedRoutes.ContainsKey(routeHex))
                {
                    linkedRoutes.Add(routeHex, new List<Hex>());
                }
                linkedRoutes[routeHex].Add(buildingHex);
            }
        }
    }

    List<Hex> LinkBuildingRoute(Hex building , Hex route)
    {
        List<Hex> list = World.searchRoute(building, route);
        for(int i=0; i<list.Count-1; i++)
        {
            Hex hex = list[i];
            hex.HexModel.GetComponent<MeshRenderer>().sharedMaterial.color = Color.green;
        }
        return list;
    }

    void RandomBuildings()
    {
        Vector2 center = new Vector2(Mathf.CeilToInt(mapGridSize.x / 2), Mathf.CeilToInt(mapGridSize.y / 2));
        int rand = Random.Range(minBuilding, maxBuilding);
        for(int i=0; i<rand; i++)
        {
            bool b = true;
            while (b)
            {
                int xRange = 0;
                int yRange = 0;

                float rnd = Random.Range(0f, 1f);
                if(rnd>0.9f)
                {
                    xRange = Mathf.CeilToInt(mapGridSize.x / 2);
                    yRange = Mathf.CeilToInt(mapGridSize.y / 2);
                }
                else if(rnd>0.6f)
                {
                    xRange = Mathf.CeilToInt(mapGridSize.x / 3);
                    yRange = Mathf.CeilToInt(mapGridSize.y / 3);
                }
                else
                {
                    xRange = Mathf.CeilToInt(mapGridSize.x / 4);
                    yRange = Mathf.CeilToInt(mapGridSize.y / 4);
                }

                int r1 = (int)center.x + Random.Range(-xRange, xRange);
                int r2 = (int)center.y + Random.Range(-yRange, yRange);

                Vector2 idx = new Vector2(r1, r2);
                if (!buildings.ContainsKey(idx))
                {
                    bool invalid = false;
                    foreach(Vector2 existIdx in buildings.Keys)
                    {
                        if( Vector2.Distance(idx, existIdx) < minBuildingDist)
                        {
                            invalid = true;
                            break;
                        }

                    }
                    if (!invalid)
                    {
                        Hex hex = world.Hexes[idx];
                        hex.cost = 999f;
                        hex.HexModel.GetComponent<MeshRenderer>().sharedMaterial.color = Color.white;
                        hex.dir = Random.Range(1, 6);
                        buildings.Add(idx, hex);
                        b = false;
                    }
                }
            }
        }
    }

    void RandomRoutes()
    {       
        int rand = Random.Range(minRoute, maxRoute);
        Vector2 center = new Vector2(Mathf.CeilToInt(mapGridSize.x / 2), Mathf.CeilToInt(mapGridSize.y / 2));
        for (int i = 0; i < rand; i++)
        {
            int xRange = 0;
            int yRange = 0;

            float rnd = Random.Range(0f, 1f);
            if (rnd > 0.9f)
            {
                xRange = Mathf.CeilToInt(mapGridSize.x / 2);
                yRange = Mathf.CeilToInt(mapGridSize.y / 2);
            }
            else if (rnd > 0.6f)
            {
                xRange = Mathf.CeilToInt(mapGridSize.x / 3);
                yRange = Mathf.CeilToInt(mapGridSize.y / 3);
            }
            else
            {
                xRange = Mathf.CeilToInt(mapGridSize.x / 4);
                yRange = Mathf.CeilToInt(mapGridSize.y / 4);
            }

            int r1 = (int)center.x + Random.Range(-xRange, xRange);
            int r2 = (int)center.y + Random.Range(-yRange, yRange);

            Vector2 idx = new Vector2(r1, r2);
            if (!buildings.ContainsKey(idx) && !routes.ContainsKey(idx))
            {
                world.Hexes[idx].HexModel.GetComponent<MeshRenderer>().sharedMaterial.color = Color.red;
                routes.Add(idx, world.Hexes[idx]);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
