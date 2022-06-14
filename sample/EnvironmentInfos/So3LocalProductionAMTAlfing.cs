namespace Sample.EnvironmentInfos
{
    internal class So3LocalProductionAMTAlfing : So3_DB_Production0007
    {
        ////public override string ApiUrl => "http://localhost:5000";
        public override string ApiUrl => "https://webapi.amt-alfing-01.de1.sodrei.com";
        public override string Username => "ImportUser";
        public override string Password => "123";

        public override string ExcelImportFacilityPath => "SO3/Projects/Alfing/Alfing/F1";
        public override string ExcelImportPageName => "Import";
    }
}