using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ARTapToPlace : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    private bool isPlaneVisible = true;
    private readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool placingState = false;
    private GameObject objectToPlace = null;
    private GameObject placedModel = null;
    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        Touch.onFingerDown += OnFingerDown;
        planeManager.trackablesChanged.AddListener(OnPlanesChanged);
    }

    void OnDisable()
    {
        Touch.onFingerDown -= OnFingerDown;
        EnhancedTouchSupport.Disable();
        planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("READY!");
    }

    private void OnFingerDown(Finger finger)
    {
        Debug.Log("Touch detected");
        if (objectToPlace == null || raycastManager == null || !placingState) return;
        Vector2 touchPosition = finger.currentTouch.screenPosition;
        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            Debug.Log("RAYCASTED!" + hitPose);
            placedModel = Instantiate(objectToPlace, hitPose.position, hitPose.rotation);
            //placedModel.transform.localScale = objectToPlace.transform.localScale ;
            placedModel.transform.localRotation = Quaternion.Euler(0, 180, 0);
            placingState = false;
            SetVisible(false);
            Debug.Log("Placed!");
        }
    }

    private void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        foreach (var plane in args.added)
        {
            SetPlaneVisible(plane, isPlaneVisible);
        }

        foreach (var plane in args.updated)
        {
            SetPlaneVisible(plane, isPlaneVisible);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void placeModel(CharacterData data)
    {
        placingState = true;
        objectToPlace = data.model;
        SetVisible(true);
        Debug.Log("Waiting Placement...");
    }

    public void clearModel()
    {
        if (placedModel == null) return;
        Destroy(placedModel);
        Debug.Log("Model cleared.");
    }

    private void SetPlaneVisible(ARPlane plane, bool visible)
    {
        if (plane.TryGetComponent<ARPlaneMeshVisualizer>(out var visualizer))
        {
            visualizer.enabled = visible;
        }
    }

    private void SetVisible(bool visible)
    {
        Debug.Log("Plane visibility set to " + visible);
        isPlaneVisible = visible;
        foreach (var plane in planeManager.trackables)
        {
            SetPlaneVisible(plane, visible);
        }
    }
}