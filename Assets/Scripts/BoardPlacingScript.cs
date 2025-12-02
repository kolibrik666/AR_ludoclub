using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;

public class BoardPlacingScript : MonoBehaviour
{
    [Header("Prefab to Place")]
    [SerializeField] private GameObject prefabToPlace;

    private ARRaycastManager arRaycastManager;
    private ARPlaneManager arPlaneManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject placedObject;
    private bool hasBeenPlaced = false;

    private void Awake()
    {
        arRaycastManager = FindFirstObjectByType<ARRaycastManager>();
        arPlaneManager = FindFirstObjectByType<ARPlaneManager>();
    }

    private void Update()
    {
        if (hasBeenPlaced)
            return;

        if (arRaycastManager == null)
            return;

        Touchscreen touchscreen = Touchscreen.current;
        Mouse mouse = Mouse.current;

        bool touchDetected = touchscreen != null && touchscreen.primaryTouch.press.isPressed;
        bool mouseClicked = mouse != null && mouse.leftButton.wasPressedThisFrame;

        if (touchDetected || mouseClicked)
        {
            Vector2 touchPosition;

            if (touchDetected)
                touchPosition = touchscreen.primaryTouch.position.ReadValue();
            else
                touchPosition = mouse.position.ReadValue();

            if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                ARPlane hitPlane = hits[0].trackable as ARPlane;
                PlacePrefab(hitPose, hitPlane);
            }
        }
    }

    private void PlacePrefab(Pose pose, ARPlane plane)
    {
        placedObject = Instantiate(prefabToPlace, pose.position, pose.rotation);
        hasBeenPlaced = true;
        DisablePlaneDetection();
    }

    private void DisablePlaneDetection()
    {
        arPlaneManager.enabled = false;

        foreach (ARPlane plane in arPlaneManager.trackables)
            plane.gameObject.SetActive(false);
    }

    public void ResetPlacement()
    {
        if (placedObject != null)
        {
            Destroy(placedObject);
            placedObject = null;
        }

        hasBeenPlaced = false;
        arPlaneManager.enabled = true;

        foreach (ARPlane plane in arPlaneManager.trackables)
            plane.gameObject.SetActive(true);
    }
}
