using System;
using System.Collections.Generic;

namespace SmartLec
{
    /// <summary>
    /// ThingSpeakから取得したデータを格納するクラス
    /// Class for storing data fetched from ThingSpeak
    /// </summary>
    public class ThingSpeakData
    {
        private int _count; // データ数 / Number of data entries

        List<DateTime> datetimeData = new List<DateTime>(); // タイムスタンプ一覧 / List of timestamps
        List<float> _cpuData = new List<float>(); // CPU使用率一覧 / List of CPU usage values
        List<float> _memoryData = new List<float>(); // メモリ使用率一覧 / List of memory usage values
        List<float> _hddData = new List<float>(); // HDD使用率一覧 / List of HDD usage values
        List<float> _sndBytes = new List<float>(); // データ送信量（差分）一覧 / List of sent bytes (difference per interval)
        List<float> _rcvBytes = new List<float>(); // データ受信量（差分）一覧 / List of received bytes (difference per interval)
        List<float> _totalBytesSentList = new List<float>(); // 累積データ送信量一覧 / List of total bytes sent
        List<float> _totalBytesRecvList = new List<float>(); // 累積データ受信量一覧 / List of total bytes received
        List<float> _cpuTemp = new List<float>(); // CPU温度一覧 / List of CPU temperature values (if needed)

        // 各データリストへのプロパティアクセス / Properties for accessing each data list
        public List<DateTime> DatetimeData { get => datetimeData; set => datetimeData = value; }
        public List<float> CpuData { get => _cpuData; set => _cpuData = value; }
        public List<float> MemoryData { get => _memoryData; set => _memoryData = value; }
        public List<float> HddData { get => _hddData; set => _hddData = value; }
        public List<float> SndBytes { get => _sndBytes; set => _sndBytes = value; }
        public List<float> RcvBytes { get => _rcvBytes; set => _rcvBytes = value; }
        public List<float> TotalBytesSentList { get => _totalBytesSentList; set => _totalBytesSentList = value; }
        public List<float> TotalBytesRecvList { get => _totalBytesRecvList; set => _totalBytesRecvList = value; }
        public List<float> CpuTemp { get => _cpuTemp; set => _cpuTemp = value; }

        /// <summary>
        /// 登録されているデータ件数
        /// Number of data entries registered
        /// </summary>
        public int Count { get => _count; set => _count = value; }
    }
}
