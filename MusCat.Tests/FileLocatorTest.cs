using System.Linq;
using NUnit.Framework;
using MusCat.Model;
using MusCat.Utils;

namespace MusCat.Tests
{
    [TestFixture]
    public class FileLocatorTest
    {
        [Test]
        public void TestNormalizePathString()
        {
            // 1
            Assert.That("  It's More... [2002]".NormalizePath(),Is.EqualTo("  it's more... [2002]"));
            // 2
            Assert.That("1976 - 33 1/3 ok ^ :".NormalizePath(), Is.EqualTo("1976 - 33 13 ok ^ "));
        }

        [Test]
        public void TestMakeCorrectAlbumPaths()
        {
            // ARRANGE
            var album = new Album()
            {
                ID = 73,
                Performer = new Performer
                {
                    Name = "Foo"
                }
            };
            // ACT
            var pathlist = FileLocator.MakeAlbumImagePathlist(album);
            // ASSERT
            Assert.That(pathlist.All(p => p.EndsWith(@"F\Foo\Picture\73.jpg")));
        }

        [Test]
        public void TestMakeCorrectPerformerPaths()
        {
            // ARRANGE
            var performer = new Performer
            {
                Name = "Foo"
            };
            // ACT
            var pathlist = FileLocator.MakePerformerImagePathlist(performer, "bmp");
            // ASSERT
            Assert.That(pathlist.All(p => p.EndsWith(@"F\Foo\Picture\photo.bmp")));
        }
    }
}
