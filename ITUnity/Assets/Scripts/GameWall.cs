using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Linq;

public class GameWall : MonoBehaviour
{
    //Scene understanding
    private bool sceneQuerySuccessful = false;
    private readonly Dictionary<Guid, int> _orderedRoomGuids = new Dictionary<Guid, int>();
    internal HashSet<Guid> _uuidToQuery = new HashSet<Guid>();
    private Comparison<OVRScenePlane> _wallOrderComparer;

    //Wall
    private int height;
    private Dictionary<OVRScenePlane, List<GameObject>> wallCubes = new Dictionary<OVRScenePlane, List<GameObject>>();
    List<OVRScenePlane> _walls = new List<OVRScenePlane>();

    private int xLength = 0;
    private int yLength = 0;

    private bool initialised = false;

    [SerializeField] private GameObject sampleObject;

    private GameObject floor; 

    // Start is called before the first frame update
    private void Awake()
    {
        _wallOrderComparer = (planeA, planeB) =>
        {
            bool TryGetUuid(OVRScenePlane plane, out int index)
            {
                var guid = plane.GetComponent<OVRSceneAnchor>().Uuid;
                if (_orderedRoomGuids.TryGetValue(guid, out index)) return true;

                return false;
            }

            if (!TryGetUuid(planeA, out var indexA)) return 0;
            if (!TryGetUuid(planeB, out var indexB)) return 0;

            return indexA.CompareTo(indexB);
        };
    }

    private void Update()
    {
        if (!initialised)
        {
            if (!sceneQuerySuccessful)
            {
                LoadSceneAsync();
                return;
            }

            //All the wall are not yet initialized 
            if (_walls.Count < _orderedRoomGuids.Keys.Count)
            {
                return;
            }

            initialised = true;

            _walls.Sort(_wallOrderComparer);
        }
    }

    public void AddCubes(List<GameObject> cubesToAdd, int y, OVRScenePlane plane)
    {
        height = y;
        _walls.Add(plane);
        wallCubes[plane] = cubesToAdd;
    }

    public void CreateBricks(out GameBlock[,] array)
    {
        List<List<GameObject>> cubes = new List<List<GameObject>>();

        List<Vector3> vertices = new List<Vector3>(); 
        
        foreach (OVRScenePlane wall in _walls)
        {
            cubes.Add(wallCubes[wall]);

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.parent = wall.transform;
            go.transform.localScale = Vector3.one * 0.1f;
            go.transform.localPosition = new Vector3(-wall.Dimensions.x / 2, -wall.Dimensions.y / 2, 0f);
            vertices.Add(go.transform.position);
            Destroy(go);
        }

        xLength = 0;
        yLength = height;

        for (int i = 0; i < cubes.Count; i++)
        {
            xLength += (cubes[i].Count / yLength);
        }

        array = new GameBlock[xLength, yLength];

        for (int k = 0; k < height; k++)
        {
            int l = 0;
            for (int i = 0; i < cubes.Count; i++)
            {
                int m = 0;
                for (int j = ((cubes[i].Count / height) * k); j < ((cubes[i].Count / height) * (k + 1)); j++)
                {
                    array[l + m, k] = cubes[i][j].GetComponent<GameBlock>();
                    array[l + m, k].GetComponent<GameBlock>().SetXPosition(l + m);
                    array[l + m, k].GetComponent<GameBlock>().SetYPosition(k);
                    m++;
                }
                l += (cubes[i].Count / height);
            }
        }

        floor = MeshGeneration.CreateMeshObject(vertices.ToArray());
    }

    public void CreateBricksSample(out GameBlock[,] array)
    {
        xLength = 10;
        yLength = 10;

        array = new GameBlock[xLength, yLength];

        for (int j = 0; j < yLength; j++)
        {
            for (int i = 0; i < xLength; i++)
            {
                array[i, j] = sampleObject.transform.GetChild(i + (j * xLength)).GetComponent<GameBlock>();
                array[i, j].GetComponent<GameBlock>().SetXPosition(i);
                array[i, j].GetComponent<GameBlock>().SetYPosition(j);
            }
        }
    }

    //Get the wall details
    async void LoadSceneAsync()
    {
        // fetch all rooms, with a SceneCapture fallback
        var rooms = new List<OVRAnchor>();
        await OVRAnchor.FetchAnchorsAsync<OVRRoomLayout>(rooms);
        if (rooms.Count == 0)
        {
            var sceneCaptured = await SceneManagerHelper.RequestSceneCapture();
            if (!sceneCaptured)
                return;

            await OVRAnchor.FetchAnchorsAsync<OVRRoomLayout>(rooms);
        }

        // fetch room elements, create objects for them
        var tasks = rooms.Select(async room =>
        {
            var roomObject = new GameObject($"Room-{room.Uuid}");
            if (!room.TryGetComponent(out OVRAnchorContainer container))
                return;
            if (!room.TryGetComponent(out OVRRoomLayout roomLayout))
                return;

            var children = new List<OVRAnchor>();
            await container.FetchChildrenAsync(children);

            if (!roomLayout.TryGetRoomLayout(out var ceilingUuid, out var floorUuid, out var wallUuids))
            {
                return;
            }

            _orderedRoomGuids.Clear();
            int validWallsCount = 0;
            foreach (var wallUuid in wallUuids)
            {
                sceneQuerySuccessful = true;
                _orderedRoomGuids[wallUuid] = validWallsCount++;
                if (!wallUuid.Equals(Guid.Empty)) _uuidToQuery.Add(wallUuid);
            }

        }).ToList();
        await Task.WhenAll(tasks);
    }

    public bool IsWallReady()
    {
        return initialised; 
    }

    public GameObject GetFloor()
    {
        return floor; 
    }

    public float GetWallHeight()
    {
        return _walls[0].Dimensions.y;
    }
}