//-----------------------------------------------------------------------------
// FILE:        Test_TempFolder.cs
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Neon.Common;
using Neon.IO;
using Neon.Xunit;

using Xunit;

namespace TestCommon
{
    [Trait(TestTrait.Category, TestArea.NeonCommon)]
    public class Test_TempFolder
    {
        [Fact]
        public void DefaultRoot()
        {
            string testFolderPath;
            string testFilePath;

            using (var folder = new TempFolder())
            {
                testFolderPath = folder.Path;

                Assert.Equal(Path.GetTempPath(), Path.GetFullPath(Path.Combine(folder.Path, "..") + "/"));

                testFilePath = Path.Combine(folder.Path, "test.txt");

                File.WriteAllText(testFilePath, "Hello World!");
                Assert.True(File.Exists(testFilePath));
            }

            Assert.False(Directory.Exists(testFolderPath));
            Assert.False(File.Exists(testFilePath));
        }

        [Fact]
        public void CustomRoot()
        {
            string customRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("d"));
            string testFolderPath;
            string testFilePath;

            TempFolder.Root = customRoot;

            try
            {
                using (var folder = new TempFolder())
                {
                    testFolderPath = folder.Path;

                    Assert.Equal(customRoot, Path.GetFullPath(Path.Combine(folder.Path, "..")));

                    testFilePath = Path.Combine(folder.Path, "test.txt");

                    File.WriteAllText(testFilePath, "Hello World!");
                    Assert.True(File.Exists(testFilePath));
                }

                Assert.False(Directory.Exists(testFolderPath));
                Assert.False(File.Exists(testFilePath));
            }
            finally
            {
                TempFolder.Root = null;
                Directory.Delete(customRoot);
            }
        }

        [Fact]
        public void CustomParent()
        {
            string customRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("d"));
            string testFolderPath;
            string testFilePath;

            try
            {
                using (var folder = new TempFolder(rootFolder: customRoot))
                {
                    testFolderPath = folder.Path;

                    Assert.Equal(customRoot, Path.GetFullPath(Path.Combine(folder.Path, "..")));

                    testFilePath = Path.Combine(folder.Path, "test.txt");

                    File.WriteAllText(testFilePath, "Hello World!");
                    Assert.True(File.Exists(testFilePath));
                }

                Assert.False(Directory.Exists(testFolderPath));
                Assert.False(File.Exists(testFilePath));
            }
            finally
            {
                Directory.Delete(customRoot);
            }
        }

        [Fact]
        public void NoDelete()
        {
            string path;

            using (var temFolder = new TempFolder() { DisableDelete = true })
            {
                path = temFolder.Path;

                Assert.True(Directory.Exists(path));
            }

            Assert.True(Directory.Exists(path));
            Directory.Delete(path);
        }

        [Fact]
        public void Existing()
        {
            var tempFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("d"));

            // Parameter checks.

            Assert.ThrowsAny<ArgumentNullException>(() => new TempFolder(null, Stub.Param));
            Assert.ThrowsAny<ArgumentNullException>(() => new TempFolder(string.Empty, Stub.Param));

            // The temporary folder does't exist yet, so we should see an exception.

            Assert.ThrowsAny<ArgumentException>(() => new TempFolder(tempFolderPath, Stub.Param));

            // Verify that this works.

            Directory.CreateDirectory(tempFolderPath);

            using (var tempFolder = new TempFolder(tempFolderPath, Stub.Param))
            {
                tempFolder.DisableDelete = true;
            }

            Assert.True(Directory.Exists(tempFolderPath));

            using (var tempFolder = new TempFolder(tempFolderPath, Stub.Param))
            {
                tempFolder.DisableDelete = false;
            }

            Assert.False(Directory.Exists(tempFolderPath));
        }
    }
}
