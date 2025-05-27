using UnityEngine;
using SmartLec;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

public class VisualizeMetrics : MonoBehaviour
{
    public class Config
    {
        public string CHANNEL_ID;
        public string READ_API_KEY;
    }


    private Action<ThingSpeakData> SetPositionAndRotation(int zIndex)
    {
        void generateCubesFromData(
            List<float> list,
            int xIndex = 0,
            Func<float, float> ctrlCubeHeight = null,
            Func<float, Color> ctrlCubeColor = null
        )
        {
            // キューブの高さを制御する関数が指定されていない場合はデフォルトの高さを使用
            ctrlCubeHeight ??= (value) => value;
            // キューブの色を制御する関数が指定されていない場合はデフォルトの色を使用
            ctrlCubeColor ??= (value) => Color.white;
            {
                foreach (var item in list.Select((value, index) => new { value, index }))
                {
                    GameObject cube = Instantiate(Resources.Load<GameObject>("Bar"), transform);
                    cube.transform.position = new Vector3(xIndex, 0, item.index + zIndex);
                    cube.transform.localScale = new Vector3(1, ctrlCubeHeight(item.value), 1);
                    Color color = ctrlCubeColor(item.value);
                    cube.GetComponent<Renderer>().material.color = color;
                }
            }
        }

        Func<float, Color> ctrlCubeColorFunc(float hue)
        {
            return (value) =>
            {
                float normalizedValue = Mathf.Clamp(value / 100f, 0.1f, 0.8f); // 0-100の範囲を0-1に正規化
                return Color.HSVToRGB(hue, normalizedValue, 1f); // 色相を指定して色を生成
            };
        }

        float ctrlCubeHeightFunc(float value)
        {
            return Mathf.Clamp(value / 10, 0.1f, 10f); // 高さを0.1から10の範囲に制限
        }
        // データ取得後の処理
        return (ThingSpeakData data) =>
        {
            if (data != null)
            {
                Debug.Log("Data fetched successfully!");
                generateCubesFromData(data.CpuData, 0, ctrlCubeHeightFunc, ctrlCubeColorFunc(0.5f));
                generateCubesFromData(data.CpuTemp, 1, ctrlCubeHeightFunc, ctrlCubeColorFunc(1.00f));
                generateCubesFromData(data.MemoryData, 2, ctrlCubeHeightFunc, ctrlCubeColorFunc(0.66f));
                generateCubesFromData(data.HddData, 3, ctrlCubeHeightFunc, ctrlCubeColorFunc(0.33f));
                foreach (var datetime in data.DatetimeData.Select((value, index) => new { value, index }))
                {
                    GameObject datetimeObject = new GameObject("Datetime");
                    datetimeObject.transform.SetPositionAndRotation(new Vector3(-1, 0, datetime.index + zIndex), Quaternion.Euler(90, 0, 0));
                    TextMesh textMesh = datetimeObject.AddComponent<TextMesh>();
                    textMesh.text = datetime.value.AddHours(+9).ToString();
                    textMesh.anchor = TextAnchor.LowerRight;
                    textMesh.alignment = TextAlignment.Right;
                }
                zIndex += data.Count; // z座標を更新
                Debug.Log($"Total data count: {data.Count}");
            }
            else
            {
                Debug.LogError("Failed to fetch data.");
            }
        };
    }

    private ThingSpeakAccess thingSpeakAccess;
    private int zOffset = 0; // z座標の初期値

    void Start()
    {
        // 設定ファイルを読み込む
        string configPath = Path.Combine(Application.dataPath, "config.json");
        string json = File.ReadAllText(configPath);
        Config config = JsonUtility.FromJson<Config>(json);

        // SmartLecの初期化
        thingSpeakAccess = new ThingSpeakAccess(config.CHANNEL_ID, config.READ_API_KEY);
        StartCoroutine(thingSpeakAccess.FetchThingSpeakData(100, SetPositionAndRotation(zOffset)));
        zOffset++; // 初回のz座標を更新
    }

    void Update()
    {

    }
}
