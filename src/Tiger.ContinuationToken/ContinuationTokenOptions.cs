// <copyright file="ContinuationTokenOptions.cs" company="Cimpress, Inc.">
//   Copyright 2018 Cimpress, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>

namespace Tiger.ContinuationToken
{
    /// <summary>Represents the application's options for continuation token configuration.</summary>
    public sealed class ContinuationTokenOptions
    {
        /// <summary>Gets or sets the password to use for encryption operations.</summary>
        public string Password { get; set; }

        /// <summary>Gets or sets the salt to use for encryption operations.</summary>
        public string Salt { get; set; }

        /// <summary>
        /// Gets or sets the number of iterations to perform for encryption operations.
        /// </summary>
        public int Iterations { get; set; } = 1 << 12;
    }
}
