using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float hueShiftSpeed;

    private Camera mainCamera;
    private Vector3 originalPosition;
    private Vector3 hsv;

    private void OnEnable() => Map.OnGridGenerated += SetOriginalPosition;

    private void OnDisable() => Map.OnGridGenerated -= SetOriginalPosition;

    private void Start() 
    {
        mainCamera = GetComponent<Camera>();  
        originalPosition = transform.position;
        Color.RGBToHSV(mainCamera.backgroundColor, out _, out hsv.y, out hsv.z);
        hsv.x = (Time.time * hueShiftSpeed) % 1;
    }

    private void Update()
    {
        mainCamera.orthographicSize = Mathf.Max(1, mainCamera.orthographicSize + -Input.mouseScrollDelta.y * zoomSpeed);
        transform.Translate(new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0) * moveSpeed * Time.deltaTime);
        if (Input.GetKeyDown(KeyCode.R)) transform.position = originalPosition;

        // HUE shift
        hsv.x = (Time.time * hueShiftSpeed) % 1;
        mainCamera.backgroundColor = Color.HSVToRGB(hsv.x, hsv.y, hsv.z);
    }

    private void SetOriginalPosition(IMap map)
    {
        var neighbourData = map.GetNeighbourData();
        Vector2Int size = map.GetGridSize();
        int lastColumnVerticalOffset = (size.y - 1) % 2 == 0 ? 0 : neighbourData.diagonalOffset.y;
        Vector2Int lastPosition = new Vector2Int((size.y - 1) * neighbourData.diagonalOffset.x, lastColumnVerticalOffset + (size.x - 1) * neighbourData.verticalOffset);
        originalPosition = new Vector3(lastPosition.x / 2f, lastPosition.y / 2f, mainCamera.transform.position.z);
        transform.position = originalPosition;
    }
}
