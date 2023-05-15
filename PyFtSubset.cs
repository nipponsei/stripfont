using SixLabors.Fonts;

public class PyFtSubset {

  private readonly FileInfo _assFile;
  private readonly FileInfo _fontFile;
  private string _text = string.Empty;

  public PyFtSubset(FileInfo assFile, FileInfo fontFile) {
    _assFile = assFile;
    _fontFile = fontFile;
    _text = new AssParser(_assFile, GetFontName(_fontFile)).GetUniqueCharacters();
  }

  // a bit more of work than control chars, but still useful
  // see: https://en.wikipedia.org/wiki/Whitespace_character#Unicode
  //      https://learn.microsoft.com/en-us/dotnet/api/system.char.iswhitespace?view=net-7.0
  private static string GetWhiteSpaceChars() {
    return "U+0009-000D,U+0085,U+0020,U+00A0,U+1680,U+2000-200A,U+2028-2029,U+202F,U+205F,U+3000";
  }

  // return all control characters (UnicodeCategory.Control)
  // see: https://en.wikipedia.org/wiki/List_of_Unicode_characters#Control_codes
  //      https://learn.microsoft.com/en-us/dotnet/api/system.char.iscontrol?view=net-7.0
  public string ControlChars => "U+0000-001F,U+007F,U+0080-9F";

  public string WhiteSpacesChars => GetWhiteSpaceChars();

  public string OutputFileName => Path.Join(_fontFile.DirectoryName, string.Format("{0}-subset.{1}", _fontFile.Name, _fontFile.Extension));

  public bool IsFontInUse => !string.IsNullOrWhiteSpace(_text);

  public string[] BuildArguments() {
    // --name-IDs: set as * to keep the original font informations 'Family, Style, Type, etc.)
    // --layout-features: * because we want to keep all OpenType features in our font
    // --text: list of characters to keep from the original text.
    // --unicode: list of additional unicode characters. We focus on 'non visible chars' here : whitespaces & control chars
    return new string[] {
      $"{_fontFile}",
      "--name-IDs=*",
      "--layout-features=*",
      $"--output-file={OutputFileName}",
      $"--text={_text}",
      $"--unicodes={string.Join(',', ControlChars, WhiteSpacesChars)}"
    }; 
  }

  public static bool IsValidFontFile(FileInfo? fontFile) {
    try {
      if (fontFile == null) throw new ArgumentNullException(nameof(fontFile));
      // just read the info from the font file. If the call doesn't fail then it's a valid font
      _ = new FontCollection().Add(fontFile.FullName);
      return true;
    } catch {
      return false;
    }
  }

  public static string GetFontName(FileInfo fontFile) {
    try {
      if (!IsValidFontFile(fontFile)) {
        throw new ArgumentException(nameof(fontFile));
      }
      FontCollection fonts = new FontCollection();
      var family = fonts.Add(fontFile.FullName);
      return family.Name;
    } catch (Exception ex) {
      throw new InvalidFontFileException(ex.Message);
    }
  }
}