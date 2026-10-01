using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovinApp.Shared
{
	public class ImageFile
	{
		public string id { get; set; } = "";
        public string? base64data { get; set; }
		public string? contentType { get; set; }
		public string? fileName { get; set; }
	}
}
