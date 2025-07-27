using System.Drawing;

using UnityEngine;

namespace SideXP.Core.Demos
{

    /// <summary>
    /// Illustrates usage of custom attributes for inspector GUI.
    /// </summary>
    [AddComponentMenu(Constants.AddComponentMenuDemosCore + "/Custom Attributes Demo")]
    public class CustomAttributesDemoComponent : MonoBehaviour
    {

        [AnimCurve(FColor.Orange)]
        [Tooltip("This animation curve is clamped between 0 and 1 for both axes.")]
        public AnimationCurve AnimCurve = AnimationCurveUtility.Linear;

        [AnimCurve(1, 5, FColor.Cyan)]
        [Tooltip("This animation curve is clamped between 0 and 1 along X axis, and between 0 and 5 along Y axis.")]
        public AnimationCurve AnimCurve2 = AnimationCurveUtility.EaseIn;

        [AnimCurve(-1, 1, -1, 1, FColor.Magenta)]
        [Tooltip("This animation curve is clamped between -1 and 1 for both axes.")]
        public AnimationCurve AnimCurve3 = AnimationCurveUtility.EaseOut;

        [Boolton(nameof(NotifyChangeState))]
        [Tooltip("When clicked, this button will switch the value of this boolean property and log a message.")]
        public bool ChangeStateBoolton = false;

        [Tooltip("If checked, make the next field enabled.")]
        public bool EnableNextField = true;

        [EnableIf(nameof(EnableNextField))]
        [Tooltip("This field is enabled only if the previous one is checked.")]
        public bool DisableNextField = false;

        [DisableIf(nameof(DisableNextField))]
        [Tooltip("The field is enabled only if the previous one is not checked.")]
        public string DemoEnable = "Field enabled only if previous one is not checked";

        [FolderPath]
        [Tooltip("Allow you to select a path to a folder. Use the \"" + nameof(FolderPathAttribute.AllowExternal) + "\" option to allow the user to select a path from out of this project.")]
        public string FolderPath = string.Empty;

        [Indent]
        public string Indent = "Indented at level 1";

        [Indent(2)]
        public string Indent2 = "Indented at level 2";

        [ProgressBar]
        public float ProgressBar = .3f;

        [ProgressBar(100, FColor.Red, Suffix = "%", Clamp = true)]
        public float ProgressBarPercents = 60f;

        [ProgressBar(-100, 100, FColor.Green, Clamp = true, Wide = true)]
        public int ProgressBarFull = -20;

        [Remap(0, 100, 0, 1, Units = "%", Clamped = true)]
        [Tooltip("Use Debug mode in the Inspector to see the remapped values stored in this property.")]
        public float RemapPercentsToRatio = .5f;

        [Remap(-10, 10, -1, 1)]
        [Tooltip("Use Debug mode in the Inspector to see the remapped values stored in this property.")]
        public int RemapUnclamped = 500;

        [Remap(-10, 10, -1, 1, Clamped = true)]
        [Tooltip("Use Debug mode in the Inspector to see the remapped values stored in this property." +
            "\nNote that this will remap the vector components, but won't normalize the vector itself.")]
        public Vector2 RemapAxis = Vector2.up;

        [Separator]

        public string PostSeparator = "";

        // Called when clicking on the Change State "boolton"
        private void NotifyChangeState(bool state)
        {
            Debug.Log("The state has changed: " + state);
        }

    }

}