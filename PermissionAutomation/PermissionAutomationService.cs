using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PermissionAutomation
{
    public class PermissionAutomationService
    {
        // CREATES A PERMISSION CALLING METHOD
        public void GeneratePermission(string nameSpace = "NAMESPACE_GOES_HERE", string className = "PERMISSION_CLASS")
        {
            Permission permission = new Permission();
            string unchangedClassName = className;

            // GET CALLING METHOD NAME
            StackTrace stackTrace = new StackTrace();
            string callingMethodName = stackTrace.GetFrame(1).GetMethod().Name;

            #region HANDLE REPEATED FILE NAME - INACTIVE
            /// HANDLE REPEATED FILE NAME - 
            /// INACTIVE BECAUSE IT CREATES 
            /// A NEW FILE EVERYTIME
            // className = HandleRepeatedFileName(className, unchangedClassName);
            #endregion

            permission.permissionName = PermissionCreator(callingMethodName).permissionName;
            permission.fullPermissionLine = PermissionCreator(callingMethodName).fullPermissionLine;

            WritePermissionsToClass(nameSpace, className, permission.fullPermissionLine, permission.permissionName);
        }
        // TAKES IN INTERFACE AND CREATES A PERMISSION FOR ALL ABSTRACT METHODS IN IT
        public void GeneratePermission(Type type, string nameSpace = "NAMESPACE_GOES_HERE", string className = "PERMISSION_CLASS")
        {
            Permission permission = new Permission();
            string unchangedClassName = className;

            #region HANDLE REPEATED FILE NAME - INACTIVE
            /// HANDLE REPEATED FILE NAME - 
            /// INACTIVE BECAUSE IT CREATES 
            /// A NEW FILE EVERYTIME
            // className = HandleRepeatedFileName(className, unchangedClassName);
            #endregion

            // CREATES PERMISSION FOR EACH ABSTRACT METHOD
            foreach (var method in type.GetMethods())
            {
                if (method.IsAbstract)
                {
                    permission.permissionName = PermissionCreator(method.Name).permissionName;
                    permission.fullPermissionLine = PermissionCreator(method.Name).fullPermissionLine;
                    WritePermissionsToClass(nameSpace, className, permission.fullPermissionLine, permission.permissionName);
                }
            }
        }
        // WRITES THE GENERATED PERMISSIONS TO A CLASS FILE (.cs)
        private void WritePermissionsToClass(string nameSpace,  string className,  string fullPermissionLine, string permissionName)
        {
            string projectDir = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + '/';
            string[] allLines = { };
            // READ CLASS IF IT EXISTS
            if (File.Exists($"{projectDir}{className}.cs"))
            {
                allLines = File.ReadAllLines($"{projectDir}{className}.cs");
                // IF PERMISSION NAME IS ALREADY USED, STOP!
                foreach (var line in allLines)
                {
                    if (line.Contains(permissionName))
                    {
                        return;
                    }
                }
            }

            #region CODE CONSTRUCTION
            // CONSTRUCT CODE TO BE WRITTEN IN CLASS
            string code =
                "using System.ComponentModel.DataAnnotations;\n" +
                "using System.Text.Json.Serialization;\n\n" +
                $"namespace {nameSpace}\n\n" +
                "{\n" +
                    $"\tpublic class {className}\n" +
                    "\t{\n" +
                        "\t\t[Key]\n" +
                        "\t\t[JsonIgnore]\n" +
                        "\t\tpublic int Id { get; set; }\n" +
                        $"{fullPermissionLine}" +
                    "\t}\n" +
                "}";
            #endregion

            #region CODE WRITER
            // WRITE CODE TO CLASS
            if (!File.Exists($"{projectDir}{className}.cs") || new FileInfo($"{projectDir}{className}.cs").Length == 0)
            {
                using (StreamWriter pen = File.AppendText($"{projectDir}{className}.cs"))
                {
                    pen.WriteLine(code);
                }
            }
            else
            {
                using (StreamWriter pen = new StreamWriter($"{projectDir}{className}.cs"))
                {
                    for (int currentLine = 1; currentLine <= allLines.Count(); ++currentLine)
                    {
                        if (currentLine == allLines.Count() - 2)
                        {
                            pen.WriteLine(fullPermissionLine);
                        }
                        else
                        {
                            pen.WriteLine(allLines[currentLine - 1]);
                        }
                    }
                }
            }
            #endregion
        }
        // RETURNS PERMISSION NAME AND FULL PERMISSION LINE BASED ON METHOD NAME PASSED
        private Permission PermissionCreator(string methodName)
        {
            Permission permission = new Permission();
            // WRITE PERMISSION BASED ON METHOD NAME
            if (methodName.ToLower().StartsWith("get") && methodName.ToLower().EndsWith("s"))
            {
                permission.permissionName = $"CanView{methodName.Replace("Get", "")}";
            }
            else if (methodName.ToLower().StartsWith("get") && !methodName.ToLower().EndsWith("s"))
            {
                permission.permissionName = $"CanView{methodName.Replace("Get", "")}s";
            }
            else
            {
                permission.permissionName = $"Can{methodName}";
            }
            // APPEND PERMISSION TO TEXT FILE FOR LATER EXTRACTION
            using (StreamWriter pen = new StreamWriter("permissions.txt"))
            {
                pen.WriteLine($"\t\tpublic bool {permission.permissionName}" + " { get; set; } = false;");
            }
            permission.fullPermissionLine = File.ReadAllText("permissions.txt");
            File.Delete("permissions.txt");
            return permission;
        }
        // REPEATED FILE NAME HANDLER - INACTIVE
        private string HandleRepeatedFileName(string className, string unchangedClassName)
        {
            int inc = 0;
            // CHECK IF THERE IS A FILE ALREADY EXISTING WITH THAT NAME
            bool existingFile = Directory.GetParent(Environment.CurrentDirectory).Parent.GetFiles($"{className}.cs", SearchOption.AllDirectories).Any();
            // IF EXISTING FILE IS TRUE, THEN ADD NUMBER TO DISTINGUISH
            if (existingFile)
            {
                while (existingFile)
                {
                    existingFile = Directory.GetParent(Environment.CurrentDirectory).Parent.GetFiles($"{className}.cs", SearchOption.AllDirectories).Any();
                    className = unchangedClassName + "Permission" + inc.ToString();
                    inc++;
                    existingFile = Directory.GetParent(Environment.CurrentDirectory).Parent.GetFiles($"{className}.cs", SearchOption.AllDirectories).Any();
                }
            }
            return className;
        }
    }
}
