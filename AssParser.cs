using System.Text.RegularExpressions;

public class AssParser {

  private readonly FileInfo _assFile;
  private readonly string _fontName;
  private List<string> _allLines = new List<string>();

  public AssParser(FileInfo assFile, string fontName) {
    _assFile = assFile;
    _fontName = fontName;
    _allLines.AddRange(File.ReadAllLines(_assFile.FullName));
  }

  public static string ScriptInfoSection = "[Script Info]";
  public static Regex TagRegex = new Regex(@"({.*?})");
  public static Regex FnTagGroupRegex = new Regex(@"(\\fn.*?)(\\|})");

  public static bool IsValidAssFile(FileInfo? assFile) {
		try {
      if (assFile == null) throw new ArgumentNullException(nameof(assFile));
      string line = string.Empty;
      using (StreamReader reader = new(assFile.FullName)) {
        line = reader.ReadLine() ?? "";
      }
      // we just check the Script Info section. I'm not gonna write a full parser! (not yet...)
      return (line == ScriptInfoSection);
		} catch {
      return false;
		}
  }
  
  public string GetUniqueCharacters() {
    // get all dialogue lines with a corresponding style (i.e.: a style that references the font). Assuming a styled line won't override the \fn tag
    List<string> lines = new List<string>(GetStyledDialogueLines());
    // and non styled line where the font is used by name (i.e.: using the \fn tag)
    lines.AddRange(GetMatchingFnTagDialogueLines(lines));

    // strip all tags from those lines so we can focus on the text
    var dialogues = DeleteDialogueTags(lines);

    // finally, we can focus on characters occurences
    var uniqueChars = GetUniqueCharacters(dialogues);
    if (uniqueChars != null && uniqueChars.Count > 0) {
      return new string(uniqueChars.ToArray());
    } else {
      return string.Empty;
    }
  }

  private IEnumerable<string> GetStyledDialogueLines() {
    // get all lines that start with 'Style' and contains the exact name of the font.
    // Then we get the name of each corresonding style
    var styles = _allLines.Where(l => l.StartsWith("Style"))
      .Where(l => l.Contains($",{_fontName},"))
      .Select(l => l[7..l.NthIndexOf(',', 1)]).ToList();
    
    // now we filter the dialogue lines containing one the style found earlier
    var styledLines = _allLines.Where(l => l.StartsWith("Dialogue"))
      .Where(l => styles.Contains(l.Substring(l.NthIndexOf(',', 3) + 1, (l.NthIndexOf(',', 4) - (l.NthIndexOf(',', 3) + 1))))).ToList();
    return styledLines;
  }

  private IEnumerable<string> GetMatchingFnTagDialogueLines(IEnumerable<string> exceptLines) {
    string[] acceptedMatches = new string[] { $"\\fn{_fontName}\\", $"\\fn{_fontName}}}" };
    List<string> acceptedLines = new List<string>();

    // get all unstyled dialogue lines
    var nonStyledLines = _allLines.Where(l => l.StartsWith("Dialogue")).Except(exceptLines);
    var matchingLines = new List<string>();
    foreach (var line in nonStyledLines) {
      // ok, here we're lookin for the \fn tag up to the next tag: '\' or the end of the block: '}'
      var matches = FnTagGroupRegex.Matches(line);
      foreach (Match match in matches.Cast<Match>()) {
        // we found a \fn tag, now we check that the font is the one we're looking for
        if (match.Success && acceptedMatches.Contains(match.Value)) {
          acceptedLines.Add(line);
        }
      }
    }
    return acceptedLines;
  }

  private IEnumerable<string> DeleteDialogueTags(IEnumerable<string> inputLines) {
    List<string> dialogues = new List<string>();
    foreach (var inputLine in inputLines.Distinct()) {
      // dialogue starts after the 9th comma in a ASS file
      string dialogueText = inputLine.Substring(inputLine.NthIndexOf(',', 9) + 1);

      // check the existence of tags, and store their position and length
      List<Tuple<int, int>> tagOccurences = new List<Tuple<int, int>>();
      var matches = TagRegex.Matches(dialogueText);
      foreach (Match match in matches.Cast<Match>()) {
        if (match.Success) {
          tagOccurences.Add(new Tuple<int, int>(match.Index, match.Length));
        }
      }

      // strip the line from tags, starting from the end of the line
      foreach (var occurence in tagOccurences.ToArray().Reverse()) {
        dialogueText = dialogueText.Remove(occurence.Item1, occurence.Item2);
      }
      // add the sanitized dialog text
      dialogues.Add(dialogueText);
    }
    return dialogues.Distinct();
  }
    
  private List<char>? GetUniqueCharacters(IEnumerable<string> dialogues) {
    var charList = new List<char>();
    foreach (var dialogue in dialogues) {
      var chars = dialogue.ToCharArray();
      charList.AddRange(chars);
    }
    return new List<char>(charList.Distinct());
  }
}