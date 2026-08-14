// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectExtensionsTests.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to test the <see cref="ObjectExtensions" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDavSynologySyncer.Tests;

/// <summary>
/// A class to test the <see cref="ObjectExtensions"/> class.
/// </summary>
[TestClass]
public class ObjectExtensionsTests
{
    /// <summary>
    /// Checks whether null counts as empty.
    /// </summary>
    [TestMethod]
    public void NullIsEmptyOrNull()
    {
        List<string>? enumerable = null;

        Assert.IsTrue(enumerable.IsEmptyOrNull());
    }

    /// <summary>
    /// Checks whether an empty enumerable counts as empty.
    /// </summary>
    [TestMethod]
    public void AnEmptyEnumerableIsEmptyOrNull()
    {
        Assert.IsTrue(new List<string>().IsEmptyOrNull());
        Assert.IsTrue(Array.Empty<int>().IsEmptyOrNull());
    }

    /// <summary>
    /// Checks whether an enumerable with one element does not count as empty.
    /// </summary>
    [TestMethod]
    public void AFilledEnumerableIsNotEmptyOrNull()
    {
        Assert.IsFalse(new List<string> { string.Empty }.IsEmptyOrNull());
        Assert.IsFalse(new[] { 1, 2, 3 }.IsEmptyOrNull());
    }

    /// <summary>
    /// Checks whether only the first element of a lazy enumerable is pulled. The service calls the extension on
    /// query results, a full enumeration would be paid twice.
    /// </summary>
    [TestMethod]
    public void OnlyTheFirstElementOfALazyEnumerableIsPulled()
    {
        var pulled = 0;

        IEnumerable<int> Counted()
        {
            foreach (var value in new[] { 1, 2, 3 })
            {
                pulled++;
                yield return value;
            }
        }

        Assert.IsFalse(Counted().IsEmptyOrNull());
        Assert.AreEqual(1, pulled);
    }
}
