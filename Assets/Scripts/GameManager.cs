using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using GoogleARCore.Examples.Common;

#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject m_Prefab;
    [SerializeField] private GameObject m_Indicator;

    private GameObject m_CurrentIndicator;
    private Camera m_Cam;

    
    void Start() {
        m_Cam = Camera.main;
    }

    
    void Update() {
        UpdateApplicationLifecycle();
        ActiveGridAndSpawnObject(m_Prefab);
    }


    private void UpdateApplicationLifecycle() {
        if (Input.GetKey(KeyCode.Escape)) {
            Application.Quit();
        }

        if (Session.Status != SessionStatus.Tracking)
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        else 
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private bool ActiveGridAndSpawnObject(GameObject prefab) {
        TrackableHitFlags _raycastFilter = TrackableHitFlags.PlaneWithinPolygon;
        Vector3 _position = m_Cam.transform.position;
        
        //showing indicator if found plane
        bool _found = Frame.Raycast(_position.x + (m_Cam.pixelWidth / 2), _position.y + (m_Cam.pixelHeight / 2), 
                                                                                    _raycastFilter, out TrackableHit _hit);
        if (_found) {
            if (m_CurrentIndicator == null) 
                m_CurrentIndicator = Instantiate(m_Indicator, _hit.Pose.position, _hit.Pose.rotation);
            else {
                m_CurrentIndicator.transform.position = _hit.Pose.position;
                m_CurrentIndicator.transform.rotation = _hit.Pose.rotation;
            }
            m_CurrentIndicator.SetActive(true);
        }

        if (Input.touchCount < 1 || (Input.GetTouch(0)).phase != TouchPhase.Began) {
            return false;
        }

       
       //spawning prefab on hit pos 
        if (_found) {
            if ((_hit.Trackable is DetectedPlane) &&
                    Vector3.Dot(m_Cam.transform.position - _hit.Pose.position, _hit.Pose.rotation * Vector3.up) < 0) {
                        Debug.Log("Hit at back of the DetectedPlane");
            }
            else {
                GameObject _prefab = null;
                if (_hit.Trackable is FeaturePoint)
                    _prefab = prefab;
                else if (_hit.Trackable is DetectedPlane) {
                    DetectedPlane _detectedPlane = _hit.Trackable as DetectedPlane;
                    if ((_detectedPlane.PlaneType == DetectedPlaneType.HorizontalUpwardFacing) ||
                                                     (_detectedPlane.PlaneType == DetectedPlaneType.Vertical)) {
                        _prefab = prefab;
                    }
                }
                else
                    _prefab = prefab;

                if (_prefab != null) {
                    var _tmpObject = Instantiate(_prefab, _hit.Pose.position, _hit.Pose.rotation);
                    var _anchor = _hit.Trackable.CreateAnchor(_hit.Pose);
                    _tmpObject.transform.parent = _anchor.transform;
                    return true;
                }
            }
        }
        return false;
    }

}