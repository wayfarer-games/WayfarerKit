using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WayfarerKit.UI
{
    public static class TextFormatterExt
    {
        public static string FormatNumber(int number) =>
            number switch { < 1000 => number.ToString(), < 1000000 => (number / 1000f).ToString("0.#") + "K", _ => (number / 1000000f).ToString("0.#") + "M" };
    }
}