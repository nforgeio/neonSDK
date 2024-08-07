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

# This base image deploys a simple container/or service used
# for cluster unit tests.
#
# ARGUMENTS:
#
#   ORGANIZATION    - The Docker Hub organization
#   BRANCH          - The current GitHub branch

ARG         ORGANIZATION
ARG         BRANCH
FROM        alpine:latest
MAINTAINER  devops@neonforge.com
STOPSIGNAL  SIGTERM

# Environment

ENV TZ=UTC

# Configure

COPY docker-entrypoint.sh   /

RUN chmod 700 /*.sh \
    && ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

ENTRYPOINT ["/docker-entrypoint.sh"]
