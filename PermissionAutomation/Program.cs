using System;

namespace PermissionAutomation
{
    public class Program
    {

        static void Main(string[] args)
        {
            PermissionAutomationService _permissionAutomationService = new PermissionAutomationService();
            _permissionAutomationService.GeneratePermission(typeof(IListOfAbstractMethods));
        }
    }
}
