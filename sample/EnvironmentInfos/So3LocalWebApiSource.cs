﻿namespace Sample.EnvironmentInfos
{
    internal class So3LocalWebApiSource : So3_DB_WebApiSource
    {
        public override string ApiUrl => "http://localhost:5000";
        public override string Username => string.Empty;
        public override string Password => string.Empty;
    }
}