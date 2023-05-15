using SixLabors.Fonts;

public static class Extensions {

  // find the Nth index of a character in a line
  public static int NthIndexOf(this string s, char c, int n) {
    var takeCount = s.TakeWhile(x => (n -= (x == c ? 1 : 0)) > 0).Count();
    return takeCount == s.Length ? -1 : takeCount;
  }

  // FileInfo class is sealed: we can't inherit it, so let's just use what we can: extension method!
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
}