using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PermissionAutomation
{
    public class PermissionAutomationService
    {
        /// <summary>
        /// 
        /// TAKES `NAMESPACE` AND `CLASS NAME` AS PARAMS
        /// TO BE USED FOR THE FILE TO BE CREATED 
        /// IF `NAMESPACE` OR `CLASS NAME` ARE NULL OR EMPTY
        /// IT USES THE `NAMESPACE` AND `CLASS NAME` OF THE
        /// CALLING METHOD
        /// 
        /// </summary>
        public void GeneratePermission(string nameSpace = "NAMESPACE_GOES_HERE", string className = "PERMISSION_CLASS")
        {
            string permissionName = "";
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

            #region PERMISSION NAME CREATOR
            // WRITE PERMISSION BASED ON METHOD NAME
            if (callingMethodName.ToLower().StartsWith("get") && callingMethodName.ToLower().EndsWith("s"))
            {
                permissionName = $"CanView{callingMethodName.Replace("Get", "")}";
            }
            else if (callingMethodName.ToLower().StartsWith("get") && !callingMethodName.ToLower().EndsWith("s"))
            {
                permissionName = $"CanView{callingMethodName.Replace("Get", "")}s";
            }
            else
            {
                permissionName = $"Can{callingMethodName}";
            }

            // APPEND PERMISSION TO TEXT FILE FOR LATER EXTRACTION
            using (StreamWriter pen = new StreamWriter("permissions.txt"))
            {
                pen.WriteLine($"\t\tpublic bool {permissionName}" + " { get; set; } = false;");
            }
            #endregion

            // EXTRACT FROM FILE TO WRITE TO CS FILE
            string fullPermissionLine = File.ReadAllText("permissions.txt");
            WritePermissionsToClass(nameSpace, className, fullPermissionLine, permissionName);
        }
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
        // REPEATED FILE NAME HANDLER
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
