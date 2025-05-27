using UnityEngine;

public class MainCamera : MonoBehaviour
{
    void Start()
    {
        // 初期設定
        Cursor.lockState = CursorLockMode.Locked; // カーソルを画面中央に固定
        Cursor.visible = false; // カーソルを非表示
    }
    public float moveSpeed = 5f; // カメラの移動速度
    public float dashSpeed = 15f; // ダッシュ時の速度
    public float mouseSensitivity = 2f; // マウス感度

    float rotationX = 0f;
    float rotationY = 0f;

    void Update()
    {
        // キーで移動
        float h = Input.GetAxis("Horizontal"); // 左右移動
        float v = Input.GetAxis("Vertical");   // 前後移動
        float y = 0f;
        if (Input.GetKey(KeyCode.Space)) y += 1f;           // スペースキーで上昇
        if (Input.GetKey(KeyCode.LeftShift)) y -= 1f;        // 左Shiftで下降（任意）

        // ダッシュ判定（左Ctrl押下でダッシュ）
        float speed = Input.GetKey(KeyCode.LeftControl) ? dashSpeed : moveSpeed;

        // カメラのローカルz方向（前方向）をXZ平面に投影
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();

        // カメラのローカルx方向（右方向）をXZ平面に投影
        Vector3 right = transform.right;
        right.y = 0;
        right.Normalize();

        // 前後左右移動（XZ平面のみ）
        Vector3 moveHorizontal = right * h * speed * Time.deltaTime;
        Vector3 moveVertical = forward * v * speed * Time.deltaTime;
        transform.Translate(moveHorizontal + moveVertical, Space.World);

        // 上下移動（ワールド座標系）
        transform.Translate(0, y * speed * Time.deltaTime, 0, Space.World);

        // マウスで視点回転
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // 上下の回転制限

        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }
}
