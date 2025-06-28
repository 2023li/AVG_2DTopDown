using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SaveSelectButon:MonoBehaviour
{
    public string SaveFullPath;
    public string PlayerName;
    public string SaveDate;
    public TMP_Text txt_PlayerName;
    public TMP_Text txt_SaveDate;
    public Button btn;

    public void Init(string playerName,string saveDate,string tSave)
    {
        if (btn ==null)
        {
            btn = GetComponent<Button>();
        }
        PlayerName = playerName;
        SaveDate = saveDate;
        SaveFullPath = tSave;

        txt_PlayerName.text = PlayerName;
        txt_SaveDate.text = SaveDate;

        btn.onClick.AddListener(() =>
        {
            PersistenceManager.Instance.LoadSave(SaveFullPath);
        });
    }


}

