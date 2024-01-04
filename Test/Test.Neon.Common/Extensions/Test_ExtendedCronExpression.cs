// -----------------------------------------------------------------------------
// FILE:	    Test_ExtendedCronExpression.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Neon.Common;
using Neon.Xunit;

using Quartz;

using Xunit;

namespace TestCommon.Extensions
{
    public class Test_ExtendedCronExpression
    {
        /// <summary>
        /// Verifies that random values are generated for specific cron fields and also
        /// iterate to ensure that random values are actually generated.
        /// </summary>
        /// <param name="expression">Specifies the extended cron expression being checked.</param>
        /// <param name="index">Index of the field being randomized.</param>
        private void ValidateRandomField(string expression, int index)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(expression), nameof(expression));
            Covenant.Requires<ArgumentException>(0 <= index && index <= 6, nameof(index));

            // We're going to iterate up to 10 million times to verify that we end up seeing
            // all possible random values for the field.
            //
            // NOTE: It's technically possible but extermely unlikely that we won't see
            //       all possible field values after 10M iterations.

            var values = new HashSet<int>();

            for (int i = 0; i < 10000000; i++)
            {
                var standard = NeonExtendedHelper.FromEnhancedCronExpression(expression);

                CronExpression.ValidateExpression(standard);

                var fields = standard.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (index >= fields.Length)
                {
                    Covenant.Requires<ArgumentException>(false, nameof(index));
                }

                var field = int.Parse(fields[index]);
                int min;
                int max;

                switch (index)
                {
                    case 0:     // seconds
                    case 1:     // minutes

                        min = 0;
                        max = 59;
                        break;

                    case 2:     // hours

                        min = 0;
                        max = 23;
                        break;

                    case 3:     // day-of-month

                        min = 1;
                        max = 31;
                        break;

                    case 4:     // month

                        min = 1;
                        max = 12;
                        break;

                    case 5:     // day-of-week

                        min = 1;
                        max = 7;
                        break;

                    default:

                        Assert.Fail();
                        return;
                }

                Assert.True(min <= field && field <= max);

                if (!values.Contains(field))
                {
                    values.Add(field);
                }

                if (values.Count >= max - min + 1)
                {
                    return;
                }
            }

            Assert.Fail("Missing one or more random values");
        }

        [Fact]
        public void RandomFields()
        {
            // Verify that standard cron expressions work.

            CronExpression.ValidateExpression(NeonExtendedHelper.FromEnhancedCronExpression("0 0 0 * JAN ?"));

            // Verify that cron fields set to "R" are randomized to acceptable values.

            ValidateRandomField("R 0 0 * JAN ?", 0);
            ValidateRandomField("0 R 0 * JAN ?", 1);
            ValidateRandomField("0 0 R * JAN ?", 2);
            ValidateRandomField("0 0 0 R JAN ?", 3);
            ValidateRandomField("0 0 0 * R ?", 4);
            ValidateRandomField("0 0 0 ? JAN R", 5);
        }
    }
}
