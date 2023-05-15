using SixLabors.Fonts;

public class PyFtSubset {
  
  private readonly FileInfo _assFile;
  private readonly FileInfo _fontFile;
  private string _randomFileName = string.Empty;
  private string _text = string.Empty;

  public PyFtSubset(FileInfo assFile, FileInfo fontFile) {
    _assFile = assFile;
    _fontFile = fontFile;
    var randomFileName = GetRandomFileName();
    _randomFileName = $"{randomFileName}";
    _text = new AssParser(_assFile, GetFontName(_fontFile)).GetUniqueCharacters();
  }

  private static string GetRandomFileName() {
    // return a randomely generated file name on 8 characters, minus the extension
    var rnd = Path.GetRandomFileName().ToUpper();
    return rnd[..rnd.LastIndexOf('.')];
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

  public string RandomFileName {
    get { return _randomFileName; }
  }

  public string[] BuildArguments() {
    // --name-IDs: set as * to keep the original font informations 'Family, Style, Type, etc.)
    // --flavor: format of the output file. Here we use woff2
    // --layout-features: * because we want to keep all OpenType features in our font
    // --text: list of characters to keep from the original text.
    return new string[] {
      $"{_fontFile}",
      "--name-IDs=*",
      "--flavor=woff2",
      "--layout-features=*",
      $"--output-file={_randomFileName}.woff2",
      $"--text={_text}",
      $"--unicodes={string.Join(',', ControlChars, WhiteSpacesChars)}"
    }; 
  }

  public string[] BuildPythonArguments() {
    return new string[] {
      "-c",
      $"\"from fontTools.ttLib import woff2; import brotli; woff2.decompress('{_randomFileName}.woff2', '{_randomFileName}.ttf')\""
    };
  }

  public void DeleteTempFontFile() {
    var fontDirectory = new DirectoryInfo(_fontFile.Directory!.FullName);
    var woffFiles = fontDirectory.EnumerateFiles("*.woff2").ToList();
    woffFiles.ForEach(f => f.Delete());
  }

  public static bool IsValidFontFile(FileInfo? fontFile) {
    try {
      var fonts = new FontCollection();
      if (fontFile == null) throw new ArgumentNullException(nameof(fontFile));
      // just read the info from the font file. If it returns something then it's a valid font
      var fontFamily = fonts.Add(fontFile.FullName);
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