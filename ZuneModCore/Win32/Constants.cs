using static Vanara.PInvoke.AdvApi32;

namespace ZuneModCore.Win32
{
    public static class Constants
    {
		public const string SERVICES_ACTIVE_DATABASE = "ServicesActive";
		public const string SE_DEBUG_NAME = "SeDebugPrivilege";
		public const string SE_IMPERSONATE_NAME = "SeImpersonatePrivilege";

		public const ServiceAccessTypes SERVICE_GENERIC_READ = ServiceAccessTypes.SERVICE_QUERY_CONFIG
			| ServiceAccessTypes.SERVICE_QUERY_STATUS | ServiceAccessTypes.SERVICE_INTERROGATE
			| ServiceAccessTypes.SERVICE_ENUMERATE_DEPENDENTS;
		public const ServiceAccessTypes SERVICE_GENERIC_EXECUTE = ServiceAccessTypes.SERVICE_START
			| ServiceAccessTypes.SERVICE_STOP | ServiceAccessTypes.SERVICE_PAUSE_CONTINUE
			| ServiceAccessTypes.SERVICE_USER_DEFINED_CONTROL;

		public const int DELETE = 0x00010000;
		public const int READ_CONTROL = 0x00020000;
		public const int WRITE_DAC = 0x00040000;
		public const int WRITE_OWNER = 0x00080000;
		public const int SYNCHRONIZE = 0x00100000;
		public const int STANDARD_RIGHTS_EXECUTE = READ_CONTROL;
		public const int STANDARD_RIGHTS_ALL = DELETE | READ_CONTROL | WRITE_DAC | WRITE_OWNER | SYNCHRONIZE;
		public const int MAXIMUM_ALLOWED = 0x02000000;
	}
}
