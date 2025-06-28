using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace MoreMountains.InventoryEngine
{
    internal static class TextToTMPExtensions
    {
        public static void SetTMPAlignment(this TMP_Text tmp, TextAlignmentOptions alignment)
        {
            tmp.alignment = alignment;
        }

        public static void SetTMPAlignment(this TMP_Text tmp, TextAnchor alignment)
        {
            switch (alignment)
            {
                case TextAnchor.LowerLeft: tmp.alignment = TextAlignmentOptions.BottomLeft; break;
                case TextAnchor.LowerCenter: tmp.alignment = TextAlignmentOptions.Bottom; break;
                case TextAnchor.LowerRight: tmp.alignment = TextAlignmentOptions.BottomRight; break;
                case TextAnchor.MiddleLeft: tmp.alignment = TextAlignmentOptions.Left; break;
                case TextAnchor.MiddleCenter: tmp.alignment = TextAlignmentOptions.Center; break;
                case TextAnchor.MiddleRight: tmp.alignment = TextAlignmentOptions.Right; break;
                case TextAnchor.UpperLeft: tmp.alignment = TextAlignmentOptions.TopLeft; break;
                case TextAnchor.UpperCenter: tmp.alignment = TextAlignmentOptions.Top; break;
                case TextAnchor.UpperRight: tmp.alignment = TextAlignmentOptions.TopRight; break;
                default: tmp.alignment = TextAlignmentOptions.Center; break;
            }
        }

        public static void SetTMPFontStyle(this TMP_Text tmp, FontStyles fontStyle)
        {
            tmp.fontStyle = fontStyle;
        }

        public static void SetTMPFontStyle(this TMP_Text tmp, FontStyle fontStyle)
        {
            FontStyles fontStyles;
            switch (fontStyle)
            {
                case FontStyle.Bold: fontStyles = FontStyles.Bold; break;
                case FontStyle.Italic: fontStyles = FontStyles.Italic; break;
                case FontStyle.BoldAndItalic: fontStyles = FontStyles.Bold | FontStyles.Italic; break;
                default: fontStyles = FontStyles.Normal; break;
            }

            tmp.fontStyle = fontStyles;
        }

        public static void SetTMPHorizontalOverflow(this TMP_Text tmp, HorizontalWrapMode overflow)
        {
#if TMP_3_2_OR_NEWER
			tmp.textWrappingMode = ( overflow == HorizontalWrapMode.Wrap ) ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
#else
            tmp.enableWordWrapping = (overflow == HorizontalWrapMode.Wrap);
#endif
        }

        public static HorizontalWrapMode GetTMPHorizontalOverflow(this TMP_Text tmp)
        {
#if TMP_3_2_OR_NEWER
			return ( tmp.textWrappingMode == TextWrappingModes.Normal || tmp.textWrappingMode == TextWrappingModes.PreserveWhitespace ) ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
#else
            return tmp.enableWordWrapping ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
#endif
        }

        public static void SetTMPVerticalOverflow(this TMP_Text tmp, TextOverflowModes overflow)
        {
            tmp.overflowMode = overflow;
        }

        public static void SetTMPVerticalOverflow(this TMP_Text tmp, VerticalWrapMode overflow)
        {
            tmp.overflowMode = (overflow == VerticalWrapMode.Overflow) ? TextOverflowModes.Overflow : TextOverflowModes.Truncate;
        }

        public static void SetTMPLineSpacing(this TMP_Text tmp, float lineSpacing)
        {
            tmp.lineSpacing = (lineSpacing - 1) * 100f;
        }

        public static void SetTMPCaretWidth(this TMP_InputField tmp, int caretWidth)
        {
            tmp.caretWidth = caretWidth;
        }

        public static void SetTMPCaretWidth(this TMP_InputField tmp, float caretWidth)
        {
            tmp.caretWidth = (int)caretWidth;
        }
    }
}
