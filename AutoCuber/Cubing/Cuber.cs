using GMSMacro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AutoCuber.Structs;

namespace AutoCuber.Cubing
{
    public class Cuber
    {
        public async Task Run()
        {
            var oneMoreTryImg = Image.FromFile(@"images/onemoretry.png") as Bitmap
                ?? throw new Exception("Error loading required image");
            var oneMoreTryHoverImg = Image.FromFile(@"images/onemoretry_hov.png")! as Bitmap
                ?? throw new Exception("Error loading required image");
            var okImg = Image.FromFile(@"images/ok.png") as Bitmap
                ?? throw new Exception("Error loading required image");
            var resultImg = Image.FromFile(@"images/result.png") as Bitmap
                ?? throw new Exception("Error loading required image");
            var afterImg = Image.FromFile(@"images/after.png") as Bitmap
                ?? throw new Exception("Error loading required image");
            var postClickDelay = 100;
            var postRerollDelay = 1400;
            var imageSearchTimeout = 2500;

            var procs = Process.GetProcesses();
            var procHandle = procs.FirstOrDefault(p => p.ProcessName.ToLower().Contains("maplestory"))?.MainWindowHandle
                ?? throw new DirectoryNotFoundException("Unable to find maplestory proc");

            // get desired roll
            var desiredCombos = new List<List<CubeLine>>();
            var combo1 = new List<CubeLine> {
                new CubeLine()
                {
                    Type = CubeLine.LUK,
                    Value = 27
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
            //desiredCombos.Add(combo2);
            var combo3 = new List<CubeLine> {
                new CubeLine()
                {
                    Type = CubeLine.Drop,
                    Value = 40
                },
            };
            //desiredCombos.Add(combo3);

            var combo4 = new List<CubeLine> {
                new CubeLine()
                {
                    Type = CubeLine.Meso,
                    Value = 40
                },
            };
            //desiredCombos.Add(combo4);
            var combo5 = new List<CubeLine> {
                new CubeLine()
                {
                    Type = CubeLine.Crit,
                    Value = 16
                },
            };
            desiredCombos.Add(combo5);
            var combo6 = new List<CubeLine> {
                new CubeLine()
                {
                    Type = CubeLine.BossDmg,
                    Value = 3
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
                    Value = 6
                },
                new CubeLine()
                {
                    Type = CubeLine.ATT,
                    Value = 9
                },
            };
            desiredCombos.Add(combo8);
            var combo9 = new List<CubeLine> {
                new CubeLine()
                {
                    Type = CubeLine.IED,
                    Value = 30
                },
                new CubeLine()
                {
                    Type = CubeLine.ATT,
                    Value = 18
                },
            };
            desiredCombos.Add(combo9);  
            var combo10 = new List<CubeLine> {
                new CubeLine()
                {
                    Type = CubeLine.IED,
                    Value = 30
                },              
                new CubeLine()
                {
                    Type = CubeLine.BossDmg,
                    Value = 3
                },
                new CubeLine()
                {
                    Type = CubeLine.ATT,
                    Value = 9
                },
            };
            desiredCombos.Add(combo10);

            bool hit = false;

            while (!hit)
            {
                var oneMoreTryPoint = await ImageHelpers.FindOneImageCoordsInProcAsync(procHandle, [oneMoreTryImg, oneMoreTryHoverImg], .1, imageSearchTimeout)
                    ?? throw new Exception("Couldn't find one more try button");

                var targetPoint = new Point(oneMoreTryPoint.X + oneMoreTryImg.Width + 10, oneMoreTryPoint.Y);
                InputHub.ClickOnPoint(targetPoint, procHandle);
                var reset = false;
                for (int i = 0; i < 3; i++)
                {
                    var okPoint = await ImageHelpers.FindImageCoordsInProcAsync(procHandle, okImg!, .1, imageSearchTimeout);
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

                oneMoreTryPoint = await ImageHelpers.FindOneImageCoordsInProcAsync(procHandle, [oneMoreTryImg, oneMoreTryHoverImg], .1, imageSearchTimeout)
                    ?? throw new Exception("Couldn't find one more try button");
                await Task.Delay(postRerollDelay);

                // determine current roll

                var resultsBounds = await ImageHelpers.FindOneImageCoordsInProcAsync(procHandle, [afterImg, resultImg], .1, imageSearchTimeout)
                    ?? throw new Exception("Couldn't find results");

                var resultImage = ScreenCapture.CaptureWindow(procHandle, new Rectangle(new Point(resultsBounds.X + 2, resultsBounds.Y + 32), new Size(160, 42)));
                int partHeight = resultImage.Height / 3;
                // split resultImage into 3 lines
                var slicedImages = Enumerable.Range(0, 3).Select((idx) =>
                    resultImage.Clone(new Rectangle(0, idx * partHeight, resultImage.Width, partHeight), resultImage.PixelFormat)).ToArray();

                resultImage.Save($"results/{Guid.NewGuid()}_result.png");
                foreach (var b in slicedImages)
                    b.Save($"results/{Guid.NewGuid()}_line.png");

                var results = slicedImages.Select(b => TesseractHelper.ReadBitmap(b)).ToList();
                File.AppendAllLines("cuberesults.txt", results);

                var parsedResults = results.Select(r => new CubeLine(r)).ToArray();

                foreach (var parsedResult in parsedResults)
                    Console.WriteLine(parsedResult.ToString());

                var sumsDictionary = parsedResults
                    .GroupBy(r => r.Type)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Sum(item => item.Value));


                // apply all stat sum value to each stat sum
                if (sumsDictionary.TryGetValue(CubeLine.AllStats, out var allStatSum))
                {
                    var statKeys = new[] { CubeLine.INT, CubeLine.DEX, CubeLine.STR, CubeLine.LUK };

                    foreach (var key in statKeys)
                    {
                        if (sumsDictionary.ContainsKey(key))
                        {
                            sumsDictionary[key] += allStatSum;
                        }
                        else
                        {
                            sumsDictionary.Add(key, allStatSum);
                        }
                    }
                }

                // check for a hit
                foreach (var combo in desiredCombos)
                {
                    hit = true;
                    foreach (var option in combo)
                    {
                        if (!sumsDictionary.TryGetValue(option.Type, out var relevantSumValue) || relevantSumValue < option.Value)
                        {
                            hit = false;
                            break;
                        }
                    }
                    if (hit)
                    {
                        break; // If hit is true, we found our desired combo and can exit the loop
                    }
                }
            }
        }
    }
}
