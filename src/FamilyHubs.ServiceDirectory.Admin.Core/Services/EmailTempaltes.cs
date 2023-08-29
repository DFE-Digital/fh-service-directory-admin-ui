namespace FamilyHubs.ServiceDirectory.Admin.Core.Services
{
    public static class EmailTempaltes
    {
        //Account Permission Added
        public static readonly string LaPermissionAddedDualRoleEmailTemplateId = "eee5cb96-8387-4095-a942-dfe4885b4db3";
        public static readonly string LaPermissionAddedManagerEmailTemplateId = "cc0ba892-c9ae-4990-a07d-f38c4062fd59";
        public static readonly string LaPermissionAddedProfessionalEmailTemplateId = "5074a730-74bc-42fd-ad5b-d1100d7f11ca";

        public static readonly string VcsPermissionAddedDualRoleEmailTemplateId = "74acfbed-428e-49d6-b35c-9b6279b4b2ee";
        public static readonly string VcsPermissionAddedManagerEmailTemplateId = "2bfb0fdd-374f-478f-b842-973466a96efe";
        public static readonly string VcsPermissionAddedProfessionalEmailTemplateId = "6d5e73b8-5db8-497e-892f-6edd7e5506ec";

        //LA Permissions Changed 
        public static readonly string LaPermissionChangedFromLaManagerToLaProfessional = "5fd7a0e7-1126-4c1d-9626-026689ec1c7e";
        public static readonly string LaPermissionChangedFromLaProfessionalToLaManager = "8533816f-0cd3-4ecb-9725-f41fd526ab73";
        public static readonly string LaPermissionChangedFromLaManagerToLaDualRole = "b79fe749-b99e-4b4a-9bfc-797f662c20cb";
        public static readonly string LaPermissionChangedFromLaProfessionalToLaDualRole = "684eaef7-d234-4e87-b41c-c6764cd5f01a";
        public static readonly string LaPermissionChangedFromLaDualRoleToLaManager = "0ee9cafc-2d8d-4649-bd34-179188e67ad4";
        public static readonly string LaPermissionChangedFromLaDualRoleToLaProfessional = "f3f1ec29-3048-4f37-b285-630d69d931c0";

        //VCS Permissions Changed 
        public static readonly string VcsPermissionChangedFromVcsManagerToVcsProfessional = "d8b02428-3bd8-477a-a052-8d522ec6126f";
        public static readonly string VcsPermissionChangedFromVcsProfessionalToVcsManager = "a9dbc333-dfbb-475c-b89f-bd446f8d9724";
        public static readonly string VcsPermissionChangedFromVcsManagerToVcsDualRole = "7c6e253d-8492-4c06-9164-dc244375d961";
        public static readonly string VcsPermissionChangedFromVcsProfessionalToVcsDualRole = "b39e9655-bfce-42b2-8f5e-09f73c372282";
        public static readonly string VcsPermissionChangedFromVcsDualRoleToVcsManager = "e7349937-8d55-43d8-8d3c-3ebbcd3652d5";
        public static readonly string VcsPermissionChangedFromVcsDualRoleToVcsProfessional = "ffbd4db9-3b9a-4d99-9c7e-a53b68e754f2";

        //Email Updated
        public static readonly string LaEmailUpdatedForLaManager = "e77a15a5-b7d8-43cf-9c79-8843e8269d54";
        public static readonly string LaEmailUpdatedForLaProfessional = "9d113e50-5149-495a-b8f1-f8c32bb6766a";
        public static readonly string LaEmailUpdatedForLaDualRole = "6d9e878b-2d28-4358-9aa9-b2f6c3014549";
        public static readonly string VcsEmailUpdatedForVcsManager = "7f69bcbb-6d4d-43b4-b79a-f99e163e5e60";
        public static readonly string VcsEmailUpdatedForVcsProfessional = "d85e10ed-084d-481f-898d-25421d4d3a96";
        public static readonly string VcsEmailUpdatedForVcsDualRole = "485bcd7d-396f-4d86-b4d6-6567168cb8f1";


        //Account Deleted 
        public static readonly string LaAccountDeletedForLaManager = "0808987f-be86-4353-bf41-6e4aedfc38c6";
        public static readonly string LaAccountDeletedForLaProfessional = "b4f35ec4-189d-48fa-961b-5e5cb47e5d4d";
        public static readonly string LaAccountDeletedForLaDualRole = "ce47a6c7-700e-40b1-933f-63e28072e875";
        public static readonly string VcsAccountDeletedForVcsManager = "12307140-879e-4262-a9e6-08d7c89eab68"; //Replace this "38bf84ea-5b86-486f-b133-2e84bcd82217"
        public static readonly string VcsAccountDeletedForVcsProfessional = "bf1033e4-027f-458e-9aa9-1a84d1b69da5";
        public static readonly string VcsAccountDeletedForVcsDualRole = "a3eabb9f-5916-40ae-88cf-f21b43eb9fa5";
    }
}
