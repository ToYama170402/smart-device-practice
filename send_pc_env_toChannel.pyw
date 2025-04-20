import psutil
import requests  # requests ライブラリをインポート
import time  # データ送信間隔を設定するための time ライブラリ
import datetime
import os

# -------------------
# 定数定義
# -------------------
THINGSPEAK_URL = "https://api.thingspeak.com/update"
THINGSPEAK_API_KEY = "WRITE_API_KEY"  # 各自の WRITE_API_KEY　を設定

# 各自ローカルフォルダの任意の場所とファイル名を指定する
LOCAL_LOG_FILE = "C:/MyWork/2025_LecSmartDevice/log/work.log"
print(LOCAL_LOG_FILE)


# -------------------
# ログファイル出力
# -------------------
def log(message):
    # ログ出力用にファイルを開く
    print(message)
    log_file = open(LOCAL_LOG_FILE, "a", encoding="utf-8")
    log_file.write(f"{datetime.datetime.now()}: {message}\n")
    log_file.flush()
    log_file.close()


# -------------------
# CPU、メモリ、ディスク、ネットワーク使用率を取得
# -------------------
def get_system_stats():

    # CPU使用率
    cpu_usage = psutil.cpu_percent(interval=0.1)
    print(f"全体CPU使用率: {cpu_usage}")
    # -- 各CPUコアの使用率を取得
    cpu_usage_per_core = psutil.cpu_percent(interval=1, percpu=True)
    print(f"各CPUコアごとの使用率: {cpu_usage_per_core}")
    # # -- 各CPUコアの最大使用率を取得
    # max_cpu_usage = max(cpu_usage_per_core)
    # print(f"max_CPU使用率: {max_cpu_usage}")
    # # -- 平均だと低く出るので、本件では最大値を使用率とみなす
    # cpu_usage = max_cpu_usage

    # メモリ使用率
    memory_usage = psutil.virtual_memory().percent

    # ディスク使用率
    disk_usage = psutil.disk_usage("/").percent

    # ネットワーク送受信バイト数を取得（インターフェースを指定）
    net_io = psutil.net_io_counters()
    bytes_sent = net_io.bytes_sent / 1024
    bytes_recv = net_io.bytes_recv / 1024

    return cpu_usage, memory_usage, disk_usage, bytes_sent, bytes_recv


# -------------------
# ThingSpeakへ送信
# -------------------
def send_data_to_thingspeak(cpu, memory, disk, bytes_sent, bytes_recv):
    # ThingSpeakにデータを送信する
    payload = {
        "api_key": THINGSPEAK_API_KEY,
        "field1": cpu,
        "field2": memory,
        "field3": disk,
        "field4": bytes_sent,
        "field5": bytes_recv,
    }
    log("データ送信中: " + str(payload))

    try:
        response = requests.post(THINGSPEAK_URL, data=payload)
        if response.status_code == 200:
            log("データ送信成功")
        else:
            log(f"データ送信失敗: {response.status_code}")

    except Exception as e:
        log(f"エラー: {e}")


# -------------------
# メインループ
# -------------------
while True:
    log("== start ==")
    # システムステータスを取得
    cpu, memory, disk, bytes_sent, bytes_recv = get_system_stats()
    print(
        f"CPU使用率: {cpu}% | メモリ使用率: {memory}% | ディスク使用率: {disk}%  | 送信バイト数: {bytes_sent} | 受信バイト数: {bytes_recv}"
    )

    # ThingSpeakにデータを送信
    send_data_to_thingspeak(cpu, memory, disk, bytes_sent, bytes_recv)

    # 30秒間待機
    time.sleep(30)

    log("== end ==")
