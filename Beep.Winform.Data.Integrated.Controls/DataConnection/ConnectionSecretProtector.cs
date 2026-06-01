using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TheTechIdea.Beep.ConfigUtil;

namespace TheTechIdea.Beep.Winform.Controls
{
    internal static class ConnectionSecretProtector
    {
        private const string EncPrefix = "__enc__:";
        private static readonly string[] SecretPropertyNames =
        {
            nameof(ConnectionProperties.Password),
            nameof(ConnectionProperties.ApiKey),
            nameof(ConnectionProperties.KeyToken),
            nameof(ConnectionProperties.ClientSecret),
            nameof(ConnectionProperties.ProxyPassword),
            nameof(ConnectionProperties.ClientCertificatePassword),
            nameof(ConnectionProperties.OAuthAccessToken),
            nameof(ConnectionProperties.OAuthRefreshToken),
            nameof(ConnectionProperties.OAuthClientSecret),
            nameof(ConnectionProperties.AuthCode)
        };

        public static ConnectionProperties Encrypt(ConnectionProperties source)
        {
            var clone = Clone(source);
            foreach (var propertyName in SecretPropertyNames)
            {
                var prop = typeof(ConnectionProperties).GetProperty(propertyName);
                if (prop == null || !prop.CanRead || !prop.CanWrite)
                {
                    continue;
                }

                if (prop.GetValue(clone) is not string raw || string.IsNullOrWhiteSpace(raw))
                {
                    continue;
                }

                if (raw.StartsWith(EncPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var bytes = Encoding.UTF8.GetBytes(raw);
                var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
                prop.SetValue(clone, EncPrefix + Convert.ToBase64String(encrypted));
            }

            return clone;
        }

        public static ConnectionProperties Decrypt(ConnectionProperties source)
        {
            var clone = Clone(source);
            foreach (var propertyName in SecretPropertyNames)
            {
                var prop = typeof(ConnectionProperties).GetProperty(propertyName);
                if (prop == null || !prop.CanRead || !prop.CanWrite)
                {
                    continue;
                }

                if (prop.GetValue(clone) is not string raw || string.IsNullOrWhiteSpace(raw))
                {
                    continue;
                }

                if (!raw.StartsWith(EncPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var payload = raw.Substring(EncPrefix.Length);
                try
                {
                    var encrypted = Convert.FromBase64String(payload);
                    var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                    prop.SetValue(clone, Encoding.UTF8.GetString(decrypted));
                }
                catch
                {
                    // Keep encrypted value when decryption fails.
                }
            }

            return clone;
        }

        public static ConnectionProperties StripSecrets(ConnectionProperties source)
        {
            var clone = Clone(source);
            foreach (var propertyName in SecretPropertyNames)
            {
                var prop = typeof(ConnectionProperties).GetProperty(propertyName);
                if (prop?.CanWrite == true)
                {
                    prop.SetValue(clone, string.Empty);
                }
            }

            return clone;
        }

        private static ConnectionProperties Clone(ConnectionProperties source)
        {
            var clone = new ConnectionProperties();
            var properties = typeof(ConnectionProperties).GetProperties()
                .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0);

            foreach (var property in properties)
            {
                try
                {
                    property.SetValue(clone, property.GetValue(source));
                }
                catch
                {
                    // Ignore unsupported copy properties.
                }
            }

            clone.ParameterList = source.ParameterList != null
                ? new Dictionary<string, string>(source.ParameterList, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            return clone;
        }
    }
}
