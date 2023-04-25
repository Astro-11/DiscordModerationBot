using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace DiscordBot2._0
{
    internal class Spreadsheet
    {
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string ApplicationName = "NaughtyList";
        static readonly string SpreadSheetID = ""; //Insert Google Spreadsheet ID
        static readonly string sheet = "NaughtyList";
        static SheetsService service;

        public static void GoogleSheet()
        {
            GoogleCredential credential;

            using (var stream = new FileStream("ServiceAccountCredentials.json", FileMode.Open, FileAccess.Read)) //Insert json with Google API ServiceAccountCredentials
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }

            service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        public static string[] ReadEntry(string _startingCell, string _endingCell)
        {
            var range = $"{sheet}!{_startingCell}:{_endingCell}";
            var request = service.Spreadsheets.Values.Get(SpreadSheetID, range);
            var response = request.Execute();
            var values = response.Values;

            if (values != null && values.Count > 0)
            {
                List<string> cellsList = new List<string>();
                int i = 0;

                foreach (var row in values)
                {
                    foreach (string cell in row)
                    {
                        cellsList.Add(cell);
                    }
                }

                return cellsList.ToArray();
            }
            else
            {
                Console.WriteLine("No data was found");
                return null;
            }
        }

        public static void CreateUser(string _userName, string _userID)
        {
            var range = $"{sheet}!A:B";
            var valueRange = new ValueRange();

            var objectList = new List<object>() { $"{_userName}", $"{_userID}", "/", "/", "/", "/", "/", "", "/", "/", "/", "/" };
            valueRange.Values = new List<IList<object>> { objectList };

            var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadSheetID, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
        }

        public static void CreateLog(string _messageToLog)
        {
            var range = $"{sheet}!N:N";
            var valueRange = new ValueRange();

            var objectList = new List<object>() { _messageToLog };
            valueRange.Values = new List<IList<object>> { objectList };

            var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadSheetID, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
        }

        public static void UpdateEntry(string _columnToEdit, string _rowToEdit, string _updatedEntry)
        {
            var range = $"{sheet}!{_columnToEdit}{_rowToEdit}";
            var valueRange = new ValueRange();

            var objectList = new List<object>() { $"{_updatedEntry}" };
            valueRange.Values = new List<IList<object>> { objectList };

            var updateRequest = service.Spreadsheets.Values.Update(valueRange, SpreadSheetID, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var updateResponse = updateRequest.Execute();
        }

        public static void DeleteEntry(string _startingCell, string _endingCell)
        {
            var range = $"{sheet}!{_startingCell}:{_endingCell}";
            var requestBody = new ClearValuesRequest();

            var deleteRequest = service.Spreadsheets.Values.Clear(requestBody, SpreadSheetID, range);
            var deleteResponse = deleteRequest.Execute();
        }
    }
}
