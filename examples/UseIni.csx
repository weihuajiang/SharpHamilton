
using System.Runtime.InteropServices;

[DllImport("kernel32")]
internal static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
[DllImport("kernel32")]
internal static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

public void WriteValue(string configFile, string Section, string Key, string Value)
{
    WritePrivateProfileString(Section, Key, Value, configFile);
}
public string ReadValue(string configFile, string Section, string Key)
{
    StringBuilder temp = new StringBuilder(255);
    int i = GetPrivateProfileString(Section, Key, "", temp,
                                255, configFile);
    if (i <= 0)
        return "";
    String Value = temp.ToString();
    return Value;
}
string file="config.ini";
WriteValue(file, "config","key1", "123");
WriteValue(file, "config", "key2","456");

int key1=int.Parse(ReadValue(file, "config", "key1"));
int key2=int.Parse(ReadValue(file, "config", "key2"));
Console.WriteLine($"key1 is {key1}");
Console.WriteLine($"key2 is {key2}");