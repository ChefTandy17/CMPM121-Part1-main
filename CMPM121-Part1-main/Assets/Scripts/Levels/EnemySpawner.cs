using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject selector = Instantiate(button, level_selector.transform);
        selector.transform.localPosition = new Vector3(0, 130);
        selector.GetComponent<MenuSelectorController>().spawner = this;
        selector.GetComponent<MenuSelectorController>().SetLevel("Start");
    
        enemyInformation = JsonUtility.FromJson<>
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //used reading JSON file example from Markus Eger's Lecture 4: Coupling
    void LoadEnemiesJSON(){
        enemy_types = new Dictionary<string, Enemy>();                  
        var enemytext = Resources.Load<TextAsset>('enemies');           //variable to obtain the enemies.json

        JToken jo = JToken.Parse(enemytext.text);
        foreach (var enemy in jo){
            Enemy en = enemy.ToObject<Enemy>();                         //to read the enemies type
            enemy_types[en.name] = en;
        }
    }

    //used https://stackoverflow.com/questions/11126242/using-jsonconvert-deserializeobject-to-deserialize-json-to-a-c-sharp-poco-class to understand DeserializeObject
    void LoadLevelsJSON(){

        var leveltext = Resources.Load<TextAsset>('levels');            //to obtain the levels.json
        levels = JsonConvert.DeserializeObject<List<Level>>(leveltext.text);
    }

    public void StartLevel(string levelname)
    {
        level_selector.gameObject.SetActive(false);
        // this is not nice: we should not have to be required to tell the player directly that the level is starting
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        StartCoroutine(SpawnWave());
    }

    public void NextWave()
    {
        StartCoroutine(SpawnWave());
    }


    IEnumerator SpawnWave()
    {
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }
        GameManager.Instance.state = GameManager.GameState.INWAVE;
        for (int i = 0; i < 10; ++i)
        {
            yield return SpawnZombie();
        }
        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        GameManager.Instance.state = GameManager.GameState.WAVEEND;
    }

    IEnumerator SpawnZombie()
    {
        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;
                
        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(0);
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(50, Hittable.Team.MONSTERS, new_enemy);
        en.speed = 10;
        GameManager.Instance.AddEnemy(new_enemy);
        yield return new WaitForSeconds(0.5f);
    }
}

//enemy class
[System.Serializable]
public class Enemy{
    public string name;
    public int sprite;
    public int hp;
    public int speed;
    public int damage;
}

//level class
[System.Serializable]
public class Level{
    public string name;
    public int wave;
    public List<Spawn> spawns;  //double check?
}

[System.Serializable]
public class Spawn{
    public string enemy;
    public string count;
    public string hp;
    public int delay;
    public List<int> sequence;      //double check?
    public string location;
}

