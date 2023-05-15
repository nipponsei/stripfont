using SixLabors.Fonts;

public class PyFtSubset {
  
  private readonly FileInfo _assFile;
  private readonly FileInfo _fontFile;
  private string _randomFileName = string.Empty;
  private string? _unicodes;

  public PyFtSubset(FileInfo assFile, FileInfo fontFile) {
    _assFile = assFile;
    _fontFile = fontFile;
    var randomFileName = GetRandomFileName();
    _randomFileName = $"{randomFileName}";
    _unicodes = new AssParser(_assFile, GetFontName(_fontFile)).GetUniqueCharactersSequence();
  }

  private static string GetRandomFileName() {
    // return a randomely generated file name on 8 characters, minus the extension
    var rnd = Path.GetRandomFileName().ToUpper();
    return rnd[..rnd.LastIndexOf('.')];
  }

  public string RandomFileName {
    get { return _randomFileName; }
  }

  public string[] BuildArguments() {
    // --flavor: format of the output file. Here we use woff2
    // --layout-features: * because we want to keep all OpenType features in our font
    // --unicodes: comma-separated list of the characters we want in our font.
    // each character is formatted like this: U+xxxx. ex: A -> 65(dec) -> 41(hex) -> U+0041
    return new string[] {
      $"{_fontFile}",
      "--name-IDs=*",
      "--flavor=woff2",
      "--layout-features=*",
      $"--output-file={_randomFileName}.woff2",
      $"--unicodes={_unicodes}"
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