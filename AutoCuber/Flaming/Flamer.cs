using AutoCuber.AutoFlamer;
using GMSMacro;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using static AutoCuber.Structs;

namespace AutoCuber.Flaming
{
    public class Flamer
    {

        public async Task Run()
        {
            using FileStream stream = File.OpenRead(@"RequestedFlame.json");
            var requestedFlame = JsonSerializer.Deserialize<RequestedFlame>(stream);
            var flameScoreFactors = requestedFlame.FlameScoreValues.ToDictionary(
                keySelector: x => x.Stat,
                elementSelector: x => x.Value);

            Console.WriteLine($"Flame score factors:\n" +
                $"{string.Join('\n',flameScoreFactors.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}\n" +
                $"Target score: {requestedFlame.RequestedFlameScore}\n");

            var tryAgainImg = Image.FromFile(@"images/tryagain.png") as Bitmap
                ?? throw new Exception("Error loading required image");
            var tryAgainHoverImg = Image.FromFile(@"images/tryagain_hov.png")! as Bitmap
                ?? throw new Exception("Error loading required image");
            var okImg = Image.FromFile(@"images/ok.png") as Bitmap
                ?? throw new Exception("Error loading required image");
            var resultImg = Image.FromFile(@"images/result.png") as Bitmap
                ?? throw new Exception("Error loading required image");
            var afterImg = Image.FromFile(@"images/after.png") as Bitmap
                ?? throw new Exception("Error loading required image");

            var postClickDelay = 100;
            var postRerollDelay = 1400;
            var imageSearchTimeout = 10000;

            var procs = Process.GetProcesses();
            var procHandle = procs.FirstOrDefault(p => p.ProcessName.ToLower().Contains("maplestory"))?.MainWindowHandle
                ?? throw new DirectoryNotFoundException("Unable to find maplestory proc");


            double currentFlameScore;

            do
            {
                currentFlameScore = 0;

                var tryAgainPoint = await ImageHelpers.FindOneImageCoordsInProcAsync(procHandle, [tryAgainImg, tryAgainHoverImg], .1, imageSearchTimeout)
                   ?? throw new Exception("Couldn't find one more try button");

                var targetPoint = new Point(tryAgainPoint.X + tryAgainImg.Width + 2, tryAgainPoint.Y);

                InputHub.ClickOnPoint(targetPoint, procHandle);
                for (int i = 0; i < 3; i++)
                {
                    var okPoint = await ImageHelpers.FindImageCoordsInProcAsync(procHandle, okImg!, .1, imageSearchTimeout)
                     ?? throw new Exception("Couldn't find ok button");

                    await Task.Delay(postClickDelay);
                    InputHub.SendKey(ScanCodeShort.RETURN);
                }

                tryAgainPoint = await ImageHelpers.FindOneImageCoordsInProcAsync(procHandle, [tryAgainImg, tryAgainHoverImg], .1, imageSearchTimeout)
                   ?? throw new Exception("Couldn't find one more try button");
                await Task.Delay(postRerollDelay);

                var resultsPoint = await ImageHelpers.FindOneImageCoordsInProcAsync(procHandle, [afterImg, resultImg], .1, imageSearchTimeout)
                    ?? throw new Exception("Couldn't find results");

                var fullResultsBounds = new Rectangle(resultsPoint.X+5, resultsPoint.Y + 22, 150, 86);
                int maxLines = 6;
                var resultLineBounds = Enumerable.Range(0, maxLines)
                    .Select(i => new Rectangle(
                        x: fullResultsBounds.X,
                        y: fullResultsBounds.Y + (fullResultsBounds.Height / maxLines * i),
                        width: fullResultsBounds.Width,
                        height: fullResultsBounds.Height / maxLines));

                var resultImages = resultLineBounds.Select(r => ScreenCapture.CaptureWindow(procHandle, r));

                //foreach (var img in resultImages)
                //{
                //    img.Save($"results/{Guid.NewGuid()}_result.png");
                //}

                var stringResults = resultImages.Select(TesseractHelper.ReadBitmap).ToList();

                var parsedResults = stringResults.Where(s => !string.IsNullOrWhiteSpace(s)).Select(r => new FlameLine(r!));

                foreach (var res in parsedResults)
                {
                    var factor = flameScoreFactors.GetValueOrDefault(res.Stat, 0);
                    var contribution = res.Value * factor;
                    Console.WriteLine($"Stat: {res.Stat}, Score contribution: {contribution}");
                    currentFlameScore += contribution;
                }

                Console.Out.WriteLine($"Total flame Score: {currentFlameScore}\n" +
                    $"====================================================");
            }
            while (requestedFlame.RequestedFlameScore > currentFlameScore);
        }

    }
}
