#if DEBUG
using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Godot.Control;

namespace GodotUtils.Debugging;

/// <summary>
/// Methods that can be executed manually by pressing a button in-game. Parameters are supported.
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
    /// Creates the UI needed for the method parameters
    /// </summary>
    public static VBoxContainer CreateMethodParameterControls(MethodInfo method, object[] providedValues)
    {
        VBoxContainer paramsVBox = new();
        paramsVBox.AddThemeConstantOverride("separation", ParamsVBoxSeparation);

        ParameterInfo[] paramInfos = method.GetParameters();

        for (int i = 0; i < paramInfos.Length; i++)
        {
            ParameterInfo paramInfo = paramInfos[i];
            Type paramType = paramInfo.ParameterType;

            providedValues[i] = CreateDefaultValue(paramType);

            int capturedIndex = i;

            VisualControlInfo control = VisualControlTypes.CreateControlForType(paramType, null, new VisualControlContext(providedValues[i], v =>
            {
                providedValues[capturedIndex] = v;
            }));

            if (control.VisualControl != null)
            {
                HBoxContainer paramRow = new() { Alignment = BoxContainer.AlignmentMode.End };
                paramRow.AddThemeConstantOverride("separation", ParamRowSeparation);
                paramRow.AddChild(new Label { Text = VisualText.ToDisplayName(paramInfo.Name!) });
                paramRow.AddChild(control.VisualControl.Control);
                paramsVBox.AddChild(paramRow);
            }
        }

        return paramsVBox;
    }

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
            if (!hasParams)
            {
                object[] parameters = ParameterConverter.ConvertParameterInfoToObjectArray(paramInfos, providedValues);
                method.Invoke(target, parameters);
                Visualize.Update();
                return;
            }

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
            if (runButton != null)
            {
                runButton.Pressed -= OnRunPressed;
            }
            if (popup != null)
            {
                popup.AboutToPopup -= OnPopupAboutToPopup;
                popup.PopupHide -= OnPopupHide;
            }
            button.TreeExited -= OnExitedTree;
        }

        button.Pressed += OnPressed;
        if (runButton != null)
        {
            runButton.Pressed += OnRunPressed;
        }
        if (popup != null)
        {
            popup.AboutToPopup += OnPopupAboutToPopup;
            popup.PopupHide += OnPopupHide;
        }
        button.TreeExited += OnExitedTree;

        return button;
    }

    public static object CreateDefaultValue(Type type)
    {
        // Examples of Value Types: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/value-types#kinds-of-value-types-and-type-constraints
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type)!;
        }
        // Examples of Reference Types: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/reference-types
        else if (type == typeof(string))
        {
            return string.Empty;
        }

        if (!type.IsAbstract && type.GetConstructor(Type.EmptyTypes) != null)
        {
            return Activator.CreateInstance(type)!;
        }

        return null!;
    }
}
#endif
