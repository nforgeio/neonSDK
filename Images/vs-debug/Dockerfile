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

FROM        mcr.microsoft.com/dotnet/sdk:7.0
MAINTAINER  jeff@lill.io
STOPSIGNAL  SIGTERM

#------------------------------------------------------------------------------
# Environment

ENV TZ=UTC

# Set the dotnet runtime to invariant cuture mode (required by [vsdbg]).
#
#   https://learn.microsoft.com/en-us/dotnet/core/runtime-config/globalization

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

#------------------------------------------------------------------------------
# Install SSHD and the remote debugger at: \vsdbg\vsdbg
#
# We also need to proactively create the [/root/.vs-debugger] directory
# so the VS debugger will be able to attach and do its thing.

RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates unzip curl openssh-server \
    && rm -rf /var/lib/apt/lists/* \
    && curl -sSL https://aka.ms/getvsdbgsh -o /tmp/vsdbg.sh \
    && bash /tmp/vsdbg.sh -v latest -l /vsdbg \
    && rm /tmp/vsdbg.sh \
    && mkdir -p /root/.vs-debugger

#------------------------------------------------------------------------------
# Configure SSHD

# We need to explcitly configure the server's SSH certificates so developers
# won't need to verify the SSH connection everytime a new pod is deployed.

COPY ssh/* /etc/ssh

# We're going to allow root login via SSH with a blank password to make
# ie easy to attach a debugger to a Kubernetes pod with this container
# attached as an ephemeral sidecar.
#
# This looks insecure but the user doing this will be using Kubernetes
# port forwarding to connect to the ephemeral container which means the
# connection will already be secured via the user's Kubernetes certificate.

RUN mkdir -p /etc/ssh/sshd_config.d \
    && echo "PermitRootLogin yes"         > /etc/ssh/sshd_config.d/05-auth.conf \
    && echo "PasswordAuthentication yes" >> /etc/ssh/sshd_config.d/05-auth.conf \
    && echo "PermitEmptyPasswords yes"   >> /etc/ssh/sshd_config.d/05-auth.conf \
    && echo "ssh"                         > /etc/securetty \
    && touch /root/.hushlogin \
    && passwd --delete root \
    && chmod 600 /etc/ssh/*

#------------------------------------------------------------------------------
# Install the .NET diagnostic tools.

RUN dotnet tool install --tool-path /tools dotnet-trace \
  && dotnet tool install --tool-path /tools dotnet-dump \
  && dotnet tool install --tool-path /tools dotnet-counters

#------------------------------------------------------------------------------
# Install useful debugging tools.

RUN apt-get update \
    && apt-get install -yq dnsutils \
    && apt-get install -yq iproute2 \
    && apt-get install -yq iputils-ping \
    && apt-get install -yq mtr \
    && apt-get install -yq nano \
    && apt-get install -yq netcat \
    && apt-get install -yq net-tools \
    && apt-get install -yq traceroute \
    && apt-get install -yq tcpdump \
    && apt-get install -yq rsync \
    && apt-get install -yq wget \
    && rm -rf /var/lib/apt/lists/*

# Start the SSHD server automatically.

CMD ["service", "ssh", "start", "-D"]
