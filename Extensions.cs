public static class Extensions {

  // find the Nth index of a character in a line
  public static int NthIndexOf(this string s, char c, int n) {
    var takeCount = s.TakeWhile(x => (n -= (x == c ? 1 : 0)) > 0).Count();
    return takeCount == s.Length ? -1 : takeCount;
  }

  // return a pyftsubset-compliant unicode character
  public static string GetUnicodeEscapeChar(this char c) {
    // example: A -> 65(dec) -> 41(hex) -> 0041
    return $"U+{(int)c:x4}";
  }
}