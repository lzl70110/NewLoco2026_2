namespace GCommon
{
    public static class Messages
    {
        public static class FuelServiceKeys
        {
            // Validation / Domain message keys (keep as-is with Msg_ prefix)
            public const string Msg_FuelInFuture = "Msg_FuelInFuture";
            public const string Msg_FinalFuelTooHigh = "Msg_FinalFuelTooHigh";
            public const string Msg_NoFuelRecordForLoco = "Msg_NoFuelRecordForLoco";
            public const string Msg_NotEnoughFuel = "Msg_NotEnoughFuel";
        }

        public static class Locomotive
        {
            // Direct EN messages (keep plain text style)
            public const string Error_Number_Required = "Number required";
            public const string Error_Number_Length = "Number must be exactly 6 chars (NN-NNN)";
            public const string Error_Number_Format = "Number must be in format NN-NNN";
            public const string Error_Note_Length = "Note length must be between {1} and {0}";
        }

        public static class Rbac
        {
            // Roles listing / general
            public const string Info_NoRolesFound = "No roles found.";
            public const string Info_ManageRoles = "Manage application roles";

            // Create role
            public const string Error_RoleName_Required = "Role name is required.";
            public const string Error_Role_AlreadyExists = "Role already exists.";
            public const string Error_Role_CreateFailed = "Failed to create role: {0}";
            public const string Info_Role_Created = "Role '{0}' created.";

            // Update role (optional, for future UI)
            public const string Error_Role_UpdateFailed = "Failed to update role: {0}";
            public const string Info_Role_Updated = "Role '{0}' updated.";

            // Delete role
            public const string Error_RoleName_Missing = "Role name is required.";
            public const string Error_CannotDelete_SysAdmin = "SysAdmin role cannot be deleted.";
            public const string Error_Role_NotFound = "Role not found.";
            public const string Error_Role_DeleteFailed = "Failed to delete role: {0}";
            public const string Info_Role_Deleted = "Role '{0}' deleted.";
            public const string Error_Role_DeleteHasUsers = "Role '{0}' has assigned users and cannot be deleted."; // use if you add such guard

            // Claims (permissions) — RoleClaims
            public const string Info_Claims_ForRole = "Permission claims for role '{0}'";
            public const string Error_Claim_AddFailed = "Failed to add claim: {0}";
            public const string Error_Claim_RemoveFailed = "Failed to remove claim: {0}";
            public const string Info_Claim_Added = "Claim '{0}' added.";
            public const string Info_Claim_Removed = "Claim '{0}' removed.";

            // Bulk permissions update
            public const string Info_Permissions_Updated = "Permissions updated for '{0}'.";
            public const string Error_Permissions_UpdateFailed = "Failed to update permissions: {0}";
            public const string Info_Nothing_Changed = "No changes were made.";
            public const string Error_Role_Claims_LoadFailed = "Unable to load claims for the selected role.";
        }

        public static class Ui // optional: common UI toasts/labels
        {
            public const string Toast_Success = "Success";
            public const string Toast_Error = "Error";
            public const string Btn_Create = "Create";
            public const string Btn_Save = "Save";
            public const string Btn_Delete = "Delete";
            public const string Btn_Back = "Back";
        }
    }
}