namespace RestAPI_Exercise.Application.Exceptions;
/// <summary>
/// 内部エラーを表す例外クラス
/// HTTPstatuscode:500 Internal Server Errorを表す
/// </summary>
public class InternalException : Exception
{
    public InternalException(string message) : 
    base(message) { }
    public InternalException(string message, Exception innerException) : 
    base(message, innerException) { }
}