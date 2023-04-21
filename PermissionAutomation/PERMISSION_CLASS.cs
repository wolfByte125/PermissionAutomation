using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NAMESPACE_GOES_HERE

{
	public class PERMISSION_CLASS
	{
		[Key]
		[JsonIgnore]
		public int Id { get; set; }
		public bool CanViewMain { get; set; } = false;
		public bool CanCreateMain { get; set; } = false;
		public bool CanUpdateMain { get; set; } = false;
		public bool CanDeleteMain { get; set; } = false;
		public bool CanMain { get; set; } = false;

	}
}
