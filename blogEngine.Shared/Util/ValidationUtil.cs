using System;
using System.Text.RegularExpressions;

namespace Foundation.Common.Util
{
	/// <summary>
	/// Summary description for ValidationUtil.
	/// </summary>
	[Serializable]
    public class ValidationUtil
	{
		private ValidationUtil() {}

        [Serializable]
        public struct RegularExpressions {
            public const string Alpha = @"[A-Za-z]";
            public const string Numeric = @"[0-9]";
            public const string AlphaNumeric = @"[A-Za-z0-9]";
            public const string Phone = @"^((\(\d{3}\) ?)|(\d{3}-))?\d{3}-\d{4}$";
            public const string Street = @"[A-Za-z0-9]";
            public const string City = @"[A-Za-z]";
            public const string PostalCode = @"[A-Za-z0-9]";
            public const string Email = @"^\s*([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})\s*$";
            public const string MultipleEmailAddresses = @"^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})((,\s*|;\s*|\s+)([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}))*$";
            public const string EmptyOrMutipleEmailAddress = @"^\s*$|^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})((,\s*|;\s*|\s+)([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}))*$";
            public const string RequiredField = @"\S";
            public const string PositiveInteger = @"^[1-9]\d*$";
            //=======================================================================================================================
            // Issue: 080429-000021                 Dev: tlupan
            // Date: 05/05/2008                     Type: Bug
            // Commment: Modified regular expression for restricted attachment file types to match those restricted by Outlook 2007.
            //  Also added restriction for attachments with no extension.
            //=======================================================================================================================
            public const string RestrictedAttachmentFileTypes = @"[.].+$(?<!([.]|ade|adp|app|asp|bas|bat|cer|chm|cmd|com|cpl|crt|csh|der|exe|fxp|hlp|hta|inf|ins|isp|its|js|jse|ksh|lnk|mad|maf|mag|mam|maq|mar|mas|mat|mau|mav|maw|mda|mdb|mde|mdt|mdw|mdz|msc|msh|msh1|msh2|mshxml|msh1xml|msh2xml|msi|msp|mst|ops|pcd|pif|plg|prf|prg|ps1|ps1xml|ps2|ps2xml|psc1|psc2|pst|reg|scf|scr|sct|shb|shs|tmp|url|vb|vbe|vbs|vsmacros|vsw|ws|wsc|wsf|wsh|xnk)$)";
            public const string Date = @"^(?:0?[1-9]|1[012])/(?:0?[1-9]|[12][0-9]|3[01])/(?:19|20)\d\d$";
        }


		public static Boolean RegExHasMatch(string regularExpression, string input) {
			Regex reg = new Regex(regularExpression);
			return reg.IsMatch(input);
		}
	}
}
