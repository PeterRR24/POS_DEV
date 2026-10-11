using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RR.HardwareStation.Models
{
	public sealed class BarcodeCode39 : Barcode
	{
		// Token: 0x06000009 RID: 9 RVA: 0x00002280 File Offset: 0x00000480
		public BarcodeCode39()
		{
			this.supportedCharacters.AddRange(from n in Enumerable.Range(32, 60)
											  where n == 32 || (n >= 36 && n <= 38) || (n >= 43 && n <= 57) || (n >= 65 && n <= 90)
											  select n);
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600000A RID: 10 RVA: 0x000022D8 File Offset: 0x000004D8
		public override string FontName
		{
			get
			{
				return "BC C39 2 to 1 Narrow";
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600000B RID: 11 RVA: 0x000022DF File Offset: 0x000004DF
		public override int FontSize
		{
			get
			{
				return 30;
			}
		}

		// Token: 0x0600000C RID: 12 RVA: 0x000022E4 File Offset: 0x000004E4
		public override string Encode(string text)
		{
			//ThrowIf.Null<string>(text, "text");
			StringBuilder stringBuilder = new StringBuilder(text.Length + 2);
			stringBuilder.Append('*');
			foreach (char c in text)
			{
				if (this.supportedCharacters.BinarySearch((int)c) >= 0)
				{
					stringBuilder.Append(c);
				}
			}
			stringBuilder.Replace(' ', ',');
			stringBuilder.Append('*');
			return stringBuilder.ToString();
		}

		// Token: 0x04000005 RID: 5
		private const char StartEndMarker = '*';

		// Token: 0x04000006 RID: 6
		private const string BarcodeFontName = "BC C39 2 to 1 Narrow";

		// Token: 0x04000007 RID: 7
		private const int BarcodeFontSize = 30;

		// Token: 0x04000008 RID: 8
		private readonly List<int> supportedCharacters = new List<int>(100);
	}
}
