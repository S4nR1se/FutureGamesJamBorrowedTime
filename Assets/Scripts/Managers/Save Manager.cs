using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [Serializable]
    public struct Data_To_Save
    {
        public string name;
        public int score;
    }

    [SerializeField] string save_file_name = "";

    public static SaveManager save_instance = null;
    public Data_To_Save player_data;

    void Awake()
    {
        if (save_instance == null)
        {
            save_instance = this;
        }
        else if (save_instance != this)
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
        string json_file = JsonUtility.ToJson(player_data, true);
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
        player_data = JsonUtility.FromJson<Data_To_Save>(jason_file);
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
        return Application.persistentDataPath + "/" + save_file_name + ".json";
    }
}
