// -----------------------------------------------------------------------------
// FILE:	    AesProtector.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Neon.Cryptography;

namespace Neon.Web
{
    /// <summary>
    /// Provides data protection using AES Cipher.
    /// </summary>
    public class AesProtector : IDataProtector
    {
        private AesCipher cipher;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="aesCipher"></param>
        public AesProtector(AesCipher aesCipher)
        {
            cipher = aesCipher;
        }

        /// <inheritdoc/>
        public IDataProtector CreateProtector(string purpose)
        {
            return this;
        }

        /// <inheritdoc/>
        public byte[] Protect(byte[] plaintext)
        {
            return cipher.EncryptToBytes(plaintext);
        }

        /// <inheritdoc/>
        public byte[] Unprotect(byte[] protectedData)
        {
            return cipher.DecryptBytesFrom(protectedData);
        }
    }

    /// <summary>
    /// Extension methods for <see cref="IDataProtectionBuilder"/>.
    /// </summary>
    public static class DataProtectionExtensions
    {
        /// <summary>
        /// Adds an AES data protection provider to the service collection.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IDataProtectionBuilder UseAesDataProtectionProvider(this IDataProtectionBuilder builder)
        {
            builder.Services.Replace(ServiceDescriptor.Singleton<IDataProtectionProvider, AesProtector>((services) =>
            {
                return new AesProtector(services.GetRequiredService<AesCipher>());
            }));

            return builder;
        }

        /// <summary>
        /// Adds an AES data protection provider to the service collection.
        /// </summary>
        /// <param name="builder">The <see cref="IDataProtectionBuilder"/></param>
        /// <param name="cipherKey">The Cipher key</param>
        /// <returns></returns>
        public static IDataProtectionBuilder UseAesDataProtectionProvider(
            this IDataProtectionBuilder builder,
            string cipherKey)
        {
            builder.Services.AddSingleton<AesCipher>(new AesCipher(cipherKey));
            builder.Services.Replace(ServiceDescriptor.Singleton<IDataProtectionProvider, AesProtector>((services) =>
            {
                return new AesProtector(services.GetRequiredService<AesCipher>());
            }));

            return builder;
        }
    }
}
