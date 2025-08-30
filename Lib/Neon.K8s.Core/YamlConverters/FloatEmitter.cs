// -----------------------------------------------------------------------------
// FILE:	    IntOrStringYamlConverter.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2025 by NEONFORGE LLC.  All rights reserved.
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
using System.Text;

using YamlDotNet.Core;
using YamlDotNet.Serialization.EventEmitters;
using YamlDotNet.Serialization;
using YamlDotNet.Core.Events;

namespace Neon.K8s.YamlConverters
{
    internal class FloatEmitter : ChainedEventEmitter
    {
        public FloatEmitter(IEventEmitter nextEmitter)
            : base(nextEmitter)
        {
        }

        public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
        {
            switch (eventInfo.Source.Value)
            {
                // Floating point numbers should always render at least one zero (e.g. 1.0f => '1.0' not '1')

                case double d:

                    emitter.Emit(new Scalar(d.ToString("0.0######################")));
                    break;

                case float f:

                    emitter.Emit(new Scalar(f.ToString("0.0######################")));
                    break;

                default:

                    base.Emit(eventInfo, emitter);
                    break;
            }
        }
    }
}
