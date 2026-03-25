// File: GCommon/Messages.cs
// Purpose: Centralize UI / domain messages (no localization, English only).

namespace GCommon
{
    public static class Messages
    {
        // ---------------------------------------
        // Fuel service domain / validation texts
        // ---------------------------------------
        public static class FuelServiceKeys
        {
            public const string Msg_FuelInFuture = "Fuel date cannot be in the future.";
            public const string Msg_FinalFuelTooHigh = "Final fuel cannot exceed initial plus refueled amount.";
            public const string Msg_NoFuelRecordForLoco = "No fuel stock for this locomotive. Please refuel first.";
            public const string Msg_NotEnoughFuel = "Not enough fuel available.";

            // Depot rule: amounts must be multiples of 10 liters
            public const string Msg_FuelAmountMustBeMultipleOf10 = "Fuel amounts must be multiples of 10 liters.";
            public const string Msg_InvalidFuelAmount = "Invalid fuel amount.";

            // Used by FuelService to block operations below the per-class hard floor
            public const string Msg_FinalBelowHardFloorFmt =
                "Final fuel would drop below the hard floor ({0} L). Please refuel first.";
        }

        // ---------------------------------------
        // ShiftWork / Work entries
        // ---------------------------------------
        public static class ShiftWork
        {
            // Domain / validation (plain EN)
            public const string Msg_InvalidHours = "Invalid motohours value.";
            public const string Msg_InvalidKilometers = "Invalid kilometers value.";
            public const string Msg_ManualFuelRequiredForKm = "Manual fuel entry is required for kilometer-based locomotives.";
            public const string Msg_NoteRequiredForOnSite = "A note is required when working on site with zero kilometers.";

            // UI texts
            public const string Error_FinalGreaterThanInitial = "Final counter must be greater than the initial counter.";
            public const string Error_FuelTooHighFmt = "Fuel seems too high for {0:0.##} hours. Please review.";

            public const string Info_ShiftFuelRecorded = "Shift and fuel recorded.";
            public const string Info_ShiftUpdated = "Shift work updated.";
            public const string Info_ShiftDeleted = "Shift work deleted.";
            public const string Info_ShiftRestored = "Shift work restored.";

            public const string Error_ShiftSaveFailed = "Failed to save shift and fuel.";
            public const string Error_ShiftUpdateFailed = "Failed to update shift work.";
            public const string Error_ShiftDeleteFailed = "Failed to delete shift work.";
            public const string Error_ShiftRestoreFailed = "Failed to restore shift work.";
        }

        // ---------------------------------------
        // Locomotive validation/messages
        // ---------------------------------------
        public static class Locomotive
        {
            public const string Error_Number_Required = "Number required.";
            public const string Error_Number_Length = "Number must be exactly 6 chars (NN-NNN).";
            public const string Error_Number_Format = "Number must be in format NN-NNN.";
            public const string Error_Note_Length = "Note length must be between {1} and {0}.";
        }

        // ---------------------------------------
        // RBAC / Roles / Claims
        // ---------------------------------------
        public static class Rbac
        {
            public const string Info_NoRolesFound = "No roles found.";
            public const string Info_ManageRoles = "Manage application roles";

            public const string Error_RoleName_Required = "Role name is required.";
            public const string Error_Role_AlreadyExists = "Role already exists.";
            public const string Error_Role_CreateFailed = "Failed to create role: {0}";
            public const string Info_Role_Created = "Role '{0}' created.";

            public const string Error_Role_UpdateFailed = "Failed to update role: {0}";
            public const string Info_Role_Updated = "Role '{0}' updated.";

            public const string Error_RoleName_Missing = "Role name is required.";
            public const string Error_CannotDelete_SysAdmin = "SysAdmin role cannot be deleted.";
            public const string Error_Role_NotFound = "Role not found.";
            public const string Error_Role_DeleteFailed = "Failed to delete role: {0}";
            public const string Info_Role_Deleted = "Role '{0}' deleted.";
            public const string Error_Role_DeleteHasUsers = "Role '{0}' has assigned users and cannot be deleted.";

            public const string Info_Claims_ForRole = "Permission claims for role '{0}'";
            public const string Error_Claim_AddFailed = "Failed to add claim: {0}";
            public const string Error_Claim_RemoveFailed = "Failed to remove claim: {0}";
            public const string Info_Claim_Added = "Claim '{0}' added.";
            public const string Info_Claim_Removed = "Claim '{0}' removed.";

            public const string Info_Permissions_Updated = "Permissions updated for '{0}'.";
            public const string Error_Permissions_UpdateFailed = "Failed to update permissions: {0}";
            public const string Info_Nothing_Changed = "No changes were made.";
            public const string Error_Role_Claims_LoadFailed = "Unable to load claims for the selected role.";
        }

        // ---------------------------------------
        // Fuel (UI-level toasts + internal developer messages)
        // ---------------------------------------
        public static class Fuel
        {
            // UI toasts
            public const string Info_FuelRecorded = "Fuel record created.";
            public const string Error_FuelSaveFailed = "Failed to save fuel record.";

            // Internal / developer-facing
            public const string Error_PoliciesNotConfigured = "Fuel policies are not configured.";
            public const string Error_PolicyMissingFmt = "Fuel policy missing for '{0}'.";
            public const string NotesJoinSeparator = " | ";

            // Optional UI warning shown before commit (soft threshold)
            public const string Warn_FinalBelowSoftFmt =
                "Projected final fuel will be below the recommended reserve ({0} L).";
        }

        // ---------------------------------------
        // UI common labels / toasts
        // ---------------------------------------
        public static class Ui
        {
            public const string Toast_Success = "Success";
            public const string Toast_Error = "Error";

            public const string Btn_Create = "Create";
            public const string Btn_Save = "Save";
            public const string Btn_Delete = "Delete";
            public const string Btn_Back = "Back";
            public const string Btn_Confirm = "Confirm";
            public const string Btn_Cancel = "Cancel";
        }

        // ---------------------------------------
        // TempData keys (unified usage)
        // ---------------------------------------
        public static class TempDataKeys
        {
            public const string Success = "Success";
            public const string Error = "Error";
            public const string Warning = "Warning";
            public const string Info = "Info";
        }

        // ---------------------------------------
        // Generic validation texts (no localization)
        // ---------------------------------------
        public static class Validation
        {
            public const string Msg_FieldRequired = "Field is required.";
            public const string Msg_InvalidFormat = "Invalid format.";
            public const string Msg_RangeViolation = "Value is out of range.";
        }
    }
}