using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BankAccounts.Data;
using BankAccounts.Exceptions;
using BankAccounts.Models;
using ExcelDataReader;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace BankAccounts.Helpers
{
    public class ExcelHelper
    {
        private IUnitOfWork unitOfWork;

        public ExcelHelper(IUnitOfWork unitOfWork) => this.unitOfWork = unitOfWork;

        /// <summary>
        /// Читает информацию из потока и сохраняет в базу данных
        /// </summary>
        /// <param name="stream">Поток, из которого считываются данные загружаемого файла</param>
        /// <param name="fileName">Имя загружаемого файла</param>
        /// <returns>Объект Task, который представляет асинхронную операцию</returns>
        public async Task SaveFileToDbAsync(Stream stream, string fileName)
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                using (var transaction = await unitOfWork.Context.Database.BeginTransactionAsync())
                {
                    string bankName;
                    string actualFileName = Path.GetFileNameWithoutExtension(fileName);
                    // При наличии в БД файла с таким же именем, происходит добавление
                    // номера: file->file (1), или присвоение следующего номера: file (3)->file (4)
                    while ((await unitOfWork.FileRepository.GetAllAsync()).Where(f => f.Name == actualFileName).Count() > 0)
                        actualFileName = GetNextFileName(actualFileName);

                    if (reader.Read())
                        bankName = reader.GetString(0) ?? "";
                    else
                        throw new FileEmptyException();

                    LoadFile(actualFileName, bankName);
                    await unitOfWork.SaveAsync();

                    while (reader.Read())
                    {
                        if (IsAccount(reader))
                        {
                            object val0 = reader.GetValue(0);
                            object val1 = reader.GetValue(1);
                            string classNumber = val0.ToString().Substring(0, 1);
                            int classId = (await unitOfWork.FileRepository.GetAllAsync()).Single(f => f.Name == actualFileName).Classes.Single(c => c.Number == classNumber).Id;
                            LoadAccount(reader, classId);
                            //После загрузки аккаунта в контекст БД сохранение не производится,
                            //так как не является необходимым, что позволяет повысить произоводительность
                        }
                        else if (IsClass(reader))
                        {
                            string str0 = reader.GetString(0);
                            int fileId = (await unitOfWork.FileRepository.GetAllAsync()).Single(f => f.Name == actualFileName).Id;
                            LoadClass(str0, fileId);
                            await unitOfWork.SaveAsync();
                        }
                    }

                    await unitOfWork.SaveAsync();
                    await transaction.CommitAsync();
                }
            }
        }

        /// <summary>
        /// Извлекает данные из файла в БД
        /// </summary>
        /// <param name="id">Идентификатор файла</param>
        /// <returns>
        /// Объект, представляющий асинхронную операцию с результатом 
        /// в виде массива байтов
        /// </returns>
        public async Task<byte[]> GetFileContentAsync(int id)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var excelPackage = new ExcelPackage())
            {
                ExcelWorksheet sheet = excelPackage.Workbook.Worksheets.Add("Sheet1");
                var file = await unitOfWork.FileRepository.GetByIdAsync(id);
                sheet.Cells["A1"].Value = file.BankName;

                CreateHeader(sheet);
                int row = 5;
                decimal[] fileValues = new decimal[6];
                var classes = file.Classes.OrderBy(c => c.Number);
                foreach (var @class in classes)
                {
                    sheet.Cells[row, 1, row, 7].Merge = true;
                    sheet.Cells[row, 1, row, 7].Value = @class.Description;
                    sheet.Cells[row, 1, row, 7].Style.Font.Bold = true;
                    sheet.Cells[row, 1, row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[row, 1, row, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    sheet.Row(row).Height *= 2;
                    ++row;
                    decimal[] classValues = new decimal[6];

                    var groups = @class.Accounts.GroupBy(a => a.Number.Substring(0, 2)).OrderBy(g => g.Key);
                    foreach (var group in groups)
                    {
                        decimal[] groupValues = new decimal[6];

                        var accounts = group.OrderBy(a => a.Number);
                        foreach (var account in accounts)
                        {
                            decimal[] accountValues = new decimal[] { account.InActive, account.InPassive, account.Debet, account.Credit, account.OutActive, account.OutPassive };
                            sheet.Cells[row, 1].Value = account.Number;
                            accountValues.Select((e, i) => sheet.Cells[row, i + 2].Value = e).Count();
                            sheet.Cells[row, 2, row, 7].Style.Numberformat.Format = "#,##0.00";
                            ++row;

                            groupValues = groupValues.Select((v, i) => v + accountValues[i]).ToArray();
                        }

                        sheet.Cells[row, 1].Value = group.Key;
                        groupValues.Select((e, i) => sheet.Cells[row, i + 2].Value = e).Count();
                        sheet.Cells[row, 1, row, 7].Style.Font.Bold = true;
                        sheet.Cells[row, 2, row, 7].Style.Numberformat.Format = "#,##0.00";
                        ++row;

                        classValues = classValues.Select((v, i) => v + groupValues[i]).ToArray();
                    }

                    sheet.Cells[row, 1].Value = "По классу";
                    classValues.Select((e, i) => sheet.Cells[row, i+2].Value = e).Count();
                    sheet.Cells[row, 1, row, 7].Style.Font.Bold = true;
                    sheet.Cells[row, 2, row, 7].Style.Numberformat.Format = "#,##0.00";
                    ++row;

                    fileValues = fileValues.Select((v, i) => v + classValues[i]).ToArray();
                }

                sheet.Cells[row, 1].Value = "Баланс";
                fileValues.Select((e, i) => sheet.Cells[row, i+2].Value = e).Count();
                sheet.Cells[row, 1, row, 7].Style.Font.Bold = true;
                sheet.Cells[row, 2, row, 7].Style.Numberformat.Format = "#,##0.00";

                return excelPackage.GetAsByteArray();
            }
        }

        /// <summary>
        /// Удаляет файл из базы данных
        /// </summary>
        /// <param name="id">Идентификатор файла</param>
        /// <returns>Объект, представляющий асинхронную операцию</returns>
        public Task DeleteFileAsync(int id)
        {
            unitOfWork.FileRepository.Delete(id);
            return unitOfWork.SaveAsync();
        }

        /// <summary>
        /// Конструирует представление данных файла
        /// </summary>
        /// <param name="fileId">Идентификатор файла</param>
        /// <returns>
        /// Объект, представляющий асинхронную операцию, с результатом
        /// в виде представления файла
        /// </returns>
        public async Task<FileViewModel> GetFileViewAsync(int fileId)
        {
            var file = (await unitOfWork.FileRepository.GetByIdAsync(fileId));
            if (file == null)
                return null;
            
            var fileView = new FileViewModel() { Id = file.Id, Name = file.Name, BankName = file.BankName, Values = new decimal[6], Classes = new List<ClassViewModel>(), };
            var classes = file.Classes.OrderBy(c => c.Number);
            foreach (var @class in classes)
            {
                var classView = new ClassViewModel() { Description = @class.Description, Values = new decimal[6], Groups = new List<GroupViewModel>() };
                var groups = @class.Accounts.GroupBy(a => a.Number.Substring(0, 2)).OrderBy(g => g.Key);
                foreach (var group in groups)
                {
                    var groupView = new GroupViewModel() { Number = group.Key, Values = new decimal[6], Accounts = new List<AccountViewModel>() };
                    var accounts = group.OrderBy(a => a.Number);
                    foreach (var account in accounts)
                    {
                        var accountView = new AccountViewModel()
                        {
                            Number = account.Number,
                            Values = new decimal[] { account.InActive, account.InPassive, account.Debet, account.Credit, account.OutActive, account.OutPassive }
                        };
                        groupView.Values = groupView.Values.Select((v, i) => v + accountView.Values[i]).ToArray();
                        groupView.Accounts.Add(accountView);
                    }

                    classView.Values = classView.Values.Select((v, i) => v + groupView.Values[i]).ToArray();
                    classView.Groups.Add(groupView);
                }

                fileView.Values = fileView.Values.Select((v, i) => v + classView.Values[i]).ToArray();
                fileView.Classes.Add(classView);
            }

            return fileView;
        }

        /// <summary>
        /// Проверяет, является ли считанная строка записью об аккаунте
        /// </summary>
        /// <param name="val0">Предполагаемая первая ячейка строки</param>
        /// <param name="val1">Предполагаемая вторая ячейка строки</param>
        /// <returns>Значение, определяющее, является ли строк</returns>
        private bool IsAccount(IExcelDataReader reader)
        {
            object val0 = reader.GetValue(0);
            object val1 = reader.GetValue(1);
            return val1 as double? != null && int.TryParse(val0?.ToString(), out _) && val0.ToString().Length > 2;
        }

        /// <summary>
        /// Проверяет, является ли считанная строка записью об классе
        /// </summary>
        /// <param name="val0"></param>
        /// <returns></returns>
        private bool IsClass(IExcelDataReader reader)
        {
            object val0 = reader.GetValue(0);
            return val0 is string str && str.Length > 5 && str.Substring(0, 5).ToUpper() == "КЛАСС";
        }

        /// <summary>
        /// Получает имя файла с номером повторившегося имени файла
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <returns>Имя файла с номером повторившегося имени файла</returns>
        private string GetNextFileName(string fileName)
        {
            var regex = new Regex(@" \((\d+)\)$");
            if (regex.IsMatch(fileName))
            {
                var match = regex.Match(fileName);
                var number = int.Parse(match.Groups[1].Value);
                ++number;
                return Regex.Replace(fileName, @" \(\d+\)$", $" ({number})");
            }
            else
                return fileName + " (2)";
        }

        /// <summary>
        /// Формирует заголовок и ширину колонок рабочего листа
        /// </summary>
        /// <param name="sheet">Рабочий лист</param>
        private void CreateHeader(ExcelWorksheet sheet)
        {
            sheet.Column(1).Width += 5;
            sheet.Column(2).Width = 23;
            sheet.Column(3).Width = 23;
            sheet.Column(4).Width = 23;
            sheet.Column(5).Width = 23;
            sheet.Column(6).Width = 23;
            sheet.Column(7).Width = 23;
            sheet.Cells["A3:A4"].Merge = true;
            sheet.Cells["A3:A4"].Value = "Б/сч";
            sheet.Cells["B3:C3"].Merge = true;
            sheet.Cells["B3:C3"].Value = "Входящее сальдо";
            sheet.Cells["D3:E3"].Merge = true;
            sheet.Cells["D3:E3"].Value = "Обороты";
            sheet.Cells["F3:G3"].Merge = true;
            sheet.Cells["F3:G3"].Value = "Исходящее сальдо";
            sheet.Cells["B4"].Value = "Актив";
            sheet.Cells["C4"].Value = "Пассив";
            sheet.Cells["D4"].Value = "Дебет";
            sheet.Cells["E4"].Value = "Кредит";
            sheet.Cells["F4"].Value = "Актив";
            sheet.Cells["G4"].Value = "Пассив";
            sheet.Cells["A3:G4"].Style.Font.Bold = true;
            sheet.Cells["A3:G4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells["A3:G4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            sheet.Cells["A3:G3"].Style.Border.Top.Style = ExcelBorderStyle.Medium;
            sheet.Cells["A4:G4"].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
            sheet.Cells["G3:G4"].Style.Border.Right.Style = ExcelBorderStyle.Medium;
            sheet.Cells["A3:A4"].Style.Border.Right.Style = ExcelBorderStyle.Medium;
            sheet.Cells["B3:G3"].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
            sheet.Cells["C3:C4"].Style.Border.Right.Style = ExcelBorderStyle.Medium;
            sheet.Cells["E3:E4"].Style.Border.Right.Style = ExcelBorderStyle.Medium;
            sheet.Cells["D4"].Style.Border.Right.Style = ExcelBorderStyle.Medium;
            sheet.Cells["F4"].Style.Border.Right.Style = ExcelBorderStyle.Medium;
            sheet.Cells["B4"].Style.Border.Right.Style = ExcelBorderStyle.Medium;
        }

        /// <summary>
        /// Загружает файл в репозиторий
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <param name="bankName">Название банка</param>
        private void LoadFile(string fileName, string bankName)
        {
            var fileEntity = new Entities.File() { BankName = bankName, Name = fileName };
            unitOfWork.FileRepository.Create(fileEntity);
        }

        /// <summary>
        /// Загружает класс в репозиторий
        /// </summary>
        /// <param name="str">Строка описания класса</param>
        /// <param name="fileId">Идентификатор файла</param>
        private void LoadClass(string str, int fileId)
        {
            string[] array = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var classEntity = new Entities.Class() { Number = array[1], Description = str, FileId = fileId };
            unitOfWork.ClassRepository.Create(classEntity);
        }

        /// <summary>
        /// Загружает аккаунт в репозиторий
        /// </summary>
        /// <param name="reader">Считыватель данных из Excel файла</param>
        /// <param name="classId">Идентификатор класса</param>
        private void LoadAccount(IExcelDataReader reader, int classId)
        {
            var accountEntity = new Entities.Account()
            {
                ClassId = classId,
                Number = reader.GetValue(0).ToString(),
                InActive = (decimal)reader.GetDouble(1),
                InPassive = (decimal)reader.GetDouble(2),
                Debet = (decimal)reader.GetDouble(3),
                Credit = (decimal)reader.GetDouble(4),
                OutActive = (decimal)reader.GetDouble(5),
                OutPassive = (decimal)reader.GetDouble(6)
            };
            unitOfWork.AccountRepository.Create(accountEntity);
        }
    }
}
