namespace ConnectFour
{
    internal abstract class ConsoleUtil
    {
        public static void WriteLineWithColor(string text, ConsoleColor color)
        {
            WriteWithColor($"{text}\n", color);
        }

        public static void WriteWithColor(string text, ConsoleColor color)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = originalColor;
        }

        public static void WriteLineWithColors(params (string text, ConsoleColor color)[] coloredTexts)
        {
            WriteWithColors(coloredTexts);
            Console.WriteLine();
        }

        public static void WriteWithColors(params (string text, ConsoleColor color)[] coloredTexts)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            foreach ((string text, ConsoleColor color) in coloredTexts)
            {
                Console.ForegroundColor = color;
                Console.Write(text);
            }

            Console.ForegroundColor = originalColor;
        }

        public static string GetUserInput(int row, string prompt, ConsoleColor promptColor, ConsoleColor playerColor)
        {
            Console.SetCursorPosition(0, row);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, row);
            WriteWithColor(prompt, promptColor);
            Console.ForegroundColor = playerColor;
            string input = Console.ReadLine() ?? string.Empty;
            Console.SetCursorPosition(0, row);
            Console.Write(new string(' ', Console.WindowWidth));
            return input;
        }
    }
}