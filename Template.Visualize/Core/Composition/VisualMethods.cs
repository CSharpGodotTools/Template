#if DEBUG
using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Godot.Control;

namespace GodotUtils.Debugging;

/// <summary>
/// Builds runtime UI for invoking visualize-marked methods, including parameter editors and popup execution.
/// </summary>
internal static class VisualMethods
{
    private const int ParamsVBoxSeparation = 2;
    private const int ParamRowSeparation = 6;
    private const int PopupContentSeparation = 8;
    private const int PopupMargin = 8;
    private const int PopupOffsetY = 4;
    private const float ButtonBackgroundAlpha = 0.35f;
    private const float ButtonBorderAlpha = 0.5f;
    private const int BorderWidth = 1;
    private const int CornerRadius = 3;
    private const int ButtonContentMarginLeft = 8;
    private const int ButtonContentMarginRight = 8;
    private const int ButtonContentMarginTop = 4;
    private const int ButtonContentMarginBottom = 4;
    private static readonly Color _methodButtonBackgroundColor = new(0, 0, 0, ButtonBackgroundAlpha);
    private static readonly Color _methodButtonBorderColor = new(0, 0, 0, ButtonBorderAlpha);

    /// <summary>
    /// Creates editable controls for each method parameter and stores edited values in <paramref name="providedValues"/>.
    /// </summary>
    /// <param name="method">Method whose parameters should be editable.</param>
    /// <param name="providedValues">Array that receives current parameter values from controls.</param>
    /// <returns>Container with one editor row per method parameter.</returns>
    public static VBoxContainer CreateMethodParameterControls(MethodInfo method, object[] providedValues)
    {
        VBoxContainer paramsVBox = new();
        paramsVBox.AddThemeConstantOverride("separation", ParamsVBoxSeparation);

        ParameterInfo[] paramInfos = method.GetParameters();

        for (int i = 0; i < paramInfos.Length; i++)
        {
            ParameterInfo paramInfo = paramInfos[i];
            Type paramType = paramInfo.ParameterType;

            // Seed each parameter slot so invocation always has a full argument list.
            providedValues[i] = CreateDefaultValue(paramType);

            int capturedIndex = i;

            // Build a typed editor and capture updates into providedValues.
            VisualControlInfo control = VisualControlTypes.CreateControlForType(paramType, null, new VisualControlContext(providedValues[i], v =>
            {
                providedValues[capturedIndex] = v;
            }));

            // Add a row only when a visual control exists for the parameter type.
            if (control.VisualControl != null)
            {
                // Each row combines a display label with the generated editor.
                HBoxContainer paramRow = new() { Alignment = BoxContainer.AlignmentMode.End };
                paramRow.AddThemeConstantOverride("separation", ParamRowSeparation);
                paramRow.AddChild(new Label { Text = VisualText.ToDisplayName(paramInfo.Name!) });
                paramRow.AddChild(control.VisualControl.Control);
                paramsVBox.AddChild(paramRow);
            }
        }

        return paramsVBox;
    }

    /// <summary>
    /// Adds one invoke button per method to the provided container.
    /// </summary>
    /// <param name="vbox">Container that receives method buttons.</param>
    /// <param name="methods">Methods to expose as invokable actions.</param>
    /// <param name="target">Invocation target for instance methods.</param>
    public static void AddMethodInfoElements(Control vbox, IEnumerable<MethodInfo> methods, object target)
    {
        foreach (MethodInfo method in methods)
        {
            ParameterInfo[] paramInfos = method.GetParameters();
            object[] providedValues = new object[paramInfos.Length];
            Button button = CreateMethodButton(method, target, paramInfos, providedValues);
            vbox.AddChild(button);
        }
    }

    /// <summary>
    /// Creates a button that invokes a method directly or through a parameter popup.
    /// </summary>
    /// <param name="method">Method to invoke.</param>
    /// <param name="target">Invocation target for instance methods.</param>
    /// <param name="paramInfos">Method parameter metadata.</param>
    /// <param name="providedValues">Current parameter values to convert for invocation.</param>
    /// <param name="minButtonWidth">Optional minimum width for the created button.</param>
    /// <returns>Configured method invocation button.</returns>
    public static Button CreateMethodButton(MethodInfo method, object target, ParameterInfo[] paramInfos, object[] providedValues, float minButtonWidth = 0)
    {
        bool hasParams = paramInfos.Length > 0;

        Button button = new()
        {
            Text = method.Name,
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
            Alignment = HorizontalAlignment.Left,
            Flat = true
        };

        // Apply optional width constraint for aligned method-button layouts.
        if (minButtonWidth > 0)
        {
            button.CustomMinimumSize = new Vector2(minButtonWidth, 0);
        }

        StyleBoxFlat methodButtonStyle = new()
        {
            BgColor = _methodButtonBackgroundColor,
            BorderColor = _methodButtonBorderColor,
            BorderWidthLeft = BorderWidth,
            BorderWidthRight = BorderWidth,
            BorderWidthTop = BorderWidth,
            BorderWidthBottom = BorderWidth,
            CornerRadiusTopLeft = CornerRadius,
            CornerRadiusTopRight = CornerRadius,
            CornerRadiusBottomLeft = CornerRadius,
            CornerRadiusBottomRight = CornerRadius,
            ContentMarginLeft = ButtonContentMarginLeft,
            ContentMarginRight = ButtonContentMarginRight,
            ContentMarginTop = ButtonContentMarginTop,
            ContentMarginBottom = ButtonContentMarginBottom,
        };

        button.AddThemeStyleboxOverride("normal", methodButtonStyle);
        button.AddThemeStyleboxOverride("hover", methodButtonStyle);
        button.AddThemeStyleboxOverride("pressed", methodButtonStyle);
        button.AddThemeStyleboxOverride("focus", methodButtonStyle);
        button.AddThemeStyleboxOverride("disabled", methodButtonStyle);

        PopupPanel? popup = null;
        Button? runButton = null;

        // Methods with parameters are executed from a popup after editing values.
        if (hasParams)
        {
            VBoxContainer paramsControls = CreateMethodParameterControls(method, providedValues);
            runButton = new Button
            {
                Text = $"Run {VisualText.ToDisplayName(method.Name)}",
                SizeFlagsHorizontal = SizeFlags.ShrinkCenter
            };

            VBoxContainer popupContent = new();
            popupContent.AddThemeConstantOverride("separation", PopupContentSeparation);
            popupContent.AddChild(paramsControls);
            popupContent.AddChild(runButton);

            MarginContainer popupMargin = new();
            popupMargin.AddThemeConstantOverride("margin_left", PopupMargin);
            popupMargin.AddThemeConstantOverride("margin_top", PopupMargin);
            popupMargin.AddThemeConstantOverride("margin_right", PopupMargin);
            popupMargin.AddThemeConstantOverride("margin_bottom", PopupMargin);
            popupMargin.AddChild(popupContent);

            popup = new PopupPanel();
            popup.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = Colors.Black,
                BorderColor = Colors.Black,
                BorderWidthLeft = BorderWidth,
                BorderWidthRight = BorderWidth,
                BorderWidthTop = BorderWidth,
                BorderWidthBottom = BorderWidth,
                CornerRadiusTopLeft = CornerRadius,
                CornerRadiusTopRight = CornerRadius,
                CornerRadiusBottomLeft = CornerRadius,
                CornerRadiusBottomRight = CornerRadius,
            });
            popup.AddChild(popupMargin);
            button.AddChild(popup);
        }

        void OnPopupAboutToPopup()
        {
            button.Disabled = true;
        }

        void OnPopupHide()
        {
            button.Disabled = false;
        }

        void OnPressed()
        {
            // Parameterless methods execute immediately.
            if (!hasParams)
            {
                object[] parameters = ParameterConverter.ConvertParameterInfoToObjectArray(paramInfos, providedValues);
                method.Invoke(target, parameters);
                Visualize.Update();
                return;
            }

            // Guard against popup creation failures in parameterized mode.
            if (popup == null)
            {
                return;
            }

            Vector2 popupSize = popup.GetContentsMinimumSize();
            Vector2 buttonScreenPosition = button.GetScreenPosition();
            Vector2 popupPosition = new(buttonScreenPosition.X, buttonScreenPosition.Y + button.Size.Y + PopupOffsetY);

            Vector2 screenSize = DisplayServer.ScreenGetSize();
            popupPosition.X = Mathf.Clamp(popupPosition.X, 0, Mathf.Max(0, screenSize.X - popupSize.X));
            popupPosition.Y = Mathf.Clamp(popupPosition.Y, 0, Mathf.Max(0, screenSize.Y - popupSize.Y));

            popup.Popup(new Rect2I((Vector2I)popupPosition, (Vector2I)popupSize));
        }

        void OnRunPressed()
        {
            // Run button exists only for parameterized popup flows.
            if (runButton == null)
            {
                return;
            }

            object[] parameters = ParameterConverter.ConvertParameterInfoToObjectArray(paramInfos, providedValues);
            method.Invoke(target, parameters);
            Visualize.Update();
        }

        void OnExitedTree()
        {
            button.Pressed -= OnPressed;
            runButton?.Pressed -= OnRunPressed;

            // Unsubscribe popup lifecycle handlers when popup exists.
            if (popup != null)
            {
                popup.AboutToPopup -= OnPopupAboutToPopup;
                popup.PopupHide -= OnPopupHide;
            }
            
            button.TreeExited -= OnExitedTree;
        }

        button.Pressed += OnPressed;
        runButton?.Pressed += OnRunPressed;

        // Subscribe popup lifecycle handlers only when popup was created.
        if (popup != null)
        {
            popup.AboutToPopup += OnPopupAboutToPopup;
            popup.PopupHide += OnPopupHide;
        }
        
        button.TreeExited += OnExitedTree;

        return button;
    }

    /// <summary>
    /// Creates a default parameter value for UI initialization based on a parameter type.
    /// </summary>
    /// <param name="type">Parameter type.</param>
    /// <returns>Default value instance suitable for editing in parameter controls.</returns>
    public static object CreateDefaultValue(Type type)
    {
        // Value types use their zero-initialized default instance.
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type)!;
        }
        // Strings use empty text rather than null for a friendlier UI default.
        else if (type == typeof(string))
        {
            return string.Empty;
        }

        // Concrete reference types with default constructors can be instantiated.
        if (!type.IsAbstract && type.GetConstructor(Type.EmptyTypes) != null)
        {
            return Activator.CreateInstance(type)!;
        }

        return null!;
    }
}
#endif
