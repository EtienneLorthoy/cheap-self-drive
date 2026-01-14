using System.Runtime.InteropServices;
using System.Text;

namespace SelfDriveInstaller.Services;

internal class PasswordManager
{
    private const string KeyPrefix = "CheapSelfDrive:";
    private const int CRED_TYPE_GENERIC = 1;
    private const int CRED_PERSIST_LOCAL_MACHINE = 2;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CREDENTIAL
    {
        public int Flags;
        public int Type;
        public string TargetName;
        public string Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public int CredentialBlobSize;
        public IntPtr CredentialBlob;
        public int Persist;
        public int AttributeCount;
        public IntPtr Attributes;
        public string TargetAlias;
        public string UserName;
    }

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] int flags);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredRead(string target, int type, int reservedFlag, out IntPtr credentialPtr);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool CredDelete(string target, int type, int reservedFlag);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern void CredFree([In] IntPtr cred);

    internal void StorePassword(string identifier, string password)
    {
        try
        {
            var targetName = KeyPrefix + identifier;
            var passwordBytes = Encoding.Unicode.GetBytes(password);
            var credential = new CREDENTIAL
            {
                Type = CRED_TYPE_GENERIC,
                TargetName = targetName,
                CredentialBlob = Marshal.AllocHGlobal(passwordBytes.Length),
                CredentialBlobSize = passwordBytes.Length,
                Persist = CRED_PERSIST_LOCAL_MACHINE,
                UserName = identifier
            };

            try
            {
                Marshal.Copy(passwordBytes, 0, credential.CredentialBlob, passwordBytes.Length);

                if (!CredWrite(ref credential, 0))
                {
                    throw new InvalidOperationException($"Failed to write credential: {Marshal.GetLastWin32Error()}");
                }
            }
            finally
            {
                if (credential.CredentialBlob != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(credential.CredentialBlob);
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to store password for {identifier}: {ex.Message}", ex);
        }
    }

    internal string RetrievePassword(string identifier)
    {
        try
        {
            var targetName = KeyPrefix + identifier;
            IntPtr credPtr = IntPtr.Zero;

            try
            {
                if (!CredRead(targetName, CRED_TYPE_GENERIC, 0, out credPtr))
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error == 1168) // ERROR_NOT_FOUND
                    {
                        return string.Empty;
                    }
                    throw new InvalidOperationException($"Failed to read credential: {error}");
                }

                var credential = Marshal.PtrToStructure<CREDENTIAL>(credPtr);
                var passwordBytes = new byte[credential.CredentialBlobSize];
                Marshal.Copy(credential.CredentialBlob, passwordBytes, 0, credential.CredentialBlobSize);
                return Encoding.Unicode.GetString(passwordBytes);
            }
            finally
            {
                if (credPtr != IntPtr.Zero)
                {
                    CredFree(credPtr);
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve password for {identifier}: {ex.Message}", ex);
        }
    }

    internal bool HasStoredPassword(string identifier)
    {
        try
        {
            var targetName = KeyPrefix + identifier;
            IntPtr credPtr = IntPtr.Zero;

            try
            {
                if (CredRead(targetName, CRED_TYPE_GENERIC, 0, out credPtr))
                {
                    return true;
                }

                int error = Marshal.GetLastWin32Error();
                if (error == 1168) // ERROR_NOT_FOUND
                {
                    return false;
                }

                // Other errors - return false to be safe
                return false;
            }
            finally
            {
                if (credPtr != IntPtr.Zero)
                {
                    CredFree(credPtr);
                }
            }
        }
        catch
        {
            return false;
        }
    }

    internal void DeletePassword(string identifier)
    {
        try
        {
            var targetName = KeyPrefix + identifier;
            if (!CredDelete(targetName, CRED_TYPE_GENERIC, 0))
            {
                int error = Marshal.GetLastWin32Error();
                if (error != 1168) // Ignore ERROR_NOT_FOUND
                {
                    throw new InvalidOperationException($"Failed to delete credential: {error}");
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete password for {identifier}: {ex.Message}", ex);
        }
    }
}