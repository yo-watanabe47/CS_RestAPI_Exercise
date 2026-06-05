namespace RestAPI_Exercise.Application.Exceptions;
/// <summary>
/// データが既に存在するエラーを表す例外クラス
/// </summary>
public class ExistsException : Exception
{
    public ExistsException(string message) : base(message) { }
    public ExistsException(string message, Exception innerException) : base(message, innerException) { }
}