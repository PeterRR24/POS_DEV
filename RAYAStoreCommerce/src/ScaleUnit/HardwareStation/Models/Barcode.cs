using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace RR.HardwareStation.Models
{
	public abstract class Barcode
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000003 RID: 3
		public abstract string FontName { get; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000004 RID: 4
		public abstract int FontSize { get; }

		// Token: 0x06000005 RID: 5
		public abstract string Encode(string text);

		// Token: 0x06000006 RID: 6 RVA: 0x00002068 File Offset: 0x00000268
		public virtual Image Create(string text, float dpiX = 96f, float dpiY = 96f)
		{
			Bitmap bitmap = null;
			using (Font font = new Font(this.FontName, (float)this.FontSize))
			{
				if (font.Name.Equals(font.OriginalFontName, StringComparison.Ordinal))
				{
					using (Font font2 = new Font("Courier New", 10f))
					{
						try
						{
							text = this.Encode(text);
							SizeF textSizeF = Barcode.GetTextSizeF(text, font, dpiX, dpiY);
							float height = font2.GetHeight(dpiY);
							bitmap = new Bitmap((int)textSizeF.Width, (int)(textSizeF.Height + height));
							bitmap.SetResolution(dpiX, dpiY);
							using (Graphics graphics = Graphics.FromImage(bitmap))
							{
								float num = dpiX / 96f * 5f;
								graphics.DrawString(text, font, Brushes.Black, num, num);
								RectangleF layoutRectangle = new RectangleF(0f, textSizeF.Height, textSizeF.Width, height);
								using (StringFormat stringFormat = new StringFormat(StringFormatFlags.NoClip)
								{
									Alignment = StringAlignment.Center
								})
								{
									graphics.DrawString(text, font2, Brushes.Black, layoutRectangle, stringFormat);
								}
							}
						}
						catch (Exception)
						{
							if (bitmap != null)
							{
								bitmap.Dispose();
							}
						}
					}
				}
			}
			return bitmap;
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002218 File Offset: 0x00000418
		private static SizeF GetTextSizeF(string text, Font font, float dpiX, float dpiY)
		{
			SizeF result;
			using (Bitmap bitmap = new Bitmap(1, 1))
			{
				bitmap.SetResolution(dpiX, dpiY);
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					result = graphics.MeasureString(text, font);
				}
			}
			return result;
		}

		// Token: 0x04000002 RID: 2
		protected const float DefaultDpi = 96f;

		// Token: 0x04000003 RID: 3
		protected const string TextFontName = "Courier New";

		// Token: 0x04000004 RID: 4
		protected const int TextFontSize = 10;
	}
}
