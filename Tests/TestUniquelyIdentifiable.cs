using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReaderLib;

namespace Tests
{
  [TestClass]
  public class TestUniquelyIdentifiable
  {
    [TestMethod]
    public void TestUniqueness()
    {
      UniquelyIdentifiable one = new UniquelyIdentifiable();
      UniquelyIdentifiable two = new UniquelyIdentifiable();
      Assert.AreNotEqual(one.Identity, two.Identity);
    }
  }
}
