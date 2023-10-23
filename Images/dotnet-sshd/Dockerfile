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

# This is used when testing this container image on Docker.  Our tooling
# will expose SSHD port 22 when running in production.

EXPOSE 22/tcp

# Install SSHD and other tools required for remote debugging.

RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates openssh-server unzip curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /root/.vs-debugger

# SSHD configuration:
#
# We're going to allow root login via SSH with a blank password to make
# ie easy to attach a debugger to a Kubernetes pod with this container
# attached as an ephemeral sidecar.
#
# This may look insecure but the user doing this will be using Kubernetes
# port forwarding to connect to the ephemeral container which means the
# connection will already be secured via the user's Kubernetes certificate.

RUN mkdir -p /etc/ssh/sshd_config.d \
    && echo "PermitRootLogin yes"         > /etc/ssh/sshd_config.d/05-auth.conf \
    && echo "PasswordAuthentication yes" >> /etc/ssh/sshd_config.d/05-auth.conf \
    && echo "PermitEmptyPasswords yes"   >> /etc/ssh/sshd_config.d/05-auth.conf \
    && echo "ssh"                         > /etc/securetty \
    && touch /root/.hushlogin \
    && passwd --delete root

# We need to proactively create the [/root/.vs-debugger] directory so
# the VS debugger will be able to attach and do its thing and we also
# need to explcitly configure the server's SSH certificats so developers
# won't need to reverify the connection everytime a new pod is deployed.

COPY ssh/* /etc/ssh

# Start the SSHD server automatically.

CMD ["service", "ssh", "start", "-D"]
