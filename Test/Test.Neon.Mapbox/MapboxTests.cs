// -----------------------------------------------------------------------------
// FILE:	    MapboxTests.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Text.Json;

using Neon.Mapbox;
using Neon.Mapbox.Models;

using NetTopologySuite.Geometries;

namespace Test.Neon.Mapbox
{
    public class MapboxTests
    {
        [Fact]
        public void EncodeAmsterdamCoordinate()
        {
            var encoder = new PolyLineEncoder();
            var actual = encoder.Encode(TestData.AmsterdamCoordinate);

            Assert.Equal(TestData.AmsterdamCoordinateEncode, actual);
        }

        [Fact]
        public void EncodeAppleHeadCoordinate()
        {
            var encoder = new PolyLineEncoder();
            var actual = encoder.Encode(TestData.AppleHeadCoordinate);

            Assert.Equal(TestData.AppleHeadCoordinateEncode, actual);
        }

        [Fact]
        public void EncodeAmsterdamCoordinateAndAppleCoordinate()
        {
            var encoder = new PolyLineEncoder();
            var actual = encoder.Encode(TestData.AmsterdamCoordinate, TestData.AppleHeadCoordinate);

            Assert.Equal("%7bps%7eHya%7b%5czexzAnbueW", actual);
        }

        [Fact]
        public void EncodeGoogleExample()
        {
            var encoder = new PolyLineEncoder();
            var actual = encoder.Encode(TestData.GoogleExampleCoordinates);

            Assert.Equal(TestData.GoogleExampleCoordinatesEncode, actual);
        }

        [Fact]
        public void CoordinateEqualityComparer()
        {
            var c1 = new Coordinate(1.0000, 1.0000);
            var c2 = new Coordinate(1.0000, 1.0000);
            var c3 = new Coordinate(1.0000, 1.00001);

            Assert.Equal(c1, c2);
            Assert.NotEqual(c1, c3);
        }
        [Fact]
        public void DeserializeGeoJsonSource()
        {
            var sourceString = @"{
  ""type"": ""geojson"",
  ""data"": {
    ""type"": ""FeatureCollection"",
    ""features"": [
      {
        ""type"": ""Feature"",
        ""geometry"": {
          ""type"": ""Point"",
          ""coordinates"": [
            -122.32526303536436,
            47.551866501168185
          ]
        }
      }
    ]
  }
}";
            var source = JsonSerializer.Deserialize<GeoJsonSource>(sourceString, MapboxMap.DefaultSerializerOptions);

        }
    }

    

    public static class TestData
    {

        /// <summary>
        /// Extra info 
        /// https://developers.google.com/maps/documentation/utilities/polylineutility
        /// </summary>

        public static Coordinate AmsterdamCoordinate = new Coordinate(4.895168, 52.3702160);
        public static Coordinate AppleHeadCoordinate = new Coordinate(-122.030189, 37.3316760);
        public static Coordinate MicrosofHeadCoordinate = new Coordinate( -122.075624, 37.4160740);

        public static List<Coordinate> GoogleExampleCoordinates = new List<Coordinate>() {
            new Coordinate( -120.2, 38.5),
            new Coordinate( -120.95, 40.7),
            new Coordinate( -126.453, 43.252),
        };

        public const string AmsterdamCoordinateEncode = @"%7bps%7eHya%7b%5c";
        public const string AppleHeadCoordinateEncode = @"_jzbFt_ygV";
        public const string MicrosoftHeadCoordinateEncode = @"myjcFr{ahV";
        public const string GoogleExampleCoordinatesEncode = @"_p%7eiF%7eps%7cU_ulLnnqC_mqNvxq%60%40";

        public const string TripAroundZwanenBurg =
        @"wns~Hwo{\JiBf@eF??PaBb@qA??zErFzB`CMfCQBmD_DgDuCWWX_CZ{@[We@hA[~CaA_Ae@lDqBgBUxAbF|E";
    }
}