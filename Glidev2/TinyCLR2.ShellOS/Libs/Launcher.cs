using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
//using IConnector;

namespace ShellOS.Tool
{
    
    public class Launcher
    {
        #region Launcher
        public static void ExecApp(string Filename)
        {
            
            AppDomain AD = AppDomain.CreateDomain("ATOM4_NETMF");
            var ILauncher = (IApplicationLauncher)AD.CreateInstanceAndUnwrap(typeof(IApplicationLauncher).Assembly.FullName, typeof(MyApplication).FullName);
            if (ILauncher.LoadApp(Filename, GetDependencies(Filename)))
                Debug.WriteLine("PROGRAM COMPLETE");
            else
                Debug.WriteLine("LAUNCH FAILED");
            AppDomain.Unload(AD);
        }
        private static string[] GetDependencies(string Filename)
        {
            string[] dep = null;
            string sDir = Filename.Substring(0, Filename.LastIndexOf("\\") + 1);

            // Check for app.config
            if (File.Exists(sDir + "app.config"))
            {
                string[] sFiles = new string(UTF8Encoding.UTF8.GetChars(File.ReadAllBytes(sDir + "app.config"))).Split(',');
                dep = new string[sFiles.Length];
                for (int f = 0; f < sFiles.Length; f++)
                    dep[f] = sDir + sFiles[f].Trim();
            }
            else
            {
                // Load dependencies
                string[] s = Directory.GetFiles(sDir);
                for (int d = 0; d < s.Length; d++)
                {
                    try
                    {
                        if (Path.GetExtension(s[d]).ToLower() == ".pe" && s[d].ToLower() != Filename.ToLower())
                        {
                            if (dep == null)
                                dep = new string[] { s[d] };
                            else
                            {
                                string[] tmp = new string[dep.Length + 1];
                                Array.Copy(dep, tmp, dep.Length);
                                tmp[tmp.Length - 1] = s[d];
                                dep = tmp;
                                tmp = null;
                            }
                        }
                    }
                    catch (Exception) { Debug.WriteLine("Dependency failed: " + s[d]); }
                }
            }

            return dep;
        }

        #endregion
    }
    public interface IApplicationLauncher
    {

        #region Methods

        bool LoadApp(string filename, string[] Dependencies);

        #endregion

    }

    [Serializable]
    public class MyApplication : MarshalByRefObject, IApplicationLauncher
    {

        public bool LoadApp(string filename, string[] Dependencies)
        {
            int j;

            // Load Application
            Assembly asm;

            // Dependencies
            if (Dependencies != null)
            {
                for (int d = 0; d < Dependencies.Length; d++)
                    Assembly.Load(File.ReadAllBytes(Dependencies[d]));
            }

            try
            {
                asm = Assembly.Load(File.ReadAllBytes(filename));
                if (asm == null)
                    return false;

                Type[] t = asm.GetTypes();
                MethodInfo[] m;
                for (int i = 0; i < t.Length; i++)
                {
                    Debug.WriteLine(t[i].BaseType.FullName + ":" + t[i].Name);
                    if (t[i].BaseType.FullName == "System.Object")
                    {
                        try
                        {
                            m = t[i].GetMethods();
                            if (m != null)
                            {
                                for (j = 0; j < m.Length; j++)
                                {
                                    if (m[j].Name == "Main")

                                    {
                                        m[j].Invoke(this, null);
                                        return true;
                                    }
                                    else
                                        Debug.WriteLine(m[j].Name);
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                    else
                    {
                        try
                        {
                            m = t[i].GetMethods();
                            if (m != null)
                            {
                                for (j = 0; j < m.Length; j++)
                                {
                                    Debug.WriteLine(m[j].Name);
                                    if (m[j].Name == "Main")
                                    {
                                        m[j].Invoke(this, null);
                                        return true;
                                    }
                                    else
                                        Debug.WriteLine(m[j].Name);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message + "_" + ex.StackTrace);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Launch Application Error: " + ex.ToString() + "_" + ex.StackTrace);
            }

            return false;
        }

    }
    
    /*
     public class Launcher
    {
        #region Launcher
        public static void ExecApp(string Filename, DeviceController ctl)
        {
            
            AppDomain AD = AppDomain.CreateDomain("ATOM4_NETMF");
            var ILauncher = (IApplicationLauncher)AD.CreateInstanceAndUnwrap(typeof(IApplicationLauncher).Assembly.FullName, typeof(MyApplication).FullName);
            if (ILauncher.LoadApp(Filename, GetDependencies(Filename), ctl))
                Debug.WriteLine("PROGRAM COMPLETE");
            else
                Debug.WriteLine("LAUNCH FAILED");
            AppDomain.Unload(AD);
        }
        private static string[] GetDependencies(string Filename)
        {
            string[] dep = null;
            string sDir = Filename.Substring(0, Filename.LastIndexOf("\\") + 1);

            // Check for app.config
            if (File.Exists(sDir + "app.config"))
            {
                string[] sFiles = new string(UTF8Encoding.UTF8.GetChars(File.ReadAllBytes(sDir + "app.config"))).Split(',');
                dep = new string[sFiles.Length];
                for (int f = 0; f < sFiles.Length; f++)
                    dep[f] = sDir + sFiles[f].Trim();
            }
            else
            {
                // Load dependencies
                string[] s = Directory.GetFiles(sDir);
                for (int d = 0; d < s.Length; d++)
                {
                    try
                    {
                        if (Path.GetExtension(s[d]).ToLower() == ".pe" && s[d].ToLower() != Filename.ToLower())
                        {
                            if (dep == null)
                                dep = new string[] { s[d] };
                            else
                            {
                                string[] tmp = new string[dep.Length + 1];
                                Array.Copy(dep, tmp, dep.Length);
                                tmp[tmp.Length - 1] = s[d];
                                dep = tmp;
                                tmp = null;
                            }
                        }
                    }
                    catch (Exception) { Debug.WriteLine("Dependency failed: " + s[d]); }
                }
            }

            return dep;
        }

        #endregion
    }
    public interface IApplicationLauncher
    {

        #region Methods

        bool LoadApp(string filename, string[] Dependencies, DeviceController param);

        #endregion

    }

    [Serializable]
    public class MyApplication : MarshalByRefObject, IApplicationLauncher
    {

        public bool LoadApp(string filename, string[] Dependencies, DeviceController param)
        {
            int j;

            // Load Application
            Assembly asm;

            // Dependencies
            if (Dependencies != null)
            {
                for (int d = 0; d < Dependencies.Length; d++)
                    Assembly.Load(File.ReadAllBytes(Dependencies[d]));
            }

            try
            {
                asm = Assembly.Load(File.ReadAllBytes(filename));
                if (asm == null)
                    return false;

                Type[] t = asm.GetTypes();
                MethodInfo[] m;
                for (int i = 0; i < t.Length; i++)
                {
                    Debug.WriteLine(t[i].BaseType.FullName+":"+t[i].Name);
                    if (t[i].BaseType.FullName == "System.Object")
                    {
                        try
                        {
                            m = t[i].GetMethods();
                            if (m != null)
                            {
                                for (j = 0; j < m.Length; j++)
                                {
                                    //if (m[j].Name == "Main")
                                    if (m[j].Name == "RunApp")
                                    {
                                        m[j].Invoke(this,new object[] { param });
                                        return true;
                                    }
                                    else
                                        Debug.WriteLine(m[j].Name);
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                    else
                    {
                        try {
                            m = t[i].GetMethods();
                            if (m != null)
                            {
                                for (j = 0; j < m.Length; j++)
                                {
                                    Debug.WriteLine(m[j].Name);
                                    //if (m[j].Name == "Main")
                                    if (m[j].Name == "RunApp")
                                    {
                                        m[j].Invoke(this, new object[] { param });
                                        return true;
                                    }
                                    else
                                        Debug.WriteLine(m[j].Name);
                                }
                            }
                        }
                        catch (Exception ex) { Debug.WriteLine(ex.Message + "_" + ex.StackTrace);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Launch Application Error: " + ex.ToString()+"_"+ex.StackTrace);
            }

            return false;
        }
        
    }*/
    

}
