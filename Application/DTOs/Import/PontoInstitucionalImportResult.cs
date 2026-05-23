namespace AppTurismoIndustrial.Api.Application.DTOs.Import;

public class PontoInstitucionalImportResult
{
    public int TotalRecords { get; set; }
    public int Inserted { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public string Status { get; set; } = "Completed";
    public List<PontoInstitucionalImportError> Errors { get; set; } = [];
}

public class PontoInstitucionalImportError
{
    public int LineNumber { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
