using System.Text.RegularExpressions;

public class PromptRules
{
    public int MinLength { get; set; } = 10;
    public int MaxLength { get; set; } = 500;

    public List<string> RestrictedWords { get; set; } = new List<string>
        {
            "ilegal", "violencia", "contenido sensible"
        };

    public bool AllowSpecialCharacters { get; set; } = true;
    public string AllowedSpecialCharacters { get; set; } = "@#$%^&*()_+-=[]{}|;':,.<>?";

    public bool AllowNumbers { get; set; } = true;
    public bool AllowLetters { get; set; } = true;
    public string ExpectedFormat { get; set; } = "texto";
    public string ResponseTone { get; set; } = "educativo";
    public string ResponseComplexity { get; set; } = "intermedio";

    public string FullContext { get; set; }
    public string Language { get; set; } = "es";

    public List<string> ExamplePrompts { get; set; } = new List<string>(); // En lugar de un booleano

    public bool AllowOpinion { get; set; } = false;
    public int MaxResponseLength { get; set; } = 1000;


    public string SanitizePrompt(string prompt)
    {
        if (!AllowSpecialCharacters)
        {
            prompt = Regex.Replace(prompt, $"[^{AllowedSpecialCharacters}\\w\\s]", "");
        }
        if (!AllowNumbers)
        {
            prompt = Regex.Replace(prompt, @"\d", "");
        }
        if (!AllowLetters)
        {
            prompt = Regex.Replace(prompt, @"[a-zA-Z]", "");
        }
        return prompt.Trim();
    }


    public void ValidatePrompt(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("El prompt no puede estar vacío.");

        prompt = SanitizePrompt(prompt);

        if (prompt.Length < MinLength || prompt.Length > MaxLength)
            throw new ArgumentException($"El prompt debe tener entre {MinLength} y {MaxLength} caracteres.");

        if (RestrictedWords.Any(word => prompt.Contains(word, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("El prompt contiene palabras restringidas.");

        if (!string.IsNullOrEmpty(FullContext) && !prompt.Contains(FullContext, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("El prompt no contiene el contexto requerido.");

        if (ExamplePrompts.Count > 0 && !ExamplePrompts.Any(e => prompt.Contains(e, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("El prompt no sigue los ejemplos requeridos.");
    }
}