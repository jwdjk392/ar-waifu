using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using System;

[Serializable]
public enum CharacterMotion
{
    Idle,
    Thinking,
    Talking,
    Waving,
    Fainting
}

public class ARModelManager : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    private bool isPlaneVisible = true;
    private readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool placingState = false;
    private GameObject objectToPlace = null;
    public GameObject currentModel = null;
    private float modelScale = 1f;
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
            currentModel = Instantiate(objectToPlace, hitPose.position, hitPose.rotation);
            currentModel.transform.localRotation = Quaternion.Euler(0, 180, 0);
            ChangeModelScale(modelScale);
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
    
    public void PlaceModel(CharacterData data)
    {
        ClearModel();
        placingState = true;
        objectToPlace = data.model;
        SetVisible(true);
        Debug.Log("Waiting Placement...");
    }

    public void ClearModel()
    {
        if (currentModel == null) return;
        Destroy(currentModel);
        currentModel = null;
        Debug.Log("Model cleared.");
    }

    public void ChangeModelScale(float scale)
    {
        if (currentModel == null) return;
        modelScale = scale;
        currentModel.transform.localScale = new Vector3(scale, scale, scale);
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

    public void SetAnimation(CharacterMotion motion)
    {
        if (currentModel == null) return;
        currentModel.GetComponent<Animator>().SetInteger("motion", (int)motion);
    }
}