//-----------------------------------------------------------------------------
// FILE:        nanoshell.cs
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

int main(int argc, char *argv[])
{
    char* usage =
        "\n"
        "nanoshell: An extremely lightweight shell used for working with\n"
        "scratch Docker container images\n"
        "\n"
        "LIMITATIONS:\n"
        "\n"
        "PATH or other environment variables are not recognized so you'll need to use\n"
        "absolute directory and file references.\n"
        "\n"
        "Single and double quoted strings are not currently supported.\n"
        "\n"
        "File pipe, redirection and other operators are not supported.\n"
        "\n"
        "Double and single quotes are not supported nor is character escaping.\n"
        "\n"
        "COMMANDS:\n"
        "\n"
        "------------------------------------------------------------\n"
        "help\n"
        "\n"
        "Prints help.\n"
        "\n"
        "------------------------------------------------------------\n"
        "run PATH SPACE-SEPARATED-ARGS\n"
        "\n"
        "Runs the binary at PATH, passing any arguments passed\n"
        "\n"
        "------------------------------------------------------------\n"
        "mv SOURCE-PATH TARGET-PATH\n"
        "\n"
        "Moves a single file from SOURCE-PATH to TARGET-PATH.\n"
        "Wildcards are not supported\n"
        "\n";

    if (argc <= 1)
    {
        fprintf(stderr, "%s", usage);
        fprintf(stderr, "*** ERROR: Command expected\n\n");
        return 1;
    }

    char    *command = argv[1];
    int     exitcode;

    if (strcmp(command, "help") == 0)
    {
        fprintf(stdout, "%s", usage);
        return 0;
    }
    else if (strcmp(command, "run") == 0)
    {
        fprintf(stderr, "\n");

        if (argc <= 2)
        {
            fprintf(stderr, "%s", "*** ERROR: Missing PATH argument.\n\n");
            return 1;
        }

        // Format an array of arguments by putting the target path into
        // the first element of the array, followed by the command arguments,
        // and then terminated by a NULL.

        char    *execPath  = argv[2];
        char    **execArgs = malloc((argc + 1) * sizeof(char*));
        int     pos        = 0;

        execArgs[pos++] = argv[0];

        for (int i = 2; i < argc; i++)
        {
            execArgs[pos++] = argv[i];
        }

        execArgs[pos] = NULL;

        // Execute the program.

        exitcode = execv(execPath, execArgs);

        if (exitcode != 0)
        {
            fprintf(stderr, "%s", "*** ERROR: Run failed.\n\n");
        }

        return exitcode;
    }
    else if (strcmp(command, "mv") == 0)
    {
        fprintf(stderr, "\n");

        if (argc <= 2)
        {
            fprintf(stderr, "%s", "*** ERROR: Missing SOURCE-PATH argument.\n\n");
            return 1;
        }

        if (argc <= 3)
        {
            fprintf(stderr, "%s", "*** ERROR: Missing TARGET-PATH argument.\n\n");
            return 1;
        }

        char    *sourcePath = argv[2];
        char    *targetPath = argv[3];
        char    buffer[8096];
        size_t  cb;
        FILE    *source = NULL;
        FILE    *target = NULL;

        source = fopen(sourcePath, "r");
        target = fopen(targetPath, "w");

        while ((cb = fread(buffer, sizeof(char), sizeof(buffer), source)) > 0)
        {
            if (fwrite(buffer, sizeof(char), cb, target) != cb)
            {
                fprintf(stderr, "%s", "*** ERROR: Copy failed.\n\n");
                return 1;
            }
        }

        if (source != NULL)
        {
            fclose(source);
        }

        if (target != NULL)
        {
            fclose(target);
        }

        remove(sourcePath);

        return 0;
    }

    fprintf(stderr, "\n*** ERROR: Unexpected command: %s\n\n", command);

    return 1;
}


