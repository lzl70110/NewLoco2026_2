using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace NewLoco.Web.Auth
{
    /// Dynamically builds policies for names like "Perm.X.Y".
    /// Guarantees that each Perm.* policy requires authentication + a concrete permission requirement.
    /// Unknown policy names return null so AuthorizeAsync(...) fails fast (no silent fallback).
    public class ConventionalAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
    {
        // Reuse the default provider for default/fallback policies
        private readonly DefaultAuthorizationPolicyProvider fallbackProvider = new(options);

        // Cache built policies in the shared AuthorizationOptions to avoid rebuilding every time
        private readonly AuthorizationOptions _options = options.Value;

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
            => fallbackProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
            => fallbackProvider.GetFallbackPolicyAsync(); // keep global fallback for endpoints without [Authorize]

        public Task<AuthorizationPolicy?> GetPolicyAsync(string? policyName)
        {
            // Normalize input (defend against accidental whitespace / casing differences)
            policyName = policyName?.Trim();

            // If already registered (explicitly or previously built), reuse it
            if (!string.IsNullOrWhiteSpace(policyName))
            {
                var existing = _options.GetPolicy(policyName);
                if (existing is not null)
                    return Task.FromResult<AuthorizationPolicy?>(existing);
            }

            // Recognize "Perm.*" (case-insensitive)
            if (!string.IsNullOrWhiteSpace(policyName) &&
                policyName.StartsWith("Perm.", StringComparison.OrdinalIgnoreCase))
            {
                // Build a strict permission policy:
                // - must be authenticated
                // - must satisfy our PermissionRequirement for the specific key
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()                               // baseline
                    .AddRequirements(new PermissionRequirement(policyName))   // concrete permission
                    .Build();

                // Cache the policy so subsequent calls are fast and consistent
                _options.AddPolicy(policyName, policy);

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            // Strict mode for unknown named policies:
            // return null so AuthorizeAsync(User, "Unknown") results in Failed, not in Default/Fallback success.
            // (Endpoints without a named policy still benefit from the global FallbackPolicy.)
            return Task.FromResult<AuthorizationPolicy?>(null);
        }
    }
}