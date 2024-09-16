using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace AutoCuber
{
    public static class TesseractHelper
    {

        public static string? ReadBitmap(Bitmap bitmap)
        {
            using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
            {
                using (var img = PixConverter.ToPix(bitmap))
                {
                    using (var result = engine.Process(img))
                    {
                        var text = result.GetText();
                        return text;
                    }
                }
            }
        }
    }
}
