// ============================================================================
// AdventureGame.Engine.Tests/VfsTests.cs
// ============================================================================
namespace AdventureGame.Engine.Tests;

[TestClass]
public class VfsTests
{
    [TestMethod]
    public void Vfs_AddReplaceRemove_Works()
    {
        var vfs = new GamePackVfs();

        var img = new byte[] { 1, 2, 3, 4 };
        vfs.AddOrReplace("images/door.png", "image/png", img);

        Assert.IsTrue(vfs.TryGet("images/door.png", out var e));
        CollectionAssert.AreEqual(img, e.Data);
        Assert.AreEqual("image/png", e.ContentType);

        // Replace
        var img2 = new byte[] { 5, 6 };
        vfs.AddOrReplace("images/door.png", "image/png", img2);
        Assert.IsTrue(vfs.TryGet("images/door.png", out var e2));
        CollectionAssert.AreEqual(img2, e2.Data);

        // Remove
        Assert.IsTrue(vfs.Remove("images/door.png"));
        Assert.IsFalse(vfs.TryGet("images/door.png", out _));

        // Validate (empty VFS is valid)
        vfs.ValidateOrThrow();
    }
}