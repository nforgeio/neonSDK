#!/bin/sh
#------------------------------------------------------------------------------
# FILE:         docker-entrypoint.sh
# CONTRIBUTOR:  Jeff Lill
# COPYRIGHT:    Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.

# Add the root directory to the PATH.

PATH=${PATH}:/

# Launch the service.

exec test-api
