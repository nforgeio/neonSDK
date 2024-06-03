//-----------------------------------------------------------------------------
// FILE:        main.cs
// CONTRIBUTOR: Jeff Lill
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

#include <stdio.h>                                            
#include <stdlib.h>                                           
#include <string.h>                                           
#include <unistd.h>                                           

int main()
{
    char    buffer[64];
    FILE*   file;
    char*   value;

    // Read the [health-status] file from the same directory as this executable.
    // We'll exit immediately if the file doesn't exist or we couldn't read the
    // first line.

    file = fopen("./health-status", "r");

    if (file == NULL)
    {
        return 1;
    }

    value = fgets(buffer, sizeof(buffer), file);

    fclose(file);

    if (value == NULL)
    {
        return 1;
    }

    // The value read may include a linefeed.  We'll trim that here.

    char* endingPos = strchr(value, '\n');

    if (endingPos != NULL)
    {
        *endingPos = 0;
    }

    // Determine whether the service is ready.

    if (strcmp(value, "running") == 0)
    {
        return 0;
    }

    return 1;
}


