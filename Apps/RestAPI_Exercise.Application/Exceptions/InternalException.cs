namespace RestAPI_Exercise.Application.Exceptions;
/// <summary>
/// 内部エラーを表す例外クラス
/// </summary>
public class InternalException : Exception
{
    public InternalException(string message) : 
    base(message) { }
    public InternalException(string message, Exception innerException) : 
    base(message, innerException) { }
}