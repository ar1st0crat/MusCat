using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces;
using MusCat.Infrastructure.Services;
using NUnit.Framework;

namespace MusCat.Tests
{
    [TestFixture]
    public class SonglistTest
    {
        private readonly Mp3SonglistHelper _parser = new Mp3SonglistHelper();

        [Test]
        public void TestFixOrdinaryDurations()
        {
            // ARRANGE
            var songs = new []
            {
                new SongEntry { Duration = "1:23" },
                new SongEntry { Duration = "0:02" },
                new SongEntry { Duration = "12:59" },
                new SongEntry { Duration = "8:00" }
            };

            // ACT
            var time = _parser.FixDurations(songs);

            // ASSERT
            Assert.That(time, Is.EqualTo("22:24"));
            Assert.That(songs[0].Duration, Is.EqualTo("1:23"));
            Assert.That(songs[1].Duration, Is.EqualTo("0:02"));
            Assert.That(songs[2].Duration, Is.EqualTo("12:59"));
            Assert.That(songs[3].Duration, Is.EqualTo("8:00"));
        }

        [Test]
        public void TestFixTimeCollectionNull()
        {
            // ARRANGE, ACT, ASSERT
            Assert.Throws<NullReferenceException>(() => _parser.FixDurations(null));
        }

        [Test]
        public void TestFixTimeEmptyCollection()
        {
            // ARRANGE
            var songs = new List<SongEntry>();
            // ACT
            var time = _parser.FixDurations(songs);
            // ASSERT
            Assert.That(time, Is.EqualTo("0:00"));
        }

        [Test]
        public void TestFixNonStandardDurations()
        {
            // ARRANGE
            var songs = new []
            {
                new SongEntry { Duration = "1:2 " },
                new SongEntry { Duration = " 3 " },
                new SongEntry { Duration = "2: " },
                new SongEntry { Duration = " :2" },
                new SongEntry { Duration = "1a:5s" }
            };

            // ACT
            var time = _parser.FixDurations(songs);

            // ASSERT
            Assert.That(time, Is.EqualTo("7:09"));
            Assert.That(songs[0].Duration, Is.EqualTo("1:02"));
            Assert.That(songs[1].Duration, Is.EqualTo("3:00"));
            Assert.That(songs[2].Duration, Is.EqualTo("2:00"));
            Assert.That(songs[3].Duration, Is.EqualTo("0:02"));
            Assert.That(songs[4].Duration, Is.EqualTo("1:05"));
        }

        [Test]
        public void TestFixUnderscores()
        {
            // ARRANGE
            var songs = new []
            {
                new SongEntry { Title="_well_well__well" }
            };

            // ACT
            _parser.FixTitles(songs);

            // ASSERT
            Assert.That(songs[0].Title, Is.EqualTo("Well Well Well"));
        }

        [Test]
        public void TestFixWhitespaces()
        {
            // ARRANGE
            var songs = new []
            {
                new SongEntry { Title="  Well   how     aRE  you?  " }
            };

            // ACT
            _parser.FixTitles(songs);

            // ASSERT
            Assert.That(songs[0].Title, Is.EqualTo("Well How Are You?"));
        }

        [Test]
        public void FixTrackNumbers()
        {
            // ARRANGE
            var songs = new []
            {
                new SongEntry { No = 1, Title = "1" },
                new SongEntry { No = 2, Title = "2" },
                new SongEntry { No = 5, Title = "3" },
                new SongEntry { No = 4, Title = "4" }
            };

            // ACT
            _parser.FixTitles(songs);

            // ASSERT
            CollectionAssert.AreEqual(Enumerable.Range(1, 4), songs.Select(s => s.No));
        }

        [Test]
        public void TestFixEmptySongNames()
        {
            // ARRANGE
            var songs = new []
            {
                new SongEntry { Title = string.Empty },
                new SongEntry { Title = "" }
            };

            // ACT
            _parser.FixTitles(songs);

            // ASSERT
            Assert.That(songs[0].Title, Is.EqualTo(string.Empty));
            Assert.That(songs[1].Title, Is.EqualTo(""));
        }

        [Test]
        public void TestFixPunctuations()
        {
            // ARRANGE
            var songs = new []
            {
                new SongEntry { Title="  Well ,  song(yeAh ?yeah!)  " },
                new SongEntry { Title="wow...  it(is so cool...)  " },
                new SongEntry { Title="   ((There's )   been)  some, right?))  " },
                new SongEntry { Title="Hush! it's    ( \"an\" ( ! ) ) experiment.. ." },
                new SongEntry { Title="Tell me \"yes\"" }
            };

            // ACT
            _parser.FixTitles(songs);

            // ASSERT
            Assert.That(songs[0].Title, Is.EqualTo("Well, Song (Yeah? Yeah!)"));
            Assert.That(songs[1].Title, Is.EqualTo("Wow... It (Is So Cool...)"));
            Assert.That(songs[2].Title, Is.EqualTo("((There's) Been) Some, Right?))"));
            Assert.That(songs[3].Title, Is.EqualTo("Hush! It's (\"An\" (!)) Experiment..."));
            Assert.That(songs[4].Title, Is.EqualTo("Tell Me \"Yes\""));
        }
    }
}
