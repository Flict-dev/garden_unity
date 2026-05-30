using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public static class XRControllerAdapter
{
    private static readonly List<InputDevice> Devices = new List<InputDevice>();
    private static InputDevice _headDevice;
    private static InputDevice _leftController;
    private static InputDevice _rightController;
    private static bool _devicesDirty = true;

    public static bool IsXRAvailable
    {
        get
        {
            RefreshDevicesIfNeeded();
            return _headDevice.isValid || _leftController.isValid || _rightController.isValid;
        }
    }

    public static string Status
    {
        get
        {
            RefreshDevicesIfNeeded();
            if (!IsXRAvailable)
            {
                return "XR: no headset/controllers detected";
            }

            string head = _headDevice.isValid ? "HMD" : "no HMD";
            string left = _leftController.isValid ? "L-controller" : "no L-controller";
            string right = _rightController.isValid ? "R-controller" : "no R-controller";
            return "XR: " + head + ", " + left + ", " + right;
        }
    }

    static XRControllerAdapter()
    {
        InputDevices.deviceConnected += _ => _devicesDirty = true;
        InputDevices.deviceDisconnected += _ => _devicesDirty = true;
        InputDevices.deviceConfigChanged += _ => _devicesDirty = true;
    }

    public static Vector2 ReadMoveAxis()
    {
        RefreshDevicesIfNeeded();
        return TryReadAxis(_leftController, CommonUsages.primary2DAxis, out Vector2 axis)
            ? Vector2.ClampMagnitude(axis, 1f)
            : Vector2.zero;
    }

    public static Vector2 ReadTurnAxis()
    {
        RefreshDevicesIfNeeded();
        return TryReadAxis(_rightController, CommonUsages.primary2DAxis, out Vector2 axis)
            ? Vector2.ClampMagnitude(axis, 1f)
            : Vector2.zero;
    }

    public static bool IsSprintPressed()
    {
        RefreshDevicesIfNeeded();
        return TryReadButton(_leftController, CommonUsages.gripButton)
            || TryReadFloat(_leftController, CommonUsages.grip, out float grip) && grip > 0.65f;
    }

    public static bool IsJumpPressed()
    {
        RefreshDevicesIfNeeded();
        return TryReadButton(_leftController, CommonUsages.primaryButton)
            || TryReadButton(_leftController, CommonUsages.primary2DAxisClick);
    }

    public static bool IsAttackPressed()
    {
        RefreshDevicesIfNeeded();
        return TryReadButton(_rightController, CommonUsages.triggerButton)
            || TryReadFloat(_rightController, CommonUsages.trigger, out float trigger) && trigger > 0.65f;
    }

    public static bool TryApplyHeadPose(Camera playerCamera, Vector3 baseLocalPosition)
    {
        RefreshDevicesIfNeeded();
        if (playerCamera == null || !_headDevice.isValid)
        {
            return false;
        }

        if (_headDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position))
        {
            Vector3 horizontalOffset = new Vector3(position.x, 0f, position.z);
            playerCamera.transform.localPosition = baseLocalPosition + horizontalOffset;
        }
        else
        {
            playerCamera.transform.localPosition = baseLocalPosition;
        }

        if (_headDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
        {
            playerCamera.transform.localRotation = rotation;
            return true;
        }

        return false;
    }

    private static void RefreshDevicesIfNeeded()
    {
        if (!_devicesDirty && (_headDevice.isValid || _leftController.isValid || _rightController.isValid))
        {
            return;
        }

        _headDevice = default;
        _leftController = default;
        _rightController = default;

        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, Devices);
        if (Devices.Count > 0)
        {
            _headDevice = Devices[0];
        }

        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left,
            Devices);
        if (Devices.Count > 0)
        {
            _leftController = Devices[0];
        }

        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right,
            Devices);
        if (Devices.Count > 0)
        {
            _rightController = Devices[0];
        }

        _devicesDirty = false;
    }

    private static bool TryReadAxis(InputDevice device, InputFeatureUsage<Vector2> usage, out Vector2 value)
    {
        value = Vector2.zero;
        return device.isValid && device.TryGetFeatureValue(usage, out value);
    }

    private static bool TryReadButton(InputDevice device, InputFeatureUsage<bool> usage)
    {
        return device.isValid && device.TryGetFeatureValue(usage, out bool value) && value;
    }

    private static bool TryReadFloat(InputDevice device, InputFeatureUsage<float> usage, out float value)
    {
        value = 0f;
        return device.isValid && device.TryGetFeatureValue(usage, out value);
    }
}
