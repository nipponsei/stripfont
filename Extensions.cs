using SixLabors.Fonts;

/// <summary>Utility Class that serves a as host for extensions methods.</summary>
public static class Extensions {

  /// <summary>Finds the Nth index of a character in a line of text.</summary>
  /// <param name="s">The current instance of the string</param>
  /// <param name="c">The character we are looking for</param>
  /// <param name="n">The index of the instance we are looking for</param>
  /// <returns></returns>
  public static int NthIndexOf(this string s, char c, int n) {
    var takeCount = s.TakeWhile(x => (n -= (x == c ? 1 : 0)) > 0).Count();
    return takeCount == s.Length ? -1 : takeCount;
  }

  /// <summary>Returns the friendly or family name of a font file.</summary>
  /// <param name="fileInfo">FileInfo object representing the font file</param>
  /// <returns>The font's family name if the file is a valid font file, otherwise its file name.</returns>
  public static string GetDisplayName(this FileInfo fileInfo) {
    try {
      // return the user friendly name of the font
      _ = new FontCollection().Add(fileInfo.FullName, out FontDescription description);
      return description.FontFamilyInvariantCulture;
    } catch (Exception) {      
      // fallback to the file name in case this is not a font file
      return fileInfo.Name;
    }
  }

  /// <summary>Verifies if a file is a valid font file.</summary>
  /// <param name="fileInfo">FileInfo object to a file</param>
  /// <returns><b>True</b> if the file is a valid font file, otherwise <b>False</b>.</returns>
  public static bool IsValidFontFile(this FileInfo fileInfo) {
    try {
      FontCollection collection = new();
      collection.Add(fileInfo.FullName);
      return collection.Families.Count() > 0;
    } catch {
      return false;
    }
  }

  /// <summary>Verifies if a file is a valid ASS script file.</summary>
  /// <param name="fileInfo">FileInfo object to an ASS script file</param>
  /// <returns><b>True</b> if the file is a valid ASS script file, otherwise <b>False</b>.</returns>
  public static bool IsValidAssFile(this FileInfo fileInfo) {
    string line = string.Empty;
    using (StreamReader reader = new(fileInfo.FullName)) {
      line = reader.ReadLine() ?? "";
    }
    // We just check the Script Info section. I'm not gonna write a full parser! (not yet...)
    return (line == AssParser.ScriptInfoSection);
  }
}