namespace ESILV_A2_S1_ConceptionBD.App;

public static class ConsoleHelpers
{
    public static string Prompt(string label)
    {
        Console.Write(label);
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    public static int PromptInt(string label)
    {
        while (true)
        {
            string raw = Prompt(label);
            if (int.TryParse(raw, out int value))
            {
                return value;
            }
            Console.WriteLine("Valeur invalide.");
        }
    }

    public static DateTime PromptDateTime(string label)
    {
        while (true)
        {
            string raw = Prompt(label);
            if (DateTime.TryParse(raw, out DateTime value))
            {
                return value;
            }
            Console.WriteLine("Format invalide. Exemple: 2025-12-31 18:30");
        }
    }
}
