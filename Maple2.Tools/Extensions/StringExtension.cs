using Pastel;

namespace Maple2.Tools.Extensions;

public static class StringExtension {
    public static string ColorBlue(this string input) {
        return input.Pastel("#00d7ff");
    }

    public static string ColorGreen(this string input) {
        return input.Pastel("#aced66");
    }
    
    public static string ColorPurple(this string input) {
        return input.Pastel("#ff00d7");
    }

    public static string ColorRed(this string input) {
        return input.Pastel("#E05561");
    }

    public static string ColorYellow(this string input) {
        return input.Pastel("#FFE212");
    }
}
