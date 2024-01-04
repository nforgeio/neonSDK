//-----------------------------------------------------------------------------
// FILE:        TailwindExtensions.cs
// CONTRIBUTOR: Marcus Bowyer
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Neon.Common;
using Neon.Tailwind;

namespace Neon.Tailwind
{
    public static class TailwindExtensions
    {
        [UnsupportedOSPlatform("browser")]
        public static void RunTailwind(this IApplicationBuilder applicationBuilder,
            string inputCssPath  = "./Styles/tailwind.css",
            string outputCssPath = "./wwwroot/css/tailwind.css",
            bool watch           = true)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            EnsureConfiguration(inputCssPath);

            var enviromentPath = System.Environment.GetEnvironmentVariable("PATH");
            var paths          = enviromentPath.Split(';');
            var executablePath = paths.Where(path => File.Exists(Path.Combine(path, "npx.exe")) ||
                                                     File.Exists(Path.Combine(path, "npx.cmd")) ||
                                                     File.Exists(Path.Combine(path, "npx")))
                                    .FirstOrDefault();
            Regex reg          = new Regex(@"([\/\\](\w+\.\w+)?)*(npx.?(exe|cmd|))$");
            var executable     = Directory.GetFiles(executablePath, "npx.cmd").Where(path => reg.IsMatch(path)).FirstOrDefault();

            var args = new List<string>
            {
                "--prefix",
                Environment.CurrentDirectory,
                "tailwindcss",
                "-i",
                inputCssPath,
                "-o",
                outputCssPath
            };

            if (watch)
            {
                args.Add("--watch");
            }

            var nodeRunner = new NodeRunner(executable, args.ToArray());
        }

        [UnsupportedOSPlatform("browser")]
        public static void RunTailwind(this IApplicationBuilder applicationBuilder, string script)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            var enviromentPath = System.Environment.GetEnvironmentVariable("PATH");
            var paths          = enviromentPath.Split(';');
            var executablePath = paths.Where(path => File.Exists(Path.Combine(path, "npm.exe")) ||
                                                     File.Exists(Path.Combine(path, "npm.cmd")) ||
                                                     File.Exists(Path.Combine(path, "npm")))
                                    .FirstOrDefault();
            Regex reg          = new Regex(@"([\/\\](\w+\.\w+)?)*(npm.?(exe|cmd|))$");
            var executable     = Directory.GetFiles(executablePath, "npm.cmd").Where(path => reg.IsMatch(path)).FirstOrDefault();

            var args = new List<string>
            {
                "--prefix",
                Environment.CurrentDirectory,
                "run",
                script
            };

            var nodeRunner = new NodeRunner(executable, args.ToArray());
        }

        public static IServiceCollection AddTailwind(this IServiceCollection builder)
        {
            builder.AddScoped<IPortalBinder, PortalBinder>();

            return builder;
        }

        private static void EnsureConfiguration(string inputCssPath)
        {
            var fileSystem = Assembly.GetExecutingAssembly().GetResourceFileSystem("Neon.Tailwind.Resources");

            var tailwindConfigPath = "./tailwind.config.js";
            if (!File.Exists(tailwindConfigPath))
            {
                var file = fileSystem.GetFile("/tailwindconfig.js");
                File.WriteAllText(tailwindConfigPath, file.ReadAllText());
            }

            if (!File.Exists(inputCssPath))
            {
                var file = fileSystem.GetFile("/tailwind.css");
                Directory.CreateDirectory(Path.GetDirectoryName(inputCssPath));
                File.WriteAllText(inputCssPath, file.ReadAllText());
            }
        }
    }
}
