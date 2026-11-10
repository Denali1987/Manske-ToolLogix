using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using ToolLogix.Models;

namespace ToolLogix.Controllers
{
    [Authorize]
    public class DowntimesController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new DowntimeDashboardViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> Index(
            DowntimeDashboardViewModel viewModel)
        {
            if (viewModel.CsvFile == null ||
                viewModel.CsvFile.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(viewModel.CsvFile),
                    "Select a downtime CSV file.");

                return View(viewModel);
            }

            if (!string.Equals(
                    Path.GetExtension(viewModel.CsvFile.FileName),
                    ".csv",
                    StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    nameof(viewModel.CsvFile),
                    "The selected file must be a CSV file.");

                return View(viewModel);
            }

            if (viewModel.CsvFile.Length > 10_000_000)
            {
                ModelState.AddModelError(
                    nameof(viewModel.CsvFile),
                    "The CSV file cannot exceed 10 MB.");

                return View(viewModel);
            }

            try
            {
                using var stream = viewModel.CsvFile.OpenReadStream();

                var options = new CsvReaderOptions
                {
                    TrimWhitespace = true
                };

                using var csv = CsvReader.Create(
                    stream,
                    options);

                var records = new List<DowntimeCsvRecord>();

                await foreach (
                    var record in csv
                        .GetRecordsAsync<DowntimeCsvRecord>())
                {
                    records.Add(record);
                }

                if (records.Count == 0)
                {
                    ModelState.AddModelError(
                        nameof(viewModel.CsvFile),
                        "The CSV file does not contain any downtime records.");

                    return View(viewModel);
                }

                viewModel.FileName =
                    Path.GetFileName(viewModel.CsvFile.FileName);

                viewModel.AsOfTime = records.Max(
                    record => record.LocalEndTime);

                // Use only the latest record for each press/work center.
                var latestRecords = records
                    .GroupBy(
                        record => record.WorkCenter,
                        StringComparer.OrdinalIgnoreCase)
                    .Select(group => group
                        .OrderByDescending(record =>
                            record.LocalEndTime)
                        .ThenByDescending(record =>
                            record.LocalStartTime)
                        .First())
                    .ToList();

                foreach (var record in latestRecords)
                {
                    var status = GetToolroomStatus(
                        record.DowntimeReasonCode);

                    if (status == null)
                    {
                        continue;
                    }

                    var row = new DowntimeRowViewModel
                    {
                        WorkCenter = record.WorkCenter,
                        WorkOrder = record.WorkOrder,
                        PartNumber = record.PartNumber,
                        Tool = record.Tool,
                        DowntimeReason = record.DowntimeReason,
                        DowntimeReasonCode =
                            record.DowntimeReasonCode,
                        Status = status,
                        EmployeeName = record.EmployeeName,
                        StartTime = record.LocalStartTime,
                        EndTime = record.LocalEndTime,
                        DurationMinutes = record.DurationMinutes,
                        Planned = record.Planned == 1,
                        Costed = record.Costed,
                        SapBucket = record.SAPBucket
                    };

                    if (status == "Tool Repair" ||
                        status == "Toolroom Changeover")
                    {
                        viewModel.InProcess.Add(row);
                    }
                    else
                    {
                        viewModel.Waiting.Add(row);
                    }
                }

                viewModel.InProcess = viewModel.InProcess
                    .OrderByDescending(row => row.DurationMinutes)
                    .ThenBy(row => row.WorkCenter)
                    .ToList();

                viewModel.Waiting = viewModel.Waiting
                    .OrderByDescending(row => row.DurationMinutes)
                    .ThenBy(row => row.WorkCenter)
                    .ToList();

                viewModel.RepairInProcessCount =
                    viewModel.InProcess.Count(row =>
                        row.Status == "Tool Repair");

                viewModel.ChangeoverInProcessCount =
                    viewModel.InProcess.Count(row =>
                        row.Status == "Toolroom Changeover");

                viewModel.WaitingForRepairCount =
                    viewModel.Waiting.Count(row =>
                        row.Status == "Waiting for Repair");

                viewModel.WaitingForChangeoverCount =
                    viewModel.Waiting.Count(row =>
                        row.Status == "Waiting for Changeover");

                AddDataWarnings(viewModel);

                return View(viewModel);
            }
            catch (KeyNotFoundException)
            {
                ModelState.AddModelError(
                    nameof(viewModel.CsvFile),
                    "The CSV file does not contain the expected downtime columns.");
            }
            catch (CsvFormatException)
            {
                ModelState.AddModelError(
                    nameof(viewModel.CsvFile),
                    "A downtime record contains invalid CSV data.");
            }
            catch (FormatException)
            {
                ModelState.AddModelError(
                    nameof(viewModel.CsvFile),
                    "A downtime value or date is not in the expected format.");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    nameof(viewModel.CsvFile),
                    "The CSV file could not be processed.");
            }

            return View(viewModel);
        }

        private static string? GetToolroomStatus(string code)
        {
            if (code.Contains(
                    "Waiting for TR Repair",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Waiting for Repair";
            }

            if (code.Contains(
                    "Waiting for TR CO",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Waiting for Changeover";
            }

            if (code.Contains(
                    "Tool Repair",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Tool Repair";
            }

            if (code.Contains(
                    "Toolroom Setup / CO",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Toolroom Changeover";
            }

            return null;
        }

        private static void AddDataWarnings(
            DowntimeDashboardViewModel viewModel)
        {
            var displayedRows = viewModel.InProcess
                .Concat(viewModel.Waiting)
                .ToList();

            var missingToolCount = displayedRows.Count(row =>
                string.IsNullOrWhiteSpace(row.Tool));

            if (missingToolCount > 0)
            {
                viewModel.Warnings.Add(
                    $"{missingToolCount} displayed " +
                    $"record{(missingToolCount == 1 ? "" : "s")} " +
                    "do not include a tool number.");
            }

            var genericEmployeeCount = displayedRows.Count(row =>
                string.Equals(
                    row.EmployeeName,
                    "aip_opc",
                    StringComparison.OrdinalIgnoreCase));

            if (genericEmployeeCount > 0)
            {
                viewModel.Warnings.Add(
                    $"{genericEmployeeCount} displayed " +
                    $"record{(genericEmployeeCount == 1 ? "" : "s")} " +
                    "use the generic employee value aip_opc.");
            }
        }
    }
}