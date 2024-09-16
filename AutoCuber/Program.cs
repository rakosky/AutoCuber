using GMSMacro;
using IronOcr;
using System;
using System.Diagnostics;
using System.Drawing;
using Tesseract;
using static AutoCuber.Structs;

namespace AutoCuber
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var oneMoreTryImg = Bitmap.FromFile(@"images/onemoretry.png") as Bitmap
                ?? throw new Exception("Error loading required image");
            var oneMoreTryHoverImg = Bitmap.FromFile(@"images/onemoretry_hov.png")! as Bitmap
                ?? throw new Exception("Error loading required image");
            var okImg = Bitmap.FromFile(@"images/ok.png") as Bitmap
                ?? throw new Exception("Error loading required image");
            var resultImg = Bitmap.FromFile(@"images/result.png") as Bitmap
                ?? throw new Exception("Error loading required image");
            var afterImg = Bitmap.FromFile(@"images/after.png") as Bitmap
                ?? throw new Exception("Error loading required image");
            var postClickDelay = 100;
            var postRerollDelay = 1400;

            var procs = Process.GetProcesses();
            var procHandle = procs.FirstOrDefault(p => p.ProcessName.ToLower().Contains("maplestory"))?.MainWindowHandle
                ?? throw new DirectoryNotFoundException("Unable to find maplestory proc");

            // get desired roll
            var desiredCombos = new List<List<CubeLine>>();
            var combo1 = new List<CubeLine> {
                new CubeLine()
                {
                    Type = CubeLine.AllStats,
                    Value = 15
                }
            };
            desiredCombos.Add(combo1);
            var combo2 = new List<CubeLine> {
                new CubeLine()
                {
                    Type = CubeLine.Drop,
                    Value = 20
                },
                new CubeLine()
                {
                    Type = CubeLine.Meso,
                    Value = 20
                }
            };
            desiredCombos.Add(combo2);
            var combo3 = new List<CubeLine> {
                new CubeLine()
                {
                    Type = CubeLine.Drop,
                    Value = 40
                },
            };
            desiredCombos.Add(combo3);
                        
            var combo4 = new List<CubeLine> {
                new CubeLine()
                {
                    Type = CubeLine.Meso,
                    Value = 40
                },
            };
            desiredCombos.Add(combo4);                        
            //var combo5 = new List<CubeLine> {
            //    new CubeLine()
            //    {
            //        Type = CubeLine.Crit,
            //        Value = 8
            //    },
            //};
            //desiredCombos.Add(combo5); 
            var combo6 = new List<CubeLine> {
                new CubeLine()
                {
                    Type = CubeLine.BossDmg,
                    Value = 30
                },
                new CubeLine()
                {
                    Type = CubeLine.ATT,
                    Value = 18
                },
            };
            desiredCombos.Add(combo6);
                        
            var combo7 = new List<CubeLine> {
                new CubeLine()
                {
                    Type = CubeLine.ATT,
                    Value = 27
                },
            };
            desiredCombos.Add(combo7);
            var combo8 = new List<CubeLine> {
                new CubeLine()
                {
                    Type = CubeLine.BossDmg,
                    Value = 60
                },
                new CubeLine()
                {
                    Type = CubeLine.ATT,
                    Value = 9
                },
            }; 
            desiredCombos.Add(combo8);

            bool hit = false;

            while (!hit)
            {
                var oneMoreTryPoint = await ImageHelpers.FindOneImageCoordsInProcAsync(procHandle, [oneMoreTryImg, oneMoreTryHoverImg], .1, 2500)
                    ?? throw new Exception("Couldn't find one more try button");

                var targetPoint = new Point(oneMoreTryPoint.X + oneMoreTryImg.Width + 10, oneMoreTryPoint.Y);
                InputHub.ClickOnPoint(targetPoint, procHandle);
                var reset = false;
                for(int i = 0; i <3; i++)
                {

                    var okPoint = await ImageHelpers.FindImageCoordsInProcAsync(procHandle, okImg!, .1, 2500);
                    if (okPoint == null)
                    {
                        if (i == 2) //only finding 2 OK buttons means we got the 'try again' shit, so reset flow
                            reset = true;
                        else
                            throw new Exception("Couldn't find ok button");
                    }
                    await Task.Delay(postClickDelay);
                    InputHub.SendKey(ScanCodeShort.RETURN);
                }
                if (reset)
                    continue;

                oneMoreTryPoint = await ImageHelpers.FindOneImageCoordsInProcAsync(procHandle, [oneMoreTryImg, oneMoreTryHoverImg], .1, 2500)
                    ?? throw new Exception("Couldn't find one more try button");
                await Task.Delay(postRerollDelay);

                // determine current roll

                var resultsBounds = await ImageHelpers.FindOneImageCoordsInProcAsync(procHandle, [afterImg, resultImg], .1, 2500)
                    ?? throw new Exception("Couldn't find results");

                var resultImage = ScreenCapture.CaptureWindow(procHandle, new Rectangle(new Point(resultsBounds.X + 4, resultsBounds.Y + 32), new Size(160,42)));
                int partHeight = resultImage.Height / 3;
                // split resultImage into 3 lines
                var slicedImages = Enumerable.Range(0, 3).Select((idx) => 
                    resultImage.Clone(new Rectangle(0, idx * partHeight, resultImage.Width, partHeight), resultImage.PixelFormat)).ToArray();

                var results = slicedImages.Select(b => TesseractHelper.ReadBitmap(b)).ToList();
                File.AppendAllLines("cuberesults.txt", results);
                
                var parsedResults = results.Select(r => new CubeLine(r)).ToArray();

                foreach (var parsedResult in parsedResults)
                    Console.WriteLine(parsedResult.ToString());

                hit = true;
                var sums = parsedResults
                    .GroupBy(r => r.Type)
                    .Select(group => new CubeLine
                    {
                        Type = group.Key,
                        Value = group.Sum(item => item.Value)
                    });


                foreach (var combo in desiredCombos)
                {
                    foreach (var option in combo)
                    {
                        hit = true;
                        var releventSum = sums.FirstOrDefault(s => s.Type == option.Type);
                        if (releventSum is null || releventSum.Value < option.Value)
                        {
                            hit = false;
                            break;
                        }
                    }
                    if (hit == true)
                        break;
                }
            }
        }
    }
}
