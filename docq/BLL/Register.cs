using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace docq.BLL
{
    class Register
    {
        /// <summary>
        /// for auto start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void cb_zddl_CheckedChanged(object sender, EventArgs e)    
        {          
            if (cb_zddl.Checked)            
            {                
                MessageBox.Show("设置开机自启动，需要修改注册表", "提示");                
                string path = Application.ExecutablePath;                
                RegistryKey rk = Registry.LocalMachine;                
                RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");               
                rk2.SetValue("Form1", path);                
                rk2.Close();                
                rk.Close();            
            }            
            else             
            {                
                MessageBox.Show("取消开机自启动，需要修改注册表", "提示");                
                string path = Application.ExecutablePath;               
                RegistryKey rk = Registry.LocalMachine;               
                RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");             
                rk2.DeleteValue("Form1", false);              
                rk2.Close();               
                rk.Close();           
            }          
        }*/
    }
}
