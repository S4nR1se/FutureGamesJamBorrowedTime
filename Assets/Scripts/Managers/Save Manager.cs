using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [Serializable]
    public struct DataToSave
    {
        public string Name;
        public int Score;
    }

    [SerializeField] private string _saveFileName = "";

    public static SaveManager SaveInstance = null;
    public DataToSave PlayerData;

    void Awake()
    {
        if (SaveInstance == null)
        {
            SaveInstance = this;
        }
        else if (SaveInstance != this)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        Save_Data();
    }

    void Save_Data()
    {
        string json_file = JsonUtility.ToJson(PlayerData, true);
        File.WriteAllText(Get_Path(), json_file);
    }

    public void Load_Data()
    {
        if(!File.Exists(Get_Path()))
        {
            Save_Data();
            return;
        }
        string jason_file = File.ReadAllText(Get_Path());
        PlayerData = JsonUtility.FromJson<DataToSave>(jason_file);
    }

    //public void Set_Score(int score)
    //{
    //    if (player_data.score >= score)
    //    {
    //        return;
    //    }

    //    player_data.score = score;
        
    //    Save_Data();
    //}

    //public void Set_Player_Name(string name)
    //{
    //    if(name == player_data.name)
    //    {
    //        return;
    //    }

    //    player_data.name = name;
    //    Save_Data();
    //}

    //public int Get_Score()
    //{
    //    return player_data.score;
    //}

    //public string Get_Player_Name()
    //{
    //    return player_data.name;
    //}

    public void Set_Player_Data()
    {

    }

    public void Get_Player_Data()
    {

    }

    string Get_Path()
    {
        return Application.persistentDataPath + "/" + _saveFileName + ".json";
    }
}
