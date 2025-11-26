// -----------------------------------------------------------------------------
// FILE:	    PolyLineEncoder.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using NetTopologySuite.Geometries;

namespace Neon.Mapbox
{
    public class PolyLineEncoder
    {
        public string Encode(double latitude, double longitude)
        {
            return Encode(new Coordinate(latitude, longitude));
        }

        public string Encode(Coordinate point)
        {
            return Encode(new[] { point });
        }

        public string Encode(IEnumerable<Tuple<double, double>> points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));

            var geoPoints = points.Select(point => new Coordinate(point.Item1, point.Item2)).Cast<Coordinate>().ToList();
            return Encode(geoPoints);
        }

        public string Encode(params Coordinate[] points)
        {
            return Encode(points.ToList());
        }

        public string Encode(IEnumerable<Coordinate> points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));

            var encodedPolyline = new StringBuilder();

            var encodeDiff = (Action<int>)(diff =>
            {
                var shifted = diff << 1;
                if (diff < 0)
                    shifted = ~shifted;

                var rem = shifted;
                while (rem >= 0x20)
                {
                    encodedPolyline.Append((char)((0x20 | (rem & 0x1f)) + 63));
                    rem >>= 5;
                }
                encodedPolyline.Append((char)(rem + 63));
            });

            var lastLat = 0;
            var lastLng = 0;
            foreach (var point in points)
            {
                var roundedLat = Math.Round(point.Y, 5);
                var roundedLng = Math.Round(point.X, 5);

                var lat = (int)Math.Round(roundedLat * 1E5);
                var lng = (int)Math.Round(roundedLng * 1E5);

                encodeDiff(lat - lastLat);
                encodeDiff(lng - lastLng);

                lastLat = lat;
                lastLng = lng;
            }

            return HttpUtility.UrlEncode(encodedPolyline.ToString());
        }
    }
}
