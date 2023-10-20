#------------------------------------------------------------------------------
# FILE:         Dockerfile
# CONTRIBUTOR:  Jeff Lill
# COPYRIGHT:    Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

FROM        ubuntu:22.04
MAINTAINER  jeff@lill.io
STOPSIGNAL  SIGTERM

# Environment

ENV TZ=UTC

# Set the dotnet runtime to invariant cluture mode.
#
#   https://learn.microsoft.com/en-us/dotnet/core/runtime-config/globalization

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# Configure: Install the remote debugger at: \vsdbg\vsdbg

RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates unzip wget \
    && rm -rf /var/lib/apt/lists/* \
    && wget https://aka.ms/getvsdbgsh -O /tmp/vsdbg.sh \
    && bash /tmp/vsdbg.sh -v latest -l /vsdbg \
    && rm /tmp/vsdbg.sh
