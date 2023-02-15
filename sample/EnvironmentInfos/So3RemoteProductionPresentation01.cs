using static System.Net.WebRequestMethods;

namespace Sample.EnvironmentInfos
{
    internal class So3RemoteProductionPresentation01 : So3_DB_Production0007
    {
        /// <summary>
        /// So3_Production0007
        /// https://webapi.presentation-so3-01.de1.sodrei.com/
        /// </summary>
        public override string ApiUrl => "https://webapi.presentation-so3-01.de1.sodrei.com/";
        public override string Username => "ApiUser";
        public override string Password => "cD7et2pwZrVaGVhTQuqz";  // stored also in KeePass

        public override string ExcelImportFacilityPath => "SO3/Projects/Demo/Standard NoName 2023/Standard NoName 2023/F1";
        public override string ExcelImportPageName => "99";
    }
}