using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Bar
{
    public partial class INVCheck : Form
    {
        public INVCheck()
        {
            InitializeComponent();
            InstallHook();

        }
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private string barcodeData = "";

        private IntPtr hookHandle = IntPtr.Zero;

        private IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    char key = (char)vkCode;

                    if (key == '\r')
                    {
                        string cleanedBarcodeData = barcodeData.Trim();
                        cleanedBarcodeData = RemoveSpecialCharacters(cleanedBarcodeData);

                        using (SqlConnection conn = ConnectDB.connectDB_TEAMDB())
                        {
                            using (SqlCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = $"SELECT * FROM [DB_QA].[dbo].[Inverter] where barcode=@barcode"; //수정할 것
                                cmd.Parameters.AddWithValue("@barcode", cleanedBarcodeData);
                                try
                                {
                                    using (SqlDataReader dr = cmd.ExecuteReader())
                                    {
                                        if (dr.Read())
                                        {
                                            UninstallHook();
                                            ComponentCheck componentCheck = new ComponentCheck(dr[0].ToString(), dr[1].ToString());
                                            componentCheck.ShowDialog();
                                            this.Hide();
                                        }
                                     
                                    }

                                }
                                catch (Exception ex)
                                {
                                  
                                    Log.writeLog(ex.ToString());
                                }
                            }
                        }
                        barcodeData = "";
                    }
                    else
                    {
                        barcodeData += key.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.writeLog(ex.ToString());
            }
            return CallNextHookEx(hookHandle, nCode, wParam, lParam);
        }

        private string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if (c >= 32 && c <= 126)
                {
                    sb.Append(c);
                }

            }
            return sb.ToString();
        }

        // Hook Install
        private void InstallHook()
        {
            LowLevelKeyboardProc proc = HookProc;
            IntPtr moduleHandle = GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);
            hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, proc, moduleHandle, 0);
        }

        // Hook Uninstall
        private void UninstallHook()
        {
            if (hookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(hookHandle);
                hookHandle = IntPtr.Zero;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            UninstallHook();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 바코드 수정할 수 잇는 창 뛰우기
      
        }
    }
}