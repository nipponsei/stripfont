public static class Extensions {

  // find the Nth index of a character in a line
  public static int NthIndexOf(this string s, char c, int n) {
    var takeCount = s.TakeWhile(x => (n -= (x == c ? 1 : 0)) > 0).Count();
    return takeCount == s.Length ? -1 : takeCount;
  }
}