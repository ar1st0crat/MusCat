using System;
using System.Collections.Generic;
using MusCat.Core.Entities;
using MusCat.Core.Services;
using NUnit.Framework;

namespace MusCat.Tests
{
    [TestFixture]
    public class RateCalculatorTest
    {
        readonly RateCalculator _rateCalculator = new RateCalculator();

        [Test]
        public void TestCalculateRateForNoAlbums()
        {
            // ARRANGE
            var albums = new List<Album>();
            // ACT
            var rate = _rateCalculator.Calculate(albums);
            // ASSERT
            Assert.That(rate, Is.Null);
        }

        [Test]
        public void TestCalculateSingleRate()
        {
            // ARRANGE
            var albums = new List<Album>
            {
                new Album { Rate = 5 }
            };
            // ACT
            var rate = _rateCalculator.Calculate(albums);
            // ASSERT
            Assert.That(rate, Is.EqualTo(5));
        }

        [Test]
        public void TestCalculateDoubleRate()
        {
            // ARRANGE
            var albums = new List<Album>
            {
                new Album { Rate = 5 },
                new Album { Rate = 10 }
            };
            // ACT
            var rate = _rateCalculator.Calculate(albums);
            // ASSERT
            Assert.That(rate, Is.EqualTo(8));
        }

        [Test]
        public void TestCalculateRate()
        {
            // ARRANGE
            var albums = new List<Album>
            {
                new Album { Rate = 1 },
                new Album { Rate = 10 },
                new Album { Rate = 2 },
                new Album { Rate = 4 }
            };
            // ACT
            var rate = _rateCalculator.Calculate(albums);
            // ASSERT
            Assert.That(rate, Is.EqualTo(3));
        }

        [Test]
        public void TestCalculateRateNullCollection()
        {
            // ARRANGE, ACT, ASSERT
            Assert.Throws<ArgumentNullException>(() => _rateCalculator.Calculate(null));
        }

        [Test]
        public void TestCalculateSomeNullRates()
        {
            // ARRANGE
            var albums = new List<Album>
            {
                new Album { Rate = 1 },
                new Album { Rate = null },
                new Album { Rate = 2 },
                new Album { Rate = 4 }
            };
            // ACT
            var rate = _rateCalculator.Calculate(albums);
            // ASSERT
            Assert.That(rate, Is.EqualTo(2));
        }
    }
}
