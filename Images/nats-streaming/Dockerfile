#------------------------------------------------------------------------------
# FILE:         Dockerfile
# CONTRIBUTOR:  NEONFORGE Team
# COPYRIGHT:    Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
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

# Base NATS-STREAMING image.
#
# ARGUMENTS:
#
#   VERSION         - The source NATS image version (e.g. "0.12.2")

ARG         VERSION
FROM        nats-streaming:${VERSION}-linux
MAINTAINER  devops@neonforge.com
STOPSIGNAL  SIGTERM

# Expose client and management ports
EXPOSE 4222 8222
