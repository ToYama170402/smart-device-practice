using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace SmartLec
{
    public class ThingSpeakAccess
    {
        private string CHANNEL_ID = "";
        private string READ_API_KEY = "";
        private int INITIAL_FETCH_COUNT = 10;
        private float FETCH_INTERVAL = 30f;

        ThingSpeakData _data = new ThingSpeakData();

        public string CHANNEL_ID1 { get => CHANNEL_ID; set => CHANNEL_ID = value; }
        public string READ_API_KEY1 { get => READ_API_KEY; set => READ_API_KEY = value; }
        public int INITIAL_FETCH_COUNT1 { get => INITIAL_FETCH_COUNT; set => INITIAL_FETCH_COUNT = value; }
        public float FETCH_INTERVAL1 { get => FETCH_INTERVAL; set => FETCH_INTERVAL = value; }

        public ThingSpeakAccess(string cHANNEL_ID, string rEAD_API_KEY)
        {
            CHANNEL_ID1 = cHANNEL_ID;
            READ_API_KEY1 = rEAD_API_KEY;
        }

        [System.Serializable]  // ★Jsonデータを受信するために必要 / Required for receiving JSON data
        public class ThingSpeakResponse
        {
            public Feed[] feeds;
        }

        [System.Serializable]  // ★Jsonデータを受信するために必要 / Required for receiving JSON data
        public class Feed
        {
            public string created_at; // 作成日時 / Timestamp when the data was created
            public string field1;      // CPU使用率 / CPU usage
            public string field2;      // メモリ使用率 / Memory usage
            public string field3;      // HDD使用率 / HDD usage
            public string field4;      // データ送信量 / Amount of data sent
            public string field5;      // データ受信量 / Amount of data received
            public string field6;      // CPU温度 / CPU temperature (if needed)
        }

        /// <summary>
        /// ThingSpeakからデータを取得するコルーチン
        /// Coroutine to fetch data from ThingSpeak
        /// </summary>
        /// <param name="requestCount">取得するデータ件数 / Number of records to fetch</param>
        /// <param name="onDataFetched">データ取得後に呼び出すコールバック / Callback after data is fetched</param>
        /// <returns>IEnumerator (コルーチン用) / IEnumerator for coroutine</returns>
        public IEnumerator FetchThingSpeakData(int requestCount, Action<ThingSpeakData> onDataFetched)
        {
            string uri = $"https://api.thingspeak.com/channels/{CHANNEL_ID}/feeds.json?api_key={READ_API_KEY}&results={requestCount}";

            Debug.Log("Fetching data from: " + uri);

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error: " + webRequest.error);
                    onDataFetched?.Invoke(null); // エラー時にはnullを返す / Return null in case of error
                }
                else
                {
                    // レスポンスを解析し、コールバックに渡す / Parse response and pass to callback
                    ThingSpeakData data = ProcessResponse(webRequest.downloadHandler.text);
                    onDataFetched?.Invoke(data);
                }
            }
        }


        /// <summary>
        /// ThingSpeakのレスポンスを解析する
        /// Parse the response text from ThingSpeak
        /// </summary>
        /// <param name="responseText">レスポンステキスト / Response text</param>
        /// <returns>解析されたデータ / Parsed data</returns>
        public ThingSpeakData ProcessResponse(string responseText)
        {
            ThingSpeakResponse thingSpeakData = JsonUtility.FromJson<ThingSpeakResponse>(responseText);

            foreach (Feed feed in thingSpeakData.feeds)
            {
                // UTC日時をパースしてJSTに変換 / Parse UTC time and convert to JST
                string utcTimeString = feed.created_at;
                DateTime currentDatetime = DateTime.Parse(utcTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);

                // 最新のデータと同じ日時分ならスキップ / Skip if same timestamp already exists
                // 秒以下のデータは無視 / Ignore data with seconds precision
                if (_data.Count > 0)
                {
                    var lastDatetime = _data.DatetimeData[^1];
                    lastDatetime = lastDatetime.AddSeconds(-lastDatetime.Second); // 分以下を切り捨て
                    currentDatetime = currentDatetime.AddSeconds(-currentDatetime.Second); // 秒以下を切り捨て
                    if (currentDatetime == lastDatetime)
                    {
                        // 同じ日時分なら平均を取る
                        var latestCpu = _data.CpuData[^1];
                        var latestMemory = _data.MemoryData[^1];
                        var latestHdd = _data.HddData[^1];
                        var latestCpuTemp = _data.CpuTemp[^1];
                        latestCpu = (latestCpu + (float.TryParse(feed.field1, out float cpu2) ? cpu2 : 0)) / 2;
                        latestMemory = (latestMemory + (float.TryParse(feed.field2, out float memory2) ? memory2 : 0)) / 2;
                        latestHdd = (latestHdd + (float.TryParse(feed.field3, out float hdd2) ? hdd2 : 0)) / 2;
                        latestCpuTemp = (latestCpuTemp + (float.TryParse(feed.field6, out float temp2) ? temp2 : 0)) / 2;
                        _data.CpuData[^1] = latestCpu;
                        _data.MemoryData[^1] = latestMemory;
                        _data.HddData[^1] = latestHdd;
                        _data.CpuTemp[^1] = latestCpuTemp;

                        var latestSendBytes = _data.SndBytes[^1];
                        var latestReceiveBytes = _data.RcvBytes[^1];
                        latestSendBytes = (latestSendBytes + (float.TryParse(feed.field4, out float snd2) ? snd2 : 0)) / 2;
                        latestReceiveBytes = (latestReceiveBytes + (float.TryParse(feed.field5, out float rcv2) ? rcv2 : 0)) / 2;
                        _data.SndBytes[^1] = latestSendBytes;
                        _data.RcvBytes[^1] = latestReceiveBytes;
                        continue;
                    }
                }
                _data.DatetimeData.Add(currentDatetime.AddSeconds(-currentDatetime.Second));

                // 各フィールドをパースしデータに追加 / Parse and add each field
                _data.CpuData.Add(float.TryParse(feed.field1, out float cpu) ? cpu : 0);
                _data.MemoryData.Add(float.TryParse(feed.field2, out float memory) ? memory : 0);
                _data.HddData.Add(float.TryParse(feed.field3, out float hdd) ? hdd : 0);

                var crntSendBytes = float.TryParse(feed.field4, out float snd) ? snd : 0;
                var crntReceiveBytes = float.TryParse(feed.field5, out float rcv) ? rcv : 0;

                _data.SndBytes.Add(crntSendBytes);
                _data.RcvBytes.Add(crntReceiveBytes);

                _data.CpuTemp.Add(float.TryParse(feed.field6, out float temp) ? temp : 0);


                Debug.Log($"{currentDatetime} {_data.CpuData[^1]}, {_data.MemoryData[^1]}, {_data.HddData[^1]}, {crntSendBytes}, {crntReceiveBytes}");
                _data.Count++;
            }

            return _data;
        }
    }
}
