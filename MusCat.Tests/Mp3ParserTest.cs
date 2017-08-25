using System;
using System.Collections.ObjectModel;
using System.Linq;
using MusCat.Entities;
using MusCat.Services;
using NUnit.Framework;

namespace MusCat.Tests
{
    [TestFixture]
    public class Mp3ParserTest
    {
        private readonly Mp3Parser _parser = new Mp3Parser();

        [Test]
        public void TestFixOrdinaryDurations()
        {
            // ARRANGE
            var songs = new ObservableCollection<Song>
            {
                new Song {TimeLength = "1:23"},
                new Song {TimeLength = "0:02"},
                new Song {TimeLength = "12:59"},
                new Song {TimeLength = "8:00"}
            };
            // ACT
            var time = _parser.FixTimes(songs);
            // ASSERT
            Assert.That(time, Is.EqualTo("22:24"));
            Assert.That(songs[0].TimeLength, Is.EqualTo("1:23"));
            Assert.That(songs[1].TimeLength, Is.EqualTo("0:02"));
            Assert.That(songs[2].TimeLength, Is.EqualTo("12:59"));
            Assert.That(songs[3].TimeLength, Is.EqualTo("8:00"));
        }

        [Test]
        public void TestFixTimeCollectionNull()
        {
            // ARRANGE, ACT, ASSERT
            Assert.Throws<NullReferenceException>(() => _parser.FixTimes(null));
        }

        [Test]
        public void TestFixTimeEmptyCollection()
        {
            // ARRANGE
            var songs = new ObservableCollection<Song>();
            // ACT
            var time = _parser.FixTimes(songs);
            // ASSERT
            Assert.That(time, Is.EqualTo("0:00"));
        }

        [Test]
        public void TestFixNonStandardDurations()
        {
            // ARRANGE
            var songs = new ObservableCollection<Song>
            {
                new Song { TimeLength = "1:2 " },
                new Song { TimeLength = " 3 " },
                new Song { TimeLength = "2: " },
                new Song { TimeLength = " :2" },
                new Song { TimeLength = "1a:5s" }
            };
            // ACT
            var time = _parser.FixTimes(songs);
            // ASSERT
            Assert.That(time, Is.EqualTo("7:09"));
            Assert.That(songs[0].TimeLength, Is.EqualTo("1:02"));
            Assert.That(songs[1].TimeLength, Is.EqualTo("3:00"));
            Assert.That(songs[2].TimeLength, Is.EqualTo("2:00"));
            Assert.That(songs[3].TimeLength, Is.EqualTo("0:02"));
            Assert.That(songs[4].TimeLength, Is.EqualTo("1:05"));
        }

        [Test]
        public void TestFixUnderscores()
        {
            // ARRANGE
            var songs = new ObservableCollection<Song>
            {
                new Song { Name="_well_well__well" }
            };
            // ACT
            _parser.FixNames(songs);
            // ASSERT
            Assert.That(songs[0].Name, Is.EqualTo("Well Well Well"));
        }

        [Test]
        public void TestFixWhitespaces()
        {
            // ARRANGE
            var songs = new ObservableCollection<Song>
            {
                new Song { Name="  Well   how     aRE  you?  " }
            };
            // ACT
            _parser.FixNames(songs);
            // ASSERT
            Assert.That(songs[0].Name, Is.EqualTo("Well How Are You?"));
        }

        [Test]
        public void FixTrackNumbers()
        {
            // ARRANGE
            var songs = new ObservableCollection<Song>
            {
                new Song { TrackNo = 1, Name = "1" },
                new Song { TrackNo = 2, Name = "2" },
                new Song { TrackNo = 5, Name = "3" },
                new Song { TrackNo = 4, Name = "4" }
            };
            // ACT
            _parser.FixNames(songs);
            // ASSERT
            CollectionAssert.AreEqual(Enumerable.Range(1, 4), songs.Select(s => s.TrackNo));
        }

        [Test]
        public void TestFixEmptySongNames()
        {
            // ARRANGE
            var songs = new ObservableCollection<Song>
            {
                new Song { Name = string.Empty },
                new Song { Name = "" }
            };
            // ACT
            _parser.FixNames(songs);
            // ASSERT
            Assert.That(songs[0].Name, Is.EqualTo(string.Empty));
            Assert.That(songs[1].Name, Is.EqualTo(""));
        }

        [Test]
        public void TestFixPunctuations()
        {
            // ARRANGE
            var songs = new ObservableCollection<Song>
            {
                new Song { Name="  Well ,  song(yeAh ?yeah!)  " },
                new Song { Name="wow...  it(is so cool...)  " },
                new Song { Name="   ((There's )   been)  some, right?))  " },
                new Song { Name="Hush! it's    ( \"an\" ( ! ) ) experiment.. ." },
                new Song { Name="Tell me \"yes\"" }
            };
            // ACT
            _parser.FixNames(songs);
            // ASSERT
            Assert.That(songs[0].Name, Is.EqualTo("Well, Song (Yeah? Yeah!)"));
            Assert.That(songs[1].Name, Is.EqualTo("Wow... It (Is So Cool...)"));
            Assert.That(songs[2].Name, Is.EqualTo("((There's) Been) Some, Right?))"));
            Assert.That(songs[3].Name, Is.EqualTo("Hush! It's (\"An\" (!)) Experiment..."));
            Assert.That(songs[4].Name, Is.EqualTo("Tell Me \"Yes\""));
        }
    }
}
