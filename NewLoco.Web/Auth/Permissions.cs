using System.Collections.Generic;
using System.Linq;

namespace NewLoco.Web.Auth
{
    /// <summary>
    /// Centralized permission names (policy name == claim value).
    /// Keep values stable; they are stored in claims and used by policies.
    /// </summary>
    public static class Perm
    {
        // Claim type used across PermissionHandler and RoleClaims controller
        public const string ClaimType = "permission";

        // NOTE: Wildcards are supported by PermissionHandler (exact + *. matching).
        // Be careful: do NOT seed wildcards (Perm.*.* / Perm.X.*) to "Everyone"/all users.
        public static class Any
        {
            public const string All = "Perm.*.*";     // full access to everything (use ONLY for superuser)
            public const string FuelAll = "Perm.Fuel.*";  // full access to Fuel
            public const string LocoAll = "Perm.Loco.*";  // full access to Locomotive
            public const string ShiftAll = "Perm.Shift.*"; // full access to ShiftWork
            public const string ToolsAll = "Perm.Tools.*"; // full access to Tools
            public const string AdminAll = "Perm.Admin.*"; // full access to Admin area
        }

        public static class Fuel
        {
            public const string View = "Perm.Fuel.View";
            public const string Create = "Perm.Fuel.Create";
            public const string Edit = "Perm.Fuel.Edit";
            public const string Delete = "Perm.Fuel.Delete";
            public const string Report = "Perm.Fuel.Report";
        }

        public static class Locomotive
        {
            public const string View = "Perm.Loco.View";
            public const string Create = "Perm.Loco.Create";
            public const string Edit = "Perm.Loco.Edit";
            public const string Delete = "Perm.Loco.Delete";
        }

        public static class ShiftWork
        {
            public const string View = "Perm.Shift.View";
            public const string Create = "Perm.Shift.Create";
            public const string Edit = "Perm.Shift.Edit";
            public const string Delete = "Perm.Shift.Delete";
        }

        public static class Tools
        {
            public const string Calculator = "Perm.Tools.Calculator";
            public const string Calendar = "Perm.Tools.Calendar";
        }

        public static class Admin
        {
            public static class Roles
            {
                public const string Read = "Perm.Admin.Roles.Read";
                public const string Edit = "Perm.Admin.Roles.Edit";
            }

            public static class Users
            {
                public const string Read = "Perm.Admin.Users.Read";
                public const string Edit = "Perm.Admin.Users.Edit";
            }
        }

        // Deterministic, cached list of concrete permissions (excluding Perm.Any.All by design).
        // You can still use Any.* as claim values; policies for concrete keys are built dynamically.
        private static readonly string[] _all =
        [
            // Wildcards (kept for reference; generally NOT registered as concrete policies)
            Any.FuelAll, Any.LocoAll, Any.ShiftAll, Any.ToolsAll, Any.AdminAll,
            // NOTE: Any.All is intentionally NOT included here; it is for claims only, not for static listings.

            // Fuel
            Fuel.View, Fuel.Create, Fuel.Edit, Fuel.Delete, Fuel.Report,

            // Locomotive
            Locomotive.View, Locomotive.Create, Locomotive.Edit, Locomotive.Delete,

            // ShiftWork
            ShiftWork.View, ShiftWork.Create, ShiftWork.Edit, ShiftWork.Delete,

            // Tools
            Tools.Calculator, Tools.Calendar,

            // Admin
            Admin.Roles.Read, Admin.Roles.Edit,
            Admin.Users.Read, Admin.Users.Edit
        ];

        /// <summary>
        /// Returns a stable, read-only collection of known permissions for UI/seeding tools.
        /// </summary>
        public static IReadOnlyCollection<string> All() => _all;
    }
}