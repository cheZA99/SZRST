using System.Collections.Generic;
using SZRST.Domain.Constants;

//TODO
public static class Permissions
{
	public const string ViewReservations = "ViewReservations";
	public const string ManageLocations = "ManageLocations";
	public const string ManageResources = "ManageResources";
}

public static class RolePermissions
{
	public static readonly Dictionary<string, string[]> Map = new()
	{
		[Roles.Admin] = new[]
	    {
		  Permissions.ViewReservations,
		  Permissions.ManageLocations,
		  Permissions.ManageResources
	   },
		[Roles.Uposlenik] = new[]
	    {
		  Permissions.ViewReservations
	   }
	};
}