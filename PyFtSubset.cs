/// <summary>Utility class used to interact with <i>pyftsubset</i>.</summary>
public class PyFtSubset {

  private readonly FileInfo _assFile;
  private readonly FileInfo _fontFile;
  private string _text = string.Empty;

  /// <summary>Creates a new instance of the Python library helper.</summary>
  /// <param name="assFile">FileInfo object to an ASS script file</param>
  /// <param name="fontFile">FileInfo object to a font file</param>
  public PyFtSubset(FileInfo assFile, FileInfo fontFile) {
    _assFile = assFile;
    _fontFile = fontFile;
    _text = new AssParser(_assFile, _fontFile.GetDisplayName()).GetUniqueCharacters();
  }

  /// <summary>Returns the list of Unicode Controls characters.</summary>
  /// <remarks>For a complete reference on Controls characters, see: <see href="https://en.wikipedia.org/wiki/List_of_Unicode_characters#Control_codes">List of Unicode characters Control codes</see>, <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.char.iscontrol?view=net-7.0">Char.IsControl Method</seealso>.</remarks>
  public static string ControlChars => "U+0000-001F,U+007F,U+0080-009F";

  /// <summary>Returns the list of Unicode Whitespace characters.</summary>
  /// <remarks>For a complete reference on Whitespace characters, see: <see href="https://en.wikipedia.org/wiki/Whitespace_character#Unicode">Whitespace character</see>, <see href="https://learn.microsoft.com/en-us/dotnet/api/system.char.iswhitespace?view=net-7.0">Char.IsWhiteSpace Method</see>.</remarks>
  public string WhiteSpacesChars => "U+0009-000D,U+0085,U+0020,U+00A0,U+1680,U+2000-200A,U+2028-2029,U+202F,U+205F,U+3000";

  /// <summary>Returns the full path of the subset font.</summary>
  public string OutputFileName => Path.Join(_fontFile.DirectoryName, string.Format("{0}-subset.{1}", _fontFile.Name, _fontFile.Extension));

  /// <summary>Indicates if the font currently referenced is being used by the ASS script file.</summary>
  public bool IsFontInUse => !string.IsNullOrWhiteSpace(_text);

  /// <summary>Creates and returns the list of argument to pass to <i>pyftsubset</i>.</summary>
  /// <returns>A array of string containing the arguments to pass to <i>pyftsubset</i>.</returns>
  public string[] BuildArguments() {
    // to remember what we're passing, and why!
    // fontFile: the source font file
    // --name-IDs: set as * to keep the original font informations (Family, Style, Type, etc.)
    // --layout-features: * because we want to keep all OpenType features in our font
    // --text: list of characters to keep from the original font.
    // --unicode: list of additional unicode characters. We mainly add 'non visible chars' here : whitespaces & control chars
    return new string[] {
      $"{_fontFile}",
      "--name-IDs=*",
      "--layout-features=*",
      $"--output-file={OutputFileName}",
      $"--text={_text}",
      $"--unicodes={string.Join(',', ControlChars, WhiteSpacesChars)}"
    };
  }
}