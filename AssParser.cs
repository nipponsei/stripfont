using System.Text.RegularExpressions;

/// <summary>Utility class to manipulate the content of an ASS script file.</summary>
public class AssParser {

  private readonly FileInfo _assFile;
  private readonly string _fontName;
  private List<string> _allLines = new();

  /// <summary>Creates a new instance of the ASS Parser.</summary>
  /// <param name="assFile">FileInfo object to an ASS script file</param>
  /// <param name="fontName">Friendly name of the font we need to use</param>
  public AssParser(FileInfo assFile, string fontName) {
    _assFile = assFile;
    _fontName = fontName;
    _allLines.AddRange(File.ReadAllLines(_assFile.FullName));
  }

  /// <summary>Header that identifies an ASS script file</summary>
  public static string ScriptInfoSection = "[Script Info]";

  /// <summary>Regular expression that matches braces, i.e.: {} containing override codes.</summary>
  public static Regex TagRegex = new(@"({.*?})");

  /// <summary>Regular expression that matches a font name tag: i.e.: \fn.</summary>
  public static Regex FnTagGroupRegex = new(@"(\\fn.*?)(\\|})");

  /// <summary>Returns all unique Unicode characters contained in the dialogue section of the ASS script file.</summary>
  /// <returns>A string containing all unique characters of the dialogues.</returns>
  public string GetUniqueCharacters() {
    // Get all dialogue lines with a corresponding style (i.e.: a style that references the font).
    // Assuming a styled line won't override the \fn tag
    List<string> lines = new(GetStyledDialogueLines());
    // Get all non styled line where the font is used by name (i.e.: using the \fn tag)
    lines.AddRange(GetMatchingFnTagDialogueLines(lines));

    // Strip all tags from those lines so we only keep the displayed text
    var dialogues = DeleteDialogueTags(lines);

    // Finally, we can trim the fat out of these lines to keep a single instance of each character
    var uniqueChars = GetUniqueCharacters(dialogues);
    if (uniqueChars != null && uniqueChars.Count > 0) {
      return new string(uniqueChars.ToArray());
    } else {
      return string.Empty;
    }
  }

  private IEnumerable<string> GetStyledDialogueLines() {
    // Get all lines that start with 'Style' and contains the exact name of the font.
    // Then we get the name of each corresonding style
    var styles = _allLines.Where(l => l.StartsWith("Style"))
      .Where(l => l.Contains($",{_fontName},"))
      .Select(l => l[7..l.NthIndexOf(',', 1)]).ToList();

    // Now we filter the dialogue lines containing one the style found earlier
    var styledLines = _allLines.Where(l => l.StartsWith("Dialogue"))
      .Where(l => styles.Contains(l[(l.NthIndexOf(',', 3) + 1)..l.NthIndexOf(',', 4)])).ToList();
    return styledLines;
  }

  private IEnumerable<string> GetMatchingFnTagDialogueLines(IEnumerable<string> exceptLines) {
    string[] acceptedMatches = new string[] { $"\\fn{_fontName}\\", $"\\fn{_fontName}}}" };
    List<string> acceptedLines = new();

    // Get all unstyled dialogue lines
    var nonStyledLines = _allLines.Where(l => l.StartsWith("Dialogue")).Except(exceptLines);
    
    var matchingLines = new List<string>();
    foreach (var line in nonStyledLines) {
      // We're looking for the \fn tag up to the next tag: '\' or the end of the block: '}'
      var matches = FnTagGroupRegex.Matches(line);
      foreach (Match match in matches.Cast<Match>()) {
        // We found a \fn tag, now we check that the font is the one we're looking for
        if (match.Success && acceptedMatches.Contains(match.Value)) {
          acceptedLines.Add(line);
        }
      }
    }
    return acceptedLines;
  }

  private IEnumerable<string> DeleteDialogueTags(IEnumerable<string> inputLines) {
    List<string> dialogues = new();
    foreach (var inputLine in inputLines.Distinct()) {
      // Dialogue starts after the 9th comma in a ASS file
      string dialogueText = inputLine[(inputLine.NthIndexOf(',', 9) + 1)..];

      // Check the existence of tags, and store their position and length
      List<Tuple<int, int>> tagOccurences = new();
      var matches = TagRegex.Matches(dialogueText);
      foreach (Match match in matches.Cast<Match>()) {
        if (match.Success) {
          tagOccurences.Add(new Tuple<int, int>(match.Index, match.Length));
        }
      }

      // Strip the line from tags, going backwards from the end of the line
      foreach (var occurence in tagOccurences.ToArray().Reverse()) {
        dialogueText = dialogueText.Remove(occurence.Item1, occurence.Item2);
      }
      dialogues.Add(dialogueText);
    }
    return dialogues.Distinct();
  }

  private static List<char>? GetUniqueCharacters(IEnumerable<string> dialogues) {
    // Concatenate all characters of the dialogues
    var charList = new List<char>();
    foreach (var dialogue in dialogues) {
      var chars = dialogue.ToCharArray();
      charList.AddRange(chars);
    }
    // Then only return distinct elments
    return new List<char>(charList.Distinct());
  }
}