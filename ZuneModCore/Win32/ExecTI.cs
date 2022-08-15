// C# port of https://github.com/nfedera/run-as-trustedinstaller/blob/7c84750dd00f7fed81ae121f09731142f5f0f678/run-as-trustedinstaller/main.cpp

using System;
using Vanara.PInvoke;
using System.Runtime.InteropServices;
using static Vanara.PInvoke.Kernel32;
using static Vanara.PInvoke.AdvApi32;
using static ZuneModCore.Win32.Constants;

namespace ZuneModCore.Win32
{
    public static class ExecTI
    {
		public static uint GetProcessIdByName(string processName)
		{
			SafeHSNAPSHOT hSnapshot = CreateToolhelp32Snapshot(TH32CS.TH32CS_SNAPPROCESS, 0);
			if (hSnapshot.IsInvalid)
			{
				Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
			}

			uint pid = unchecked((uint)-1);
			PROCESSENTRY32 pe = PROCESSENTRY32.Default;
			if (Process32First(hSnapshot, ref pe))
			{
				while (Process32Next(hSnapshot, ref pe))
				{
					if (pe.szExeFile == processName)
					{
						pid = pe.th32ProcessID;
						break;
					}
				}
			}
			else
			{
				CloseHandle(hSnapshot.DangerousGetHandle());
				Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
			}

			if (unchecked((int)pid) == -1)
			{
				CloseHandle(hSnapshot.DangerousGetHandle());
				throw new Exception("Process not found: " + processName);
			}

			CloseHandle(hSnapshot.DangerousGetHandle());
			return pid;
		}

		public static unsafe uint StartTrustedInstallerService()
		{
			SC_HANDLE hSCManager = OpenSCManager(
				null,
				SERVICES_ACTIVE_DATABASE,
				ScManagerAccessTypes.SC_MANAGER_CONNECT | ScManagerAccessTypes.SC_MANAGER_LOCK);
			if (hSCManager == SC_HANDLE.NULL)
			{
				Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
			}

			SC_HANDLE hService = OpenService(
				hSCManager,
				"TrustedInstaller",
				SERVICE_GENERIC_READ | SERVICE_GENERIC_EXECUTE);
			if (hService == SC_HANDLE.NULL)
			{
				CloseServiceHandle(hSCManager);
				Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
			}

			SERVICE_STATUS_PROCESS statusBuffer = default;
			while (QueryServiceStatusEx(
				hService,
				SC_STATUS_TYPE.SC_STATUS_PROCESS_INFO,
                (IntPtr)(&statusBuffer),
                (uint)sizeof(SERVICE_STATUS_PROCESS),
				out var bytesNeeded))
			{
				if (statusBuffer.dwCurrentState == ServiceState.SERVICE_STOPPED)
				{
					if (!StartService(hService, 0, null))
					{
						CloseServiceHandle(hService);
						CloseServiceHandle(hSCManager);
						Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
					}
				}
				if (statusBuffer.dwCurrentState == ServiceState.SERVICE_START_PENDING ||
					statusBuffer.dwCurrentState == ServiceState.SERVICE_STOP_PENDING)
				{
					Sleep(statusBuffer.dwWaitHint);
					continue;
				}
				if (statusBuffer.dwCurrentState == ServiceState.SERVICE_RUNNING)
				{
					CloseServiceHandle(hService);
					CloseServiceHandle(hSCManager);
					return statusBuffer.dwProcessId;
				}
			}
			CloseServiceHandle(hService);
			CloseServiceHandle(hSCManager);
			Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
			return 0;
		}

		public static void ImpersonateSystem()
		{
			var systemPid = GetProcessIdByName("winlogon.exe");
			ACCESS_MASK am = new(ProcessAccess.PROCESS_DUP_HANDLE | ProcessAccess.PROCESS_QUERY_INFORMATION);
			var hSystemProcess = OpenProcess(am, false, systemPid);
			if (hSystemProcess.IsInvalid)
			{
				Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
			}

			if (!OpenProcessToken(
				hSystemProcess,
				(TokenAccess)MAXIMUM_ALLOWED,
				out var hSystemToken))
			{
				CloseHandle(hSystemProcess.DangerousGetHandle());
				Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
			}

			SECURITY_ATTRIBUTES tokenAttributes = new()
            {
                lpSecurityDescriptor = default,
                bInheritHandle = false
            };
            if (!DuplicateTokenEx(
				hSystemToken,
				TokenAccess.TOKEN_ALL_ACCESS,
				tokenAttributes,
				SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
				TOKEN_TYPE.TokenImpersonation,
				out var hDupToken))
			{
				CloseHandle(hSystemToken.DangerousGetHandle());
				Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
			}

			if (!ImpersonateLoggedOnUser(hDupToken))
			{
				CloseHandle(hDupToken.DangerousGetHandle());
				CloseHandle(hSystemToken.DangerousGetHandle());
				Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
			}

			CloseHandle(hDupToken.DangerousGetHandle());
			CloseHandle(hSystemToken.DangerousGetHandle());
		}

		public static SafePROCESS_INFORMATION? CreateProcessAsTrustedInstaller(uint pid, string commandLine)
		{
			TokenManipulator.AddPrivilege(SE_DEBUG_NAME);
			TokenManipulator.AddPrivilege(SE_IMPERSONATE_NAME);
			ImpersonateSystem();

			ACCESS_MASK am = new(ProcessAccess.PROCESS_DUP_HANDLE | ProcessAccess.PROCESS_QUERY_INFORMATION);
			var hTIProcess = OpenProcess(am, false, pid);
			if (hTIProcess.IsInvalid)
			{
				Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
			}

			if (!OpenProcessToken(
				hTIProcess,
				TokenAccess.TOKEN_ALL_ACCESS,
				out var hTIToken))
			{
				CloseHandle(hTIProcess.DangerousGetHandle());
				Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
			}

            SECURITY_ATTRIBUTES tokenAttributes = new()
            {
                lpSecurityDescriptor = PSECURITY_DESCRIPTOR.NULL,
                bInheritHandle = false
            };
            if (!DuplicateTokenEx(
				hTIToken,
				TokenAccess.TOKEN_ALL_ACCESS,
				tokenAttributes,
				SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
				TOKEN_TYPE.TokenImpersonation,
				out var hDupToken))
			{
				CloseHandle(hTIToken.DangerousGetHandle());
				Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
			}

			STARTUPINFO startupInfo = new()
            {
				lpDesktop = "Winsta0\\Default",
			};
			if (!CreateProcessWithTokenW(
				hDupToken,
				ProcessLogonFlags.LOGON_WITH_PROFILE,
				null,
				new System.Text.StringBuilder(commandLine),
				CREATE_PROCESS.CREATE_UNICODE_ENVIRONMENT,
				null,
				null,
				startupInfo,
				out var processInfo))
			{
				Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
			}

			return processInfo;
		}
	}
}
