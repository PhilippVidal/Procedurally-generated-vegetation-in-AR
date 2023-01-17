using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class Ivy : MonoBehaviour
{   
    int mMeshResolution;
    float mRadius;
    float mDistanceSinceLastLeaf;
    float mMinDistanceToNextLeaf;
    float mMaxDistanceToNextLeaf;
    float mRandRotX;
    float mRandRotY;
    float mRandRotZ;
    string mPlantName;
    float mBranchChance;
    int mIterations;
    float mLeafGrowthTime;
    float mLeafEdgeDistance;

    int mLastFinishedNode;
    bool mIsMoving;
    bool mCanBranch = true;
    GameObject mIvyTip;
    List<Node> mIvyPath;
    Vector3[] mTemplateVertices;
    Pathfinding mPathfinder;
    ObjectPooling mObjectPooler;
    Coroutine mMoveToCoroutine;
    List<GameObject> mGrownBranches;

    Mesh mMesh;
    MeshFilter mMeshFilter;
    List<Vector3> mMeshVertices;
    List<int> mMeshIndices;
    MeshRenderer mMeshRenderer; 

    Mesh mIvyTipMesh;
    MeshFilter mIvyTipMeshFilter;
    List<Vector3> mIvyTipMeshVertices;
    List<int> mIvyTipMeshIndices;
    MeshRenderer mIvyTipMeshRenderer;

    GameObject mLeaves;
    MeshFilter mLeavesMeshFilter;
    MeshRenderer mLeavesMeshRenderer;
    Queue<GameObject> mLeavesToCombine;

    int mNumberOfNodes => mIvyPath.Count;

    private void Awake()
    {
        mMeshFilter = GetComponent<MeshFilter>();
        mMeshRenderer = GetComponent<MeshRenderer>();
        mMesh = new Mesh();
        mMesh.name = "IvyMesh";
        mMeshFilter.sharedMesh = mMesh;
        mLastFinishedNode = 0;
        mGrownBranches = new List<GameObject>();

        //Setup IvyTip
        mIvyTip = new GameObject("IvyTip");
        mIvyTip.transform.position = transform.position;
        mIvyTip.transform.rotation = transform.rotation;
        mIvyTip.transform.parent = transform;

        mIvyTipMeshFilter = mIvyTip.AddComponent<MeshFilter>();
        mIvyTipMesh = new Mesh();
        mIvyTipMesh.name = "IvyTipMesh";
        mIvyTipMeshFilter.sharedMesh = mIvyTipMesh;
        mIvyTipMeshRenderer = mIvyTip.AddComponent<MeshRenderer>();
        mIvyTipMeshRenderer.material = mMeshRenderer.sharedMaterial;

        //Setup Pathfinder
        mPathfinder = GetComponent<Pathfinding>();
        mPathfinder.mHelper = new GameObject("Pathfinding Helper");       
        mPathfinder.mHelper.transform.position = transform.position;
        mPathfinder.mHelper.transform.rotation = transform.rotation; 
        mPathfinder.mHelper.transform.parent = transform;
        mPathfinder.mIvy = this;
      
        //Leaf Combining
        mLeavesToCombine = new Queue<GameObject>();
        mLeaves = new GameObject("IvyLeaves");   
        mLeaves.transform.position = transform.position;
        mLeaves.transform.rotation = transform.rotation;
        mLeaves.transform.parent = transform;
        mLeavesMeshFilter = mLeaves.AddComponent<MeshFilter>();
        mLeavesMeshRenderer = mLeaves.AddComponent<MeshRenderer>();    
    }

    private void Start()
    {
        mPathfinder.init();
        mMeshResolution = GameManager.SETTINGS.IVYSETTINGS.MeshResolution;
        mRadius = GameManager.SETTINGS.IVYSETTINGS.Radius;
        mMinDistanceToNextLeaf = GameManager.SETTINGS.IVYSETTINGS.MinDistanceToNextLeaf;
        mMaxDistanceToNextLeaf = GameManager.SETTINGS.IVYSETTINGS.MaxDistanceToNextLeaf;
        mRandRotX = GameManager.SETTINGS.IVYSETTINGS.RandomRotationX;
        mRandRotY = GameManager.SETTINGS.IVYSETTINGS.RandomRotationY;
        mRandRotZ = GameManager.SETTINGS.IVYSETTINGS.RandomRotationZ;
        mObjectPooler = GameManager.OBJECTPOOLER;
        mPlantName = GameManager.SETTINGS.IVYSETTINGS.LeafPoolName;
        mLeafGrowthTime = GameManager.SETTINGS.IVYSETTINGS.LeafGrowthTime;
        mLeafEdgeDistance = GameManager.SETTINGS.IVYSETTINGS.LeafEdgeDistance;
        mBranchChance = GameManager.SETTINGS.IVYSETTINGS.BranchPercentage;
        mIterations = GameManager.SETTINGS.IVYSETTINGS.Iterations;

        mIvyPath = new List<Node>();

        //Add position as start node to path
        AddNodeToPath(transform.position, transform.rotation, transform.up, NodeType.START);

        GenerateMesh();

        mIsMoving = false;
        mDistanceSinceLastLeaf = 0f;     
    }

    private void Update()
    {
        if (!mIsMoving)
        {
            if (mLastFinishedNode < mNumberOfNodes - 1)
            {
                if (mMoveToCoroutine != null)
                {
                    StopCoroutine(mMoveToCoroutine);
                }

                int node = GetNextNode(mLastFinishedNode);

                if (mCanBranch)
                {
                    Branch(node);
                }

                mIsMoving = true;
                mMoveToCoroutine = StartCoroutine("GoToNode", node); 
            }
        }    
    }

    public void SetCanBranch(bool value)
    {
        mCanBranch = value;
    }

    public void SetIterations(int iterations)
    {
        mIterations = iterations;
    }

    void Branch(int node)
    {
        if (node != 0 && node <= mIvyPath.Count - 5)
        {
            float value = Random.value;

            if (value <= mBranchChance)
            {
                CreateBranch(node);
            }
        }
    }

    void CreateBranch(int node)
    {
        float angle = Random.Range(10f, 45f);
        int k = Random.Range(0, 2);
        if (k == 0)
        {
            angle *= -1;
        }
        Vector3 direction = (Quaternion.AngleAxis(angle, mIvyTip.transform.up) * mIvyTip.transform.forward).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction, mIvyTip.transform.up);
        GameObject newBranch = Instantiate(GameManager.SETTINGS.IVYFLOWERSETTINGS.IvyPrefab, mIvyTip.transform.position, rotation, transform);
        Ivy ivy = newBranch.GetComponent<Ivy>();
        ivy.SetCanBranch(false);

        int iterations = mIterations - node;
        StartCoroutine(GrowBranchDelayed(ivy, iterations));
        mGrownBranches.Add(newBranch);
    }

    IEnumerator GrowBranchDelayed(Ivy ivy, int iterations)
    {
        yield return new WaitForSeconds(1f);

        ivy.SetIterations(iterations);
        ivy.StartGrowing();
    }
    IEnumerator CombineLeaves()
    {
        while(true)
        {
            CheckLeafCombining();
            yield return new WaitForSeconds(mLeafGrowthTime);
        }
    }
    void CheckLeafCombining()
    {
        if (mLeavesToCombine.Count != 0)
        {
            int count = mLeavesToCombine.Count; 
            GameObject[] leavesToCombine = new GameObject[count];
            for (int i = 0; i < count; i++)
            {
                leavesToCombine[i] = mLeavesToCombine.Dequeue();
            }

            MeshRenderer renderer; 

            if (!leavesToCombine[0].TryGetComponent<MeshRenderer>(out renderer))
            {
                leavesToCombine[0].transform.GetChild(0).TryGetComponent<MeshRenderer>(out renderer);
            }
            mLeavesMeshRenderer.sharedMaterial = renderer.sharedMaterial; 
            MeshCombiner.CombineLeaves(leavesToCombine, mLeaves.transform, mLeavesMeshFilter);        
        }      
    }

    public void AddNodeToPath(Vector3 position, Quaternion rotation, Vector3 up, NodeType type)
    {
        mIvyPath.Add(new Node(position, rotation, up, type));
    }

    public void AddNodeToPath(Node node)
    {
        mIvyPath.Add(node);
    }

    public List<Node> GetPath()
    {
        return mIvyPath;
    }

    void GenerateMesh()
    {
        GenerateTemplateVertices();
        GenerateTipMesh();
        GenerateBodyMesh();
    }
    void GenerateBodyMesh()
    {
        mMeshVertices = new List<Vector3>();
        mMeshIndices = new List<int>();

        //Add initial vertices
        for (int i = 0; i < mTemplateVertices.Length; i++)
        {
            mMeshVertices.Add(transform.InverseTransformPoint((mIvyTip.transform.rotation * mTemplateVertices[i]) + mIvyTip.transform.position));
        }

        //Add first layer of vertices that gets updated with the IvyTip
        AddVerticesToBody();
        AddTrianglesToBody();

    }
    void GenerateTipMesh()
    {
        int length = mTemplateVertices.Length;       
        float distanceMultiplier = GameManager.SETTINGS.IVYSETTINGS.TipDistanceMultiplier; 
        List<Vector2> uvs = new List<Vector2>();

        mIvyTipMesh.Clear();
      
        mIvyTipMeshVertices = new List<Vector3>();
        for (int i = 0; i < length; i++)
        {
            mIvyTipMeshVertices.Add(mTemplateVertices[i]);

            float t = i / (float)length;
            uvs.Add(new Vector2(t, 1));
        }

        mIvyTipMeshVertices.Add(new Vector3(0f, 0f, distanceMultiplier * mRadius));
        uvs.Add(new Vector2(0.5f, 0));

        //Add triangle indices
        mIvyTipMeshIndices = new List<int>();

        for (int i = 0; i < length; i++)
        {
            mIvyTipMeshIndices.Add(i % length);
            mIvyTipMeshIndices.Add((i + 1) % length);
            mIvyTipMeshIndices.Add(length);
        }

        //Update Mesh
        mIvyTipMesh.vertices = mIvyTipMeshVertices.ToArray();
        mIvyTipMesh.triangles = mIvyTipMeshIndices.ToArray();
        mIvyTipMesh.SetUVs(0, uvs.ToArray());
        mIvyTipMesh.RecalculateNormals();
    }

    void GenerateTemplateVertices()
    {    
        mTemplateVertices = new Vector3[mMeshResolution];

        for (int i = 0; i < mMeshResolution; i++)
        {
            float angleRad = (i / (float)mMeshResolution) * MathFunctions.pi * 2;
            Vector2 vertice2D = MathFunctions.AngleToVector(angleRad);
            mTemplateVertices[i] = vertice2D.normalized * mRadius; 
        }    
    }

    void UpdateLatestVertices()
    {
        for (int i = 0; i < mMeshResolution; i++)
        {
            int length = mMeshVertices.Count;
            mMeshVertices[length - mMeshResolution + i] = transform.InverseTransformPoint((mIvyTip.transform.rotation * (mTemplateVertices[i])) + mIvyTip.transform.position);
            mMesh.vertices = mMeshVertices.ToArray();
        }
        mMesh.RecalculateNormals();
    }
    
    void AddVerticesToBody()
    {
        for (int i = 0; i < mTemplateVertices.Length; i++)
        {
            mMeshVertices.Add(mTemplateVertices[i]);
        }  

        mMesh.vertices = mMeshVertices.ToArray();
    }

    void AddTrianglesToBody()
    {
        int offset = mMeshVertices.Count - mMeshResolution * 2;

        for (int i = 0; i < mMeshResolution; i++)
        {
            mMeshIndices.Add(offset + i);
            mMeshIndices.Add(offset + ((1 + i) % mMeshResolution));
            mMeshIndices.Add(offset + mMeshResolution + i);

            mMeshIndices.Add(offset + mMeshResolution + i);
            mMeshIndices.Add(offset + ((1 + i) % mMeshResolution));
            mMeshIndices.Add(offset + mMeshResolution + ((1 + i) % mMeshResolution));
        }

        mMesh.triangles = mMeshIndices.ToArray();
    }

    IEnumerator GoToNode(int node)
    {

        int previousNode = GetPreviousNode(node);
        int nextNode = GetNextNode(node);

        if (mLastFinishedNode != node)
        {
            mIvyTip.transform.rotation = MathFunctions.DirectionToRotation(mIvyPath[node].position - mIvyTip.transform.position, mIvyPath[previousNode].up);
        }

        float distanceToNextLeaf = Random.Range(mMinDistanceToNextLeaf, mMaxDistanceToNextLeaf);
        bool nextNodeIsEdge = false;
        
        if ( mIvyPath[node].type == NodeType.EDGE)
        {
            nextNodeIsEdge = true;          
        }

        //Update Position
        while (mIsMoving)
        {
            Vector3 lastPosition = mIvyTip.transform.position;

            float growthSpeed = GameManager.SETTINGS.IVYSETTINGS.GrowthSpeed;
            mIvyTip.transform.position = Vector3.MoveTowards(mIvyTip.transform.position, mIvyPath[node].position, Time.deltaTime * growthSpeed);

            //Leaf Placing
            float distanceFromLastPosition = Vector3.Distance(lastPosition, mIvyTip.transform.position);
            mDistanceSinceLastLeaf += distanceFromLastPosition;

            float distanceToNode = DistanceToNode(node); 

            if (mDistanceSinceLastLeaf >= distanceToNextLeaf)
            {
                if (nextNodeIsEdge)
                {
                    if (distanceToNode >= mLeafEdgeDistance)
                    {
                        AddNewLeave();
                        mDistanceSinceLastLeaf = 0f;
                        distanceToNextLeaf = Random.Range(mMinDistanceToNextLeaf, mMaxDistanceToNextLeaf);
                    }
                }
                else
                {
                    AddNewLeave();
                    mDistanceSinceLastLeaf = 0f;
                    distanceToNextLeaf = Random.Range(mMinDistanceToNextLeaf, mMaxDistanceToNextLeaf);
                }
            }

            if (distanceToNode < GameManager.SETTINGS.IVYSETTINGS.NodeRadius)
            {
                
                //stop moving and update to latest node
                mIsMoving = false;
                mLastFinishedNode = node;
                

                //set rotation if it isn't the last node in the path;   
                if (node != nextNode)
                {
                    Vector3 directionFromEarlierNode = (mIvyPath[node].position - mIvyPath[previousNode].position).normalized;
                    Vector3 directionToLaterNode = (mIvyPath[nextNode].position - mIvyPath[node].position).normalized;
                    Vector3 newDirection = (directionFromEarlierNode + directionToLaterNode).normalized;
                    mIvyTip.transform.rotation = MathFunctions.DirectionToRotation(newDirection, mIvyPath[previousNode].up);
                }   

                mIvyTip.transform.position = mIvyPath[node].position;
                NodeReached();
            }

            UpdateLatestVertices();
            yield return null;   
        }
    }

    public void StartGrowing()
    { 
        mPathfinder.FindPath(mIterations);
        StartCoroutine(CombineLeaves());

        GameManager.INSTANCE.numberOfSpawnedPlants++;
    }

    void AddNewLeave()
    {
        Vector3 position = mIvyTip.transform.up * mRadius + mIvyTip.transform.position;
        
        GameObject obj = mObjectPooler.GetObjectFromPool(mPlantName);
        obj.transform.position = position;
        Quaternion rotation = mIvyTip.transform.rotation * obj.transform.rotation;
        Quaternion randRotation = Quaternion.Euler(Random.Range(mRandRotX, 0f), Random.Range(-mRandRotY, mRandRotY), Random.Range(-mRandRotZ, mRandRotZ));
        obj.transform.rotation = rotation * randRotation;
        obj.transform.parent = transform;
        IvyLeaf leaf = obj.AddComponent<IvyLeaf>();
        leaf.StartGrowing(0.1f, 2f, this);
    }
    public void NodeReached()
    {
        UpdateLatestVertices();
        AddVerticesToBody();
        AddTrianglesToBody();
    }
    float DistanceToNode(int node)
    {
        if (node < mNumberOfNodes)
        {
            Vector3 position = mIvyTip.transform.position;
            Vector3 targetPosition = mIvyPath[node].position;
            float distance = Vector3.Distance(position, targetPosition);
            return distance; 
        }

        return -1f; 
    }

    public void AddFullyGrownLeaf(GameObject obj)
    {
        mLeavesToCombine.Enqueue(obj);
    }

    int GetNextNode(int node)
    {
        if (node < mNumberOfNodes - 1)
        {
            return node + 1; 
        }

        return node; 
    }

    int GetPreviousNode(int node)
    {
        if (node > 0)
        {
            return node - 1;
        }

        return node;
    }
}
