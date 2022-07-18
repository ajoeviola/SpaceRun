using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformAnimator : MonoBehaviour
{
    // Public inspector variables
    public bool autoDisable = true;
    public bool isAnimating = false;
    public bool animatesPosition = true;
    public bool animatesRotation = true;
    public bool animatesScale = true;
    public float animationSpeed = 0.3f;
    public float completionThreshold = 0.01f;
    public float autoDisableThreshold = 0.01f;
    public Vector3 targetPosition;
    public Quaternion targetRotation;
    public Vector3 targetScale;
    public enum Properties
    {
        Position,
        Rotation,
        Scale
    };

    // Private variables
    private RectTransform rectTransform;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 startScale;
    private Dictionary<Properties, List<Action>> pendingOnceActions;

    void Awake()
    {
        // Initialize the pending once actions dictionary
        pendingOnceActions = new Dictionary<Properties, List<Action>>();

        // Attempt to cache the rect transform
        rectTransform = GetComponent<RectTransform>();

        // Store the initial position, rotation, and scale
        startPosition = targetPosition = rectTransform != null ? rectTransform.anchoredPosition3D : transform.position;
        startRotation = targetRotation = transform.rotation;
        startScale = targetScale = transform.localScale;
    }

    void FixedUpdate()
    {
        // Ensure we are animating and we can animate some set of components
        bool canAnimate = isAnimating && (animatesPosition || animatesRotation || animatesScale);
        if (canAnimate)
        {
            // Retrieve the acting transform
            Transform actingTransform = rectTransform != null ? rectTransform : transform;

            // Lerp the position, rotation, and scale to the target based on the duration seconds
            if (rectTransform != null)
            {
                // Set the position, rotation, and scale on the rectTransform
                if (animatesPosition) rectTransform.anchoredPosition3D = Vector3.Lerp(rectTransform.anchoredPosition3D, targetPosition, animationSpeed);
                if (animatesRotation) actingTransform.rotation = Quaternion.Lerp(actingTransform.rotation, targetRotation, animationSpeed);
                if (animatesScale) actingTransform.localScale = Vector3.Lerp(actingTransform.localScale, targetScale, animationSpeed);

                // Trigger the position completion action if the anchored position is close enough
                float distance = Vector3.Distance(rectTransform.anchoredPosition3D, targetPosition);
                if (animatesPosition && distance <= completionThreshold)
                {
                    // Trigger the position completion action
                    TriggerAnimationComplete(Properties.Position);

                    // Determine if the distance has reached the auto disable threshold
                    if (distance <= autoDisableThreshold)
                    {
                        // Disable the animator if auto disabling is enabled
                        if (autoDisable) isAnimating = false;

                        // Set the anchored position to the target position to prevent further lerping
                        rectTransform.anchoredPosition3D = targetPosition;
                    }
                }
            }
            else
            {
                // Set the position, rotation, and scale on the transform
                if (animatesPosition) actingTransform.position = Vector3.Lerp(actingTransform.position, targetPosition, animationSpeed);
                if (animatesRotation) actingTransform.rotation = Quaternion.Lerp(actingTransform.rotation, targetRotation, animationSpeed);
                if (animatesScale) actingTransform.localScale = Vector3.Lerp(actingTransform.localScale, targetScale, animationSpeed);

                // Trigger the position completion action if the position is close enough
                float distance = Vector3.Distance(actingTransform.position, targetPosition);
                if (animatesPosition && distance <= completionThreshold)
                {
                    // Trigger the position completion action
                    TriggerAnimationComplete(Properties.Position);

                    // Disable the animator if the distance is less than auto stop
                    if (distance <= autoDisableThreshold)
                    {
                        // Disable the animator if auto disabling is enabled
                        if (autoDisable) isAnimating = false;

                        // Set the position to the target position to prevent further lerping
                        actingTransform.position = targetPosition;
                    }
                }
            }

            // Trigger the rotation completion action if the rotation is close enough
            if (animatesRotation && Quaternion.Angle(actingTransform.rotation, targetRotation) <= completionThreshold) TriggerAnimationComplete(Properties.Rotation);

            // Trigger the scale completion action if the scale is close enough
            if (animatesScale && Vector3.Distance(actingTransform.localScale, targetScale) <= completionThreshold) TriggerAnimationComplete(Properties.Scale);
        }
    }

    public Vector3 GetStartPosition()
    {
        return startPosition;
    }

    public Quaternion GetStartRotation()
    {
        return startRotation;
    }

    public Vector3 GetStartScale()
    {
        return startScale;
    }

    public void SetInstantPosition(Vector3 position)
    {
        // Set the position instantly
        Transform actingTransform = rectTransform != null ? rectTransform : transform;
        actingTransform.position = position;
        targetPosition = position;
    }

    public void SetInstantRotation(Quaternion rotation)
    {
        // Set the rotation instantly
        Transform actingTransform = rectTransform != null ? rectTransform : transform;
        actingTransform.rotation = rotation;
        targetRotation = rotation;
    }

    public void SetInstantScale(Vector3 scale)
    {
        // Set the scale instantly
        Transform actingTransform = rectTransform != null ? rectTransform : transform;
        actingTransform.localScale = scale;
        targetScale = scale;
    }

    public void OnceAnimationComplete(Properties property, Action callback)
    {
        // Initialize the actions list for this property if it doesn't exist
        if (!pendingOnceActions.ContainsKey(property)) pendingOnceActions[property] = new List<Action>();

        // Add the callback to the list
        pendingOnceActions[property].Add(callback);
    }

    void TriggerAnimationComplete(Properties property)
    {
        // Ensure we have a list of actions for this property
        if (pendingOnceActions.ContainsKey(property))
        {
            // Ensure we have some actions to trigger
            if (pendingOnceActions[property].Count > 0)
            {
                // Loop through the list of actions and call them
                foreach (Action callback in pendingOnceActions[property])
                {
                    callback();
                }

                // Clear the list
                pendingOnceActions[property].Clear();
            }
        }
    }
}
