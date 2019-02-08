/*
 * ImageTargetDistanceScaleManager.cs
 *
 * Project: 4ME306 - Cross Media Design and Production; AR workshop 2
 *
 * Supported Unity version: 2018.2.19f1 Personal (tested)
 *
 * Author: Nico Reski
 * Web: http://reski.nicoversity.com
 * Twitter: @nicoversity
 ****************************
 *Revised version of Nico Reskis original code by:
 *Torbjörn Holgersson
 *Mail: th222mw@student.lnu.se
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageTargetDistanceScaleManager2 : MonoBehaviour
{
    // indicator if custom debug messages are shown in the Unity Console (default = true; can also be set in the Unity Inspector)
    public bool showDebugMessages = true;

    // references to image target (= AR marker) game objects (set in the Unity Inspector)
    // (specifically: references to ImageTargetTracker script components of these game objects)
    public ImageTargetTracker imageTargetContent;
    public ImageTargetTracker imageTargetMinus;

    // reference to content virtual object of the CONTENT AR marker (set in the Unity Inspector)
    public Transform contentVirtualObject;

    // helper value to keep track of the CONTENT AR marker's virtual object original scale
    private Vector3 contentVirtualObjectOriginalScale;

    // helper value indicating the maximum scaling of our CONTENT AR marker's virtual object
    private static readonly float minScale = -2.0f;

    // initialization; called at the start of the application/scene
    void Start()
    {
        // check if references via Unity Inspector are set
        if (!imageTargetContent) Debug.LogError("[ImageTargetDistanceManager] Reference not set: Image Target Content");
        if (!imageTargetMinus) Debug.LogError("[ImageTargetDistanceManager] Reference not set: Image Target Minus");
        if (!contentVirtualObject) Debug.LogError("[ImageTargetDistanceManager] Reference not set: Content Virtual Object");

        // register ("remember") the original scale of the CONTENT AR marker's virtual object at the start of the application/scene
        contentVirtualObjectOriginalScale = contentVirtualObject.transform.localScale;
    }

    // update; called once per frame
    void Update()
    {
        // check if the CONTENT AR marker is tracked
        if (imageTargetContent.isTracked)
        {
            // while CONTENT AR marker is tracked, check also if Minus AR marker is tracked
            if (imageTargetMinus.isTracked)
            {
                // CONTENT AR and Minus AR markers are tracked = calculate scale up the CONTENT AR marker's virtual object
                if (showDebugMessages) Debug.Log("Distance: Content <-> Minus = " + distanceContentToMinus());
                scaleUpContent(distanceContentToMinus());
            }
        }
    }


    #region DISTANCE

    // calculate distance between CONTENT and Minus AR markers (0.0f == one or both markers not detected)
    private float distanceContentToMinus()
    {
        float distance = 0.0f;
        if (imageTargetContent.isTracked && imageTargetMinus.isTracked)
        {
            Vector3 contentPosition = imageTargetContent.transform.position;
            Vector3 minusPosition = imageTargetMinus.transform.position;
            distance = Vector3.Distance(contentPosition, minusPosition);
        }
        return distance;
    }

    #endregion


    #region SCALING

    // helper method to calculate a scale factor based on a distance (quantization)
    private float calculateScaleMultiplier(float distance)
    {
        // float helper value to calculate a new scaling based on the distance
        float scaleMultiplier = -0.0001f;
        if (distance >= 3.5f) scaleMultiplier = -0.0001f;
        else if (distance >= 3.0f && distance < 3.5) scaleMultiplier = -0.0005f;
        else if (distance >= 2.5f && distance < 3.0) scaleMultiplier = -0.001f;
        else if (distance >= 2.0f && distance < 2.5) scaleMultiplier = -0.005f;
        else if (distance >= 1.5f && distance < 2.0) scaleMultiplier = -0.01f;
        else if (distance >= 1.0f && distance < 1.5) scaleMultiplier = -0.02f;
        else if (distance < 1.0f) scaleMultiplier = -0.03f;

        return scaleMultiplier;
    }

    // scale up the virtual object of the CONTENT AR marker based on a distance value
    private void scaleUpContent(float distance)
    {
        // get scale multiplier based on distance
        float scaleMultiplier = calculateScaleMultiplier(distance);

        // check for upper scaling maximum (to restrict the virtual object from growing any further)
        if ((contentVirtualObject.transform.localScale.x > contentVirtualObjectOriginalScale.x / minScale) &&
           (contentVirtualObject.transform.localScale.y > contentVirtualObjectOriginalScale.y / minScale) &&
           (contentVirtualObject.transform.localScale.z > contentVirtualObjectOriginalScale.z / minScale))
        {
            // apply scaling
            contentVirtualObject.transform.localScale += new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);
        }

        // re-align virtual object to AR marker
        repositionVirtualObject();
    }

    // helper method to reposition the virtual object on top of the AR marker after it was scaled
    private void repositionVirtualObject()
    {
        // reposition virtual object to "float" (align) slightly over the the physical AR marker
        contentVirtualObject.transform.localPosition = new Vector3(contentVirtualObject.transform.localPosition.x,
                                                                    contentVirtualObject.transform.localScale.y * 0.5f + 0.25f,
                                                                    contentVirtualObject.transform.localPosition.z);
    }

    #endregion
}
