import platform
import wmi
import subprocess


def get_cpu_temperature_windows():
    w = wmi.WMI(namespace="root\\OpenHardwareMonitor")
    temperature_info = w.Sensor()
    for sensor in temperature_info:
        if sensor.SensorType == "Temperature":
            return sensor.Value
    return None


def get_cpu_temperature_linux():
    try:
        result = subprocess.run(["sensors"], stdout=subprocess.PIPE, text=True)
        for line in result.stdout.split("\n"):
            if "Core 0" in line:  # CPUコアの温度を取得
                return float(line.split("+")[1].split("°")[0])
    except Exception as e:
        print(f"Error: {e}")
    return None


def get_cpu_temperature_mac():
    try:
        result = subprocess.run(["osx-cpu-temp"], stdout=subprocess.PIPE, text=True)
        return float(result.stdout.strip().replace("°C", ""))
    except Exception as e:
        print(f"Error: {e}")
    return None


def get_cpu_temperature():
    system = platform.system()
    if system == "Windows":
        return get_cpu_temperature_windows()
    elif system == "Linux":
        return get_cpu_temperature_linux()
    elif system == "Darwin":  # macOS
        return get_cpu_temperature_mac()
    else:
        print("Unsupported platform")
        return None
