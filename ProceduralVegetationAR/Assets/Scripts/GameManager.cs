using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using UnityEngine.SceneManagement;

public enum GameStates
{
    IsScanning, 
    HasFinishedScanning, 
    IsPlaying,
    HasFinishedPlaying
}

[RequireComponent(typeof(GameSettings))]
[RequireComponent(typeof(ObjectPlacer))]
[RequireComponent(typeof(ShootWithPhysics))]
[RequireComponent(typeof(NavMeshSurface))]
[RequireComponent(typeof(MeshSquareManager))]
public class GameManager : MonoBehaviour
{
    public static GameManager INSTANCE;
    public static GameSettings SETTINGS;
    public static ObjectPooling OBJECTPOOLER;
    public static ObjectPlacer OBJECTPLACER;
    public static MeshSquareManager MESHSQUAREMANAGER;
    public static NavMeshSurface NAVMESHSURFACE;

    int numberOfWaterballoons = 60;
    public int numberOfSpawnedPlants = 0; 
    public static bool isDesktop; 

    //enables mouse input for desktop testing
    [SerializeField] bool mIsDesktop;

    [SerializeField] ObjectPooling mObjectPooler;
    [SerializeField] ShootWithPhysics mShooter;
    [SerializeField] ValidAreaManager mValidAreaManager;
    [SerializeField] GameObject mInfoPanel;
    [SerializeField] GameObject mHighScorePanelPrefab;


    GameStates mCurrentGameState;
    IMixedRealitySpatialAwarenessMeshObserver mObserver;



    private void Awake()
    {
  
        if (INSTANCE == null)
        {
            INSTANCE = this;
        }

        if (SETTINGS == null)
        {
            SETTINGS = GetComponent<GameSettings>();
        }

        if (OBJECTPLACER == null)
        {
            OBJECTPLACER = GetComponent<ObjectPlacer>();
        }

        if (NAVMESHSURFACE == null)
        {
            NAVMESHSURFACE = GetComponent<NavMeshSurface>();
        }
        if (MESHSQUAREMANAGER == null)
        {
            MESHSQUAREMANAGER = GetComponent<MeshSquareManager>();
        }

        if (OBJECTPOOLER == null)
        {
            OBJECTPOOLER = mObjectPooler;
        }
        

        isDesktop = mIsDesktop;

        mCurrentGameState = GameStates.IsScanning;   
    }

    private void Start()
    {
        Debug.developerConsoleVisible = false;
        mObserver = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();
        
        mInfoPanel.SetActive(true);
        mInfoPanel.transform.position = Vector3.zero + Vector3.forward * 1f + Vector3.down * 0.3f;
        mInfoPanel.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
    }

    void ShootBallon(Vector3 direction)
    {
        mShooter.Shoot(direction);
        numberOfWaterballoons--;
    }

    void ShowGameStats()
    {
        string text; 
        if (HighscoreHolder.SessionHighScore < numberOfSpawnedPlants)
        {
            HighscoreHolder.SessionHighScore = numberOfSpawnedPlants;
            text = "Good job! You achieved a new highscore!";
        }
        else
        {
            text = "No new highscore achieved!  Better luck next time!";
        }
        string currentScore = "Score: " + numberOfSpawnedPlants.ToString();
        string sessionHighScore = "Highscore: " + HighscoreHolder.SessionHighScore.ToString();

        GameObject scoreboard = Instantiate(mHighScorePanelPrefab, Camera.main.transform.position + Camera.main.transform.forward * 1f + Camera.main.transform.up * -0.3f, Quaternion.LookRotation(Camera.main.transform.forward));
        scoreboard.GetComponent<ScoreBoard>().UpdateText("Scoreboard", text, sessionHighScore, currentScore);                       
    }

   
    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadSceneAsync(currentScene.name, LoadSceneMode.Single);       
    }

    public void InputRegistered(Vector3 direction)
    {     
        if (mCurrentGameState == GameStates.IsScanning) //Erster Zustand
        {
            //Aussetzen des Observers, um das Scannen zu beenden
            //Unsichtbar machen des Spatial Mapping Meshes
            if (mObserver != null)
            {
                mObserver.Suspend();
                mObserver.DisplayOption = SpatialAwarenessMeshDisplayOptions.None;
            }
            //Erstellen der Meshes der zulässigen Bereiche für das Platzieren auf horizontalen Flächen
            mValidAreaManager.GenerateValidAreaMeshes();                   
            //Wechseln des Zustandes
            mCurrentGameState = GameStates.HasFinishedScanning; 
        }
        else if (mCurrentGameState == GameStates.HasFinishedScanning) //Zweiter Zustand
        {
            //Verdeckung von Objekten durch das Spatial Mapping Mesh aktivieren 
            if (mObserver != null)
            {             
                mObserver.DisplayOption = SpatialAwarenessMeshDisplayOptions.Occlusion;
            }
            //Verstecken der Informationstafel
            mInfoPanel.SetActive(false);
            //Unsichtbar machen des zulässigen Bereichs
            mValidAreaManager.RenderValidAreaMeshes(false);
            //Automatisches Platzieren der Vegetation an vertikalen und horizontalen Flächen
            OBJECTPLACER.PlacePlantsOnWalls(5);
            OBJECTPLACER.PlaceObjectsInRadius(new Vector3(0f, 0.2f, 0f), 5f, 8);
            //Wechsel des Zustandes
            mCurrentGameState = GameStates.IsPlaying;        
        }
        else if (mCurrentGameState == GameStates.IsPlaying) //Dritter Zustand
        {
            
            if (numberOfWaterballoons > 0)
            {
                //Schießen eines Wasserballons in die erhaltene Richtung
                ShootBallon(direction);
            }
            else
            {
                //Zeige dem Spieler die aktualisierte Punktetafel und wechsel in den letzten Zustand
                ShowGameStats();
                mCurrentGameState = GameStates.HasFinishedPlaying;
            }
        } 
        else
        {
            //Nothing to do
        }
        
    }
}
