using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace SpacePew
{
	[Flags]
	public enum KbModifiers
	{
		None = 0,
		Ctrl = 1,
		Shift = 2,
		Alt = 4,
	}

	public static class KeyboardHelper
	{
		public static bool TreadNumpadAsNumeric = true;
		private static readonly string[] _unShiftedKeysString = new string[256]
    {
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      " ",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "0",
      "1",
      "2",
      "3",
      "4",
      "5",
      "6",
      "7",
      "8",
      "9",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "a",
      "b",
      "c",
      "d",
      "e",
      "f",
      "g",
      "h",
      "i",
      "j",
      "k",
      "l",
      "m",
      "n",
      "o",
      "p",
      "q",
      "r",
      "s",
      "t",
      "u",
      "v",
      "w",
      "x",
      "y",
      "z",
      "",
      "",
      "",
      "",
      "",
      "0",
      "1",
      "2",
      "3",
      "4",
      "5",
      "6",
      "7",
      "8",
      "9",
      "*",
      "+",
      "",
      "-",
      ".",
      "/",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      ";",
      "=",
      ",",
      "-",
      ".",
      "/",
      "`",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "[",
      "\\",
      "]",
      "'",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      ""
    };
		private static string[] shiftedkeysstring = new string[256]
    {
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      " ",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      ")",
      "!",
      "@",
      "#",
      "$",
      "%",
      "^",
      "&",
      "*",
      "(",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "A",
      "B",
      "C",
      "D",
      "E",
      "F",
      "G",
      "H",
      "I",
      "J",
      "K",
      "L",
      "M",
      "N",
      "O",
      "P",
      "Q",
      "R",
      "S",
      "T",
      "U",
      "V",
      "W",
      "X",
      "Y",
      "Z",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "*",
      "+",
      "",
      "-",
      ".",
      "/",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      ":",
      "+",
      "<",
      "_",
      ">",
      "?",
      "~",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "{",
      "|",
      "}",
      "\"",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      ""
    };

		static KeyboardHelper()
		{
		}

		public static bool IsKeyAlpha(Keys k)
		{
			return k >= Keys.A && k <= Keys.Z;
		}

		public static bool IsKeyNumber(Keys k)
		{
			return k >= Keys.D0 && k <= Keys.D9;
		}

		public static bool IsKeyNumberpad(Keys k)
		{
			return k >= Keys.NumPad0 && k <= Keys.NumPad9;
		}

		public static bool IsKeyNumeric(Keys k)
		{
			return KeyboardHelper.IsKeyNumber(k) || KeyboardHelper.TreadNumpadAsNumeric && KeyboardHelper.IsKeyNumberpad(k);
		}

		public static bool IsKeyAlphanumeric(Keys k)
		{
			return KeyboardHelper.IsKeyAlpha(k) || KeyboardHelper.IsKeyNumeric(k);
		}

		public static bool IsFkey(Keys k)
		{
			return k >= Keys.F1 && k <= Keys.F12;
		}

		public static bool IsKeySpace(Keys k)
		{
			return k == Keys.Space;
		}

		public static bool IsShift(Keys k)
		{
			return k == Keys.LeftShift || k == Keys.RightShift;
		}

		public static bool IsCtrl(Keys k)
		{
			return k == Keys.LeftControl || k == Keys.RightControl;
		}

		public static bool IsAlt(Keys k)
		{
			return k == Keys.LeftAlt || k == Keys.RightAlt;
		}

		public static bool IsShiftDown(KbModifiers m)
		{
			return (KbModifiers.Shift & m) == KbModifiers.Shift;
		}

		public static bool IsCtrlDown(KbModifiers m)
		{
			return (KbModifiers.Ctrl & m) == KbModifiers.Ctrl;
		}

		public static bool IsAltDown(KbModifiers m)
		{
			return (KbModifiers.Alt & m) == KbModifiers.Alt;
		}

		public static bool IsMod(Keys k)
		{
			return KeyboardHelper.IsShift(k) || KeyboardHelper.IsAlt(k) || KeyboardHelper.IsCtrl(k);
		}

		public static KbModifiers IsShiftM(Keys k)
		{
			return k == Keys.LeftShift || k == Keys.RightShift ? KbModifiers.Shift : KbModifiers.None;
		}

		public static KbModifiers IsCtrlM(Keys k)
		{
			return k == Keys.LeftControl || k == Keys.RightControl ? KbModifiers.Ctrl : KbModifiers.None;
		}

		public static KbModifiers IsAltM(Keys k)
		{
			return k == Keys.LeftAlt || k == Keys.RightAlt ? KbModifiers.Alt : KbModifiers.None;
		}

		public static string ToPrintableString(Keys k, KbModifiers m)
		{
			return KeyboardHelper.ToPrintableString(k, m, true, true, true, false);
		}

		public static string ToPrintableString(Keys k, KbModifiers m, bool selectspecials)
		{
			return KeyboardHelper.ToPrintableString(k, m, selectspecials, true, true, false);
		}

		public static string ToPrintableString(Keys k, KbModifiers m, bool selectspecials, bool selectalphas, bool selectnumerics)
		{
			return KeyboardHelper.ToPrintableString(k, m, selectspecials, selectalphas, selectnumerics, false);
		}

		public static string ToPrintableString(Keys k, KbModifiers m, bool selectspecials, bool selectalphas, bool selectnumerics, bool suppressspace)
		{
			if (KeyboardHelper.IsKeySpace(k) && !suppressspace)
				return " ";
			if (KeyboardHelper.IsKeyAlpha(k) && selectalphas || KeyboardHelper.IsKeyNumber(k) && selectnumerics || KeyboardHelper.TreadNumpadAsNumeric && KeyboardHelper.IsKeyNumberpad(k) && selectnumerics || selectspecials && (!KeyboardHelper.IsKeyAlpha(k) && !KeyboardHelper.IsKeyNumeric(k) || KeyboardHelper.IsKeyNumber(k) && KeyboardHelper.IsShiftDown(m)))
			{
				if (!KeyboardHelper.IsShiftDown(m))
					return KeyboardHelper._unShiftedKeysString[k.GetHashCode()];
				if (selectspecials || !KeyboardHelper.IsKeyNumber(k))
					return KeyboardHelper.shiftedkeysstring[k.GetHashCode()];
			}
			return "";
		}

		public static KbModifiers GetModifiers(KeyboardState ks)
		{
			return ks.GetPressedKeys().Aggregate(KbModifiers.None, (current, k) => current | KeyboardHelper.IsShiftM(k) | KeyboardHelper.IsAltM(k) | KeyboardHelper.IsCtrlM(k));
		}
	}
}
