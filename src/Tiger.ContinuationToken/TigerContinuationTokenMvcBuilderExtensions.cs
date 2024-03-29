// <copyright file="TigerContinuationTokenMvcBuilderExtensions.cs" company="Cimpress, Inc.">
//   Copyright 2020–2022 Cimpress, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License") –
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

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Extensions to the functionality <see cref="IMvcBuilder"/> for <see cref="ContinuationToken{TData}"/>.</summary>
public static class TigerContinuationTokenMvcBuilderExtensions
{
    /// <summary>Adds the services necessary for proper functionality of <see cref="ContinuationToken{TData}"/>.</summary>
    /// <param name="builder">The application's MVC builder.</param>
    /// <param name="configSectionPath">The path to the configuration section by which to configure continuation token generation.</param>
    /// <returns>The modified builder.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    public static IMvcBuilder AddContinuationTokens(this IMvcBuilder builder, string configSectionPath = ContinuationTokenOptions.ContinuationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);

        _ = builder.Services
            .AddOptions<ContinuationTokenOptions>()
            .BindConfiguration(configSectionPath)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        _ = builder.Services
            .AddSingleton(_ => AwsEncryptionSdkFactory.CreateDefaultAwsEncryptionSdk())
            .AddSingleton(_ => AwsCryptographicMaterialProvidersFactory.CreateDefaultAwsCryptographicMaterialProviders())
            .AddAWSService<IAmazonKeyManagementService>()
            .AddScoped(p =>
            {
                var continuationTokenOpts = p.GetRequiredService<IOptions<ContinuationTokenOptions>>().Value;
                var kmsClient = p.GetRequiredService<IAmazonKeyManagementService>();
                var materialProviders = p.GetRequiredService<IAwsCryptographicMaterialProviders>();
                return materialProviders.CreateAwsKmsKeyring(new()
                {
                    KmsClient = kmsClient,
                    KmsKeyId = continuationTokenOpts.KmsKeyArn,
                });
            })
            .AddTransient(typeof(IEncryption<>), typeof(AwsKmsEncryption<>));

        return builder.AddMvcOptions(o => o.ModelBinderProviders.Insert(0, new ModelBinderProvider()));
    }
}
