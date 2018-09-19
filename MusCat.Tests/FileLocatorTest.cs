using System.Linq;
using MusCat.Core.Entities;
using MusCat.Infrastructure.Services;
using NUnit.Framework;

namespace MusCat.Tests
{
    [TestFixture]
    public class FileLocatorTest
    {
        [Test]
        public void TestNormalizePathString()
        {
            // 1
            Assert.That("  In & Out Of... [1971]".NormalizePath(), Is.EqualTo("  in  out of 1971"));
            // 2
            Assert.That("1976 - 33 1/3 ok ^ :".NormalizePath(), Is.EqualTo("1976  33 13 ok ^ "));
        }

        [Test]
        public void TestMakeCorrectAlbumPaths()
        {
            // ARRANGE
            var album = new Album
            {
                Id = 73,
                Performer = new Performer
                {
                    Name = "Foo"
                }
            };
            // ACT
            var pathlist = FileLocator.MakeAlbumImagePathlist(album);
            // ASSERT
            Assert.That(pathlist.All(p => p.EndsWith(@"Foo\Picture\73.jpg")));
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
            Assert.That(pathlist.All(p => p.EndsWith(@"Foo\Picture\photo.bmp")));
        }
    }
}
